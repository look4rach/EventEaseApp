using Microsoft.JSInterop;
using System.Text.Json;
using EventEaseApp;

namespace EventEaseApp
{  
    public class StateManager : IStateManager
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger<StateManager> _logger;
        private SessionState _currentSession = new();
        private const string STORAGE_KEY = "eventease_session";
        private const string GLOBAL_EVENTS_KEY = "eventease_global_events"; // New key for global events

        public event Action? OnSessionChanged;
        public SessionState CurrentSession => _currentSession;

        public StateManager(IJSRuntime jsRuntime, ILogger<StateManager> logger)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing session state");
                var savedSession = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
                
                if (!string.IsNullOrEmpty(savedSession))
                {
                    var deserializedSession = JsonSerializer.Deserialize<SessionState>(savedSession);
                    if (deserializedSession != null && deserializedSession.IsAuthenticated)
                    {
                        _currentSession = deserializedSession;
                    }
                }
                NotifySessionChanged();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing session state");
                _currentSession = new SessionState();
                NotifySessionChanged();
            }
        }

        private async Task SaveToStorageAsync()
        {
            try
            {
                var sessionJson = JsonSerializer.Serialize(_currentSession);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", STORAGE_KEY, sessionJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving session state");
                throw;
            }
        }

        public async Task LoginAsync(User user)
        {
            try
            {
                // Try to load previous user session
                var userSessionKey = $"{STORAGE_KEY}_{user.Name}";
                var savedUserSession = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", userSessionKey);
                var previousUserEvents = new List<Event>();

                if (!string.IsNullOrEmpty(savedUserSession))
                {
                    var previousSession = JsonSerializer.Deserialize<SessionState>(savedUserSession);
                    if (previousSession != null)
                    {
                        previousUserEvents = previousSession.UserEvents;
                    }
                }

                // Create new session with previous events
                _currentSession = new SessionState
                {
                    UserId = user.Name,
                    Email = user.Email,
                    IsAuthenticated = true,
                    UserEvents = previousUserEvents
                };

                await SaveToStorageAsync();
                NotifySessionChanged();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {UserId}", user.Name);
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            // Store the current user's events before logout
            if (!string.IsNullOrEmpty(_currentSession.UserId))
            {
                var userSessionKey = $"{STORAGE_KEY}_{_currentSession.UserId}";
                var userSession = new SessionState
                {
                    UserId = _currentSession.UserId,
                    Email = _currentSession.Email,
                    UserEvents = _currentSession.UserEvents
                };
                var userSessionJson = JsonSerializer.Serialize(userSession);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", userSessionKey, userSessionJson);
            }

            // Reset current session and remove from storage
            _currentSession = new SessionState();
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", STORAGE_KEY);
                        
            NotifySessionChanged();
        }

        public bool IsAuthenticated()
        {
            return _currentSession.IsAuthenticated;
        }

        private void NotifySessionChanged()
        {
            OnSessionChanged?.Invoke();
        }

        public IEnumerable<Event> GetUserEvents()
        {
            if (!_currentSession.IsAuthenticated)
            {
                throw new InvalidOperationException("User must be authenticated to access events");
            }
            return _currentSession.UserEvents.ToList();
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            try
            {
                var eventsJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", GLOBAL_EVENTS_KEY);
                if (string.IsNullOrEmpty(eventsJson))
                    return new List<Event>();

                var events = JsonSerializer.Deserialize<List<Event>>(eventsJson) ?? new List<Event>();
                return events.OrderBy(e => e.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving global events");
                return new List<Event>();
            }
        }

        private async Task SaveEventsAsync(List<Event> events)
        {
            try
            {
                var eventsJson = JsonSerializer.Serialize(events);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", GLOBAL_EVENTS_KEY, eventsJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving global events");
                throw;
            }
        }

        public async Task AddEventAsync(Event newEvent)
        {
            if (!_currentSession.IsAuthenticated)
            {
                throw new InvalidOperationException("User must be authenticated to add events");
            }

            var events = await GetAllEventsAsync();
            newEvent.CreatorId = _currentSession.Email;
            newEvent.Id = events.Count > 0 ? events.Max(e => e.Id) + 1 : 1;
            events.Add(newEvent);
            await SaveEventsAsync(events);
            NotifySessionChanged();
        }

        public async Task UpdateEventAsync(Event eventToUpdate)
        {
            if (!_currentSession.IsAuthenticated)
            {
                throw new InvalidOperationException("User must be authenticated to update events");
            }

            var events = await GetAllEventsAsync();
            var existingEvent = events.FirstOrDefault(e => e.Id == eventToUpdate.Id);
            
            if (existingEvent == null)
            {
                throw new InvalidOperationException($"Event with ID {eventToUpdate.Id} not found");
            }

            // Preserve the creator ID when updating
            eventToUpdate.CreatorId = existingEvent.CreatorId;
            
            var index = events.IndexOf(existingEvent);
            events[index] = eventToUpdate;
            await SaveEventsAsync(events);
            NotifySessionChanged();
        }

        public async Task DeleteEventAsync(int id)
        {
            if (!_currentSession.IsAuthenticated)
            {
                throw new InvalidOperationException("User must be authenticated to delete events");
            }

            var events = await GetAllEventsAsync();
            var eventToDelete = events.FirstOrDefault(e => e.Id == id);

            if (eventToDelete == null)
            {
                throw new InvalidOperationException($"Event with ID {id} not found");
            }

            if (eventToDelete.CreatorId != _currentSession.Email)
            {
                throw new InvalidOperationException("User not authorized to delete this event");
            }

            events.Remove(eventToDelete);
            await SaveEventsAsync(events);
            NotifySessionChanged();
        }
    }
}
