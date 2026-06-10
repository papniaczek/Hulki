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

        // Lista ankiet (widok wspólny)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var surveys = await _surveyService.GetAllSurveysAsync();

            // POPRAWKA: Sprawdzamy role bezpośrednio z Identity
            ViewBag.IsTherapist = User.IsInRole("Terapeuta") || User.IsInRole("Admin");
            ViewBag.UserId = user.Id;

            return View(surveys);
        }

        // TERAOPEUTA: Formularz tworzenia nowej ankiety
        [HttpGet]
        [Authorize(Roles = "Terapeuta, Admin")] // POPRAWKA: Blokada na poziomie ról
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Terapeuta, Admin")]
        public async Task<IActionResult> Create(string title, List<string> questions)
        {
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
        [Authorize(Roles = "Terapeuta, Admin")] // POPRAWKA: Tylko personel może wejść
        public async Task<IActionResult> Results(Guid id)
        {
            var survey = await _surveyService.GetSurveyByIdAsync(id);
            if (survey == null) return NotFound();

            var submissions = await _surveyService.GetSurveyResultsAsync(id);

            ViewBag.Survey = survey;
            return View(submissions);
        }
    }
}