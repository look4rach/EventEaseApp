using System.Text.Json.Serialization;

namespace EventEaseApp
{
    public class SessionState
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAuthenticated { get; set; }
        public List<Event> UserEvents { get; set; } = new List<Event>();

        public void AddEvent(Event newEvent)
        {
            UserEvents.Add(newEvent);
        }

        public void RemoveEvent(int eventId)
        {
            var eventToRemove = UserEvents.FirstOrDefault(e => e.Id == eventId);
            if (eventToRemove != null)
            {
                UserEvents.Remove(eventToRemove);
            }
        }

        public void UpdateEvent(Event updatedEvent)
        {
            var index = UserEvents.FindIndex(e => e.Id == updatedEvent.Id);
            if (index != -1)
            {
                UserEvents[index] = updatedEvent;
            }
        }
    }
}
