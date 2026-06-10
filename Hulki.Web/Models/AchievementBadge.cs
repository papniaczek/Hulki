using System;

namespace Hulki.Web.Models
{
    public class AchievementBadge
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public string ConditionType { get; set; }
        public int ConditionValue { get; set; }
    }
}
