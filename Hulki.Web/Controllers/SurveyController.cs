using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hulki.Web.Models;
using Hulki.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hulki.Web.Controllers
{
    [Authorize]
    public class SurveyController : Controller
    {
        private readonly ISurveyService _surveyService;
        private readonly UserManager<AppUser> _userManager;

        public SurveyController(ISurveyService surveyService, UserManager<AppUser> userManager)
        {
            _surveyService = surveyService;
            _userManager = userManager;
        }

        // Lista ankiet (widok wspólny, ale pacjent widzi "Wypełnij", a terapeuta "Wyniki")
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var surveys = await _surveyService.GetAllSurveysAsync();
            ViewBag.IsTherapist = user.IsTherapist;
            ViewBag.UserId = user.Id;

            return View(surveys);
        }

        // TERAOPEUTA: Formularz tworzenia nowej ankiety
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsTherapist) return Forbid();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, List<string> questions)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsTherapist) return Forbid();

            if (string.IsNullOrWhiteSpace(title) || !questions.Any(q => !string.IsNullOrWhiteSpace(q)))
            {
                ModelState.AddModelError("", "Tytuł i przynajmniej jedno pytanie są wymagane.");
                return View();
            }

            await _surveyService.CreateSurveyAsync(title, questions);
            TempData["SuccessMessage"] = "Ankieta została pomyślnie utworzona.";
            return RedirectToAction(nameof(Index));
        }

        // PACJENT: Formularz wypełniania ankiety
        [HttpGet]
        public async Task<IActionResult> Fill(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (await _surveyService.HasUserSubmittedAsync(id, user.Id))
            {
                TempData["ErrorMessage"] = "Wypełniłeś już tę ankietę.";
                return RedirectToAction(nameof(Index));
            }

            var survey = await _surveyService.GetSurveyByIdAsync(id);
            if (survey == null) return NotFound();

            return View(survey);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Guid surveyId, Dictionary<Guid, string> answers)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (await _surveyService.HasUserSubmittedAsync(surveyId, user.Id))
                return BadRequest("Zdublowane żądanie.");

            await _surveyService.SubmitSurveyAsync(surveyId, user.Id, answers);
            TempData["SuccessMessage"] = "Twoje odpowiedzi zostały zapisane. Dziękujemy!";
            return RedirectToAction(nameof(Index));
        }

        // TERAPEUTA: Podgląd wyników ankiety
        [HttpGet]
        public async Task<IActionResult> Results(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !user.IsTherapist) return Forbid();

            var survey = await _surveyService.GetSurveyByIdAsync(id);
            if (survey == null) return NotFound();

            var submissions = await _surveyService.GetSurveyResultsAsync(id);

            ViewBag.Survey = survey;
            return View(submissions);
        }
    }
}