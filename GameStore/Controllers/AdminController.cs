﻿using GameStore.Data;
using GameStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
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

            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        public async Task<IActionResult> ManageRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = _roleManager.Roles.ToList();
            var userRoles = await _userManager.GetRolesAsync(user);

            var userOrders = _context.Orders
                .Where(o => o.UserId == user.Id)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToList();

            var model = new ManageRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                RegistrationDate = user.RegistrationDate,
                AvailableRoles = roles,
                AssignedRoles = userRoles,

                Purchases = userOrders.Select(o => new UserPurchaseViewModel
                {
                    Date = o.OrderDate,
                    Items = o.OrderItems.Select(oi => oi.Product.Title).ToList(),
                    TotalAmount = o.TotalPrice
                }).ToList()
            };


            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRoles(string userId, List<string>? roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (roles == null) roles = new List<string>();

            await _userManager.RemoveFromRolesAsync(user, currentRoles.Except(roles));
            await _userManager.AddToRolesAsync(user, roles.Except(currentRoles));

            return RedirectToAction("Index");
        }
    }
}
