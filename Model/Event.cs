using System.ComponentModel.DataAnnotations;

namespace EventEaseApp
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public DateTime Date { get; set; }

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;        public string CreatorId { get; set; } = string.Empty;  // Set automatically by StateManager

        public int MaxAttendees { get; set; }
        
        public string Description { get; set; } = string.Empty;

        public List<Attendance> Attendees { get; set; } = new List<Attendance>();

        public bool HasAvailableSpots => MaxAttendees == 0 || Attendees.Count < MaxAttendees;

        public int RegisteredCount => Attendees.Count(a => a.Status == AttendanceStatus.Registered || a.Status == AttendanceStatus.Attended);

        public int AttendedCount => Attendees.Count(a => a.Status == AttendanceStatus.Attended);

        public bool IsUserRegistered(string userId) => 
            Attendees.Any(a => a.UserId == userId && 
                             (a.Status == AttendanceStatus.Registered || a.Status == AttendanceStatus.Attended));

        public void RegisterAttendee(string userId, string userName)
        {
            if (!HasAvailableSpots)
                throw new InvalidOperationException("Event is full");

            // First check if user has any previous registration
            var existingAttendance = Attendees.FirstOrDefault(a => a.UserId == userId);
            
            if (existingAttendance != null)
            {
                if (existingAttendance.Status == AttendanceStatus.Registered || 
                    existingAttendance.Status == AttendanceStatus.Attended)
                {
                    throw new InvalidOperationException("User is already registered");
                }
                
                // If previously cancelled, update the status
                existingAttendance.Status = AttendanceStatus.Registered;
                existingAttendance.RegisteredAt = DateTime.UtcNow;
                return;
            }

            // If no previous registration, create new attendance
            var attendance = new Attendance
            {
                UserId = userId,
                UserName = userName,
                RegisteredAt = DateTime.UtcNow,
                Status = AttendanceStatus.Registered
            };

            Attendees.Add(attendance);
        }

        public void UpdateAttendanceStatus(string userId, AttendanceStatus newStatus)
        {
            var attendance = Attendees.FirstOrDefault(a => a.UserId == userId);
            if (attendance == null)
                throw new InvalidOperationException("User is not registered for this event");

            attendance.Status = newStatus;
        }

        public void CancelAttendance(string userId)
        {
            var attendance = Attendees.FirstOrDefault(a => a.UserId == userId);
            if (attendance == null)
                throw new InvalidOperationException("User is not registered for this event");

            attendance.Status = AttendanceStatus.Cancelled;
        }
    }
}