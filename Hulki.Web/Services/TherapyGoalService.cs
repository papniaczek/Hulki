using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Services
{
    public interface ITherapyGoalService
    {
        Task<List<TherapyGoal>> GetUserGoalsAsync(string userId);
        Task CreateGoalAsync(TherapyGoal goal, List<string> milestones);
        Task ToggleMilestoneAsync(Guid milestoneId, string userId);
        Task DeleteGoalAsync(Guid goalId, string userId);
    }

    public class TherapyGoalService : ITherapyGoalService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBadgeService _badgeService;
        private readonly INotificationService _notificationService;

        public TherapyGoalService(
            ApplicationDbContext context, 
            IBadgeService badgeService,
            INotificationService notificationService)
        {
            _context = context;
            _badgeService = badgeService;
            _notificationService = notificationService;
        }

        public async Task<List<TherapyGoal>> GetUserGoalsAsync(string userId)
        {
            return await _context.TherapyGoals
                .Include(g => g.Milestones)
                .Where(g => g.AppUserId == userId)
                .OrderBy(g => g.IsCompleted) // Niezakończone na górze
                .ThenBy(g => g.Deadline)
                .ToListAsync();
        }

        public async Task CreateGoalAsync(TherapyGoal goal, List<string> milestones)
        {
            goal.Id = Guid.NewGuid();
            goal.IsCompleted = false;

            foreach (var m in milestones.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                goal.Milestones.Add(new GoalMilestone
                {
                    Id = Guid.NewGuid(),
                    GoalId = goal.Id,
                    Description = m,
                    IsCompleted = false
                });
            }

            _context.TherapyGoals.Add(goal);
            await _context.SaveChangesAsync();
        }

        public async Task ToggleMilestoneAsync(Guid milestoneId, string userId)
        {
            var milestone = await _context.GoalMilestones
                .Include(m => m.Goal)
                .FirstOrDefaultAsync(m => m.Id == milestoneId);

            if (milestone != null && milestone.Goal.AppUserId == userId)
            {
                milestone.IsCompleted = !milestone.IsCompleted;

                var allMilestones = await _context.GoalMilestones.Where(m => m.GoalId == milestone.GoalId).ToListAsync();
                milestone.Goal.IsCompleted = allMilestones.Any() && allMilestones.All(m => m.IsCompleted);

                // POWIADOMIENIE gdy cały cel zostaje ukończony
                if (milestone.Goal.IsCompleted)
                {
                    await _notificationService.SendNotificationAsync(
                        userId,
                        $"🎉 Wspaniała robota! Ukończyłeś cel terapeutyczny: {milestone.Goal.Title}"
                    );
                }

                await _context.SaveChangesAsync();
                await _badgeService.CheckAndAwardBadgesAsync(userId);
            }
        }

        public async Task DeleteGoalAsync(Guid goalId, string userId)
        {
            var goal = await _context.TherapyGoals.FirstOrDefaultAsync(g => g.Id == goalId && g.AppUserId == userId);
            if (goal != null)
            {
                _context.TherapyGoals.Remove(goal);
                await _context.SaveChangesAsync();
            }
        }
    }
}