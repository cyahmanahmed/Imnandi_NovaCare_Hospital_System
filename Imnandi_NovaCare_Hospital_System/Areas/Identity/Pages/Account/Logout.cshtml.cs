// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Imnandi_NovaCare_Hospital_System.Data;
using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Imnandi_NovaCare_Hospital_System.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly ApplicationDbContext _context;

        public LogoutModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<LogoutModel> logger,
            ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var username = user?.UserName;

                _logger.LogInformation("User logged out.");

                var sessionId = HttpContext.Session.GetString("AuditSessionId");

                await LogAuditAsync(
                    username: username,
                    userId: user?.Id,
                    action: "Logout",
                    details: "User logged out",
                    ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                    isLogout: true,
                    sessionId: sessionId
                );

                HttpContext.Session.Remove("AuditSessionId");
                await _signInManager.SignOutAsync();
            }

            if (!string.IsNullOrEmpty(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToPage("/Home/Index");
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
            var audit = new AuditLog
            {
                Username = username,
                UserId = userId,
                ActionTaken = action,
                Details = details,
                IpAddress = ipAddress,
                Timestamp = DateTime.Now,
                SessionId = sessionId ?? Guid.NewGuid().ToString()
            };

            if (isLogin)
                audit.LoginDateTime = DateTime.Now;

            if (isLogout)
                audit.LogoutDateTime = DateTime.Now;

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }
    }
}
