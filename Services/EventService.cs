using EventEaseApp;

using Microsoft.Extensions.Logging;

namespace EventEaseApp
{
    public class EventService : IEventService
    {
        private readonly IStateManager _stateManager;
        private readonly ILogger<EventService> _logger;
        private int _nextEventId = 1;

        public EventService(IStateManager stateManager, ILogger<EventService> logger)
        {
            _stateManager = stateManager;
            _logger = logger;
        }        public async Task<List<Event>> GetEventsAsync()
        {
            try
            {
                var events = await _stateManager.GetAllEventsAsync();
                _logger.LogInformation("Retrieved {Count} events", events.Count);
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events");
                return new List<Event>();
            }
        }        public async Task<(List<Event> Events, int TotalCount)> GetPagedEventsAsync(int pageNumber, int pageSize)
        {
            try
            {
                var allEvents = await _stateManager.GetAllEventsAsync();
                var totalCount = allEvents.Count;
                
                var pagedEvents = allEvents
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                _logger.LogInformation("Retrieved page {Page} of events ({Count} items)", pageNumber, pagedEvents.Count);
                return (pagedEvents, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged events");
                return (new List<Event>(), 0);
            }
        }

        public async Task<Event> AddEventAsync(Event newEvent)
        {
            try
            {
                newEvent.Id = _nextEventId++;
                await _stateManager.AddEventAsync(newEvent);
                _logger.LogInformation("Added new event: {EventId}", newEvent.Id);
                return newEvent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding event");
                throw;
            }
        }

        public async Task DeleteEventAsync(int id)
        {
            try
            {
                await _stateManager.DeleteEventAsync(id);
                _logger.LogInformation("Deleted event: {EventId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event {EventId}", id);
                throw;
            }
        }        public async Task<Event> UpdateEventAsync(Event eventItem)
        {
            try
            {
                await _stateManager.UpdateEventAsync(eventItem);
                _logger.LogInformation("Updated event: {EventId}", eventItem.Id);
                return eventItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event: {EventId}", eventItem.Id);
                throw;
            }
        }
    }
}