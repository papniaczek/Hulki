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
            return await _context.UserBadges
                .Include(ub => ub.Badge)
                .Where(ub => ub.AppUserId == userId)
                .OrderByDescending(ub => ub.EarnedAt)
                .ToListAsync();
        }

        public async Task CheckAndAwardBadgesAsync(string userId)
        {
            // 1. Pobierz statystyki użytkownika (np. ile celów ukończył)
            int completedGoals = await _context.TherapyGoals
                .CountAsync(g => g.AppUserId == userId && g.IsCompleted);

            // 2. Pobierz odznaki, których użytkownik JESZCZE NIE MA
            var earnedBadgeIds = await _context.UserBadges
                .Where(ub => ub.AppUserId == userId)
                .Select(ub => ub.BadgeId)
                .ToListAsync();

            var availableBadges = await _context.AchievementBadges
                .Where(b => !earnedBadgeIds.Contains(b.Id))
                .ToListAsync();

            bool anyAwarded = false;

            // 3. Weryfikacja warunków
            foreach (var badge in availableBadges)
            {
                bool conditionsMet = false;

                if (badge.ConditionType == "GoalsCompleted" && completedGoals >= badge.ConditionValue)
                {
                    conditionsMet = true;
                }

                if (conditionsMet)
                {
                    _context.UserBadges.Add(new UserBadge
                    {
                        Id = Guid.NewGuid(),
                        AppUserId = userId,
                        BadgeId = badge.Id,
                        EarnedAt = DateTime.Now
                    });

                    await _notificationService.SendNotificationAsync(userId,
                        $"Odblokowano nową odznakę: {badge.Name}! Sprawdź swój profil.");

                    anyAwarded = true;
                }
            }

            if (anyAwarded)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}