using GameStore.Models;
using GameStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IEmailService _emailService;

        public FeedbackController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(FeedbackViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            string siteEmail = "officegamestore@gmail.com"; 
            string subject = $"Новий відгук: {model.Subject}";
            string body = $@"
            <h2>Новий відгук</h2>
            <p><strong>Ім'я:</strong> {model.Name}</p>
            <p><strong>Email:</strong> {model.Email}</p>
            <p><strong>Повідомлення:</strong></p>
            <p>{model.Message}</p>";

            await _emailService.SendEmailAsync(siteEmail, subject, body);

            TempData["SuccessMessage"] = "Ваш відгук успішно надіслано!";
            return RedirectToAction("Feedback", "Home");
        }
    }
}
