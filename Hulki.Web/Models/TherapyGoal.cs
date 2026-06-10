using System;
using System.Collections.Generic;

namespace Hulki.Web.Models
{
    public class TherapyGoal
    {
        public Guid Id { get; set; }
        public string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsCompleted { get; set; }

        public virtual ICollection<GoalMilestone> Milestones { get; set; } = new List<GoalMilestone>();
    }
}