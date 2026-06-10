using System;

namespace Hulki.Web.Models
{
    public class UserBadge
    {
        public Guid Id { get; set; }
        public string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }

        public Guid BadgeId { get; set; }
        public virtual AchievementBadge Badge { get; set; }

        public DateTime EarnedAt { get; set; } = DateTime.Now;
    }
}
