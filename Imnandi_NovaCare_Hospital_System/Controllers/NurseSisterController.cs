using Imnandi_NovaCare_Hospital_System.Data;
using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Imnandi_NovaCare_Hospital_System.Controllers
{
    [Authorize(Roles = "NurseSister")]
    public class NurseSisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public NurseSisterController(
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

        public async Task<IActionResult> NurseSisterDashBoard()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var nurseSister = await _context.NurseSister
                .FirstOrDefaultAsync(n => n.Employee.UserId == currentUser.Id);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var totalPatients = await _context.Patient.CountAsync(p => !p.IsDeleted);
            var totalDoctors = await _context.Doctor.CountAsync();
            var totalNurseSisters = await _context.NurseSister.CountAsync();
            var wards = await _context.Ward.Where(n => n.NurseSisterId == nurseSister.NurseSisterId).Include(n => n.Rooms).ToListAsync();
            var assignedWardIds = await _context.Ward.Where(w => w.NurseSisterId == nurseSister.NurseSisterId && !w.IsDeleted).Select(w => w.WardId).ToListAsync();
            var totalNurses = await _context.Nurse.CountAsync(n =>!n.IsDeleted && n.WardId.HasValue && assignedWardIds.Contains(n.WardId.Value));
            var totalEmployees = await _context.Employee.CountAsync(e => !e.IsDeleted);
            var totalWards = await _context.Ward.CountAsync(w =>w.NurseSisterId == nurseSister.NurseSisterId && !w.IsDeleted);
            var totalRooms = await _context.Room.CountAsync();
            var bedsAvailable = await _context.Bed.CountAsync(b => !b.IsOccupied);
            var patientsInHospital = await _context.Patient.CountAsync(p => !p.IsDeleted && !p.IsDischarged);

            var recentRegistrations = await _context.Patient
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.DateAdmitted)
                .Take(5)
                .Select(p => $"{p.FirstName} {p.LastName}")
                .ToListAsync();
            var medicationSchedule = await _context.MedicationAdministration
                .Where(p => p.Prescription.Level > 4)
                .Where(p => p.IsSeen == false)
                .Include(p => p.Patient)
                .Include(p => p.Medication)
                .Include(p => p.Prescription)
                .ToListAsync();

            var alerts = new List<string>();

            var rooms = await _context.Room
                .Include(r => r.Beds)
                .ToListAsync();

            var nurses = await _context.Nurse
                .Where(n =>
                    n.WardId.HasValue &&
                    assignedWardIds.Contains(n.WardId.Value))
                .Include(p => p.Patients)
                .Include(w => w.ward)
                .ToListAsync();

            var model = new NurseSisterDashBoardViewModel
            {
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                UserId = currentUser.Id,
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                NurseSisterId = nurseSister.NurseSisterId,
                TotalPatients = totalPatients,
                UpcomingSchedule = medicationSchedule,
                PatientsInHospital = patientsInHospital,
                TotalDoctors = totalDoctors,
                TotalNurseSisters = totalNurseSisters,
                TotalNurses = totalNurses,
                Wards = wards,
                Nurses = nurses,
                TotalEmployees = totalEmployees,
                TotalWards = totalWards,
                BedsAvailable = bedsAvailable,
                RecentRegistrations = recentRegistrations,
            };

            return View(model);
        }






        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");
            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == user.Id);
            var model = new NurseSisterDashBoardViewModel
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
        public async Task<IActionResult> Profile(NurseSisterDashBoardViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

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
            var nursesister = await _context.NurseSister
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id);

            if (nursesister != null)
            {
                nursesister.FirstName = model.FirstName;
                nursesister.LastName = model.LastName;

                _context.NurseSister.Update(nursesister);
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
                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

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
                return RedirectToAction("NurseSisterDashBoard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }








        public async Task<IActionResult> ManageNurses()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == currentUser.Id);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee record not found.";
                return RedirectToAction("NurseSisterDashBoard");
            }

            var nurseSister = await _context.NurseSister
                .FirstOrDefaultAsync(ns => ns.EmployeeId == employee.Id);

            if (nurseSister == null)
            {
                TempData["ErrorMessage"] = "Nurse Sister record not found.";
                return RedirectToAction("NurseSisterDashBoard");
            }

            var assignedWardIds = await _context.Ward
                .Where(w =>
                    w.NurseSisterId == nurseSister.NurseSisterId &&
                    !w.IsDeleted)
                .Select(w => w.WardId)
                .ToListAsync();

            var nurses = await _context.Nurse
                .Where(n =>
                    !n.IsDeleted &&
                    n.WardId.HasValue &&
                    assignedWardIds.Contains(n.WardId.Value))
                .Include(n => n.Patients)
                .ToListAsync();

            return View(nurses);
        }


        public async Task<IActionResult> ManageSingleNurse(int nurseId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == currentUser.Id);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Employee record not found.";
                return RedirectToAction(nameof(ManageNurses));
            }

            var nurseSister = await _context.NurseSister
                .FirstOrDefaultAsync(ns => ns.EmployeeId == employee.Id);

            if (nurseSister == null)
            {
                TempData["ErrorMessage"] = "Nurse Sister record not found.";
                return RedirectToAction(nameof(ManageNurses));
            }

            var nurse = await _context.Nurse
                .Include(n => n.Employee)
                .Include(n => n.Patients)
                    .ThenInclude(p => p.Bed)
                        .ThenInclude(b => b.Room)
                            .ThenInclude(r => r.Ward)
                .Include(n => n.PerformanceNotes)
                .Include(n => n.Incidents)
                .Include(n => n.Alerts)
                .FirstOrDefaultAsync(n =>
                    n.NurseId == nurseId &&
                    !n.IsDeleted &&
                    n.WardId.HasValue &&
                    _context.Ward.Any(w =>
                        w.WardId == n.WardId.Value &&
                        w.NurseSisterId == nurseSister.NurseSisterId &&
                        !w.IsDeleted
                    )
                );

            if (nurse == null)
            {
                TempData["ErrorMessage"] =
                    "You are not authorized to manage this nurse.";

                return RedirectToAction(nameof(ManageNurses));
            }

            return View(nurse);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPerformanceNote(int nurseId, string note)
        {
            var nurse = await _context.Nurse
                .FirstOrDefaultAsync(n => n.NurseId == nurseId);

            if (nurse == null)
            {
                TempData["ErrorMessage"] = "Nurse not found.";
                return RedirectToAction(nameof(ManageNurses));
            }

            var performanceNote = new NursePerformanceNote
            {
                NurseId = nurseId,
                Note = note,
                CreatedDate = DateTime.Now
            };

            _context.NursePerformanceNotes.Add(performanceNote);
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);

            await LogAuditAsync(
                actionTaken: "Added Nurse Performance Note",
                user: currentUser,
                entity: "NursePerformanceNote",
                recordId: performanceNote.Id.ToString(),
                newValue: note,
                details: $"Performance note added for Nurse {nurse.FirstName} {nurse.LastName}."
            );

            TempData["SuccessMessage"] = "Performance note added successfully.";

            return RedirectToAction(nameof(ManageSingleNurse), new { nurseId = nurseId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportIncident(int nurseId, string description)
        {
            var nurse = await _context.Nurse
                .FirstOrDefaultAsync(n => n.NurseId == nurseId);

            if (nurse == null)
            {
                TempData["ErrorMessage"] = "Nurse not found.";
                return RedirectToAction(nameof(ManageNurses));
            }

            var incident = new NurseIncident
            {
                NurseId = nurseId,
                Description = description
            };

            _context.NurseIncidents.Add(incident);
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);

            await LogAuditAsync(
                actionTaken: "Reported Nurse Incident",
                user: currentUser,
                entity: "NurseIncident",
                recordId: incident.Id.ToString(),
                newValue: description,
                details: $"Incident reported for Nurse {nurse.FirstName} {nurse.LastName}."
            );

            TempData["SuccessMessage"] = "Incident recorded successfully.";

            return RedirectToAction(nameof(ManageSingleNurse), new { nurseId = nurseId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAlert(int nurseId, string subject, string message)
        {
            var nurse = await _context.Nurse
                .FirstOrDefaultAsync(n => n.NurseId == nurseId);

            if (nurse == null)
            {
                TempData["ErrorMessage"] = "Nurse not found.";
                return RedirectToAction(nameof(ManageNurses));
            }

            var alert = new NurseAlert
            {
                NurseId = nurseId,
                Subject = subject,
                Message = message
            };

            _context.NurseAlerts.Add(alert);
            await _context.SaveChangesAsync();

            var currentUser = await _userManager.GetUserAsync(User);

            await LogAuditAsync(
                actionTaken: "Sent Nurse Alert",
                user: currentUser,
                entity: "NurseAlert",
                recordId: alert.Id.ToString(),
                newValue: $"Subject: {subject}, Message: {message}",
                details: $"Alert '{subject}' sent to Nurse {nurse.FirstName} {nurse.LastName}."
            );

            TempData["SuccessMessage"] = "Alert sent successfully.";

            return RedirectToAction(nameof(ManageSingleNurse), new { nurseId = nurseId });
        }



        public async Task<IActionResult> NursePatients(int nurseId)
        {
            var patients =await _context.Patient
                .Where(p => p.NurseId == nurseId)
                .Where(p => !p.IsDischarged)
                .Include(n=>n.Nurse)
                .ToListAsync();
            return View(patients);

        }


        public async Task<IActionResult> ViewDoctorInstruction(int patientId)
        {
            var instrucions = await _context.Instruction.Where(d => d.PatientId == patientId )
                .Include(d=>d.Doctor)
                .Include(i => i.Patient.Room)
                .Include(i => i.Patient.Room.Ward)
                .Include(p=>p.Patient).ToListAsync();
            return View(instrucions);
        }













        public async Task<IActionResult> ViewMedicationAdmistration()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var nurseSister = await _context.NurseSister
                .FirstOrDefaultAsync(n => n.Employee.UserId == currentUser.Id);

            if (nurseSister == null)
            {
                TempData["ErrorMessage"] = "Nurse Sister record not found.";
                return RedirectToAction(nameof(NurseSisterDashBoard));
            }

            var assignedWardIds = await _context.Ward
                .Where(w =>
                    w.NurseSisterId == nurseSister.NurseSisterId &&
                    !w.IsDeleted)
                .Select(w => w.WardId)
                .ToListAsync();

            var medication = await _context.MedicationAdministration
                .Where(d =>
                    d.Prescription.Level >= 5 &&
                    !d.IsSeen &&
                    d.Patient.Room != null &&
                    d.Patient.Room.Ward != null &&
                    assignedWardIds.Contains(d.Patient.Room.Ward.WardId))
                .Include(m => m.Medication)
                .Include(p => p.Prescription)
                .Include(p => p.Patient)
                    .ThenInclude(p => p.Room)
                        .ThenInclude(r => r.Ward)
                .Include(p => p.Patient)
                    .ThenInclude(p => p.Bed)
                .Include(p => p.Nurse)
                .ToListAsync();

            return View(medication);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeenMedicationSchedule(int MedicationAdministrationId)
        {
            if (MedicationAdministrationId == 0)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var nurseSister = await _context.NurseSister
                .FirstOrDefaultAsync(n => n.Employee.UserId == currentUser.Id);

            if (nurseSister == null)
            {
                TempData["ErrorMessage"] = "Nurse Sister record not found.";
                return RedirectToAction(nameof(NurseSisterDashBoard));
            }

            var medSchedule = await _context.MedicationAdministration
                .Include(m => m.Patient)
                    .ThenInclude(p => p.Room)
                        .ThenInclude(r => r.Ward)
                .FirstOrDefaultAsync(m =>
                    m.MedicationAdministrationId == MedicationAdministrationId &&
                    !m.IsSeen &&
                    m.Patient.Room != null &&
                    m.Patient.Room.Ward != null &&
                    m.Patient.Room.Ward.NurseSisterId == nurseSister.NurseSisterId &&
                    !m.Patient.Room.Ward.IsDeleted);

            if (medSchedule == null)
            {
                TempData["ErrorMessage"] =
                    "Medication schedule not found or you are not authorized to manage it.";

                return RedirectToAction(nameof(ViewMedicationAdmistration));
            }

            medSchedule.IsSeen = true;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medication schedule marked as seen.";

            return RedirectToAction(nameof(ViewMedicationAdmistration));
        }


        public async Task<IActionResult> ViewVitalSigns(int patientId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var nurseSister = await _context.NurseSister
                .FirstOrDefaultAsync(n => n.Employee.UserId == currentUser.Id);

            if (nurseSister == null)
            {
                TempData["ErrorMessage"] = "Nurse Sister record not found.";
                return RedirectToAction(nameof(NurseSisterDashBoard));
            }

            var patientBelongsToAssignedWard = await _context.Patient
                .AnyAsync(p =>
                    p.Id == patientId &&
                    p.Room != null &&
                    p.Room.Ward != null &&
                    p.Room.Ward.NurseSisterId == nurseSister.NurseSisterId &&
                    !p.Room.Ward.IsDeleted);

            if (!patientBelongsToAssignedWard)
            {
                TempData["ErrorMessage"] =
                    "You are not authorized to view this patient's vital signs.";

                return RedirectToAction(nameof(NurseSisterDashBoard));
            }

            var signs = await _context.VitalSign
                .Where(p => p.PatientId == patientId)
                .Include(p => p.Patient)
                .Include(n => n.Nurse)
                .ToListAsync();

            return View(signs);
        }


        public async Task<IActionResult> ViewNurseMedicationHistory(
            int nurseId,
            string? search)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var nurseSister = await _context.NurseSister
                .FirstOrDefaultAsync(ns => ns.Employee.UserId == currentUser.Id);

            if (nurseSister == null)
            {
                return Unauthorized();
            }

            var nurse = await _context.Nurse
                .Include(n => n.ward)
                .FirstOrDefaultAsync(n =>
                    n.NurseId == nurseId &&
                    !n.IsDeleted);

            if (nurse == null)
            {
                TempData["ErrorMessage"] = "Nurse not found.";
                return RedirectToAction(nameof(ManageNurses));
            }

            if (!nurse.WardId.HasValue)
            {
                TempData["ErrorMessage"] =
                    "This nurse is not currently assigned to a ward.";

                return RedirectToAction(nameof(ManageNurses));
            }

            var isResponsibleForWard = await _context.Ward
                .AnyAsync(w =>
                    w.WardId == nurse.WardId.Value &&
                    w.NurseSisterId == nurseSister.NurseSisterId &&
                    !w.IsDeleted);

            if (!isResponsibleForWard)
            {
                return Forbid();
            }

            var query = _context.MedicationAdministration
                .Where(m =>
                    m.NurseId == nurseId &&
                    m.Patient != null)
                .Include(m => m.Medication)
                .Include(m => m.Patient)
                    .ThenInclude(p => p.Room)
                        .ThenInclude(r => r.Ward)
                .Include(m => m.Patient)
                    .ThenInclude(p => p.Bed)
                .Include(m => m.Prescription)
                .Include(m => m.Doctor)
                .OrderByDescending(m => m.AdministrationTime)
                .AsQueryable();

           
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(m =>
                    (m.Patient.FirstName + " " + m.Patient.LastName)
                        .Contains(search) ||

                    m.Medication.MedicationName
                        .Contains(search) ||

                    m.Patient.Room.RoomName
                        .Contains(search) ||

                    m.Patient.Bed.BedNumber
                        .Contains(search) ||

                    m.Dosage
                        .Contains(search));
            }

            var medicationHistory = await query.ToListAsync();

            ViewBag.NurseName =
                $"{nurse.FirstName} {nurse.LastName}";

            ViewBag.NurseId = nurse.NurseId;

            ViewBag.WardName =
                nurse.ward?.WardName ?? "No Ward Assigned";

            ViewBag.Search = search;

            return View(medicationHistory);
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

            var alertData = alerts.Select(a => new
            {
                a.AlertId,
                a.Message
            }).ToList();

            await LogAuditAsync(
                actionTaken: "Viewed Alerts",
                user: user,
                entity: "Alert",
                recordId: "N/A",
                oldValue: "{}",
                newValue: System.Text.Json.JsonSerializer.Serialize(alertData),
                details: $"Nurse Sister {user.FirstName} {user.LastName} viewed {alerts.Count} active alerts."
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
            {
                return Forbid();
            }

            var alreadyRead = await _context.AlertReads
                .FirstOrDefaultAsync(r =>
                    r.AlertId == alertId &&
                    r.UserId == user.Id);

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
                    details: $"Nurse Sister {user.FirstName} {user.LastName} marked alert '{alert.Message}' as read."
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