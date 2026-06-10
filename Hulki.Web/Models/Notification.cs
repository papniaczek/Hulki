using System;

namespace Hulki.Web.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string AppUserId { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
