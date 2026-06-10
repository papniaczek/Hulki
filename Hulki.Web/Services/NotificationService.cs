using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hulki.Web.Models;
using Hulki.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Services
{
    public interface INotificationService
    {
        Task<int> GetUnreadCountAsync(string userId);
        Task<List<Notification>> GetLatestNotificationsAsync(string userId, int count = 5);
        Task MarkAsReadAsync(Guid id);
        Task SendNotificationAsync(string userId, string content);
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications.CountAsync(n => n.AppUserId == userId && !n.IsRead);
        }

        public async Task<List<Notification>> GetLatestNotificationsAsync(string userId, int count = 5)
        {
            return await _context.Notifications
                .Where(n => n.AppUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SendNotificationAsync(string userId, string content)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                AppUserId = userId,
                Content = content,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}