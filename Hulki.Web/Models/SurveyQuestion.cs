using System;

namespace Hulki.Web.Models
{
    public class SurveyQuestion
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public Guid SurveyId { get; set; }
        public virtual Survey Survey { get; set; }
    }
}