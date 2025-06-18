using System;

namespace EventEaseApp
{
    public class Attendance
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Registered;
    }

    public enum AttendanceStatus
    {
        Registered,
        Attended,
        Cancelled,
        NoShow
    }
}
