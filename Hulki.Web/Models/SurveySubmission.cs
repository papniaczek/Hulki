using System;
using System.Collections.Generic;

namespace Hulki.Web.Models
{
    public class SurveySubmission
    {
        public Guid Id { get; set; }
        public string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
        public Guid SurveyId { get; set; }
        public virtual Survey Survey { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public virtual ICollection<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();
    }
}