using System;

namespace Hulki.Web.Models
{
    public class AchievementBadge
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; } // np. "bi-trophy", "bi-star-fill"

        // Logika przyznawania
        public string ConditionType { get; set; } // np. "GoalsCompleted"
        public int ConditionValue { get; set; }   // np. 1 (za 1 cel), 5 (za 5 celów)
    }
}