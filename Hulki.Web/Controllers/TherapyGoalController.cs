using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hulki.Web.Models;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hulki.Web.Controllers
{
    [Authorize]
    public class TherapyGoalController : Controller
    {
        private readonly ITherapyGoalService _goalService;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;

        public TherapyGoalController(
            ITherapyGoalService goalService,
            UserManager<AppUser> userManager,
            INotificationService notificationService
        )
        {
            _goalService = goalService;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var goals = await _goalService.GetUserGoalsAsync(user.Id);
            return View(goals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string title,
            DateTime deadline,
            List<string> milestones
        )
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Tytuł celu jest wymagany.";
                return RedirectToAction(nameof(Index));
            }

            var goal = new TherapyGoal
            {
                AppUserId = user.Id,
                Title = title,
                Deadline = deadline,
            };

            await _goalService.CreateGoalAsync(goal, milestones);

            await _notificationService.SendNotificationAsync(
                user.Id,
                $"Utworzono nowy cel terapeutyczny: {title}. Masz {milestones?.Count ?? 0} kamieni milowych do osiągnięcia!"
            );

            TempData["SuccessMessage"] = "Nowy cel został dodany.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleMilestone(Guid milestoneId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _goalService.ToggleMilestoneAsync(milestoneId, user.Id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _goalService.DeleteGoalAsync(id, user.Id);
            return RedirectToAction(nameof(Index));
        }
    }
}
