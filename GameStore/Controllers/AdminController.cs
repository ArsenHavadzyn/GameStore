using GameStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.ToList();
            }

            ViewBag.UserRoles = userRoles; // Передаємо ролі у ViewBag
            return View(users);
        }

        public async Task<IActionResult> ManageRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = _roleManager.Roles.ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new ManageRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                AvailableRoles = roles,
                AssignedRoles = userRoles
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRoles(string userId, List<string>? roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Якщо roles == null, то не змінюємо ролі (інакше користувач залишиться без ролей)
            if (roles == null) roles = new List<string>();

            await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(roles)); // Видаляємо ролі, яких немає в новому списку
            await _userManager.AddToRolesAsync(user, roles.Except(currentRoles)); // Додаємо нові ролі, яких раніше не було

            return RedirectToAction("Index");
        }
    }
}
