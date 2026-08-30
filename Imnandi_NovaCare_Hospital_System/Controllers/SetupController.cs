using Imnandi_NovaCare_Hospital_System.Data;
using Imnandi_NovaCare_Hospital_System.Models;
using Imnandi_NovaCare_Hospital_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Imnandi_NovaCare_Hospital_System.Controllers
{
    [AllowAnonymous]
    public class SetupController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public SetupController(
            UserManager<User> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> CreateAdmin()
        {
            if (await AdminAlreadyExists())
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(CreateFirstAdminViewModel model)
        {
            if (await AdminAlreadyExists())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Role = "Admin",
                    PhoneNumber = model.PhoneNumber,
                    IsActive = true,
                    IsDeleted = false
                };

                var userResult = await _userManager.CreateAsync(
                    user,
                    model.Password
                );

                if (!userResult.Succeeded)
                {
                    foreach (var error in userResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    await transaction.RollbackAsync();

                    return View(model);
                }

                var roleResult = await _userManager.AddToRoleAsync(user,"Admin");

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    await transaction.RollbackAsync();

                    return View(model);
                }

                var employee = new Employee
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    JobTitle = model.JobTitle,
                    Department = "Administration",
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    UserId = user.Id,
                    IsDeleted = false,
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.Employee.Add(employee);

                await _context.SaveChangesAsync();

                var admin = new Admin
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Department = "Administration",
                    IsAvail = true,
                    IsDeleted = false,
                    EmployeeId = employee.Id
                };

                _context.Administrator.Add(admin);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                var errorMessage = ex.ToString();

                Console.WriteLine("==============================================");
                Console.WriteLine("ERROR CREATING ADMINISTRATOR");
                Console.WriteLine("==============================================");
                Console.WriteLine(errorMessage);
                Console.WriteLine("==============================================");

                ModelState.AddModelError("",$"ERROR: {ex.Message}");

                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("",$"DATABASE ERROR: {ex.InnerException.Message}");
                }

                return View(model);
            }

            TempData["SuccessMessage"] ="Administrator account created successfully. You can now log in.";

            return Redirect("/Identity/Account/Login");
        }

        private async Task<bool> AdminAlreadyExists()
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

            return adminUsers.Any(u => !u.IsDeleted && u.IsActive);
        }
    }
}