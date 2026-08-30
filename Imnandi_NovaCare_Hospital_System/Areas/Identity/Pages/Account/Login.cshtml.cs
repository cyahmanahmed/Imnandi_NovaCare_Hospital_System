// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Imnandi_NovaCare_Hospital_System.Data;
using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Imnandi_NovaCare_Hospital_System.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _context;

        public LoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<LoginModel> logger,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }
        public bool CanCreateFirstAdmin { get; set; }
        [TempData] public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; }

            [Required, DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

            CanCreateFirstAdmin = !adminUsers.Any(u => !u.IsDeleted && u.IsActive);
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);


            var sessionId = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("AuditSessionId", sessionId);


            if (user == null || user.IsDeleted || !user.IsActive)
            {
                await LogAuditAsync(
                    username: Input.Email,
                    userId: user?.Id,
                    action: "LoginFailed",
                    details: "User not found or inactive",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    sessionId: sessionId
                );

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, Input.Password);

            if (!passwordValid)
            {
                _logger.LogWarning("Invalid login attempt for user {Email}.", user.Email);

                await LogAuditAsync(
                    username: user.UserName,
                    userId: user.Id,
                    action: "LoginFailed",
                    details: "Invalid password",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    sessionId: sessionId
                );

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
            await _signInManager.SignInAsync(user, Input.RememberMe);

            _logger.LogInformation("User logged in.");

            await LogAuditAsync(
                username: user.UserName,
                userId: user.Id,
                action: "LoginSuccess",
                details: "User logged in successfully",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                isLogin: true,
                sessionId: sessionId
            );

            return await RedirectByRole(user);
        }

        private async Task<IActionResult> RedirectByRole(User user)
        {
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("AdminDashBoard", "Admin");

            if (await _userManager.IsInRoleAsync(user, "Doctor"))
                return RedirectToAction("DoctorDashboard", "Doctor");

            if (await _userManager.IsInRoleAsync(user, "Nurse"))
                return RedirectToAction("NurseDashboard", "Nurse");

            if (await _userManager.IsInRoleAsync(user, "NurseSister"))
                return RedirectToAction("NurseSisterDashboard", "NurseSister");

            if (await _userManager.IsInRoleAsync(user, "ScriptManager"))
                return RedirectToAction("ScriptManagerDashboard", "ScriptManager");

            if (await _userManager.IsInRoleAsync(user, "StockManager"))
                return RedirectToAction("StockManagerDashboard", "StockManager");

            if (await _userManager.IsInRoleAsync(user, "WardAdmin"))
                return RedirectToAction("WardAdminDashboard", "WardAdmin");

            return RedirectToAction("Index", "Home");
        }

        private async Task LogAuditAsync(
            string username,
            string? userId = null,
            string action = "",
            string? details = null,
            string? ipAddress = null,
            bool isLogin = false,
            bool isLogout = false,
            string? sessionId = null)
        {
            AuditLog audit;

            if (isLogout && !string.IsNullOrEmpty(sessionId))
            {
                audit = await _context.AuditLogs
                    .Where(a => a.SessionId == sessionId && a.LoginDateTime != null && a.LogoutDateTime == null)
                    .OrderByDescending(a => a.LoginDateTime)
                    .FirstOrDefaultAsync();

                if (audit != null)
                {
                    audit.LogoutDateTime = DateTime.Now;
                    audit.ActionTaken = "Logout";
                    audit.Details = details;
                    audit.IpAddress = ipAddress;
                    await _context.SaveChangesAsync();
                    return;
                }
            }

            audit = new AuditLog
            {
                Username = username,
                UserId = userId,
                ActionTaken = action,
                Details = details,
                IpAddress = ipAddress,
                Timestamp = DateTime.Now,
                SessionId = sessionId ?? Guid.NewGuid().ToString(),
            };

            if (isLogin)
                audit.LoginDateTime = DateTime.Now;

            if (isLogout && audit.LogoutDateTime == null)
                audit.LogoutDateTime = DateTime.Now;

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }
    }
}