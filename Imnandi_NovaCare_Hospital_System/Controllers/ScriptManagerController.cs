using Imnandi_NovaCare_Hospital_System.Data;
using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Imnandi_NovaCare_Hospital_System.Controllers
{
    [Authorize(Roles = "ScriptManager")]
    public class ScriptManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ScriptManagerController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> ScriptManagerDashboard(string? search)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var scriptManager = await _context.ScriptManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm => sm.Employee.UserId == user.Id);

            if (scriptManager == null)
                return Unauthorized();


            var prescriptionsQuery = _context.ScriptPrescription
                .Include(p => p.Prescription)
                    .ThenInclude(pr => pr.Patient)
                .Include(p => p.Prescription)
                    .ThenInclude(pr => pr.Doctor)
                .Include(p => p.Medications)
                    .ThenInclude(spm => spm.Medication)
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                prescriptionsQuery = prescriptionsQuery.Where(p =>
                    p.Prescription.Patient.FirstName.ToLower().Contains(search) ||
                    p.Prescription.Patient.LastName.ToLower().Contains(search) ||
                    p.Medications.Any(m => m.Medication.MedicationName.ToLower().Contains(search))
                );
            }
            
            var pendingScripts = await prescriptionsQuery
                .Where(p => p.Status != null && p.Status.ToLower() == "pending")
                .OrderByDescending(p => p.Prescription.IssueDate)
                .Take(10)
                .ToListAsync();

            var completedScripts = await prescriptionsQuery
                .Where(p => p.Status != null && p.Status.ToLower() == "completed")
                .OrderByDescending(p => p.Prescription.IssueDate)
                .Take(10)
                .ToListAsync();

            var totalPrescriptions = await prescriptionsQuery.CountAsync();

            var pendingPrescriptionsQuery = _context.Prescription
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Where(p => p.Status != null && p.Status.ToLower() == "pending")
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                pendingPrescriptionsQuery = pendingPrescriptionsQuery.Where(p =>
                    p.Patient.FirstName.ToLower().Contains(search) ||
                    p.Patient.LastName.ToLower().Contains(search)
                );
            }

            var pendingPrescriptions = await pendingPrescriptionsQuery
                .OrderByDescending(p => p.IssueDate)
                .Take(10)
                .ToListAsync();

            foreach (var pres in pendingPrescriptions)
            {
                if (!pendingScripts.Any(sp => sp.PrescriptionId == pres.PrescriptionId))
                {
                    pendingScripts.Add(new ScriptPrescription
                    {
                        PrescriptionId = pres.PrescriptionId,
                        Prescription = pres,
                        Status = pres.Status
                    });
                }
            }

            var allMedicineStock = await _context.Medication
                .OrderBy(m => m.MedicationName)
                .ToListAsync();
            var totalMedicines = allMedicineStock.Count;

            var recentStockTakes = await _context.StockTake
                .Include(st => st.StockManager)
                .OrderByDescending(st => st.Date)
                .Take(5)
                .ToListAsync();

            var lowStockMedications = await GetLowStockMedicationsAsync(5);
            var expiringPrescriptions = await GetExpiringPrescriptionsAsync(3);

            var lowStockCount = lowStockMedications.Count;
            var expiringCount = expiringPrescriptions.Count;

            var model = new ScriptManagerDashboardViewModel
            {
                ScriptManagerId = scriptManager.ScriptManagerId,
                FirstName = scriptManager.Employee.FirstName,
                LastName = scriptManager.Employee.LastName,
                Email = user.Email ?? string.Empty,
                JobTitle = scriptManager.Employee.JobTitle ?? "Script Manager",
                PhoneNumber = scriptManager.Employee.PhoneNumber,
                Department = scriptManager.Employee.Department,

                TotalPrescriptions = totalPrescriptions,
                PendingPrescriptionCount = pendingScripts.Count,
                CompletedPrescriptionCount = completedScripts.Count,
                TotalMedicinesInStock = totalMedicines,

                PendingScripts = pendingScripts,
                CompletedScripts = completedScripts,
                MedicineStock = allMedicineStock,
                RecentStockTakes = recentStockTakes,
                LowStockMedications = lowStockMedications,
                ExpiringPrescriptions = expiringPrescriptions,
                LowStockCount = lowStockCount,
                ExpiringCount = expiringCount
            };

            ViewData["Search"] = search;
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
            var model = new ScriptManagerDashboardViewModel
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
        public async Task<IActionResult> Profile(ScriptManagerDashboardViewModel model)
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
           
            var scriptmanager = await _context.ScriptManager.FirstOrDefaultAsync(a => a.EmployeeId == employee.Id);
            if (scriptmanager != null)
            {
                scriptmanager.FirstName = model.FirstName;
                scriptmanager.LastName = model.LastName;
                _context.ScriptManager.Update(scriptmanager);
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
                    details: $"Script Manager {user.FirstName} {user.LastName} updated their profile."
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
                    details: $"Script Manager {user.FirstName} {user.LastName} attempted to update profile but failed."
                );
            }

            return View(model);
        }

        
        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var oldUserData = new { user.UserName, user.Email };
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

                await LogAuditAsync(
                    actionTaken: "Password Changed",
                    user: user,
                    entity: "User",
                    recordId: user.Id.ToString(),
                    oldValue: System.Text.Json.JsonSerializer.Serialize(oldUserData),
                    newValue: null,
                    details: $"Script Manager {user.FirstName} {user.LastName} successfully changed their password."
                );

                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction("ScriptManagerDashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                await LogAuditAsync(
                    actionTaken: "Password Change Failed",
                    user: user,
                    entity: "User",
                    recordId: user.Id.ToString(),
                    failureReason: error.Description,
                    details: $"Script Manager {user.FirstName} {user.LastName} attempted to change password but failed."
                );
            }
            return View(model);
        }









        [HttpGet]
        public async Task<IActionResult> ManageMedication(string? status = "all")
        {
            var query = _context.Medication.AsQueryable();

            switch (status.ToLower())
            {
                case "expiring":
                    var thirtyDaysFromNow = DateTime.Now.AddDays(30);
                    query = query.Where(m => m.ExpiryDate.HasValue &&
                                          m.ExpiryDate.Value >= DateTime.Now &&
                                          m.ExpiryDate.Value <= thirtyDaysFromNow);
                    break;
                case "expired":
                    query = query.Where(m => m.ExpiryDate.HasValue &&
                                          m.ExpiryDate.Value < DateTime.Now);
                    break;
                case "ok": 
                    var futureDate = DateTime.Now.AddDays(30);
                    query = query.Where(m => !m.ExpiryDate.HasValue ||
                                          m.ExpiryDate.Value > futureDate);
                    break;
                case "all":
                default:
                    query = query;
                    break;
            }

            var medications = await query
                .OrderBy(m => m.MedicationName)
                .ToListAsync();

            ViewBag.CurrentStatus = status;

            return View(medications);
        }
        [HttpGet]
        public IActionResult CreateMedication() => View(new Medication());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedication(Medication model)
        {
            if (!ModelState.IsValid) return View(model);

            if (!model.ExpiryDate.HasValue)
            {
                ModelState.AddModelError("ExpiryDate", "Expiry date is required.");
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            var existingMedication = await _context.Medication
                .FirstOrDefaultAsync(m => m.MedicationName.ToLower() == model.MedicationName.ToLower() && !m.IsDeleted);

            if (existingMedication != null)
            {
                ModelState.AddModelError("MedicationName", "Medication with this name already exists.");
                await LogAuditAsync(
                    "Medication Creation Failed",
                    currentUser,
                    entity: "Medication",
                    recordId: "N/A",
                    failureReason: $"Medication '{model.MedicationName}' already exists."
                );
                return View(model);
            }

            _context.Medication.Add(model);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                "Medication Created",
                currentUser,
                entity: "Medication",
                recordId: model.MedicationId.ToString(),
                oldValue: null,
                newValue: $"MedicationName: {model.MedicationName}, ExpiryDate: {model.ExpiryDate?.ToShortDateString()}",
                details: $"Medication '{model.MedicationName}' created by {currentUser?.FirstName} {currentUser?.LastName}."
            );

            TempData["SuccessMessage"] = $"Medication '{model.MedicationName}' created successfully.";
            return RedirectToAction("ManageMedication");
        }

        [HttpGet]
        public async Task<IActionResult> EditMedication(int id)
        {
            var med = await _context.Medication.FirstOrDefaultAsync(m => m.MedicationId == id);
            if (med == null || med.IsDeleted)
                return NotFound();

            return View(med);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedication(Medication model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var med = await _context.Medication.FirstOrDefaultAsync(m => m.MedicationId == model.MedicationId);
            if (med == null || med.IsDeleted)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            var oldValue = $"MedicationName: {med.MedicationName}, Description: {med.Description}, DosageForm: {med.DosageForm}, Manufacturer: {med.Manufacturer}, ExpiryDate: {med.ExpiryDate?.ToShortDateString()}";

            
            med.MedicationName = model.MedicationName;
            med.Description = model.Description;
            med.DosageForm = model.DosageForm;
            med.Manufacturer = model.Manufacturer;
            med.ExpiryDate = model.ExpiryDate;

            await _context.SaveChangesAsync();

            var newValue = $"MedicationName: {med.MedicationName}, Description: {med.Description}, DosageForm: {med.DosageForm}, Manufacturer: {med.Manufacturer}, ExpiryDate: {med.ExpiryDate?.ToShortDateString()}";
            await LogAuditAsync(
                "Medication Edited",
                currentUser,
                entity: "Medication",
                recordId: med.MedicationId.ToString(),
                oldValue: oldValue,
                newValue: newValue,
                details: $"Medication '{med.MedicationName}' edited by {currentUser?.FirstName} {currentUser?.LastName}."
            );

            TempData["SuccessMessage"] = $"Medication '{med.MedicationName}' updated successfully.";
            return RedirectToAction("ManageMedication");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedication(int id)
        {
            var med = await _context.Medication
                .FirstOrDefaultAsync(m => m.MedicationId == id && !m.IsDeleted);

            if (med == null)
            {
                TempData["ErrorMessage"] = "Medication not found or already deleted.";
                return RedirectToAction("ManageMedication");
            }

            med.IsDeleted = true;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            await LogAuditAsync(
                "Medication Deleted",
                currentUser,
                entity: "Medication",
                recordId: med.MedicationId.ToString(),
                oldValue: $"MedicationName: {med.MedicationName}, ExpiryDate: {med.ExpiryDate?.ToShortDateString()}",
                newValue: null,
                details: $"Medication '{med.MedicationName}' deleted by {currentUser?.FirstName} {currentUser?.LastName}."
            );

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Medication '{med.MedicationName}' deleted successfully.";
            return RedirectToAction("ManageMedication");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreMedication(int id)
        {
            var med = await _context.Medication.FirstOrDefaultAsync(m => m.MedicationId == id && m.IsDeleted);
            if (med == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            med.IsDeleted = false;
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                "Medication Restored",
                currentUser,
                entity: "Medication",
                recordId: med.MedicationId.ToString(),
                oldValue: null,
                newValue: $"MedicationName: {med.MedicationName}",
                details: $"Medication '{med.MedicationName}' restored by {currentUser?.FirstName} {currentUser?.LastName}."
            );

            TempData["SuccessMessage"] = $"Medication '{med.MedicationName}' restored successfully.";
            return RedirectToAction("ManageMedication");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotifyStockManager(int medicationId)
        {
            var medication = await _context.Medication.FindAsync(medicationId);
            if (medication == null) return NotFound();

            bool isExpired = medication.ExpiryDate < DateTime.Today;
            bool isExpiringSoon = !isExpired && medication.ExpiryDate <= DateTime.Today.AddDays(30);

            string statusMessage = "";

            if (!isExpired && !isExpiringSoon)
            {
                statusMessage = "This medication is not expiring soon.";
                TempData["Info"] = statusMessage;
                return RedirectToAction("ManageMedication");
            }

            var stockManagers = await _context.Employee
                .Include(e => e.User)
                .Where(e => !e.IsDeleted
                            && e.User.IsActive
                            && e.User.Role.ToLower() == "stockmanager")
                .ToListAsync();

            if (!stockManagers.Any())
            {
                TempData["Error"] = "No Stock Managers found to notify!";
                return RedirectToAction("ManageMedication");
            }

            int addedCount = 0;

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == currentUserId);

            foreach (var manager in stockManagers)
            {
                if (manager.User == null) continue;

                string expiredMessage = $"Medication '{medication.MedicationName}' has expired!";
                string expiringSoonMessage = $"Medication '{medication.MedicationName}' is expiring soon!";

                var existingNotifications = await _context.Notifications
                    .Where(n => n.UserId == manager.User.Id &&
                                n.Message.Contains(medication.MedicationName))
                    .ToListAsync();

                bool alreadyNotifiedExpired =
                    existingNotifications.Any(n => n.Message.Contains("has expired"));

                bool alreadyNotifiedExpiringSoon =
                    existingNotifications.Any(n => n.Message.Contains("is expiring soon"));

                string messageToSend = null;

                if (isExpired && !alreadyNotifiedExpired)
                    messageToSend = expiredMessage;
                else if (isExpiringSoon && !alreadyNotifiedExpiringSoon)
                    messageToSend = expiringSoonMessage;

                if (messageToSend == null) continue;

                _context.Notifications.Add(new Notification
                {
                    UserId = manager.User.Id,
                    Message = messageToSend,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });

                addedCount++;

                await LogAuditAsync(
                    "NotifyStockManager",
                    currentUser,
                    entity: "Notification",
                    recordId: medication.MedicationId.ToString(),
                    newValue: $"Message sent to {manager.User.UserName}: {messageToSend}",
                    details: $"ScriptManager '{currentUser?.UserName}' notified StockManager '{manager.User.UserName}' about medication '{medication.MedicationName}'."
                );
            }

            if (addedCount > 0)
            {
                statusMessage = isExpired
                    ? "Stock Manager(s) notified: Medication has EXPIRED."
                    : "Stock Manager(s) notified: Medication is EXPIRING soon.";

                TempData["Success"] = statusMessage;
                await _context.SaveChangesAsync();
            }
            else
            {
                statusMessage = "Stock Managers were already notified about this medication.";
                TempData["Info"] = statusMessage;
            }

            return RedirectToAction("ManageMedication");
        }












        [HttpGet]
        public async Task<IActionResult> ManagePrescriptions(string? search, string? status, string? ward)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var scriptManager = await _context.ScriptManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm => sm.Employee.UserId == user.Id);

            if (scriptManager == null) return Unauthorized();

            var allWards = await _context.Ward
                .Select(w => w.WardName)
                .Distinct()
                .OrderBy(w => w)
                .ToListAsync();

            var prescriptionsQuery = _context.Prescription
                .Include(p => p.Medication)
                .Include(p => p.Patient)
                    .ThenInclude(p => p.Room)
                        .ThenInclude(r => r.Ward)
                .Include(p => p.Doctor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(ward))
            {
                prescriptionsQuery = prescriptionsQuery
                    .Where(p => p.Patient.Room.Ward.WardName.ToLower().Equals(ward.ToLower()));
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                prescriptionsQuery = prescriptionsQuery.Where(p =>
                    p.Medication.MedicationName.ToLower().Contains(search) ||
                    p.Patient.FirstName.ToLower().Contains(search) ||
                    p.Patient.LastName.ToLower().Contains(search)
                );
            }

            if (!string.IsNullOrEmpty(status))
            {
                status = status.Trim().ToLower();
                prescriptionsQuery = prescriptionsQuery
                    .Where(p => p.Status.ToLower().Equals(status));
            }

            var prescriptions = await prescriptionsQuery
                .OrderByDescending(p => p.IssueDate)
                .ThenByDescending(p => p.IssueTime)
                .ToListAsync();

            var scriptPrescriptions = new List<ScriptPrescription>();

            foreach (var prescription in prescriptions)
            {
                var existingScriptPrescription = await _context.ScriptPrescription
                    .Include(sp => sp.Medications)
                        .ThenInclude(spm => spm.Medication)
                    .Include(sp => sp.ReceivedFrom)
                    .Include(sp => sp.ScriptManager)
                    .FirstOrDefaultAsync(sp => sp.PrescriptionId == prescription.PrescriptionId && !sp.IsDeleted);

                if (existingScriptPrescription != null)
                {
                    scriptPrescriptions.Add(existingScriptPrescription);
                }
                else
                {
                    var newScriptPrescription = new Imnandi_NovaCare_Hospital_System.Models.ScriptPrescription
                    {
                        ScriptPrescriptionId = 0,
                        PrescriptionId = prescription.PrescriptionId,
                        Prescription = prescription,
                        Status = prescription.Status,
                        ScriptManagerId = prescription.ScriptManagerId ?? 0,
                        ProcessedDate = null,
                        ReceivedDate = null,
                        Notes = null,
                        IsDeleted = false,
                        IsVerified = false,
                        ReceivedFromId = null,
                        Medications = new List<ScriptPrescriptionMedication>(),
                        VerifiedDate = DateTime.MinValue,
                        VerifiedBy = null,
                        AssignedDate = DateTime.MinValue
                    };
                    scriptPrescriptions.Add(newScriptPrescription);
                }
            }

            ViewBag.Wards = allWards;
            ViewBag.SelectedWard = ward;
            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(scriptPrescriptions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPrescription(int id, string? notes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var scriptManager = await _context.ScriptManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm => sm.Employee.UserId == user.Id);

            if (scriptManager == null) return Unauthorized();

            var existingScriptPrescription = await _context.ScriptPrescription
                .Include(sp => sp.Medications)
                    .ThenInclude(spm => spm.Medication)
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Patient)
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Medication)
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Doctor)
                .FirstOrDefaultAsync(sp => sp.ScriptPrescriptionId == id && !sp.IsDeleted);

            ScriptPrescription scriptPrescriptionToProcess;
            Prescription originalPrescription;

            if (existingScriptPrescription != null)
            {
                scriptPrescriptionToProcess = existingScriptPrescription;
                originalPrescription = scriptPrescriptionToProcess.Prescription;

                if (scriptPrescriptionToProcess.Status == "Completed" || scriptPrescriptionToProcess.Status == "Processed")
                {
                    TempData["Error"] = "This prescription has already been processed.";
                    return RedirectToAction("ManagePrescriptions");
                }
                if (scriptPrescriptionToProcess.Status != "Pending")
                {
                    TempData["Error"] = $"Cannot process prescription with status: {scriptPrescriptionToProcess.Status}";
                    return RedirectToAction("ManagePrescriptions");
                }
            }
            else
            {
                originalPrescription = await _context.Prescription
                    .Include(p => p.Patient)
                    .Include(p => p.Medication)
                    .Include(p => p.Doctor)
                    .FirstOrDefaultAsync(p => p.PrescriptionId == id);

                if (originalPrescription == null) return NotFound("Original prescription not found.");

                if (originalPrescription.Status == "Processed" || originalPrescription.Status == "Completed")
                {
                    TempData["Error"] = "This prescription has already been processed.";
                    return RedirectToAction("ManagePrescriptions");
                }
                if (originalPrescription.Status != "Pending")
                {
                    TempData["Error"] = $"Cannot process prescription with status: {originalPrescription.Status}";
                    return RedirectToAction("ManagePrescriptions");
                }

                scriptPrescriptionToProcess = new ScriptPrescription
                {
                    PrescriptionId = id,
                    Status = "Pending",
                    ScriptManagerId = scriptManager.ScriptManagerId
                };

                _context.ScriptPrescription.Add(scriptPrescriptionToProcess);
                await _context.SaveChangesAsync(); 
            }

            if (originalPrescription.Status == "Processed" || originalPrescription.Status == "Completed")
            {
                TempData["Error"] = "This prescription has already been processed.";
                return RedirectToAction("ManagePrescriptions");
            }

            scriptPrescriptionToProcess.ScriptManagerId = scriptManager.ScriptManagerId;
            scriptPrescriptionToProcess.Status = "Processed";
            scriptPrescriptionToProcess.ProcessedDate = DateTime.Now;
            scriptPrescriptionToProcess.AssignedDate = DateTime.Now;

            if (!string.IsNullOrEmpty(notes))
            {
                if (!string.IsNullOrEmpty(scriptPrescriptionToProcess.Notes))
                {
                    scriptPrescriptionToProcess.Notes += $"\n{DateTime.Now:yyyy-MM-dd HH:mm}: {notes}";
                }
                else
                {
                    scriptPrescriptionToProcess.Notes = notes;
                }
            }

            originalPrescription.ScriptManagerId = scriptManager.ScriptManagerId;
            originalPrescription.Status = "Processed";
            _context.Prescription.Update(originalPrescription);

            var spm = new ScriptPrescriptionMedication
            {
                ScriptPrescriptionId = scriptPrescriptionToProcess.ScriptPrescriptionId,
                MedicationId = originalPrescription.MedicationId,
                Quantity = 1,
                QuantityReceived = 0
            };
            _context.ScriptPrescriptionMedication.Add(spm);

            await _context.SaveChangesAsync();

            await LogAuditAsync(
                "Prescription Processed",
                user,
                entity: "ScriptPrescription",
                recordId: scriptPrescriptionToProcess.ScriptPrescriptionId.ToString(),
                details: $"Prescription for patient {originalPrescription.Patient.FirstName} {originalPrescription.Patient.LastName} processed by {user.FirstName} {user.LastName}. Medication: {originalPrescription.Medication.MedicationName}"
            );

            TempData["Success"] = "Prescription processed and forwarded to pharmacy.";
            return RedirectToAction("ManagePrescriptions");
        }

        [HttpGet]
        public async Task<IActionResult> ReceivePrescription(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var scriptPrescription = await _context.ScriptPrescription
                .Include(sp => sp.Medications)
                    .ThenInclude(spm => spm.Medication)

                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Patient)
                        .ThenInclude(p => p.Room)
                            .ThenInclude(r => r.Ward)

               
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Doctor)

    
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Medication)

                .FirstOrDefaultAsync(
                    sp => sp.ScriptPrescriptionId == id && !sp.IsDeleted
                );

            if (scriptPrescription == null)
            {
                return NotFound("Prescription not found.");
            }
            if (scriptPrescription.Status != "Processed")
            {
                TempData["Error"] =
                    "Prescription must be processed before receiving.";

                return RedirectToAction("ManagePrescriptions");
            }

            return View(scriptPrescription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceivePrescription( int id, string? notes, string action)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var scriptPrescription = await _context.ScriptPrescription
                .Include(sp => sp.Medications)
                    .ThenInclude(spm => spm.Medication)
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Patient)
                .FirstOrDefaultAsync(
                    sp => sp.ScriptPrescriptionId == id && !sp.IsDeleted
                );

            if (scriptPrescription == null)
            {
                return NotFound("Prescription not found.");
            }

            if (scriptPrescription.Prescription?.Patient == null)
            {
                TempData["Error"] =
                    "Prescription data is incomplete for audit logging.";

                return RedirectToAction("ManagePrescriptions");
            }

            var scriptManager = await _context.ScriptManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(
                    sm => sm.Employee.UserId == user.Id
                );

            if (scriptManager == null)
            {
                return Unauthorized();
            }

            switch (action?.ToLower())
            {
                case "verify":

                    scriptPrescription.IsVerified = true;
                    scriptPrescription.VerifiedBy =
                        $"{user.FirstName} {user.LastName}";
                    scriptPrescription.VerifiedDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(notes))
                    {
                        if (!string.IsNullOrEmpty(scriptPrescription.Notes))
                        {
                            scriptPrescription.Notes +=
                                $"\n{DateTime.Now:yyyy-MM-dd HH:mm}: Verification: {notes}";
                        }
                        else
                        {
                            scriptPrescription.Notes =
                                $"Verification: {notes}";
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        "Prescription verified successfully.";

                    break;


                case "receive":
                    if (scriptPrescription.Status == "Completed")
                    {
                        TempData["Error"] =
                            "This prescription has already been received.";

                        return RedirectToAction("ManagePrescriptions");
                    }
                    var originalPrescription = await _context.Prescription
                        .FirstOrDefaultAsync(
                            p => p.PrescriptionId ==
                                 scriptPrescription.PrescriptionId
                        );

                    if (originalPrescription == null)
                    {
                        TempData["Error"] =
                            "Original prescription could not be found.";

                        return RedirectToAction("ManagePrescriptions");
                    }
                    if (originalPrescription.Quantity < 1)
                    {
                        TempData["Error"] =
                            "The prescription has an invalid medication quantity.";

                        return RedirectToAction("ManagePrescriptions");
                    }
                    foreach (var spm in scriptPrescription.Medications)
                    {
                        if (spm.Medication == null)
                        {
                            TempData["Error"] =
                                "Medication information could not be found.";

                            return RedirectToAction("ManagePrescriptions");
                        }
                        if (spm.MedicationId == originalPrescription.MedicationId)
                        {
                            spm.Quantity = originalPrescription.Quantity;
                        }
                        if (spm.Quantity < 1)
                        {
                            TempData["Error"] =
                                $"Invalid quantity for {spm.Medication.MedicationName}.";

                            return RedirectToAction("ManagePrescriptions");
                        }
                        var availableStock =
                            spm.Medication.QuantityOnHand ?? 0;

                        if (availableStock < spm.Quantity)
                        {
                            TempData["Error"] =
                                $"Not enough {spm.Medication.MedicationName} in stock. " +
                                $"Available: {availableStock}, " +
                                $"Required: {spm.Quantity}.";

                            return RedirectToAction("ManagePrescriptions");
                        }
                    }

                    scriptPrescription.Status = "Completed";
                    scriptPrescription.ReceivedDate = DateTime.Now;
                    scriptPrescription.ReceivedFromId = user.Id;

                    if (!string.IsNullOrEmpty(notes))
                    {
                        if (!string.IsNullOrEmpty(scriptPrescription.Notes))
                        {
                            scriptPrescription.Notes +=
                                $"\n{DateTime.Now:yyyy-MM-dd HH:mm}: Received: {notes}";
                        }
                        else
                        {
                            scriptPrescription.Notes =
                                $"Received: {notes}";
                        }
                    }
                    originalPrescription.Status = "Completed";
                    foreach (var spm in scriptPrescription.Medications)
                    {
                        spm.QuantityReceived = spm.Quantity;

                        spm.Medication.QuantityOnHand =
                            (spm.Medication.QuantityOnHand ?? 0)
                            - spm.Quantity;
                    }


                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        "Prescription received and completed successfully.";

                    break;


                default:

                    TempData["Error"] =
                        "Invalid action specified.";

                    break;
            }
            await LogAuditAsync(
                $"Prescription {action?.ToUpper()}",
                user,
                entity: "ScriptPrescription",
                recordId: scriptPrescription.ScriptPrescriptionId.ToString(),
                details:
                    $"Prescription for patient " +
                    $"{scriptPrescription.Prescription.Patient.FirstName} " +
                    $"{scriptPrescription.Prescription.Patient.LastName} " +
                    $"{action?.ToLower()} by " +
                    $"{user.FirstName} {user.LastName}."
            );

            return RedirectToAction("ManagePrescriptions");
        }

        [HttpGet]
        public async Task<IActionResult> PrescriptionDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var scriptManager = await _context.ScriptManager
                .FirstOrDefaultAsync(sm => sm.Employee.UserId == user.Id);
            if (scriptManager == null) return Unauthorized();

            var scriptPrescription = await _context.ScriptPrescription
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Patient)
                        .ThenInclude(pa => pa.Room)
                            .ThenInclude(r => r.Ward)
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Patient)
                        .ThenInclude(pa => pa.Bed)
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Doctor)
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Medication)
                .Include(sp => sp.Medications)
                    .ThenInclude(spm => spm.Medication)
                .Include(sp => sp.ScriptManager)
                    .ThenInclude(sm => sm.Employee)
                .Include(sp => sp.ReceivedFrom)
                .FirstOrDefaultAsync(sp => sp.ScriptPrescriptionId == id && !sp.IsDeleted);

            if (scriptPrescription == null) return NotFound("Prescription not found.");

            return View(scriptPrescription);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPrescription(int id, string? notes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var scriptPrescription = await _context.ScriptPrescription
                .Include(sp => sp.ScriptManager)
                .Include(sp => sp.Medications)
                    .ThenInclude(spm => spm.Medication) 
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Patient)
                .FirstOrDefaultAsync(sp => sp.ScriptPrescriptionId == id && !sp.IsDeleted);

            if (scriptPrescription == null)
                return NotFound("Prescription not found.");

            if (scriptPrescription.IsVerified)
            {
                TempData["Error"] = "This prescription has already been verified.";
                return RedirectToAction("ReceivePrescription", new { id = scriptPrescription.ScriptPrescriptionId });
            }

            if (scriptPrescription.Status != "Processed")
            {
                TempData["Error"] = "Prescription must be processed before verification.";
                return RedirectToAction("ReceivePrescription", new { id = scriptPrescription.ScriptPrescriptionId });
            }


            scriptPrescription.IsVerified = true;
            scriptPrescription.VerifiedDate = DateTime.Now;
            scriptPrescription.VerifiedBy = $"{user.FirstName} {user.LastName}";

            if (!string.IsNullOrEmpty(notes))
            {
                if (!string.IsNullOrEmpty(scriptPrescription.Notes))
                {
                    scriptPrescription.Notes += $"\n{DateTime.Now:yyyy-MM-dd HH:mm}: Verification: {notes}";
                }
                else
                {
                    scriptPrescription.Notes = $"Verification: {notes}";
                }
            }

            _context.ScriptPrescription.Update(scriptPrescription);

            await _context.SaveChangesAsync();

            await LogAuditAsync(
                "Prescription Verified",
                user,
                entity: "ScriptPrescription",
                recordId: scriptPrescription.ScriptPrescriptionId.ToString(),
                details: $"Prescription for patient {scriptPrescription.Prescription.Patient.FirstName} {scriptPrescription.Prescription.Patient.LastName} verified by {user.FirstName} {user.LastName}. Medications verified."
            );

            TempData["Success"] = "Prescription verified successfully.";
            return RedirectToAction("ReceivePrescription", new { id = scriptPrescription.PrescriptionId });
        }














        private async Task<List<Medication>> GetLowStockMedicationsAsync(int threshold = 10, int limit = 5)
        {
            return await _context.Medication
                .Where(m => !m.IsDeleted && m.QuantityOnHand.HasValue && m.QuantityOnHand.Value <= threshold)
                .OrderBy(m => m.QuantityOnHand)
                .Take(limit)
                .ToListAsync();
        }

        private async Task<List<ScriptPrescription>> GetExpiringPrescriptionsAsync(int daysBeforeExpiry = 3)
        {
            var today = DateTime.Today;

            return await _context.ScriptPrescription
                .Include(sp => sp.Prescription)
                    .ThenInclude(p => p.Patient)
                .Include(sp => sp.Medications)
                    .ThenInclude(spm => spm.Medication)
                .Where(sp => !sp.IsDeleted &&
                             sp.Medications.Any(m => m.Medication.ExpiryDate.HasValue &&
                                                     m.Medication.ExpiryDate.Value.Date <= today.AddDays(daysBeforeExpiry)))
                .OrderBy(sp => sp.Medications.Min(m => m.Medication.ExpiryDate)) 
                .ToListAsync();
        }








        [HttpGet]
        public async Task<IActionResult> ViewAlerts()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var alerts = await _context.Alerts
                .Where(a =>
                    a.IsActive &&
                    (
                        a.UserId == user.Id ||
                        (a.UserId == null && a.TargetRole == user.Role) ||
                        (a.UserId == null && a.TargetRole == null)
                    )
                    &&
                    !_context.AlertReads.Any(r =>
                        r.AlertId == a.AlertId &&
                        r.UserId == user.Id)
                )
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            var alertData = alerts.Select(a => new { a.AlertId, a.Message }).ToList();

            await LogAuditAsync(
                actionTaken: "Viewed Alerts",
                user: user,
                entity: "Alert",
                recordId: "N/A",
                oldValue: "{}",
                newValue: System.Text.Json.JsonSerializer.Serialize(alertData),
                details: $"User {user.FirstName} {user.LastName} viewed {alerts.Count} active alerts."
            );

            return View(alerts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAlertRead(int alertId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var alert = await _context.Alerts.FindAsync(alertId);
            if (alert == null)
                return NotFound();

            bool canAccess =
                alert.UserId == user.Id ||
                (alert.UserId == null && alert.TargetRole == user.Role) ||
                (alert.UserId == null && alert.TargetRole == null);

            if (!canAccess)
                return Forbid();

            var alreadyRead = await _context.AlertReads
                .FirstOrDefaultAsync(a =>
                    a.AlertId == alertId &&
                    a.UserId == user.Id);

            if (alreadyRead == null)
            {
                var alertRead = new AlertRead
                {
                    AlertId = alertId,
                    UserId = user.Id,
                    IsRead = true,
                    ReadAt = DateTime.Now
                };

                _context.AlertReads.Add(alertRead);
                await _context.SaveChangesAsync();

                await LogAuditAsync(
                    actionTaken: "Marked Alert Read",
                    user: user,
                    entity: "Alert",
                    recordId: alert.AlertId.ToString(),
                    oldValue: "Unread",
                    newValue: "Read",
                    details: $"User {user.FirstName} {user.LastName} marked alert '{alert.Message}' as read."
                );

                TempData["Success"] = "Alert marked as read.";
            }
            else
            {
                TempData["Info"] = "This alert has already been marked as read.";
            }

            return RedirectToAction(nameof(ViewAlerts));
        }


    }
}