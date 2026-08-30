using Imnandi_NovaCare_Hospital_System.Data;
using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Imnandi_NovaCare_Hospital_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> AdminDashBoard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var employee = await _context.Employee.FirstOrDefaultAsync(e => e.UserId == currentUser.Id);
            var totalPatients = await _context.Patient.CountAsync(p => !p.IsDeleted);
            var patientsInHospital = await _context.Patient.CountAsync(p => !p.IsDeleted && !p.IsDischarged);
            var totalDoctors = await _context.Doctor.CountAsync(d => !d.IsDeleted);
            var totalNurseSisters = await _context.NurseSister.CountAsync(ns => !ns.IsDeleted);
            var totalNurses = await _context.Nurse.CountAsync(n => !n.IsDeleted);
            var totalEmployees = await _context.Employee.CountAsync(e => !e.IsDeleted);
            var totalWards = await _context.Ward.CountAsync();
            var totalRooms = await _context.Room.CountAsync();
            var bedsAvailable = await _context.Bed.CountAsync(b => !b.IsOccupied);

            var totalStockManagers = await _context.Employee
                .Include(e => e.User)
                .CountAsync(e => !e.IsDeleted && e.User != null && e.User.Role.ToLower() == "stockmanager");

            var totalScriptManagers = await _context.Employee
                .Include(e => e.User)
                .CountAsync(e => !e.IsDeleted && e.User != null && e.User.Role.ToLower() == "scriptmanager");

            var totalWardAdmins = await _context.Employee
                .Include(e => e.User)
                .CountAsync(e => !e.IsDeleted && e.User != null && e.User.Role.ToLower() == "wardadmin");

            var recentAdmissions = await _context.AdmissionFolder
                .Include(a => a.Patient)
                .Include(a => a.Bed)
                    .ThenInclude(b => b.Room)
                        .ThenInclude(r => r.Ward)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.DateCreated)
                .Take(5)
                .Select(a => new
                {
                    FullName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    DateAdmitted = a.DateCreated,
                    WardName = a.Bed != null ? a.Bed.Room.Ward.WardName : "N/A",
                    RoomNumber = a.Bed != null ? a.Bed.Room.RoomNumber : "N/A",
                    BedNumber = a.Bed != null ? a.Bed.BedNumber : "N/A",
                    ReasonForAdmission = a.ReasonForAdmission
                })
                .ToListAsync();

            var recentAlerts = await _context.Alerts
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var model = new AdminDashboardViewModel
            {
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                UserId = currentUser.Id,
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                PhoneNumber = employee?.PhoneNumber,
                Department = employee?.Department,
                JobTitle = employee?.JobTitle,

                TotalPatients = totalPatients,
                PatientsInHospital = patientsInHospital,
                TotalDoctors = totalDoctors,
                TotalNurseSisters = totalNurseSisters,
                TotalNurses = totalNurses,
                TotalEmployees = totalEmployees,
                TotalWards = totalWards,
                TotalRooms = totalRooms,
                BedsAvailable = bedsAvailable,
                TotalStockManagers = totalStockManagers,
                TotalScriptManagers = totalScriptManagers,
                TotalWardAdmins = totalWardAdmins,
                RecentAlerts = recentAlerts
            };

            return View(model);
        }






        private async Task LogAuditAsync(
                string actionTaken,
                User? user = null,
                string? entity = null,
                string? recordId = null,
                string? oldValue = null,
                string? newValue = null,
                string? failureReason = null,
                string? details = null)
        {
            var audit = new AuditLog
            {
                ActionTaken = actionTaken,
                User = user,
                UserId = user?.Id,
                Username = user?.UserName ?? User.Identity?.Name ?? "Unknown",
                Timestamp = DateTime.Now,
                Entity = entity,
                RecordId = recordId,
                OldValue = oldValue,
                NewValue = newValue,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = details,
                SessionId = HttpContext.Session.Id,
                FailureReason = failureReason
            };

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }





        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");
            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == user.Id);
            var model = new AdminDashboardViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.Employee.PhoneNumber,
                Department = user.Employee.Department,
                JobTitle = user.Employee.JobTitle,
                UserName = user.UserName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(AdminDashboardViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var oldUserData = new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber
            };

            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (employee != null)
            {
                employee.FirstName = model.FirstName;
                employee.LastName = model.LastName;
                employee.PhoneNumber = model.PhoneNumber;
                employee.Department = model.Department;
                employee.JobTitle = model.JobTitle;

                _context.Employee.Update(employee);
            }

            var admin = await _context.Administrator
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id);

            if (admin != null)
            {
                admin.FirstName = model.FirstName;
                admin.LastName = model.LastName;

                _context.Administrator.Update(admin);
            }

            await _context.SaveChangesAsync();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

                var newUserData = new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber
                };

                await LogAuditAsync(
                    actionTaken: "Profile Updated",
                    user: user,
                    entity: "User",
                    recordId: user.Id.ToString(),
                    oldValue: System.Text.Json.JsonSerializer.Serialize(oldUserData),
                    newValue: System.Text.Json.JsonSerializer.Serialize(newUserData),
                    details: "User updated their profile information."
                );

                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);

                await LogAuditAsync(
                    actionTaken: "Profile Update Failed",
                    user: user,
                    entity: "User",
                    recordId: user.Id.ToString(),
                    failureReason: error.Description,
                    details: "User attempted to update profile but failed."
                );
            }

            return View(model);
        }










        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); 
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user); 
                TempData["Success"] = "Password changed successfully.";

                await LogAuditAsync(
                    actionTaken: "Changed Password",
                    user: user,
                    entity: "User",
                    recordId: user.Id.ToString(),
                    details: "User changed their own password successfully."
                );

                return RedirectToAction("AdminDashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);

                await LogAuditAsync(
                    actionTaken: "Change Password Failed",
                    user: user,
                    entity: "User",
                    recordId: user.Id.ToString(),
                    failureReason: error.Description,
                    details: "User attempted to change password but failed."
                );
            }

            return View(model);
        }





        [HttpGet]
        public async Task<IActionResult> ResetUserPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new ResetUserPasswordViewModel
            {
                UserId = user.Id,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("ResetUserPassword")]
        public async Task<IActionResult> ResetUserPasswordPost(ResetUserPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(model);
            }
            var adminUser = await _userManager.GetUserAsync(User);
            if (adminUser == null)
                return RedirectToAction("Login", "Account");

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            var addResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (addResult.Succeeded)
            {
                var request = await _context.PeopleForgotPassword
                    .Where(r => r.UserId == model.UserId && !r.IsHandled)
                    .OrderByDescending(r => r.RequestedAt)
                    .FirstOrDefaultAsync();

                if (request != null)
                {
                    request.IsHandled = true;
                    await _context.SaveChangesAsync();
                }

                await LogAuditAsync(
                    actionTaken: "ResetUserPassword",
                    user: adminUser,
                    entity: "User",
                    recordId: null,
                    oldValue: null,
                    newValue: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        AdminName = $"{adminUser.FirstName} {adminUser.LastName}",
                        UserName = $"{user.FirstName} {user.LastName}",
                        UserEmail = user.Email
                    }),
                    details: $"Admin {adminUser.FirstName} {adminUser.LastName} reset the password for user {user.FirstName} {user.LastName} ({user.Email})."
                );
                ViewBag.SuccessMessage = $"Password for {user.Email} was successfully reset.";
                return RedirectToAction("ManageEmployees");
            }

            foreach (var error in addResult.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }











        public async Task<IActionResult> ManageEmployees(string? searchString)
        {
            var query = _context.Employee
                .Where(emp => !emp.IsDeleted)
                .Include(e => e.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                query = query.Where(e =>
                    e.FirstName.Contains(searchString) ||
                    e.LastName.Contains(searchString) ||
                    (e.FirstName + " " + e.LastName).Contains(searchString) ||
                    e.JobTitle.Contains(searchString) ||
                    e.Department.Contains(searchString) ||
                    e.Email.Contains(searchString) ||
                    e.PhoneNumber.Contains(searchString) ||
                    e.User.Role.Contains(searchString));
            }

            var employees = await query.ToListAsync();

            return View(employees);
        }

        [HttpGet]
        public async Task<IActionResult> PasswordResetRequests(string searchString)
        {
            var requests = _context.PeopleForgotPassword.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();

                requests = requests.Where(r =>
                    r.FullName.Contains(searchString) ||
                    r.Email.Contains(searchString));
            }

            var model = await requests
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            ViewBag.SearchString = searchString;

            return View(model);
        }

        public IActionResult CreateEmployee()
        {
            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
            ViewBag.Wards = new SelectList(_context.Ward, "WardId", "WardName"); 
            var model = new CreateEmployeeViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                ViewBag.Wards = new SelectList(_context.Ward, "WardId", "Name");
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already registered in the system.");
                ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                ViewBag.Wards = new SelectList(_context.Ward, "WardId", "WardName");
                return View(model);
            }

            var existingUsername = await _userManager.FindByNameAsync(model.UserName);
            if (existingUsername != null)
            {
                ModelState.AddModelError("UserName", "This username is already taken.");
                ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                ViewBag.Wards = new SelectList(_context.Ward, "WardId", "WardName");
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    IsActive = true,
                    IsDeleted = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                    ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                    ViewBag.Wards = new SelectList(_context.Ward, "WardId", "Name");
                    return View(model);
                }

                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    ModelState.AddModelError("", $"Role {model.Role} does not exist.");
                    return View(model);
                }

                var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!roleResult.Succeeded)
                {
                    ModelState.AddModelError("", $"Role assignment failed for {model.Role}");
                    return View(model);
                }

                var employee = new Employee
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    JobTitle = model.JobTitle,
                    Department = model.Department,
                    PhoneNumber = model.PhoneNumber,
                    EmergencyContact = model.EmergencyContact,
                    Email = model.Email,
                    UserId = user.Id,
                    TaxNumber = model.TaxNumber,
                    BankName = model.BankName,
                    BankAccountNumber = model.BankAccountNumber,
                    IsDeleted = false
                };

                _context.Employee.Add(employee);
                await _context.SaveChangesAsync();

                var creatorName = currentUser != null? $"{currentUser.FirstName} {currentUser.LastName}": "System";
               
                switch (model.Role)
                {
                    case "Admin":
                        var admin = new Admin
                        {
                            EmployeeId = employee.Id,
                            FirstName = employee.FirstName,
                            LastName = employee.LastName
                        };
                        _context.Administrator.Add(admin);

                        var serializedAdmin = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            admin.EmployeeId,
                            admin.FirstName,
                            admin.LastName
                        });

                        await LogAuditAsync(
                            actionTaken: "Admin Record Created",
                            user: await _userManager.GetUserAsync(User),
                            entity: "Admin",
                            recordId: $"{admin.FirstName} {admin.LastName}",
                            newValue: serializedAdmin,
                            details: $"Employee {employee.FirstName} {employee.LastName} created with role Admin"
                        );
                        break;

                    case "Doctor":
                        var doctor = new Doctor
                        {
                            EmployeeId = employee.Id,
                            FirstName = employee.FirstName,
                            LastName = employee.LastName,
                            Department = employee.Department,
                            WardId = model.WardId
                        };
                        _context.Doctor.Add(doctor);

                        var serializedDoctor = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            doctor.EmployeeId,
                            doctor.FirstName,
                            doctor.LastName,
                            doctor.Department,
                            doctor.WardId
                        });

                        await LogAuditAsync(
                            actionTaken: "Doctor Record Created",
                            user: await _userManager.GetUserAsync(User),
                            entity: "Doctor",
                            recordId: $"{doctor.FirstName} {doctor.LastName}",
                            newValue: serializedDoctor,
                            details: $"Employee {employee.FirstName} {employee.LastName} created with role Doctor by {creatorName}"
                        );
                        break;

                    case "Nurse":
                        var nurse = new Nurse
                        {
                            EmployeeId = employee.Id,
                            FirstName = employee.FirstName,
                            LastName = employee.LastName,
                            Department = employee.Department,
                            WardId = null
                        };
                        _context.Nurse.Add(nurse);

                        var serializedNurse = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            nurse.EmployeeId,
                            nurse.FirstName,
                            nurse.LastName,
                            nurse.Department,
                            nurse.WardId
                        });

                        await LogAuditAsync(
                            actionTaken: "Nurse Record Created",
                            user: await _userManager.GetUserAsync(User),
                            entity: "Nurse",
                            recordId: $"{nurse.FirstName} {nurse.LastName}",
                            newValue: serializedNurse,
                            details: $"Employee {employee.FirstName} {employee.LastName} created with role Nurse by {creatorName}"
                        );
                        break;

                    case "WardAdmin":
                        var wardAdmin = new WardAdmin
                        {
                            EmployeeId = employee.Id,
                            FirstName = employee.FirstName,
                            LastName = employee.LastName,
                            Department = employee.Department
                        };
                        _context.WardAdmin.Add(wardAdmin);

                        var serializedWardAdmin = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            wardAdmin.EmployeeId,
                            wardAdmin.FirstName,
                            wardAdmin.LastName,
                            wardAdmin.Department
                        });

                        await LogAuditAsync(
                            actionTaken: "WardAdmin Record Created",
                            user: await _userManager.GetUserAsync(User),
                            entity: "WardAdmin",
                            recordId: $"{wardAdmin.FirstName} {wardAdmin.LastName}",
                            newValue: serializedWardAdmin,
                            details: $"Employee {employee.FirstName} {employee.LastName} created with role Ward Admin by {creatorName}"
                        );
                        break;

                    case "NurseSister":
                        var nurseSister = new NurseSister
                        {
                            EmployeeId = employee.Id,
                            FirstName = employee.FirstName,
                            LastName = employee.LastName,
                            Department = employee.Department
                        };
                        _context.NurseSister.Add(nurseSister);
                        await _context.SaveChangesAsync();

                        if (model.WardId.HasValue)
                        {
                            var ward = await _context.Ward.FindAsync(model.WardId.Value);
                            if (ward != null)
                            {
                                ward.NurseSisterId = nurseSister.NurseSisterId;
                                await _context.SaveChangesAsync();

                                await LogAuditAsync(
                                    actionTaken: "Ward Assigned to NurseSister",
                                    user: await _userManager.GetUserAsync(User),
                                    entity: "Ward",
                                    recordId: ward.WardId.ToString(),
                                    details: $"Ward {ward.WardName} assigned to NurseSister {nurseSister.FirstName} {nurseSister.LastName}"
                                );
                            }
                        }

                        var serializedNurseSister = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            nurseSister.EmployeeId,
                            nurseSister.FirstName,
                            nurseSister.LastName,
                            nurseSister.Department
                        });

                        await LogAuditAsync(
                            actionTaken: "NurseSister Record Created",
                            user: await _userManager.GetUserAsync(User),
                            entity: "NurseSister",
                            recordId: $"{nurseSister.FirstName} {nurseSister.LastName}",
                            newValue: serializedNurseSister,
                            details: $"Employee {employee.FirstName} {employee.LastName} created with role Nurse Sister by {creatorName}"
                        );
                        break;

                    case "ScriptManager":
                        var scriptManager = new ScriptManager
                        {
                            EmployeeId = employee.Id,
                            FirstName = employee.FirstName,
                            LastName = employee.LastName,
                            Department = employee.Department
                        };
                        _context.ScriptManager.Add(scriptManager);

                        var serializedScriptManager = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            scriptManager.EmployeeId,
                            scriptManager.FirstName,
                            scriptManager.LastName,
                            scriptManager.Department
                        });

                        await LogAuditAsync(
                            actionTaken: "ScriptManager Record Created",
                            user: await _userManager.GetUserAsync(User),
                            entity: "ScriptManager",
                            recordId: $"{scriptManager.FirstName} {scriptManager.LastName}",
                            newValue: serializedScriptManager,
                            details: $"Employee {employee.FirstName} {employee.LastName} created with role Script Manager by {creatorName}"
                        );
                        break;

                    case "StockManager":
                        var stockManager = new StockManager
                        {
                            EmployeeId = employee.Id,
                            FirstName = employee.FirstName,
                            LastName = employee.LastName,
                            Department = employee.Department
                        };
                        _context.StockManager.Add(stockManager);

                        var serializedStockManager = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            stockManager.EmployeeId,
                            stockManager.FirstName,
                            stockManager.LastName,
                            stockManager.Department
                        });

                        await LogAuditAsync(
                            actionTaken: "StockManager Record Created",
                            user: await _userManager.GetUserAsync(User),
                            entity: "StockManager",
                            recordId: $"{stockManager.FirstName} {stockManager.LastName}",
                            newValue: serializedStockManager,
                            details: $"Employee {employee.FirstName} {employee.LastName} created with role Stock Manager by {creatorName}"
                        );
                        break;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"Employee {model.FirstName} {model.LastName} created successfully.";
                return RedirectToAction("ManageEmployees");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var errorMessage = ex.InnerException?.Message ?? ex.Message;

                await LogAuditAsync(
                    actionTaken: "Employee Creation Failed",
                    user: await _userManager.GetUserAsync(User),
                    entity: "Employee",
                    failureReason: errorMessage,
                    details: "Failed to create employee"
                );

                ModelState.AddModelError("", "Error creating employee: " + errorMessage);
                ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name");
                ViewBag.Wards = new SelectList(_context.Ward, "WardId", "Name");
                return View(model);
            }
        }

        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employee
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null)
                return NotFound();

            bool isLinkedToPatient = await _context.Patient
                .AnyAsync(p => p.DoctorId != null &&
                _context.Doctor.Any(d => d.DoctorId == p.DoctorId && d.EmployeeId == id) &&
                !p.IsDeleted);

            if (isLinkedToPatient)
            {
                TempData["ErrorMessage"] = "Cannot delete this employee because they are assigned to one or more patients.";
                return RedirectToAction("ManageEmployees");
            }

            bool hasUpcomingSchedule = await _context.Schedule
                .AnyAsync(s => s.Doctor != null && s.Doctor.EmployeeId == id && s.ScheduledDate >= DateTime.Now && !s.IsCompleted);
            if (hasUpcomingSchedule)
            {
                TempData["ErrorMessage"] = "Cannot delete this employee because they have upcoming scheduled visits.";
                return RedirectToAction("ManageEmployees");
            }

            employee.IsDeleted = true;
            employee.User.IsActive = false;

            if (employee.User != null)
            {
                employee.User.IsDeleted = true;
                employee.User.IsActive = false;
            }

            string deletedRole = "";

            var doctor = await _context.Doctor.FirstOrDefaultAsync(d => d.EmployeeId == id);
            if (doctor != null)
            {
                doctor.IsDeleted = true;
                doctor.IsAvail = false;
                doctor.WardId = null;
                deletedRole = "Doctor";
            }

            var nurse = await _context.Nurse.FirstOrDefaultAsync(n => n.EmployeeId == id);
            if (nurse != null)
            {
                nurse.IsDeleted = true;
                nurse.IsAvail = false;
                deletedRole = "Nurse";
            }

            var nurseSister = await _context.NurseSister.FirstOrDefaultAsync(ns => ns.EmployeeId == id);
            if (nurseSister != null)
            {
                nurseSister.IsDeleted = true;
                nurseSister.IsAvail = false;
                deletedRole = "NurseSister";
            }

            var wardAdmin = await _context.WardAdmin.FirstOrDefaultAsync(w => w.EmployeeId == id);
            if (wardAdmin != null)
            {
                wardAdmin.IsDeleted = true;
                wardAdmin.IsAvail = false;
                deletedRole = "WardAdmin";
            }

            var admin = await _context.Administrator.FirstOrDefaultAsync(a => a.EmployeeId == id);
            if (admin != null)
            {
                admin.IsDeleted = true;
                admin.IsAvail = false;
                deletedRole = "Admin";
            }

            var scriptManager = await _context.ScriptManager.FirstOrDefaultAsync(sm => sm.EmployeeId == id);
            if (scriptManager != null)
            {
                scriptManager.IsDeleted = true;
                scriptManager.IsAvail = false;
                deletedRole = "ScriptManager";
            }

            var stockManager = await _context.StockManager.FirstOrDefaultAsync(sm => sm.EmployeeId == id);
            if (stockManager != null)
            {
                stockManager.IsDeleted = true;
                stockManager.IsAvail = false;
                deletedRole = "StockManager";
            }

            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "SoftDeleteEmployee",
                user: currentUser,
                entity: "Employee",
                recordId: employee.Id.ToString(),
                oldValue: $"Employee: {employee.FirstName} {employee.LastName}, UserId: {employee.UserId}",
                newValue: "SoftDeleted = true, IsActive = false",
                details: $"Deleted related role record: {deletedRole}"
            );

            TempData["SuccessMessage"] = $"Employee {employee.FirstName} {employee.LastName} ({deletedRole}) was deleted successfully.";
            return RedirectToAction("ManageEmployees");
        }

        [HttpGet]
        public async Task<IActionResult> ManageDeletedEmployees()
        {
            var deletedEmployees = await _context.Employee
                .Include(e => e.User)
                .Where(e => e.IsDeleted)
                .ToListAsync();

            return View(deletedEmployees); 
        }

        [HttpGet]
        public async Task<IActionResult> RestoreEmployee(int id)
        {
            var employee = await _context.Employee
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted);

            if (employee == null)
                return NotFound();

            employee.IsDeleted = false;
            if (employee.User != null)
            {
                employee.User.IsDeleted = false;
                employee.User.IsActive = true;
            }

            string restoredRole = "";

            var doctor = await _context.Doctor.FirstOrDefaultAsync(d => d.EmployeeId == id);
            if (doctor != null)
            {
                doctor.IsDeleted = false;
                doctor.IsAvail = true;
                if (doctor.Department == "General Medicine" ||
                    doctor.Department == "Cardiology" ||
                    doctor.Department == "Oncology")
                {
                    doctor.WardId = await _context.Ward
                        .Where(w => w.WardName == "Medical Ward A")
                        .Select(w => (int?)w.WardId)
                        .FirstOrDefaultAsync();
                }
                else if (doctor.Department == "Surgery" ||
                         doctor.Department == "Orthopedics")
                {
                    doctor.WardId = await _context.Ward
                        .Where(w => w.WardName == "Surgical Ward B")
                        .Select(w => (int?)w.WardId)
                        .FirstOrDefaultAsync();
                }
                else if (doctor.Department == "Pediatrics" ||
                         doctor.Department == "Radiology")
                {
                    doctor.WardId = await _context.Ward
                        .Where(w => w.WardName == "Maternity Ward C")
                        .Select(w => (int?)w.WardId)
                        .FirstOrDefaultAsync();
                }
                restoredRole = "Doctor";
            }

            var nurse = await _context.Nurse.FirstOrDefaultAsync(n => n.EmployeeId == id);
            if (nurse != null)
            {
                nurse.IsDeleted = false;
                nurse.IsAvail = true;
                restoredRole = "Nurse";
            }

            var nurseSister = await _context.NurseSister.FirstOrDefaultAsync(ns => ns.EmployeeId == id);
            if (nurseSister != null)
            {
                nurseSister.IsDeleted = false;
                nurseSister.IsAvail = true;
                restoredRole = "NurseSister";
            }

            var wardAdmin = await _context.WardAdmin.FirstOrDefaultAsync(w => w.EmployeeId == id);
            if (wardAdmin != null)
            {
                wardAdmin.IsDeleted = false;
                wardAdmin.IsAvail = true;
                restoredRole = "WardAdmin";
            }

            var admin = await _context.Administrator.FirstOrDefaultAsync(a => a.EmployeeId == id);
            if (admin != null)
            {
                admin.IsDeleted = false;
                admin.IsAvail = true;
                restoredRole = "Admin";
            }

            var scriptManager = await _context.ScriptManager.FirstOrDefaultAsync(sm => sm.EmployeeId == id);
            if (scriptManager != null)
            {
                scriptManager.IsDeleted = false;
                scriptManager.IsAvail = true;
                restoredRole = "ScriptManager";
            }

            var stockManager = await _context.StockManager.FirstOrDefaultAsync(sm => sm.EmployeeId == id);
            if (stockManager != null)
            {
                stockManager.IsDeleted = false;
                stockManager.IsAvail = true;
                restoredRole = "StockManager";
            }

            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "RestoreEmployee",
                user: currentUser,
                entity: "Employee",
                recordId: employee.Id.ToString(),
                oldValue: "SoftDeleted = true",
                newValue: "SoftDeleted = false",
                details: $"Restored related role record: {restoredRole}"
            );

            return RedirectToAction("ManageDeletedEmployees");
        }

        public async Task<IActionResult> EditEmployee(int employeeId)
        {
            var employee = await _context.Employee.Where(emp => emp.Id == employeeId).Include(s => s.User).FirstOrDefaultAsync();

            return View(employee);
        }

        [HttpPost]
        public async Task<IActionResult> EditEmployee(int empId, Employee emp)
        {
            if (empId != emp.Id)
                return View(emp);

            var employee = await _context.Employee
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == empId);

            if (employee == null)
                return NotFound();

            var oldValue = $"Employee: {employee.FirstName} {employee.LastName}, " +
                           $"Phone: {employee.PhoneNumber}, JobTitle: {employee.JobTitle}, " +
                           $"Department: {employee.Department}, TaxNumber: {employee.TaxNumber}, " +
                           $"Bank: {employee.BankName}, Account: {employee.BankAccountNumber}";

            employee.FirstName = emp.FirstName;
            employee.LastName = emp.LastName;
            employee.PhoneNumber = emp.PhoneNumber;
            employee.JobTitle = emp.JobTitle;
            employee.Department = emp.Department;
            employee.TaxNumber = emp.TaxNumber;
            employee.BankName = emp.BankName;
            employee.BankAccountNumber = emp.BankAccountNumber;

            if (employee.User != null)
            {
                employee.User.FirstName = emp.FirstName;
                employee.User.LastName = emp.LastName;
            }

            var updatedRoles = "";

            var doctor = await _context.Doctor.FirstOrDefaultAsync(d => d.EmployeeId == empId);
            if (doctor != null)
            {
                doctor.FirstName = emp.FirstName;
                doctor.LastName = emp.LastName;
                doctor.Department = emp.Department;
                
                if (emp.Department == "General Medicine" ||
                    emp.Department == "Cardiology" ||
                    emp.Department == "Oncology")
                {
                    doctor.WardId = await _context.Ward
                        .Where(w => w.WardName == "Medical Ward A")
                        .Select(w => (int?)w.WardId)
                        .FirstOrDefaultAsync();
                }
                else if (emp.Department == "Surgery" ||
                         emp.Department == "Orthopedics")
                {
                    doctor.WardId = await _context.Ward
                        .Where(w => w.WardName == "Surgical Ward B")
                        .Select(w => (int?)w.WardId)
                        .FirstOrDefaultAsync();
                }
                else if (emp.Department == "Pediatrics" ||
                         emp.Department == "Radiology")
                {
                    doctor.WardId = await _context.Ward
                        .Where(w => w.WardName == "Maternity Ward C")
                        .Select(w => (int?)w.WardId)
                        .FirstOrDefaultAsync();
                }
                _context.Doctor.Update(doctor);
                updatedRoles = "Doctor";
            }

            var nurse = await _context.Nurse.FirstOrDefaultAsync(n => n.EmployeeId == empId);
            if (nurse != null)
            {
                nurse.FirstName = emp.FirstName;
                nurse.LastName = emp.LastName;
                nurse.Department = emp.Department;
                updatedRoles = "Nurse";
            }

            var nurseSister = await _context.NurseSister.FirstOrDefaultAsync(ns => ns.EmployeeId == empId);
            if (nurseSister != null)
            {
                nurseSister.FirstName = emp.FirstName;
                nurseSister.LastName = emp.LastName;
                nurseSister.Department = emp.Department;
                updatedRoles = "NurseSister";
            }

            var wardAdmin = await _context.WardAdmin.FirstOrDefaultAsync(w => w.EmployeeId == empId);
            if (wardAdmin != null)
            {
                wardAdmin.FirstName = emp.FirstName;
                wardAdmin.LastName = emp.LastName;
                wardAdmin.Department = emp.Department;
                updatedRoles = "WardAdmin";
            }

            var admin = await _context.Administrator.FirstOrDefaultAsync(a => a.EmployeeId == empId);
            if (admin != null)
            {
                admin.FirstName = emp.FirstName;
                admin.LastName = emp.LastName;
                updatedRoles = "Admin";
            }

            var scriptManager = await _context.ScriptManager.FirstOrDefaultAsync(sm => sm.EmployeeId == empId);
            if (scriptManager != null)
            {
                scriptManager.FirstName = emp.FirstName;
                scriptManager.LastName = emp.LastName;
                scriptManager.Department = emp.Department;
                updatedRoles = "ScriptManager";
            }

            var stockManager = await _context.StockManager.FirstOrDefaultAsync(sm => sm.EmployeeId == empId);
            if (stockManager != null)
            {
                stockManager.FirstName = emp.FirstName;
                stockManager.LastName = emp.LastName;
                stockManager.Department = emp.Department;
                updatedRoles = "StockManager";
            }

            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "EditEmployee",
                user: currentUser,
                entity: "Employee",
                recordId: employee.Id.ToString(),
                oldValue: oldValue,
                newValue: $"Employee: {employee.FirstName} {employee.LastName}, " +
                          $"Phone: {employee.PhoneNumber}, JobTitle: {employee.JobTitle}, " +
                          $"Department: {employee.Department}, TaxNumber: {employee.TaxNumber}, " +
                          $"Bank: {employee.BankName}, Account: {employee.BankAccountNumber}",
                details: $"Updated related role record: {updatedRoles}"
            );

            return RedirectToAction("ManageEmployees");
        }











        [HttpGet]
        public async Task<IActionResult> ManageAllergies()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var allergies = await _context.Allergy
                .Include(a => a.Patient) 
                .Where(a => a.Patient != null) 
                .OrderBy(a => a.Patient.FirstName)
                .ThenBy(a => a.Patient.LastName)
                .ToListAsync();

            var creatorName = currentUser != null
                ? $"{currentUser.FirstName} {currentUser.LastName}"
                : "System";

            await LogAuditAsync(
                actionTaken: "Viewed Allergies List",
                user: currentUser,
                entity: "Allergy",
                recordId: null,
                details: $"Admin {creatorName} viewed all patient allergies."
            );

            return View(allergies);
        }







        [HttpGet]
        public async Task<IActionResult> ManageMedication(string? search)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var medicationsQuery = _context.Medication
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                medicationsQuery = medicationsQuery.Where(m =>
                    m.MedicationName.Contains(search) ||
                    m.Manufacturer.Contains(search)
                );
            }

            var medications = await medicationsQuery
                .OrderBy(m => m.MedicationName)
                .ToListAsync();

            var creatorName = currentUser != null
                ? $"{currentUser.FirstName} {currentUser.LastName}"
                : "System";

            string searchDetails = string.IsNullOrWhiteSpace(search)
                ? "no search filter applied"
                : $"search term: '{search}'";

            await LogAuditAsync(
                actionTaken: "Viewed Medications List",
                user: currentUser,
                entity: "Medication",
                recordId: null,
                details: $"Admin {creatorName} viewed all medications ({searchDetails})."
            );

            return View(medications);
        }











        public async Task<IActionResult> ManageWard(string search, string filter = "All", bool ascending = true)
        {
            var wards = _context.Ward
            .Include(w => w.Rooms) 
            .Include(w => w.NurseSister)
                .ThenInclude(ns => ns.Employee) 
            .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                wards = wards.Where(w =>
                    w.WardName.Contains(search) ||
                    w.Location.Contains(search));
            }

            if (filter == "Active")
                wards = wards.Where(w => w.Capacity > 0); 
            else if (filter == "LowCapacity")
                wards = wards.Where(w => w.Capacity < 10);

            ViewData["Search"] = search;
            ViewData["Filter"] = filter;
            ViewData["Ascending"] = ascending;

            return View(await wards.ToListAsync());
        }

        public IActionResult CreateWard()
        {
            var model = new Ward();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateWard(Ward model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var wardExists = await _context.Ward
                    .AnyAsync(w => w.WardName == model.WardName && !w.IsDeleted);

                if (wardExists)
                {
                    ModelState.AddModelError("", $"A ward with the name '{model.WardName}' already exists.");
                    return View(model);
                }
                _context.Ward.Add(model);
                await _context.SaveChangesAsync();

                var currentUser = await _userManager.GetUserAsync(User);
                await LogAuditAsync(
                    actionTaken: "CreateWard",
                    user: currentUser,
                    entity: "Ward",
                    recordId: model.WardId.ToString(),
                    oldValue: null,
                    newValue: $"WardName: {model.WardName}, Description: {model.Description}",
                    details: $"Ward '{model.WardName}' created."
                );

                TempData["Success"] = $"Ward '{model.WardName}' created successfully.";
                return RedirectToAction("ManageWard");
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", "Error creating ward: " + errorMessage);
                return View(model);
            }
        }

        public async Task<IActionResult> EditWard(int wardId)
        {
            var ward = await _context.Ward.Where(w => w.WardId == wardId).FirstOrDefaultAsync();

            return View(ward);
        }
        [HttpPost]
        public async Task<IActionResult> EditWard(int wardId, Ward ward)
        {
            if (wardId != ward.WardId)
                return View(ward);

            var existingWard = await _context.Ward.AsNoTracking()
                .FirstOrDefaultAsync(w => w.WardId == wardId);

            if (existingWard == null)
                return NotFound();

            var duplicateWard = await _context.Ward
                .AnyAsync(w => w.WardName == ward.WardName && w.WardId != wardId && !w.IsDeleted);

            if (duplicateWard)
            {
                ModelState.AddModelError("", $"A ward with the name '{ward.WardName}' already exists.");
                return View(ward);
            }

            _context.Update(ward);
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "EditWard",
                user: currentUser,
                entity: "Ward",
                recordId: ward.WardName.ToString(),
                oldValue: $"WardName: {existingWard.WardName}, Description: {existingWard.Description}",
                newValue: $"WardName: {ward.WardName}, Description: {ward.Description}",
                details: $"Ward '{ward.WardName}' updated."
            );

            return RedirectToAction("ManageWard");
        }

        public IActionResult DeleteWard(int wardId)
        {
            var ward = _context.Ward.Find(wardId);

            if (ward == null)
            {
                return NotFound();
            }

            return View(ward);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWardConfirmed(int wardId)
        {
            var ward = await _context.Ward
                .Include(w => w.Rooms) 
                .FirstOrDefaultAsync(w => w.WardId == wardId);

            if (ward == null)
                return NotFound();

            if (ward.Rooms != null && ward.Rooms.Any(r => !r.IsDeleted))
            {
                TempData["Error"] = $"Cannot delete Ward '{ward.WardName}' because it has one or more rooms.";
                return RedirectToAction("ManageWard");
            }

            ward.IsDeleted = true;
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "DeleteWard",
                user: currentUser,
                entity: "Ward",
                recordId: ward.WardId.ToString(),
                oldValue: $"WardName: {ward.WardName}, Description: {ward.Description}",
                newValue: "SoftDeleted = true",
                details: $"Ward '{ward.WardName}' soft deleted."
            );

            TempData["Success"] = $"Ward '{ward.WardName}' has been deleted successfully.";
            return RedirectToAction("ManageWard");
        }


        public async Task<IActionResult> RestoreWards(int WardId)
        {
            var ward = await _context.Ward.FindAsync(WardId);
            if (ward == null || !ward.IsDeleted)
            {
                return NotFound();
            }

            ward.IsDeleted = false;
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "RestoreWard",
                user: currentUser,
                entity: "Ward",
                recordId: ward.WardId.ToString(),
                oldValue: "SoDeleted = true",
                newValue: "SoftDeleted = false",
                details: $"Ward '{ward.WardName}' restored."
            );

            TempData["SuccessMessage"] = $"Ward {ward.WardName} {ward.Description} restored successfully.";
            return RedirectToAction(nameof(ManageWard));
        }














        public async Task<IActionResult> ManageRooms(string search, string filter = "All")
        {
            var rooms = _context.Room
                .Include(r => r.Ward) 
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                rooms = rooms.Where(r => r.RoomName.Contains(search) || r.Ward.WardName.Contains(search));
            }

            ViewData["Search"] = search;

            return View(await rooms.ToListAsync());
        }

        public IActionResult CreateRoom()
        {
            ViewBag.Wards = new SelectList(_context.Ward.Where(w => !w.IsDeleted), "WardId", "WardName");
            var model = new Room();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoom(Room model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Wards = new SelectList(_context.Ward.Where(w => !w.IsDeleted), "WardId", "WardName");
                return View(model);
            }

            try
            {
                var ward = await _context.Ward.FindAsync(model.WardId);
                if (ward == null)
                {
                    ModelState.AddModelError("", "Selected ward not found.");
                    ViewBag.Wards = new SelectList(_context.Ward.Where(w => !w.IsDeleted), "WardId", "WardName");
                    return View(model);
                }

                var existingRoomCount = await _context.Room
                    .Where(r => r.WardId == model.WardId && !r.IsDeleted)
                    .CountAsync();

                if (existingRoomCount >= ward.Capacity)
                {
                    ModelState.AddModelError("", $"Cannot create more rooms. Ward '{ward.WardName}' is limited to {ward.Capacity} rooms.");
                    ViewBag.Wards = new SelectList(_context.Ward.Where(w => !w.IsDeleted), "WardId", "WardName");
                    return View(model);
                }

                var roomExists = await _context.Room
                    .AnyAsync(r => (r.RoomName == model.RoomName || r.RoomNumber == model.RoomNumber)
                                   && r.WardId == model.WardId && !r.IsDeleted);

                if (roomExists)
                {
                    ModelState.AddModelError("", $"A room with the name '{model.RoomName}' or number '{model.RoomNumber}' already exists in this ward.");
                    ViewBag.Wards = new SelectList(_context.Ward.Where(w => !w.IsDeleted), "WardId", "WardName");
                    return View(model);
                }

                _context.Room.Add(model);
                await _context.SaveChangesAsync();

                var currentUser = await _userManager.GetUserAsync(User);
                await LogAuditAsync(
                    actionTaken: "CreateRoom",
                    user: currentUser,
                    entity: "Room",
                    recordId: model.RoomId.ToString(),
                    oldValue: null,
                    newValue: $"Room: {model.RoomName}, WardId: {model.WardId}",
                    details: $"Room '{model.RoomName}' added to Ward '{ward.WardName}'."
                );

                TempData["Success"] = $"Room {model.RoomName} added successfully to Ward {ward.WardName}.";
                return RedirectToAction("ManageRooms");
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", "Error creating room: " + errorMessage);
                ViewBag.Wards = new SelectList(_context.Ward.Where(w => !w.IsDeleted), "WardId", "WardName");
                return View(model);
            }
        }

        public async Task<IActionResult> EditRoom(int roomId)
        {
            var room = await _context.Room.Where(w => w.RoomId == roomId).FirstOrDefaultAsync();
        
            return View(room);
        }

        [HttpPost]
        public async Task<IActionResult> EditRoom(int roomId, Room room)
        {
            if (roomId == room.RoomId)
            {
                var oldRoom = await _context.Room.AsNoTracking().FirstOrDefaultAsync(r => r.RoomId == roomId);

                _context.Update(room);
                await _context.SaveChangesAsync();

                var currentUser = await _userManager.GetUserAsync(User);
                await LogAuditAsync(
                    actionTaken: "EditRoom",
                    user: currentUser,
                    entity: "Room",
                    recordId: room.RoomId.ToString(),
                    oldValue: oldRoom != null ? $"RoomName: {oldRoom.RoomName}, RoomCapacity: {oldRoom.RoomCapacity}, WardId: {oldRoom.WardId}" : null,
                    newValue: $"RoomName: {room.RoomName}, RoomCapacity: {room.RoomCapacity}, WardId: {room.WardId}",
                    details: $"Room '{room.RoomName}' updated."
                );

                return RedirectToAction("ManageRooms");
            }

            return View(room);
        }

        public IActionResult DeleteRoom(int roomId)
        {
            var room = _context.Room.Find(roomId);

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoomConfirmed(int roomId)
        {
            var room = await _context.Room
                .Include(r => r.Beds) 
                .FirstOrDefaultAsync(r => r.RoomId == roomId);

            if (room == null)
                return NotFound();

            var hasOccupiedBeds = room.Beds.Any(b => b.IsOccupied && !b.IsDeleted);

            if (hasOccupiedBeds)
            {
                TempData["Error"] = $"Cannot delete room '{room.RoomName}'. One or more beds are currently occupied.";
                return RedirectToAction("ManageRooms");
            }

            room.IsDeleted = true;

            foreach (var bed in room.Beds)
            {
                bed.IsOccupied = false;
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Room '{room.RoomName}'has been deleted successfully.";

            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "DeleteRoom",
                user: currentUser,
                entity: "Room",
                recordId: room.RoomId.ToString(),
                oldValue: $"Room: {room.RoomName}, WardId: {room.WardId}",
                newValue: "SoftDeleted = true",
                details: $"Room '{room.RoomName}' in Ward '{_context.Ward.Find(room.WardId)?.WardName ?? ""}' was soft deleted."
            );

            return RedirectToAction("ManageRooms");
        }


        public async Task<IActionResult> RestoreRooms(int roomId)
        {
            var room = await _context.Room.FindAsync(roomId);
            if (room == null || !room.IsDeleted)
            {
                return NotFound();
            }
            var ward = await _context.Ward
                .Include(w => w.Rooms)
                .FirstOrDefaultAsync(w => w.WardId == room.WardId);

            if (ward != null)
            {
                var activeRoomCount = ward.Rooms.Count(r => !r.IsDeleted);
                if (activeRoomCount >= ward.Capacity) 
                {
                    TempData["Error"] = $"Cannot restore room '{room.RoomName}'. Ward '{ward.WardName}' has reached its maximum number of active rooms.";
                    return RedirectToAction("ManageRooms");
                }
            }

            room.IsDeleted = false;
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "RestoreRoom",
                user: currentUser,
                entity: "Room",
                recordId: room.RoomId.ToString(),
                oldValue: "SoftDeleted = true",
                newValue: "SoftDeleted = false",
                details: $"Room '{room.RoomName}' with capacity {room.RoomCapacity} restored."
            );

            TempData["SuccessMessage"] = $"Room {room.RoomName} {room.RoomCapacity} restored successfully.";
            return RedirectToAction(nameof(ManageRooms));
        }











        public async Task<IActionResult> ManageBeds(string search, string filter = "All", string sortBy = "BedNumber", bool ascending = true)
        {
            var bed = _context.Bed
                .Include(r => r.Room)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                bed = bed.Where(r => r.BedNumber.Contains(search) || r.BedType.Contains(search));
            }

            if (filter == "Active")
                bed = bed.Where(r => !r.IsDeleted);
            else if (filter == "Deleted")
                bed = bed.Where(r => r.IsDeleted);

            bed = sortBy switch
            {
                "Rooom" => ascending ? bed.OrderBy(r => r.BedNumber) : bed.OrderByDescending(r => r.BedNumber),
                _ => ascending ? bed.OrderBy(r => r.BedNumber) : bed.OrderByDescending(r => r.BedNumber)
            };

            ViewData["Search"] = search;
            ViewData["Filter"] = filter;
            ViewData["SortBy"] = sortBy;
            ViewData["Ascending"] = ascending;

            return View(await bed.ToListAsync());
        }

        public IActionResult CreateBed()
        {
            ViewBag.Rooms = new SelectList(_context.Room.Where(w => !w.IsDeleted), "RoomId", "RoomNumber");
            var model = new Bed();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBed(Bed model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Rooms = new SelectList(_context.Room.Where(w => !w.IsDeleted), "RoomId", "RoomNumber");
                return View(model);
            }

            try
            {
                var duplicateBed = await _context.Bed
                    .AnyAsync(b => b.BedNumber == model.BedNumber && !b.IsDeleted);

                if (duplicateBed)
                {
                    ModelState.AddModelError("", $"A bed with the number '{model.BedNumber}' already exists.");
                    ViewBag.Rooms = new SelectList(_context.Room.Where(w => !w.IsDeleted), "RoomId", "RoomNumber");
                    return View(model);
                }
                var room = await _context.Room
                    .Include(r => r.Beds)
                    .FirstOrDefaultAsync(r => r.RoomId == model.RoomId && !r.IsDeleted);

                if (room == null)
                {
                    TempData["Error"] = "Selected room does not exist.";
                    return RedirectToAction("ManageBeds");
                }

                var currentBeds = room.Beds.Count(b => !b.IsDeleted);

                if (currentBeds < room.RoomCapacity)
                {
                    _context.Bed.Add(model);
                    room.NoOccupiedBed += 1;
                    await _context.SaveChangesAsync();
                    
                    var currentUser = await _userManager.GetUserAsync(User);
                    await LogAuditAsync(
                        actionTaken: "CreateBed",
                        user: currentUser,
                        entity: "Bed",
                        recordId: model.BedId.ToString(),
                        oldValue: null,
                        newValue: $"BedNumber: {model.BedNumber}, RoomId: {model.RoomId}, IsOccupied: {model.IsOccupied}",
                        details: $"Bed {model.BedNumber} added to Room {(_context.Room.Find(model.RoomId)?.RoomNumber ?? "")}."
                    );

                    TempData["Success"] = $"Bed {model.BedNumber} added successfully to Room {(_context.Room.Find(model.RoomId)?.RoomNumber ?? "")}.";
                    return RedirectToAction("ManageBeds");
                }
                else
                {
                    TempData["Error"] = $"Cannot add bed. Room '{room.RoomNumber}' already has {room.RoomCapacity} beds.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", "Error creating bed: " + errorMessage);
                ViewBag.Rooms = new SelectList(_context.Room.Where(w => !w.IsDeleted), "RoomId", "RoomNumber");
                return View(model);
            }
        }

        public async Task<IActionResult> EditBed(int bedId)
        {
            var bed = await _context.Bed.Where(w => w.BedId == bedId).FirstOrDefaultAsync();

            return View(bed);
        }

        [HttpPost]
        public async Task<IActionResult> EditBed(int bedId, Bed bed)
        {

            if (bedId == bed.BedId)
            {

                _context.Update(bed);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageBeds");
            }


            return View(bed);

        }

        public IActionResult DeleteBed(int bedId)
        {
            var bed = _context.Bed.Find(bedId);

            if (bed == null)
            {
                return NotFound();
            }

            return View(bed);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBedConfirmed(int bedId)
        {
            var bed = await _context.Bed.FindAsync(bedId);
            if (bed == null)
            {
                return NotFound();
            }

            if (bed.IsOccupied)
            {
                TempData["Error"] = $"Cannot delete Bed {bed.BedNumber} because it is currently occupied.";
                return RedirectToAction("ManageBeds");
            }

            bed.IsDeleted = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Bed {bed.BedNumber} has been deleted successfully.";

            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "DeleteBed",
                user: currentUser,
                entity: "Bed",
                recordId: bed.BedId.ToString(),
                oldValue: $"BedNumber: {bed.BedNumber}, RoomId: {bed.RoomId}, IsOccupied: {bed.IsOccupied}",
                newValue: "IsDeleted = true",
                details: $"Bed {bed.BedNumber} in Room {(_context.Room.Find(bed.RoomId)?.RoomNumber ?? "")} soft-deleted."
            );

            return RedirectToAction("ManageBeds");
        }


        public async Task<IActionResult> RestoreBeds(int bedId)
        {
            var bed = await _context.Bed
                .Include(b => b.Room)
                    .ThenInclude(r => r.Beds)
                .FirstOrDefaultAsync(b => b.BedId == bedId);
            if (bed == null || !bed.IsDeleted)
            {
                return NotFound();
            }

            var room = bed.Room;
            if (room == null)
            {
                TempData["Error"] = $"Cannot restore bed {bed.BedNumber}. Room not found.";
                return RedirectToAction(nameof(ManageBeds));
            }

            var activeBedsCount = room.Beds.Count(b => !b.IsDeleted);
            if (activeBedsCount >= room.RoomCapacity)
            {
                TempData["Error"] = $"Cannot restore bed {bed.BedNumber}. Room {room.RoomNumber} has reached its capacity.";
                return RedirectToAction(nameof(ManageBeds));
            }

            bed.IsDeleted = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Bed {bed.BedNumber} has been restored successfully.";
            
            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "RestoreBed",
                user: currentUser,
                entity: "Bed",
                recordId: bed.BedId.ToString(),
                oldValue: "IsDeleted = true",
                newValue: "IsDeleted = false",
                details: $"Bed {bed.BedNumber} ({bed.BedType}) restored."
            );

            TempData["SuccessMessage"] = $"Bed {bed.BedNumber} {bed.BedType} restored successfully.";
            return RedirectToAction(nameof(ManageBeds));
        }








        public async Task<IActionResult> ManageAlerts()
        {
            var alerts = await _context.Alerts
                .Include(a => a.User) 
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            
            var model = alerts.Select(a =>
            {
                string recipientType;
                string targetUserName = null;
                string targetRole = null;
                int receivedCount = 0;
                bool? isRead = null;

                if (!string.IsNullOrEmpty(a.UserId))
                {
                    recipientType = "User";

                    targetUserName = a.User != null
                        ? $"{a.User.FirstName} {a.User.LastName}"
                        : "Unknown";

                    receivedCount = 1;

                    isRead = _context.AlertReads.Any(r =>
                        r.AlertId == a.AlertId &&
                        r.UserId == a.UserId);
                }
                else if (!string.IsNullOrEmpty(a.TargetRole))
                {
                    recipientType = "Role";

                    targetRole = a.TargetRole;

                    receivedCount = _context.Users
                        .Count(u => u.Role == a.TargetRole && !u.IsDeleted);

                    isRead = null;

                }
                else
                {
                    recipientType = "All";

                    receivedCount = _context.Users
                        .Count(u => !u.IsDeleted);

                    isRead = null;
                }

                return new Imnandi_NovaCare_Hospital_System.Models.ManageAlertViewModel
                {
                    AlertId = a.AlertId,
                    Message = a.Message,
                    RecipientType = recipientType,
                    TargetUserName = targetUserName,
                    TargetRole = targetRole,
                    ReceivedCount = receivedCount,
                    IsRead = isRead,
                    CreatedBy = a.CreatedBy,
                    CreatedAt = a.CreatedAt,
                    IsActive = a.IsActive
                };
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateAlert()
        {
            ViewBag.Users = await _context.Users
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            ViewBag.Roles = _context.Users
                .Select(u => u.Role)
                .Distinct()
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAlert(string message, string? userId = null, string? role = null)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError("Message", "Message cannot be empty");
                return await CreateAlert(); 
            }

            Alert alert;
            if (!string.IsNullOrEmpty(userId))
            {
                alert = new Alert
                {
                    Message = message,
                    UserId = userId,
                    CreatedBy = User.Identity?.Name,
                    IsActive = true
                };
                _context.Alerts.Add(alert);
            }
            else if (!string.IsNullOrEmpty(role))
            {
                alert = new Alert
                {
                    Message = message,
                    TargetRole = role,
                    CreatedBy = User.Identity?.Name,
                    IsActive = true
                };
                _context.Alerts.Add(alert);
            }
            else
            {
                alert = new Alert
                {
                    Message = message,
                    CreatedBy = User.Identity?.Name,
                    IsActive = true
                };
                _context.Alerts.Add(alert);
            }

            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "CreateAlert",
                user: currentUser,
                entity: "Alert",
                recordId: alert.AlertId.ToString(),
                oldValue: null,
                newValue: $"Message: {message}, UserId: {userId}, Role: {role}",
                details: $"Alert created {(userId != null ? "for user "  : role != null ? "for role " + role : "for all users")}"
            );

            TempData["Success"] = "Alert sent successfully.";
            return RedirectToAction("ManageAlerts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAlertInactive(int alertId)
        {
            var alert = await _context.Alerts.FindAsync(alertId);
            if (alert == null)
            {
                TempData["Error"] = "Alert not found.";
                return RedirectToAction("ManageAlerts");
            }

            alert.IsActive = false;
            _context.Alerts.Update(alert);
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "MarkAlertInactive",
                user: currentUser,
                entity: "Alert",
                recordId: alert.AlertId.ToString(),
                oldValue: $"IsActive = true, Message = {alert.Message}, TargetRole = {alert.TargetRole}, UserId = {alert.UserId}",
                newValue: "IsActive = false",
                details: "Alert deactivated successfully."
            );

            TempData["Success"] = "Alert deactivated successfully.";
            return RedirectToAction("ManageAlerts");
        }








        [HttpGet]
        public async Task<IActionResult> ManageAuditLogs(string? searchUsername, string? role, bool? isDeleted)
        {
            var auditLogsQuery = _context.AuditLogs
                .Include(a => a.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchUsername))
            {
                auditLogsQuery = auditLogsQuery.Where(a =>
                    a.Username.Contains(searchUsername) ||
                    (a.User != null && a.User.UserName.Contains(searchUsername)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                auditLogsQuery = auditLogsQuery.Where(a =>
                    a.User != null && a.User.Role == role);
            }

            if (isDeleted.HasValue)
            {
                auditLogsQuery = auditLogsQuery.Where(a =>
                    a.User != null && a.User.IsDeleted == isDeleted.Value);
            }

            var auditLogs = await auditLogsQuery
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return View(auditLogs);
        }

        [HttpGet]
        public async Task<IActionResult> AuditLogDetails(string userId, DateTime? fromDate = null, DateTime? toDate = null, string search = null)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return NotFound();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var query = _context.AuditLogs
                .Where(a => a.UserId == userId);

            if (fromDate.HasValue)
                query = query.Where(a => (a.LoginDateTime ?? a.Timestamp) >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => (a.LoginDateTime ?? a.Timestamp) <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(a =>
                    (a.ActionTaken ?? "").ToLower().Contains(search) ||
                    (a.Entity ?? "").ToLower().Contains(search) ||
                    (a.RecordId ?? "").ToLower().Contains(search) ||
                    (a.Details ?? "").ToLower().Contains(search) ||
                    (a.IpAddress ?? "").ToLower().Contains(search)
                );
            }

            var userLogs = await query
                .OrderByDescending(a => a.LoginDateTime ?? a.Timestamp)
                .ToListAsync();

            ViewBag.Username = user.UserName;

            if (!userLogs.Any())
                return View("NoLogs");

            return View(userLogs);
        }






        public async Task<IActionResult> ManageHospitalStore()
        {
            var stores = await _context.HospitalStore.ToListAsync();
            return View(stores);
        }

        [HttpGet]
        public IActionResult CreateHospitalStore()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHospitalStore(HospitalStore model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            _context.HospitalStore.Add(model);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Hospital Store Created",
                user: user,
                entity: "HospitalStore",
                recordId: model.HospitalStoreId.ToString(),
                newValue: System.Text.Json.JsonSerializer.Serialize(model),
                details: $"Hospital Store {model.HospitalStoreName} created by {user.FirstName} {user.LastName}."
            );

            TempData["Success"] = "Hospital Store created successfully.";
            return RedirectToAction(nameof(ManageHospitalStore));
        }

        [HttpGet]
        public async Task<IActionResult> EditHospitalStore(int id)
        {
            var store = await _context.HospitalStore.FindAsync(id);
            if (store == null) return NotFound();
            return View(store);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHospitalStore(HospitalStore model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var oldStore = await _context.HospitalStore.AsNoTracking().FirstOrDefaultAsync(s => s.HospitalStoreId == model.HospitalStoreId);

            _context.HospitalStore.Update(model);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Hospital Store Updated",
                user: user,
                entity: "HospitalStore",
                recordId: model.HospitalStoreId.ToString(),
                oldValue: System.Text.Json.JsonSerializer.Serialize(oldStore),
                newValue: System.Text.Json.JsonSerializer.Serialize(model),
                details: $"Hospital Store {model.HospitalStoreName} updated by {user.FirstName} {user.LastName}."
            );

            TempData["Success"] = "Hospital Store updated successfully.";
            return RedirectToAction(nameof(ManageHospitalStore));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHospitalStore(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var store = await _context.HospitalStore.FindAsync(id);
            if (store == null) return NotFound();

            store.IsDeleted = true;
            _context.HospitalStore.Update(store);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Hospital Store Deleted",
                user: user,
                entity: "HospitalStore",
                recordId: id.ToString(),
                details: $"Hospital Store {store.HospitalStoreName} deleted by {user.FirstName} {user.LastName}."
            );

            TempData["Success"] = "Hospital Store deleted successfully.";
            return RedirectToAction(nameof(ManageHospitalStore));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreHospitalStore(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var store = await _context.HospitalStore.FindAsync(id);
            if (store == null) return NotFound();

            store.IsDeleted = false;
            _context.HospitalStore.Update(store);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Hospital Store Restored",
                user: user,
                entity: "HospitalStore",
                recordId: id.ToString(),
                details: $"Hospital Store {store.HospitalStoreName} restored by {user.FirstName} {user.LastName}."
            );

            TempData["Success"] = "Hospital Store restored successfully.";
            return RedirectToAction(nameof(ManageHospitalStore));
        }







        public async Task<IActionResult> ManageHospitalPharmacy()
        {
            var pharmacies = await _context.HospitalPharmarcy.ToListAsync();
            return View(pharmacies);
        }

        [HttpGet]
        public IActionResult CreateHospitalPharmacy()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHospitalPharmacy(HospitalPharmarcy model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            _context.HospitalPharmarcy.Add(model);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Hospital Pharmacy Created",
                user: user,
                entity: "HospitalPharmarcy",
                recordId: model.HospitalPharmacyId.ToString(),
                newValue: System.Text.Json.JsonSerializer.Serialize(model),
                details: $"Hospital Pharmacy {model.HospitalPharmarcyName} created by {user.FirstName} {user.LastName}."
            );

            TempData["Success"] = "Hospital Pharmacy created successfully.";
            return RedirectToAction(nameof(ManageHospitalPharmacy));
        }

        [HttpGet]
        public async Task<IActionResult> EditHospitalPharmacy(int id)
        {
            var pharmacy = await _context.HospitalPharmarcy.FindAsync(id);
            if (pharmacy == null) return NotFound();
            return View(pharmacy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHospitalPharmacy(HospitalPharmarcy model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var oldPharmacy = await _context.HospitalPharmarcy.AsNoTracking().FirstOrDefaultAsync(p => p.HospitalPharmacyId == model.HospitalPharmacyId);

            _context.HospitalPharmarcy.Update(model);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Hospital Pharmacy Updated",
                user: user,
                entity: "HospitalPharmarcy",
                recordId: model.HospitalPharmacyId.ToString(),
                oldValue: System.Text.Json.JsonSerializer.Serialize(oldPharmacy),
                newValue: System.Text.Json.JsonSerializer.Serialize(model),
                details: $"Hospital Pharmacy {model.HospitalPharmarcyName} updated by {user.FirstName} {user.LastName}."
            );

            TempData["Success"] = "Hospital Pharmacy updated successfully.";
            return RedirectToAction(nameof(ManageHospitalPharmacy));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHospitalPharmacy(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var pharmacy = await _context.HospitalPharmarcy.FindAsync(id);
            if (pharmacy == null) return NotFound();

            pharmacy.IsDeleted = true;
            _context.HospitalPharmarcy.Update(pharmacy);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Hospital Pharmacy Deleted",
                user: user,
                entity: "HospitalPharmarcy",
                recordId: id.ToString(),
                details: $"Hospital Pharmacy {pharmacy.HospitalPharmarcyName} soft-deleted by {user.FirstName} {user.LastName}."
            );

            TempData["Success"] = "Hospital Pharmacy deleted successfully.";
            return RedirectToAction(nameof(ManageHospitalPharmacy));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreHospitalPharmacy(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var pharmacy = await _context.HospitalPharmarcy.FindAsync(id);
            if (pharmacy == null) return NotFound();

            pharmacy.IsDeleted = false;
            _context.HospitalPharmarcy.Update(pharmacy);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Hospital Pharmacy Restored",
                user: user,
                entity: "HospitalPharmarcy",
                recordId: id.ToString(),
                details: $"Hospital Pharmacy {pharmacy.HospitalPharmarcyName} restored by {user.FirstName} {user.LastName}."
            );

            TempData["Success"] = "Hospital Pharmacy restored successfully.";
            return RedirectToAction(nameof(ManageHospitalPharmacy));
        }

    }
}