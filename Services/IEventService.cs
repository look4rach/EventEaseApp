namespace EventEaseApp
{
    public interface IEventService
    {
        Task<List<Event>> GetEventsAsync();
        Task<(List<Event> Events, int TotalCount)> GetPagedEventsAsync(int pageNumber, int pageSize);
        Task<Event> AddEventAsync(Event eventItem);
        Task DeleteEventAsync(int id);
        Task<Event> UpdateEventAsync(Event eventItem);
    }
}