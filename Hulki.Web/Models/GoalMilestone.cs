using System;

namespace Hulki.Web.Models
{
    public class GoalMilestone
    {
        public Guid Id { get; set; }
        public Guid GoalId { get; set; }
        public virtual TherapyGoal Goal { get; set; }
        public string Description { get; set; }

        // TEGO BRAKOWAŁO W TWOIM KODZIE:
        public bool IsCompleted { get; set; }
    }
}