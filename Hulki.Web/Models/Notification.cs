using System;

namespace Hulki.Web.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public string AppUserId { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }

        // TEGO BRAKOWAŁO:
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}