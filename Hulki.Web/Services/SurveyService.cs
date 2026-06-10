using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Services
{
    public interface ISurveyService
    {
        Task<List<Survey>> GetAllSurveysAsync();
        Task<Survey> GetSurveyByIdAsync(Guid id);
        Task CreateSurveyAsync(string title, List<string> questions);
        Task SubmitSurveyAsync(Guid surveyId, string userId, Dictionary<Guid, string> answers);
        Task<List<SurveySubmission>> GetSurveyResultsAsync(Guid surveyId);
        Task<bool> HasUserSubmittedAsync(Guid surveyId, string userId);
    }

    public class SurveyService : ISurveyService
    {
        private readonly ApplicationDbContext _context;

        public SurveyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Survey>> GetAllSurveysAsync()
        {
            return await _context
                .Surveys.Include(s => s.Questions)
                .Include(s => s.Submissions)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
        }

        public async Task<Survey> GetSurveyByIdAsync(Guid id)
        {
            return await _context
                .Surveys.Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task CreateSurveyAsync(string title, List<string> questions)
        {
            var survey = new Survey { Id = Guid.NewGuid(), Title = title };

            foreach (var q in questions.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                survey.Questions.Add(
                    new SurveyQuestion
                    {
                        Id = Guid.NewGuid(),
                        SurveyId = survey.Id,
                        Text = q,
                    }
                );
            }

            _context.Surveys.Add(survey);
            await _context.SaveChangesAsync();
        }

        public async Task SubmitSurveyAsync(
            Guid surveyId,
            string userId,
            Dictionary<Guid, string> answers
        )
        {
            var submission = new SurveySubmission
            {
                Id = Guid.NewGuid(),
                SurveyId = surveyId,
                AppUserId = userId,
                SubmittedAt = DateTime.Now,
            };

            foreach (var answer in answers.Where(a => !string.IsNullOrWhiteSpace(a.Value)))
            {
                submission.Answers.Add(
                    new SurveyAnswer
                    {
                        Id = Guid.NewGuid(),
                        SubmissionId = submission.Id,
                        QuestionId = answer.Key,
                        AnswerText = answer.Value,
                    }
                );
            }

            _context.SurveySubmissions.Add(submission);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SurveySubmission>> GetSurveyResultsAsync(Guid surveyId)
        {
            return await _context
                .SurveySubmissions.Include(s => s.AppUser)
                .Include(s => s.Answers)
                    .ThenInclude(a => a.Question)
                .Where(s => s.SurveyId == surveyId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }

        public async Task<bool> HasUserSubmittedAsync(Guid surveyId, string userId)
        {
            return await _context.SurveySubmissions.AnyAsync(s =>
                s.SurveyId == surveyId && s.AppUserId == userId
            );
        }
    }
}
