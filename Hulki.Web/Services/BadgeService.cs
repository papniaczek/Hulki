using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hulki.Web.Data;
using Hulki.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Services
{
    public interface IBadgeService
    {
        Task<List<UserBadge>> GetUserBadgesAsync(string userId);
        Task CheckAndAwardBadgesAsync(string userId);
    }

    public class BadgeService : IBadgeService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public BadgeService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<UserBadge>> GetUserBadgesAsync(string userId)
        {
            return await _context
                .UserBadges.Include(ub => ub.Badge)
                .Where(ub => ub.AppUserId == userId)
                .OrderByDescending(ub => ub.EarnedAt)
                .ToListAsync();
        }

        public async Task CheckAndAwardBadgesAsync(string userId)
        {
            int completedGoals = await _context.TherapyGoals.CountAsync(g =>
                g.AppUserId == userId && g.IsCompleted
            );

            int reportsCreated = await _context.DailyReports.CountAsync(r => r.AppUserId == userId);

            int consultationsCompleted = await _context.Consultations.CountAsync(c =>
                (c.PatientId == userId || c.TherapistId == userId) && c.Status.Name == "Zakończona"
            );

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.AppUserId == userId);
            int pointsEarned = wallet?.Balance ?? 0;

            int forumPosts = await _context.ForumPosts.CountAsync(p => p.AppUserId == userId);

            var earnedBadgeIds = await _context
                .UserBadges.Where(ub => ub.AppUserId == userId)
                .Select(ub => ub.BadgeId)
                .ToListAsync();

            var availableBadges = await _context
                .AchievementBadges.Where(b => !earnedBadgeIds.Contains(b.Id))
                .ToListAsync();

            bool anyAwarded = false;

            foreach (var badge in availableBadges)
            {
                bool conditionsMet = false;

                switch (badge.ConditionType)
                {
                    case "GoalsCompleted":
                        conditionsMet = completedGoals >= badge.ConditionValue;
                        break;
                    case "ReportsCreated":
                        conditionsMet = reportsCreated >= badge.ConditionValue;
                        break;
                    case "ConsultationsCompleted":
                        conditionsMet = consultationsCompleted >= badge.ConditionValue;
                        break;
                    case "PointsEarned":
                        conditionsMet = pointsEarned >= badge.ConditionValue;
                        break;
                    case "ForumPosts":
                        conditionsMet = forumPosts >= badge.ConditionValue;
                        break;
                    case "DaysStreak":

                        var reportDates = await _context
                            .DailyReports.Where(r => r.AppUserId == userId)
                            .Select(r => r.CreatedAt.Date)
                            .Distinct()
                            .OrderByDescending(d => d)
                            .ToListAsync();

                        int streak = CalculateDayStreak(reportDates);
                        conditionsMet = streak >= badge.ConditionValue;
                        break;
                }

                if (conditionsMet)
                {
                    _context.UserBadges.Add(
                        new UserBadge
                        {
                            Id = Guid.NewGuid(),
                            AppUserId = userId,
                            BadgeId = badge.Id,
                            EarnedAt = DateTime.Now,
                        }
                    );

                    await _notificationService.SendNotificationAsync(
                        userId,
                        $"🏆 Odblokowano nową odznakę: {badge.Name}! {badge.Description}"
                    );

                    anyAwarded = true;
                }
            }

            if (anyAwarded)
            {
                await _context.SaveChangesAsync();
            }
        }

        private int CalculateDayStreak(List<DateTime> orderedDates)
        {
            if (orderedDates.Count == 0)
                return 0;

            int streak = 1;
            for (int i = 0; i < orderedDates.Count - 1; i++)
            {
                var diff = (orderedDates[i] - orderedDates[i + 1]).Days;
                if (diff == 1)
                {
                    streak++;
                }
                else
                {
                    break;
                }
            }
            return streak;
        }
    }
}
