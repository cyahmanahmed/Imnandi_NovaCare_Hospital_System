using Imnandi_NovaCare_Hospital_System.Data;
using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Imnandi_NovaCare_Hospital_System.Controllers
{
    [Authorize(Roles = "Nurse")]
    public class NurseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public NurseController(
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

        public async Task<IActionResult> NurseDashBoard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == currentUser.Id);
            var nurse = await _context.Nurse.Where(emp => emp.EmployeeId == employee.Id).FirstOrDefaultAsync();
            var totalPatients = await _context.Patient.CountAsync(p => p.NurseId == nurse.NurseId && !p.IsDeleted && !p.IsDischarged);
            var totalDoctors = await _context.Doctor.CountAsync();
            var totalNurseSisters = await _context.NurseSister.CountAsync();
            var totalNurses = await _context.Nurse.CountAsync();
            var totalEmployees = await _context.Employee.CountAsync(e => !e.IsDeleted);
            var totalWards = await _context.Ward.CountAsync();
            var totalRooms = await _context.Room.CountAsync();
            var bedsAvailable = await _context.Bed.CountAsync(b => !b.IsOccupied);
            var patientsInHospital = await _context.Patient.CountAsync(p => !p.IsDeleted && !p.IsDischarged);
            var recentRegistrations = await _context.Patient
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.DateAdmitted)
                .Take(5)
                .Select(p => $"{p.FirstName} {p.LastName}")
                .ToListAsync();

            var vitalSigns = await _context.VitalSign
                .Include(v => v.Patient).Include(w => w.Patient.Room.Ward).Include(r => r.Patient.Room).Include(b => b.Patient.Bed)
                .GroupBy(v => v.PatientId)
                .Select(g => g.OrderByDescending(v => v.VitalSignId).FirstOrDefault())
                .ToListAsync();
            var medicationSchedule = await _context.MedicationAdministration.Where(p => p.Prescription.Level <= 4).Where(p => p.IsSeen == false).Include(p => p.Patient).Include(p => p.Medication).Include(p => p.Prescription).ToListAsync();

            var alerts = new List<string>();
            var rooms = await _context.Room.Include(r => r.Beds).ToListAsync();
            var patients = await _context.Patient
                .Where(p =>
                    p.NurseId == nurse.NurseId &&
                    !p.IsDeleted &&
                    !p.IsDischarged)
                .Include(p => p.Room)
                    .ThenInclude(r => r.Ward)
                .Include(p => p.Bed)
                .ToListAsync();

            var model = new NurseDashBoardViewModel
            {
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                UserId = currentUser.Id,
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                PhoneNumber = currentUser.Employee.PhoneNumber,
                Department = currentUser.Employee.Department,
                JobTitle = currentUser.Employee.JobTitle,
                UpcomingSchedule = medicationSchedule,
                LatestVitals = vitalSigns,
                NurseId = currentUser.Employee.Nurse.NurseId,
                TotalPatients = totalPatients,
                PatientsInHospital = patientsInHospital,
                TotalDoctors = totalDoctors,
                TotalNurseSisters = totalNurseSisters,
                TotalNurses = totalNurses,
                TotalEmployees = totalEmployees,
                TotalWards = totalWards,
                BedsAvailable = bedsAvailable,
                RecentRegistrations = recentRegistrations,
                Patient = patients
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
            var model = new NurseDashBoardViewModel
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
        public async Task<IActionResult> Profile(NurseDashBoardViewModel model)
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

            var nurse = await _context.Nurse
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id);

            if (nurse != null)
            {
                nurse.FirstName = model.FirstName;
                nurse.LastName = model.LastName;

                _context.Nurse.Update(nurse);
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
                return RedirectToAction("NurseDashBoard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }




        public async Task<IActionResult> Wards()
        {

            var ward = await _context.Ward
                .Include(r => r.Rooms)
                .Include(s => s.NurseSister)
                .ToListAsync();
            return View(ward);
        }

        public async Task<IActionResult> Rooms(int wardId)
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
                return NotFound();
            }

            var nurse = await _context.Nurse
                .FirstOrDefaultAsync(n => n.EmployeeId == employee.Id);

            if (nurse == null)
            {
                return NotFound();
            }

            var rooms = await _context.Room
                .Where(r => r.WardId == wardId)
                .Where(r => _context.Patient.Any(p =>
                    p.RoomId == r.RoomId &&
                    p.NurseId == nurse.NurseId &&
                    !p.IsDeleted &&
                    !p.IsDischarged))
                .Include(r => r.Ward)
                .Include(r => r.Beds)
                .ToListAsync();

            return View(rooms);
        }

        public async Task<IActionResult> Patients(int roomId)
        {
            var bedId = _context.Bed
                .Where(b => b.RoomId == roomId)
                .FirstOrDefault();
            var patients = await _context.Patient
                .Where(p => p.BedId == bedId.BedId)
                .Where(p => !p.IsDischarged)
                .Include(p => p.Bed)
                .Include(w => w.Bed.Room.Ward)
                .ToListAsync();

            return View(patients);
        }

        public async Task<IActionResult> PatientFile(int patientId)
        {
            var patient = await _context.Patient
                .Where(p => p.Id == patientId)
                .Include(p => p.Bed).Include(w => w.Bed.Room.Ward)
                .Include(r => r.Room).Where(p => !p.IsDischarged)
                .Include(d => d.Doctor)
                .FirstOrDefaultAsync();
            return View(patient);
        }

        public async Task<IActionResult> ViewHistoryVist(int patientId)
        {
            var historyVist = await _context.Visit
                .Where(p => p.PatientId == patientId)
                .Include(p => p.Patient)
                .Include(d => d.Doctor)
                .ToListAsync();
            return View(historyVist);
        }

        public async Task<IActionResult> RecordVitalSign(int patientId, int nurseId)
        {
            ViewBag.patient = new SelectList(await _context.Patient
                .Where(p => p.Id == patientId).Where(p => !p.IsDischarged)
                .ToListAsync(), "Id", "LastName");
            ViewBag.nurse = new SelectList(await _context.Nurse
                .Where(p => p.NurseId == nurseId)
                .ToListAsync(), "NurseId", "LastName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecordVitalSign(int patientId, VitalSign vital)
        {
            if (vital == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == currentUser.Id);

            var nurse = await _context.Nurse
                .FirstOrDefaultAsync(n => n.EmployeeId == employee.Id);

            vital.PatientId = patientId;
            vital.NurseId = nurse.NurseId;
            vital.RecordedAt = DateTime.Now;

            _context.VitalSign.Add(vital);
            await _context.SaveChangesAsync();

            var patient = await _context.Patient.FindAsync(patientId);

            await LogAuditAsync(
                actionTaken: "Record Vital Signs",
                user: currentUser,
                entity: "VitalSign",
                recordId: vital.VitalSignId.ToString(),
                details: $"Nurse {nurse.FirstName} {nurse.LastName} recorded vital signs for patient {patient?.FirstName} {patient?.LastName}."
            );

            return RedirectToAction("NurseDashBoard");
        }





        public async Task<IActionResult> Instruction()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == currentUser.Id);
            var nurse = await _context.Nurse.Where(emp => emp.EmployeeId == employee.Id)
                .FirstOrDefaultAsync();


            var instructions = await _context.Instruction
                .Include(d => d.Doctor).Include(p => p.Patient)
                .Where(p => p.Patient.NurseId == nurse.NurseId & p.isActive)
                .ToListAsync();

            return View(instructions);

        }



        public async Task<IActionResult> SeenInstruction(int instructionId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var employee = await _context.Employee.FirstOrDefaultAsync(e => e.UserId == currentUser.Id);
            var nurse = await _context.Nurse.Where(emp => emp.EmployeeId == employee.Id).FirstOrDefaultAsync();

            var instruction = await _context.Instruction.FindAsync(instructionId);
            if (instruction == null)
            {
                return NotFound();
            }

            instruction.isActive = false;
            instruction.NurseId = nurse.NurseId;
            await _context.SaveChangesAsync();

            return RedirectToAction("Instruction");
        }



        public async Task<IActionResult> MedicationSchedule()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == currentUser.Id);
            var nurse = await _context.Nurse.Where(emp => emp.EmployeeId == employee.Id)
                .FirstOrDefaultAsync();

            var patients = await _context.Patient
                .Where(p => p.NurseId == nurse.NurseId)
                .Where(p => !p.IsDischarged)
                .ToListAsync();

            return View(patients);
        }


        public async Task<IActionResult> ViewMedicationSchedule(int patientId)
        {
            var medication = await _context.MedicationAdministration
                .Where(p => p.PatientId == patientId)
                .Where(s => !s.IsSeen)
                .Include(p => p.Prescription)
                .Include(p => p.Patient)
                .ToListAsync();

            return View(medication);
        }


        public async Task<IActionResult> SeenMedicationSchedule(int MedicationAdministrationId)
        {
            if (MedicationAdministrationId == 0)
            {
                return NotFound();

            }
            var medSchedule = await _context.MedicationAdministration
                .FindAsync(MedicationAdministrationId);
            medSchedule.IsSeen = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("MedicationSchedule");
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
            {
                return Forbid();
            }

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