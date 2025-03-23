using GameStore.Models;
using GameStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IEmailService _emailService;

    public AccountController(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             RoleManager<IdentityRole> roleManager,
                             IWebHostEnvironment hostEnvironment,
                             IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _hostEnvironment = hostEnvironment;
        _emailService = emailService;
    }

    // GET: /Account/PersonalCabinet
    public async Task<IActionResult> Index()
    {
        return RedirectToAction("PersonalCabinet", "Account");
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        return View(model);
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Невірний логін або пароль.");
        }

        return View(model);
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

                await _emailService.SendEmailAsync(model.Email, "Скидання пароля",
                    $"Щоб скинути пароль, перейдіть за посиланням: <a href='{resetLink}'>Скинути пароль</a>");

                return View("ForgotPasswordConfirmation");
            }

            return View("ForgotPasswordConfirmation");
        }

        return View(model);
    }


    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> ResetPassword()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var model = new ResetPasswordViewModel
        {
            Email = user.Email,
            Token = token
        };

        return View(model);
    }



    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (result.Succeeded)
        {
            return RedirectToAction("ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }


    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> PersonalCabinet()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var model = new ApplicationUser
        {
            FullName = user.FullName,
            Email = user.Email,
            ProfilePictureUrl = user.ProfilePictureUrl,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            BirthDate = user.BirthDate,
            Token = user.Token
        };

        return View(model);
    }


    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(ApplicationUser model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;
        user.Address = model.Address;
        user.BirthDate = model.BirthDate;

        if (!string.IsNullOrEmpty(model.Email) && user.Email != model.Email)
        {
            user.Email = model.Email;
            user.UserName = model.Email;
        }

        if (Request.Form.Files.Count > 0)
        {
            var file = Request.Form.Files[0];
            if (file.Length > 0)
            {
                var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                user.ProfilePictureUrl = $"/uploads/{fileName}";
            }
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("PersonalCabinet");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }



    public async Task<IActionResult> CreateAdmin()
    {
        var adminEmail = "admin@gamestore.com";
        var adminPassword = "Admin123!";

        var user = await _userManager.FindByEmailAsync(adminEmail);
        if (user == null)
        {
            user = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
            await _userManager.CreateAsync(user, adminPassword);
        }

        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        return Content("Адмін створений!");
    }


}
