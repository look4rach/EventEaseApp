using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EventEaseApp;

namespace EventEaseApp
{
    public interface IStateManager
    {
        /// <summary>
        /// Gets the current session state
        /// </summary>
        SessionState CurrentSession { get; }

        /// <summary>
        /// Event that is triggered when the session state changes
        /// </summary>
        event Action? OnSessionChanged;

        /// <summary>
        /// Initializes the state manager and loads any existing session
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Logs in a user and initializes their session
        /// </summary>
        /// <param name="user">The user to log in</param>
        Task LoginAsync(User user);

        /// <summary>
        /// Logs out the current user and preserves their data
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Gets all events from all users
        /// </summary>
        /// <returns>List of all events</returns>
        Task<List<Event>> GetAllEventsAsync();

        /// <summary>
        /// Adds a new event for the current user
        /// </summary>
        /// <param name="newEvent">The event to add</param>
        Task AddEventAsync(Event newEvent);

        /// <summary>
        /// Updates an existing event
        /// </summary>
        /// <param name="eventToUpdate">The event with updated information</param>
        Task UpdateEventAsync(Event eventToUpdate);

        /// <summary>
        /// Deletes an event for the current user
        /// </summary>
        /// <param name="eventId">The ID of the event to delete</param>
        Task DeleteEventAsync(int eventId);

        /// <summary>
        /// Gets all events for the current user
        /// </summary>
        IEnumerable<Event> GetUserEvents();

        /// <summary>
        /// Checks if a user is currently authenticated
        /// </summary>
        bool IsAuthenticated();
    }
}
