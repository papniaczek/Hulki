using System;
using System.Collections.Generic;

namespace Hulki.Web.Models
{
    public class Survey
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public virtual ICollection<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();
        public virtual ICollection<SurveySubmission> Submissions { get; set; } = new List<SurveySubmission>();
    }
}