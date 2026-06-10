using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models
{
    public class SurveyAnswer
    {
        public Guid Id { get; set; }

        public Guid SubmissionId { get; set; }

        [ForeignKey("SubmissionId")]
        public virtual SurveySubmission Submission { get; set; }

        public Guid QuestionId { get; set; }
        public virtual SurveyQuestion Question { get; set; }

        public string AnswerText { get; set; }
    }
}
