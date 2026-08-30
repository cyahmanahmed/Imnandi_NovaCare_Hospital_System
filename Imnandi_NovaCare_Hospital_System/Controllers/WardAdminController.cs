using Imnandi_NovaCare_Hospital_System.Data;
using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Imnandi_NovaCare_Hospital_System.Controllers
{
    [Authorize(Roles = "WardAdmin")]
    public class WardAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<User> _signInManager;

        public WardAdminController(
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
        
        public async Task<IActionResult> WardAdminDashboard()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == currentUser.Id);
            var wardAdmin = await _context.WardAdmin
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(w => w.Employee.UserId == currentUser.Id);
            
            var totalPatients = await _context.Patient.CountAsync(p => !p.IsDeleted);
            var totalDoctors = await _context.Doctor.CountAsync();
            var totalNurseSisters = await _context.NurseSister.CountAsync();
            var totalNurses = await _context.Nurse.CountAsync();
            var totalEmployees = await _context.Employee.CountAsync(e => !e.IsDeleted);
            var totalWards = await _context.Ward.CountAsync();
            var totalRooms = await _context.Room.CountAsync();
            var bedsAvailable = await _context.Bed.CountAsync(b => !b.IsOccupied);
            var patientsInHospital = await _context.Patient.CountAsync(p => p.AdmissionFolderId != null && !p.IsDischarged);
            var patients = await _context.Patient.Where(p=>!p.IsDeleted).Where(p=>!p.IsDischarged).ToListAsync();
            var recentRegistrations = await _context.Patient
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.DateAdmitted)
                .Take(5)
                .Select(p => $"{p.FirstName} {p.LastName}")
                .ToListAsync();
            var dischargeInstructions = await _context.DischargeInstruction
                .Include(d => d.Patient)
                .Include(d => d.Doctor)
                    .ThenInclude(doc => doc.Employee) 
                .Where(d => d.Status == "Pending") 
                .OrderByDescending(d => d.IssuedDate)
                .ToListAsync();

            var rooms = await _context.Room.Include(r => r.Beds).ToListAsync(); 

            if (wardAdmin != null)
            {
                var reminders = await _context.PatientMovement
                    .Include(pm => pm.Patient)
                    .Where(pm => pm.WardAdminId == wardAdmin.WardAdminId
                                 && pm.MovementType == "Temporary Move" 
                                 && pm.ReturnTime.HasValue
                                 && pm.ReturnTime <= DateTime.Now)
                    .ToListAsync();

                ViewBag.TemporaryMoveReminders = reminders;
            }

            var model = new WardAdminDashBoardViewModel
            {
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                UserId = currentUser.Id,
                UserName = currentUser.UserName,
                Email = currentUser.Email,
                PhoneNumber = currentUser.Employee.PhoneNumber,
                Department = currentUser.Employee.Department,
                JobTitle = currentUser.Employee.JobTitle,
                TotalPatients = totalPatients,
                PatientsInHospital = patientsInHospital,
                TotalDoctors = totalDoctors,
                TotalNurseSisters = totalNurseSisters,
                TotalNurses = totalNurses,
                TotalEmployees = totalEmployees,
                TotalWards = totalWards,
                BedsAvailable = bedsAvailable,
                RecentRegistrations = recentRegistrations,
                Patient = patients,
                DischargeInstructions = dischargeInstructions
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
            var model = new WardAdminDashBoardViewModel
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
        public async Task<IActionResult> Profile(WardAdminDashBoardViewModel model)
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

            var wardAdmin = await _context.WardAdmin
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id);

            if (wardAdmin != null)
            {
                wardAdmin.FirstName = model.FirstName;
                wardAdmin.LastName = model.LastName;

                _context.WardAdmin.Update(wardAdmin);
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
                    details: $"Ward Admin. {user.FirstName} {user.LastName} updated their profile."
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
                    details: $"Dr. {user.FirstName} {user.LastName} attempted to update profile but failed."
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

            var oldUserData = new
            {
                user.UserName,
                user.Email
            };
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
                    details: $"Ward Admin. {user.FirstName} {user.LastName} successfully changed their password."
                );

                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction("WardAdminDashBoard");
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
                    details: $"Dr. {user.FirstName} {user.LastName} attempted to change password but failed."
                );
            }

            return View(model);
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






        public async Task<IActionResult> ManagePatients(string? searchString)
        {
            var patients = from p in _context.Patient
                           where !p.IsDeleted
                           select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                patients = patients.Where(p =>
                    p.FirstName.Contains(searchString) ||
                    p.LastName.Contains(searchString) ||
                    p.Gender.Contains(searchString) ||
                    p.BankName.Contains(searchString));
            }

            var patientList = await patients
                .OrderByDescending(p => p.DateAdmitted)
                .ToListAsync();

            return View(patientList);
        }






        [HttpGet]
        public IActionResult CreatePatient()
        {
            return View(new CreatePatientViewModel());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePatient(CreatePatientViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);


            var existingPatient = await _context.Patient
                .FirstOrDefaultAsync(p => p.IdNumber == model.IdNumber);
            if (existingPatient != null)
            {
                ModelState.AddModelError("IdNumber", "A patient with this ID number already exists.");
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wardAdmin = await _context.WardAdmin
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(w => w.Employee.UserId == userId);

            if (wardAdmin == null)
            {
                ModelState.AddModelError("", "Ward Admin not found.");
                return View(model);
            }
            var patient = new Patient
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                IdNumber = model.IdNumber,
                DateOfBirth = model.DateOfBirth,
                Gender = model.Gender,
                PhoneNumber = model.PhoneNumber,
                EmergencyContact = model.EmergencyContact,
                CurrentMedication = model.CurrentMedication,
                BankName = model.BankName,
                BankAccountNumber = model.BankAccountNumber,
                IsInsured = model.IsInsured,
                InsuranceProvider = model.InsuranceProvider,
                PolicyNumber = model.PolicyNumber,
                MedicalAidPlan = model.MedicalAidPlan,
                MainMemberName = model.MainMemberName,
                DateAdmitted = DateTime.UtcNow,
                WardAdminId = wardAdmin?.WardAdminId,
                Allergies = new List<Allergies>()
            };

            if (!string.IsNullOrWhiteSpace(model.Allergies))
            {
                var allergyList = model.Allergies
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => new Allergies
                    {
                        Allergen = a.Trim(),
                        Patient = patient
                    })
                    .ToList();

                _context.Allergy.AddRange(allergyList);
            }

            if (!string.IsNullOrWhiteSpace(model.MedicalHistoryDescription))
            {
                var medicalHistory = new MedicalHistory
                {
                    Description = model.MedicalHistoryDescription,
                    Patient = patient 
                };

                patient.MedicalHistory.Add(medicalHistory);
            }

            _context.Allergy.AddRange(patient.Allergies);
            _context.Patient.Add(patient);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);

            await LogAuditAsync(
                actionTaken: "CreatePatient",
                user: user,
                entity: "Patient",
                recordId: patient.Id.ToString(),
                oldValue: null,
                newValue: System.Text.Json.JsonSerializer.Serialize(new
                {
                    PatientName = $"{patient.FirstName} {patient.LastName}",
                    IdNumber = patient.IdNumber,
                    Gender = patient.Gender,
                    PhoneNumber = patient.PhoneNumber,
                    IsInsured = patient.IsInsured,
                    InsuranceProvider = patient.InsuranceProvider,
                    PolicyNumber = patient.PolicyNumber,
                    MedicalAidPlan = patient.MedicalAidPlan,
                    MainMemberName = patient.MainMemberName,
                    DateAdmitted = patient.DateAdmitted,
                    CreatedBy = $"{wardAdmin.Employee.FirstName} {wardAdmin.Employee.LastName}"
                }),
                details: $"Ward Admin {wardAdmin.Employee.FirstName} {wardAdmin.Employee.LastName} created a new patient record named {patient.FirstName} {patient.LastName}."
            );
            TempData["TempSuccess"] = $"Patient {patient.FirstName} {patient.LastName} added successfully.";
            return RedirectToAction("WardAdminDashboard", "WardAdmin");
        }

        [HttpGet]
        public async Task<IActionResult> EditPatient(int id)
        {
            var patient = await _context.Patient
                .Include(p => p.Allergies)
                .Include(p => p.MedicalHistory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            var model = new CreatePatientViewModel
            {
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                IdNumber = patient.IdNumber,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                PhoneNumber = patient.PhoneNumber,
                EmergencyContact = patient.EmergencyContact,
                CurrentMedication = patient.CurrentMedication,
                BankName = patient.BankName,
                BankAccountNumber = patient.BankAccountNumber,
                IsInsured = patient.IsInsured,
                InsuranceProvider = patient.InsuranceProvider,
                PolicyNumber = patient.PolicyNumber,
                MedicalAidPlan = patient.MedicalAidPlan,
                MainMemberName = patient.MainMemberName,
                Allergies = string.Join(", ", patient.Allergies.Select(a => a.Allergen)),
                MedicalHistoryDescription = string.Join("\n", patient.MedicalHistory.Select(m => m.Description))
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(int id, CreatePatientViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var patient = await _context.Patient
                .Include(p => p.Allergies)
                .Include(p => p.MedicalHistory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            var duplicate = await _context.Patient
                .AnyAsync(p => p.IdNumber == model.IdNumber && p.Id != id);

            if (duplicate)
            {
                ModelState.AddModelError("IdNumber", "A patient with this ID number already exists.");
                return View(model);
            }

            var oldValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                patient.FirstName,
                patient.LastName,
                patient.IdNumber,
                patient.Gender,
                patient.PhoneNumber,
                patient.IsInsured,
                patient.InsuranceProvider,
                patient.PolicyNumber,
                patient.MedicalAidPlan,
                patient.MainMemberName,
                patient.EmergencyContact,
                patient.CurrentMedication
            });

            patient.FirstName = model.FirstName;
            patient.LastName = model.LastName;
            patient.IdNumber = model.IdNumber;
            patient.DateOfBirth = model.DateOfBirth;
            patient.Gender = model.Gender;
            patient.PhoneNumber = model.PhoneNumber;
            patient.EmergencyContact = model.EmergencyContact;
            patient.CurrentMedication = model.CurrentMedication;
            patient.BankName = model.BankName;
            patient.BankAccountNumber = model.BankAccountNumber;
            patient.IsInsured = model.IsInsured;
            patient.InsuranceProvider = model.InsuranceProvider;
            patient.PolicyNumber = model.PolicyNumber;
            patient.MedicalAidPlan = model.MedicalAidPlan;
            patient.MainMemberName = model.MainMemberName;

            patient.Allergies.Clear();
            if (!string.IsNullOrWhiteSpace(model.Allergies))
            {
                var allergyList = model.Allergies
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => new Allergies
                    {
                        Allergen = a.Trim(),
                        PatientId = patient.Id
                    })
                    .ToList();

                _context.Allergy.AddRange(allergyList);
            }

            patient.MedicalHistory.Clear();
            if (!string.IsNullOrWhiteSpace(model.MedicalHistoryDescription))
            {
                patient.MedicalHistory.Add(new MedicalHistory
                {
                    Description = model.MedicalHistoryDescription,
                    PatientId = patient.Id
                });
            }

            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);

            await LogAuditAsync(
                actionTaken: "EditPatient",
                user: user,
                entity: "Patient",
                recordId: patient.Id.ToString(),
                oldValue: oldValue,
                newValue: System.Text.Json.JsonSerializer.Serialize(new
                {
                    PatientName = $"{patient.FirstName} {patient.LastName}",
                    IdNumber = patient.IdNumber,
                    Gender = patient.Gender,
                    PhoneNumber = patient.PhoneNumber,
                    IsInsured = patient.IsInsured,
                    InsuranceProvider = patient.InsuranceProvider,
                    PolicyNumber = patient.PolicyNumber,
                    MedicalAidPlan = patient.MedicalAidPlan,
                    MainMemberName = patient.MainMemberName,
                    EditedBy = user?.UserName
                }),
                details: $"Patient {patient.FirstName} {patient.LastName} record was updated."
            );

            TempData["TempSuccess"] = $"Patient {patient.FirstName} {patient.LastName} updated successfully.";
            return RedirectToAction("ManagePatients", "WardAdmin");
        }









        [HttpGet]
        public async Task<IActionResult> GetBedsByRoom(int roomId)
        {
            var beds = await _context.Bed
                .Where(b =>
                    b.RoomId == roomId &&
                    !b.IsOccupied &&
                    !b.IsDeleted)
                .OrderBy(b => b.BedNumber)
                .Select(b => new
                {
                    bedId = b.BedId,
                    bedNumber = b.BedNumber
                })
                .ToListAsync();

            return Json(beds);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePatientFolder(int patientId)
        {
            var patient = await _context.Patient
                .FirstOrDefaultAsync(p => p.Id == patientId && !p.IsDeleted);

            if (patient == null)
                return NotFound();

            var model = new CreateAdmissionFolderViewModel
            {
                PatientId = patient.Id,
                PatientName = $"{patient.FirstName} {patient.LastName}",
                DoctorId = patient.DoctorId,

                Rooms = await _context.Room
                    .Include(r => r.Ward)
                    .Where(r => !r.IsDeleted)
                    .OrderBy(r => r.RoomNumber)
                    .ToListAsync(),

                Beds = await _context.Bed
                    .Include(b => b.Room)
                    .Where(b => !b.IsOccupied && !b.IsDeleted)
                    .OrderBy(b => b.Room.RoomNumber)
                    .ThenBy(b => b.BedNumber)
                    .ToListAsync(),

                Doctors = await _context.Doctor
                    .Include(d => d.Employee)
                    .Where(d => !d.IsDeleted)
                    .OrderBy(d => d.Employee.FirstName)
                    .ToListAsync(),

                Nurses = await _context.Nurse
                    .Include(n => n.Employee)
                    .Where(n => !n.IsDeleted)
                    .OrderBy(n => n.Employee.FirstName)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePatientFolder(CreateAdmissionFolderViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("CreatePatientFolder",model);

            if (string.IsNullOrWhiteSpace(model.ReasonForAdmission))
            {
                ModelState.AddModelError(nameof(model.ReasonForAdmission), "Reason for admission is required.");
                return View(model);
            }

            var patient = await _context.Patient
                .Include(p => p.AdmissionFolder)
                .FirstOrDefaultAsync(p => p.Id == model.PatientId);

            if (patient == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wardAdmin = await _context.WardAdmin
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(w => w.Employee.UserId == userId);

            if (wardAdmin == null)
            {
                ModelState.AddModelError("", "Ward Admin not found.");
                return View(model);
            }

            var room = await _context.Room.Include(r => r.Ward).FirstOrDefaultAsync(r => r.RoomId == model.RoomId);
            var bed = await _context.Bed.FirstOrDefaultAsync(b => b.BedId == model.BedId);
            var doctor = model.DoctorId.HasValue
                ? await _context.Doctor
                    .Include(d => d.Patients)
                    .FirstOrDefaultAsync(d => d.DoctorId == model.DoctorId.Value)
                : null;
            var nurse = await _context.Nurse.FindAsync(model.NurseId);

            if (room == null || bed == null || nurse == null)
            {
                ModelState.AddModelError("", "Invalid assignment selections.");
                return View(model);
            }

            patient.AdmissionCount += 1; 
            patient.LastDischargeDate = null; 
            patient.AdmissionHistory ??= ""; 
            patient.AdmissionHistory += $"Admitted on {DateTime.UtcNow:dd/MM/yyyy HH:mm}; Reason: {model.ReasonForAdmission}\n";

            var folder = new AdmissionFolder
            {
                PatientId = patient.Id,
                WardAdminId = wardAdmin.WardAdminId,
                BedId = bed.BedId,
                ReasonForAdmission = model.ReasonForAdmission,
                DateCreated = DateTime.UtcNow,
                IsActive = true,
                AdmissionCount = patient.AdmissionCount
            };
            _context.AdmissionFolder.Add(folder);

            patient.NurseId = nurse.NurseId;
            patient.RoomId = room.RoomId;
            patient.BedId = bed.BedId;
            patient.AdmissionFolder = folder;
            patient.AdmissionFolderId = folder.AdmissionFolderId;
            patient.IsDischarged = false;
            patient.DateDischarged = null;
            bed.IsOccupied = true;


            if (doctor != null)
            {
                DateTime scheduledDate = DateTime.Today.AddHours(8);

                while (true)
                {
                    bool conflict = await _context.Schedules
                        .AnyAsync(s => s.DoctorId == doctor.DoctorId &&
                                       s.ScheduledDate.Date == scheduledDate.Date &&
                                       s.ScheduledDate.Hour == scheduledDate.Hour &&
                                       !s.IsCompleted);

                    if (!conflict)
                        break;

                    scheduledDate = scheduledDate.AddHours(1);

                    if (scheduledDate.Hour > 17) 
                        scheduledDate = scheduledDate.Date.AddDays(1).AddHours(8);
                }

                patient.DoctorId = doctor.DoctorId;

                var schedule = new Schedule
                {
                    DoctorId = doctor.DoctorId,
                    PatientId = patient.Id,
                    ScheduledDate = scheduledDate,
                    VisitType = model.VisitType ?? "Consultation", 
                    Location = room.RoomName ?? room.RoomNumber.ToString(), 
                    IsCompleted = false
                };

                _context.Schedules.Add(schedule);
            }

            var history = new PatientHistory
            {
                PatientId = patient.Id,
                DoctorId = doctor?.DoctorId,
                AdmissionDate = DateTime.UtcNow,
                WardId = room.WardId,
                RoomId = room.RoomId,
                BedId = bed.BedId,
                TreatmentType = model.VisitType ?? "Consultation",
                TreatmentDescription = model.ReasonForAdmission
            };
            _context.PatientHistory.Add(history);

            _context.Patient.Update(patient);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);

            await LogAuditAsync(
                actionTaken: "CreatePatientFolder",
                user: user,
                entity: "AdmissionFolder",
                recordId: folder.AdmissionFolderId.ToString(),
                oldValue: null,
                newValue: System.Text.Json.JsonSerializer.Serialize(new
                {
                    PatientName = $"{patient.FirstName} {patient.LastName}",
                    ReasonForAdmission = model.ReasonForAdmission,
                    Ward = room.Ward.WardName,
                    Room = room.RoomName ?? room.RoomNumber.ToString(),
                    Bed = bed.BedNumber,
                    Nurse = $"{nurse.FirstName} {nurse.LastName}",
                    Doctor = doctor != null ? $"{doctor.FirstName} {doctor.LastName}" : "Not Assigned",
                    CreatedBy = $"{wardAdmin.Employee.FirstName} {wardAdmin.Employee.LastName}",
                    CreatedOn = folder.DateCreated
                }),
                details: $"Ward Admin {wardAdmin.Employee.FirstName} {wardAdmin.Employee.LastName} created a new admission folder for patient {patient.FirstName} {patient.LastName} in ward {room.Ward.WardName}, room {room.RoomName ?? room.RoomNumber}, bed {bed.BedNumber}. Doctor assigned: {(doctor != null ? doctor.LastName : "None")}."
            );
            TempData["SuccessMessage"] = "Patient folder created successfully.";
            return RedirectToAction("WardAdminDashboard", "WardAdmin");
        }







        [HttpGet]
        public async Task<IActionResult> AdmissionPatientFolder(int patientId)
        {
            var patient = await _context.Patient
                .Include(p => p.AdmissionFolder)
                    .ThenInclude(f => f.Bed)
                        .ThenInclude(b => b.Room)
                            .ThenInclude(r => r.Ward)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.Employee)
                .Include(p => p.Nurse)
                    .ThenInclude(n => n.Employee)
                .Include(p => p.Instructions)
                    .ThenInclude(i => i.Doctor)
                        .ThenInclude(d => d.Employee)
                .Include(p => p.PatientMovements)
                    .ThenInclude(m => m.WardAdmin)
                        .ThenInclude(wa => wa.Employee)
                .Include(p => p.VitalSigns)
                .Include(p => p.Treatments)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Doctor)
                        .ThenInclude(d => d.Employee)
                .Include(p => p.DischargeInstructions)
                    .ThenInclude(di => di.Doctor)
                        .ThenInclude(d => d.Employee)
                .Include(p => p.PatientMoveRequest)
                    .ThenInclude(r => r.Doctor)
                        .ThenInclude(d => d.Employee)
                .Include(p => p.PatientMoveRequest)
                    .ThenInclude(r => r.TargetWard)
                .FirstOrDefaultAsync(p => p.Id == patientId && !p.IsDeleted);

            if (patient == null || patient.AdmissionFolder == null)
                return NotFound();

            var model = new AdmissionFolderDetailsViewModel
            {
                PatientId = patient.Id,
                PatientName = $"{patient.FirstName} {patient.LastName}",
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                DoctorName = patient.Doctor != null ? $"{patient.Doctor.FirstName} {patient.Doctor.LastName}" : "N/A",
                NurseName = patient.Nurse != null ? $"{patient.Nurse.FirstName} {patient.Nurse.LastName}" : "N/A",
                ReasonForAdmission = patient.AdmissionFolder.ReasonForAdmission,
                WardName = patient.AdmissionFolder.Bed?.Room?.Ward?.WardName ?? "N/A",
                RoomNumber = patient.AdmissionFolder.Bed?.Room?.RoomNumber ?? "N/A",
                BedNumber = patient.AdmissionFolder.Bed?.BedNumber ?? "N/A",
                DateCreated = patient.AdmissionFolder.DateCreated,
                DateClosed = patient.AdmissionFolder.DateClosed,
                IsActive = patient.AdmissionFolder.IsActive,
                Instructions = patient.Instructions.ToList(),
                PatientMovements = patient.PatientMovements.ToList(),
                VitalSigns = patient.VitalSigns.ToList(),
                Treatments = patient.Treatments.ToList(),
                Prescriptions = patient.Prescriptions.ToList(),
                DischargeInstructions = patient.DischargeInstructions.ToList(),
                HasPendingDischarge = patient.DischargeInstructions.Any(di => di.Status == "Pending"),
                PendingMoveRequests = patient.PatientMoveRequest
                                .Where(r => r.Status == "Pending")
                                .ToList(),
                HasPendingMove = patient.PatientMoveRequest.Any(r => r.Status == "Pending")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdmissionPatientFolder(AdmissionFolderDetailsViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            var folder = await _context.AdmissionFolder
                .Include(f => f.Bed)
                .Include(f => f.Patient)  
                .FirstOrDefaultAsync(f => f.PatientId == model.PatientId);

            if (folder == null)
            {
                await LogAuditAsync(
                    "Update Admission Folder Failed",
                    currentUser,
                    entity: "AdmissionFolder",
                    recordId: model.PatientId.ToString(),
                    failureReason: "Folder not found"
                );
                return NotFound();
            }

            var oldValue = $"Reason: {folder.ReasonForAdmission}, Active: {folder.IsActive}, BedId: {folder.Bed?.BedId}, Closed: {folder.DateClosed}";

            folder.ReasonForAdmission = model.ReasonForAdmission;

            if (!model.IsActive)
            {
                folder.IsActive = false;
                folder.DateClosed = DateTime.UtcNow;

                if (folder.Bed != null)
                    folder.Bed.IsOccupied = false;

                var patient = folder.Patient;
                if (patient != null)
                {
                    patient.RoomId = null;
                    patient.BedId = null;
                    patient.DoctorId = null;
                    patient.NurseId = null;
                }
            }

            _context.AdmissionFolder.Update(folder);

            await _context.SaveChangesAsync();

            var newValue = $"Reason: {folder.ReasonForAdmission}, Active: {folder.IsActive}, BedId: {folder.Bed?.BedId}, Closed: {folder.DateClosed}";

            await LogAuditAsync(
                "Updated Admission Folder",
                currentUser,
                entity: "AdmissionFolder",
                recordId: folder.AdmissionFolderId.ToString(),
                oldValue: oldValue,
                newValue: newValue,
                details: $"Admission folder for patient {folder.Patient.FirstName} {folder.Patient.LastName} updated by Ward Admin."
            );

            TempData["SuccessMessage"] = $"Admission folder for {folder.Patient.FirstName} {folder.Patient.LastName} updated successfully.";
            return RedirectToAction("WardAdminDashboard", "WardAdmin");
        }






        [HttpGet]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patient
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }
        [HttpPost, ActionName("DeletePatient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePatientConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            if (currentUser == null) 
            {
                return RedirectToAction("Login", "Account");
            }
            var patient = await _context.Patient
                .Include(p => p.AdmissionFolder)
                .Include(p => p.Bed)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null || patient.IsDeleted)
            {
                await LogAuditAsync(
                    "Delete Patient Failed",
                    currentUser,
                    entity: "Patient",
                    recordId: id.ToString(),
                    failureReason: "Patient not found or already deleted"
                );
                return NotFound();
            }

            if (patient.AdmissionFolder != null && patient.AdmissionFolder.IsActive)
            {
                TempData["TempError"] = $"Cannot delete {patient.FirstName} {patient.LastName} because they are currently admitted.";
                await LogAuditAsync(
                    "Delete Patient Blocked",
                    currentUser,
                    entity: "Patient",
                    recordId: id.ToString(),
                    details: $"Attempted to delete patient {patient.FirstName} {patient.LastName} who is currently admitted."
                );
                return RedirectToAction(nameof(ManagePatients));
            }

            var oldValue = $"Name: {patient.FirstName} {patient.LastName}, AdmissionFolder: {patient.AdmissionFolder?.AdmissionFolderId}, Active: {patient.AdmissionFolder?.IsActive}";

            patient.IsDeleted = true;
            patient.DoctorId = null;
            patient.NurseId = null;

            if (patient.BedId != null)
            {
                var bed = await _context.Bed.FirstOrDefaultAsync(b => b.BedId == patient.BedId);

                if (bed != null)
                {
                    bed.IsOccupied = false;
                    patient.BedId = null;
                    patient.RoomId = null;
                    patient.WardAdminId = null;
                }
            }
            if (patient.AdmissionFolder != null)
            {
                patient.AdmissionFolder.IsActive = false;
                patient.AdmissionFolder.HasPendingDischarge = false;
                patient.AdmissionFolder.BedId = null;
            }
            patient.AdmissionFolderId = null;


            var futureSchedules = await _context.Schedules
                .Where(s => s.PatientId == patient.Id && s.ScheduledDate >= DateTime.UtcNow && !s.IsDeleted)
                .ToListAsync();

            foreach (var schedule in futureSchedules)
            {
                schedule.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            await LogAuditAsync(
                "Patient Deleted",
                currentUser,
                entity: "Patient",
                recordId: patient.Id.ToString(),
                oldValue: oldValue,
                newValue: "Patient deleted successfully",
                details: $"Patient {patient.FirstName} {patient.LastName} was deleted by Ward Admin."
            );
            TempData["TempSuccess"] = $"Patient {patient.FirstName} {patient.LastName} was deleted successfully.";
            return RedirectToAction(nameof(ManagePatients));
        }

        public async Task<IActionResult> DeletedPatients()
        {
            var deletedPatients = await _context.Patient
                .Where(p => p.IsDeleted)
                .OrderBy(p => p.FirstName)
                .ToListAsync();

            return View(deletedPatients);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestorePatient(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            var patient = await _context.Patient
                .Include(p => p.AdmissionFolder)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
            {
                TempData["TempError"] = "Patient not found.";
                await LogAuditAsync(
                    "Restore Patient Failed",
                    currentUser,
                    entity: "Patient",
                    recordId: id.ToString(),
                    failureReason: "Patient not found"
                );
                return RedirectToAction(nameof(DeletedPatients));
            }

            if (!patient.IsDeleted)
            {
                TempData["TempError"] = $"Patient {patient.FirstName} {patient.LastName} is already active.";
                await LogAuditAsync(
                    "Restore Patient Blocked",
                    currentUser,
                    entity: "Patient",
                    recordId: patient.Id.ToString(),
                    details: $"Attempted to restore patient {patient.FirstName} {patient.LastName} who is already active."
                );
                return RedirectToAction(nameof(DeletedPatients));
            }

            var oldValue = "Patient marked as deleted";
            var newValue = "Patient restored (IsDeleted = false)";
            patient.IsDeleted = false;
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                "Patient Restored",
                currentUser,
                entity: "Patient",
                recordId: patient.Id.ToString(),
                oldValue: oldValue,
                newValue: newValue,
                details: $"Patient {patient.FirstName} {patient.LastName} was restored by Ward Admin."
            );
            TempData["SuccessMessage"] = $"Patient {patient.FirstName} {patient.LastName} restored successfully.";
            return RedirectToAction(nameof(DeletedPatients));
        }










        [HttpGet]
        public async Task<IActionResult> ManageDoctors()
        {
            var doctors = await _context.Doctor
                .Include(d => d.Employee)
                .ToListAsync();

            return View(doctors);
        }
        [HttpGet]
        public async Task<IActionResult> DoctorDetails(int id)
        {
            var doctor = await _context.Doctor
                .Include(d => d.Employee)
                .Include(d => d.Patients.Where(p => !p.IsDeleted && !p.IsDischarged))
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
                return NotFound();

            var schedules = await _context.Schedules
                .Include(s => s.Patient)
                .Where(s => s.DoctorId == id && !s.IsDeleted)
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();

            var unassignedPatients = await _context.Patient
                .Where(p => p.DoctorId == null && !p.IsDeleted && !p.IsDischarged)
                .ToListAsync();


            ViewData["Schedules"] = schedules;
            ViewData["UnassignedPatients"] = unassignedPatients;

            return View(doctor);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPatient(int doctorId, int patientId, string visitType, DateTime scheduledDate)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            var doctor = await _context.Doctor.FindAsync(doctorId);
            var patient = await _context.Patient
                .Include(p => p.Room)
                    .ThenInclude(r => r.Ward)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (doctor == null || patient == null)
            {
                await LogAuditAsync(
                    "Assign Patient Failed",
                    currentUser,
                    entity: "Schedule",
                    recordId: $"{patientId}",
                    failureReason: doctor == null ? "Doctor not found" : "Patient not found"
                );
                return NotFound();
            }

            bool conflict = await _context.Schedules.AnyAsync(s =>
                s.DoctorId == doctorId && s.ScheduledDate == scheduledDate);

            if (conflict)
            {
                TempData["ErrorMessage"] = "This time slot is already booked.";
                await LogAuditAsync(
                    "Assign Patient Blocked",
                    currentUser,
                    entity: "Schedule",
                    recordId: $"{patientId}",
                    details: $"Attempted to assign patient {patient.FirstName} {patient.LastName} to Dr. {doctor.FirstName} at {scheduledDate:yyyy-MM-dd HH:mm}, but the time slot was already booked."
                );
                return RedirectToAction("DoctorDetails", new { id = doctorId });
            }

            var oldValue = $"PatientId: {patient.Id}, DoctorId: {patient.DoctorId?.ToString() ?? "None"}";

            patient.DoctorId = doctorId;

            var schedule = new Schedule
            {
                DoctorId = doctorId,
                PatientId = patientId,
                ScheduledDate = scheduledDate,
                VisitType = visitType,
                Location = patient.Room?.Ward?.WardName ?? "Unknown",
                IsCompleted = false
            };

            _context.Schedules.Add(schedule);
            _context.Patient.Update(patient);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                "Patient Assigned to Doctor",
                currentUser,
                entity: "Schedule",
                recordId: schedule.ScheduleId.ToString(),
                oldValue: oldValue,
                newValue: $"Assigned to DoctorId: {doctorId}, ScheduledDate: {scheduledDate:yyyy-MM-dd HH:mm}, VisitType: {visitType}",
                details: $"Patient {patient.FirstName} {patient.LastName} assigned to Dr. {doctor.FirstName} on {scheduledDate:yyyy-MM-dd HH:mm}."
            );

            TempData["SuccessMessage"] = $"Patient {patient.FirstName} {patient.LastName} assigned and scheduled for {scheduledDate:yyyy-MM-dd HH:mm}.";
            return RedirectToAction("DoctorDetails", new { id = doctorId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignDoctorsPatient(int doctorId, int patientId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            var patient = await _context.Patient
                .Include(p => p.Doctor)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                await LogAuditAsync(
                    "Unassign Patient Failed",
                    currentUser,
                    entity: "Patient",
                    recordId: patientId.ToString(),
                    failureReason: "Patient not found"
                );
                return NotFound();
            }
            var oldValue = $"DoctorId: {patient.DoctorId?.ToString() ?? "None"}";
            var today = DateTime.UtcNow.Date;

            var todaysSchedules = await _context.Schedules
                .Where(s => s.PatientId == patientId
                            && s.DoctorId == doctorId
                            && s.ScheduledDate.Date == today
                            && !s.IsDeleted)
                .ToListAsync();

            foreach (var schedule in todaysSchedules)
            {
                schedule.IsDeleted = true;
            }

            var futureSchedules = await _context.Schedules
                .Where(s => s.PatientId == patientId
                            && s.ScheduledDate > today
                            && !s.IsDeleted)
                .ToListAsync();

            foreach (var schedule in futureSchedules)
            {
                schedule.IsDeleted = true;
            }

            var doctorName = patient.Doctor?.FirstName + " " + patient.Doctor?.LastName ?? "Unknown";

            patient.DoctorId = null;
            _context.Patient.Update(patient);
            await _context.SaveChangesAsync();

            var newValue = "DoctorId: None";

            await LogAuditAsync(
                "Patient Unassigned from Doctor",
                currentUser,
                entity: "Patient",
                recordId: patient.Id.ToString(),
                oldValue: oldValue,
                newValue: newValue,
                details: $"Patient {patient.FirstName} {patient.LastName} unassigned from Dr. {doctorName} by Ward Admin."
            );

            TempData["SuccessMessage"] = $"Patient {patient.FirstName} {patient.LastName} unassigned from Dr. {doctorName}.";
            return RedirectToAction("DoctorDetails", new { id = doctorId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSchedule(int doctorId, int patientId, DateTime scheduledDate, string visitType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            var doctor = await _context.Doctor.FindAsync(doctorId);
            var patient = await _context.Patient
                .Include(p => p.Room)
                    .ThenInclude(r => r.Ward)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (doctor == null || patient == null)
            {
                await LogAuditAsync(
                    "Create Schedule Failed",
                    currentUser,
                    entity: "Schedule",
                    recordId: $"{patientId}",
                    failureReason: doctor == null ? "Doctor not found" : "Patient not found"
                );
                return NotFound();
            }

            bool conflict = await _context.Schedules.AnyAsync(s =>
                s.DoctorId == doctorId && s.ScheduledDate == scheduledDate);

            if (conflict)
            {
                TempData["ErrorMessage"] = "This slot is already booked.";
                await LogAuditAsync(
                    "Create Schedule Blocked",
                    currentUser,
                    entity: "Schedule",
                    recordId: $"{patientId}",
                    details: $"Attempted to create schedule for patient {patient.FirstName} {patient.LastName} with Dr. {doctor.FirstName} at {scheduledDate:yyyy-MM-dd HH:mm}, but the slot was already booked."
                );
                return RedirectToAction("DoctorDetails", new { id = doctorId });
            }

            var schedule = new Schedule
            {
                DoctorId = doctorId,
                PatientId = patientId,
                ScheduledDate = scheduledDate,
                VisitType = visitType,
                Location = patient.Room?.Ward?.WardName ?? "Unknown",
                IsCompleted = false,
                IsDeleted = false
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                "Schedule Created",
                currentUser,
                entity: "Schedule",
                recordId: schedule.ScheduleId.ToString(),
                oldValue: null,
                newValue: $"DoctorId: {doctorId}, PatientId: {patientId}, ScheduledDate: {scheduledDate:yyyy-MM-dd HH:mm}, VisitType: {visitType}, Location: {schedule.Location}",
                details: $"Patient {patient.FirstName} {patient.LastName} scheduled with Dr. {doctor.FirstName} on {scheduledDate:yyyy-MM-dd HH:mm} by Ward Admin."
            );
            TempData["SuccessMessage"] = "Schedule created successfully.";
            return RedirectToAction("DoctorDetails", new { id = doctorId });
        }






        [HttpGet]
        public async Task<IActionResult> GetPatientsByWard(int wardId)
        {
            var patients = await _context.Patient
                .Where(p =>
                    !p.IsDeleted &&
                    p.NurseId == null &&
                    p.Room != null &&
                    p.Room.WardId == wardId)
                .Select(p => new
                {
                    id = p.Id,
                    name = p.FirstName + " " + p.LastName,
                    idNumber = p.IdNumber,
                    room = p.Room.RoomNumber,
                    bed = p.Bed != null ? p.Bed.BedNumber : "N/A"
                })
                .OrderBy(p => p.name)
                .ToListAsync();

            return Json(patients);
        }
        [HttpGet]
        public async Task<IActionResult> ManageNurses()
        {
            var model = new AssignNurseViewModel
            {
                Nurses = await _context.Nurse
                    .Where(n => !n.IsDeleted)
                    .OrderBy(n => n.FirstName)
                    .ToListAsync(),

                Wards = await _context.Ward
                    .Where(w => !w.IsDeleted)
                    .OrderBy(w => w.WardName)
                    .ToListAsync(),

                Patients = await _context.Patient
                    .Include(p => p.Room)
                        .ThenInclude(r => r.Ward)
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.FirstName)
                    .ThenBy(p => p.LastName)
                    .ToListAsync(),

                AllNursesWithPatients = await _context.Nurse
                    .Include(n => n.Patients)
                    .Where(n => !n.IsDeleted)
                    .OrderBy(n => n.FirstName)
                    .ThenBy(n => n.LastName)
                    .ToListAsync()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageNurses(AssignNurseViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            if (model.SelectedNurseId == 0)
            {
                TempData["Error"] = "Please select a nurse.";

                await LogAuditAsync(
                    "Assign Nurse Failed",
                    currentUser,
                    entity: "Nurse",
                    recordId: "0",
                    failureReason: "No nurse selected"
                );

                return RedirectToAction("ManageNurses");
            }

            if (!model.SelectedWardId.HasValue)
            {
                TempData["Error"] = "Please select a ward.";

                await LogAuditAsync(
                    "Assign Nurse Failed",
                    currentUser,
                    entity: "Nurse",
                    recordId: model.SelectedNurseId.ToString(),
                    failureReason: "No ward selected"
                );

                return RedirectToAction("ManageNurses");
            }

            if (model.SelectedPatientIds == null || !model.SelectedPatientIds.Any())
            {
                TempData["Error"] = "Please select at least one patient.";

                await LogAuditAsync(
                    "Assign Nurse Failed",
                    currentUser,
                    entity: "Nurse",
                    recordId: model.SelectedNurseId.ToString(),
                    failureReason: "No patients selected"
                );

                return RedirectToAction("ManageNurses");
            }

            var nurse = await _context.Nurse
                .Include(n => n.Patients)
                .FirstOrDefaultAsync(n =>
                    n.NurseId == model.SelectedNurseId &&
                    !n.IsDeleted);

            if (nurse == null)
            {
                TempData["Error"] = "Nurse not found.";

                await LogAuditAsync(
                    "Assign Nurse Failed",
                    currentUser,
                    entity: "Nurse",
                    recordId: model.SelectedNurseId.ToString(),
                    failureReason: "Nurse not found"
                );

                return RedirectToAction("ManageNurses");
            }

            var ward = await _context.Ward
                .Include(w => w.Nurses)
                .FirstOrDefaultAsync(w =>
                    w.WardId == model.SelectedWardId.Value &&
                    !w.IsDeleted);

            if (ward == null)
            {
                TempData["Error"] = "Ward not found.";

                await LogAuditAsync(
                    "Assign Nurse Failed",
                    currentUser,
                    entity: "Ward",
                    recordId: model.SelectedWardId.Value.ToString(),
                    failureReason: "Ward not found"
                );

                return RedirectToAction("ManageNurses");
            }

            var patients = await _context.Patient
                .Include(p => p.Room)
                    .ThenInclude(r => r.Ward)
                .Where(p =>
                    model.SelectedPatientIds.Contains(p.Id) &&
                    !p.IsDeleted)
                .ToListAsync();

            if (!patients.Any())
            {
                TempData["Error"] = "No valid patients were found.";

                await LogAuditAsync(
                    "Assign Nurse Failed",
                    currentUser,
                    entity: "Nurse",
                    recordId: nurse.NurseId.ToString(),
                    failureReason: "No valid patients found"
                );

                return RedirectToAction("ManageNurses");
            }

            var patientsNotInWard = patients
                .Where(p => p.Room == null ||
                            p.Room.Ward == null ||
                            p.Room.Ward.WardId != ward.WardId)
                .ToList();

            if (patientsNotInWard.Any())
            {
                TempData["Error"] = "One or more selected patients do not belong to the selected ward.";

                await LogAuditAsync(
                    "Assign Nurse Failed",
                    currentUser,
                    entity: "Nurse",
                    recordId: nurse.NurseId.ToString(),
                    failureReason: $"Selected patients do not belong to ward {ward.WardName}"
                );

                return RedirectToAction("ManageNurses");
            }

            var alreadyAssignedToOtherNurse = patients
                .Where(p => p.NurseId.HasValue && p.NurseId.Value != nurse.NurseId)
                .ToList();

            if (alreadyAssignedToOtherNurse.Any())
            {
                var names = string.Join(
                    ", ",
                    alreadyAssignedToOtherNurse.Select(p => $"{p.FirstName} {p.LastName}")
                );

                TempData["Error"] = $"The following patients are already assigned to another nurse: {names}";

                await LogAuditAsync(
                    "Assign Nurse Failed",
                    currentUser,
                    entity: "Nurse",
                    recordId: nurse.NurseId.ToString(),
                    failureReason: $"Patients already assigned to another nurse: {names}"
                );

                return RedirectToAction("ManageNurses");
            }

            var oldWardId = nurse.WardId;
            var oldPatientIds = nurse.Patients
                .Select(p => p.Id)
                .ToList();

            nurse.WardId = ward.WardId;
            nurse.WardName = ward.WardName;

            if (!ward.Nurses.Any(n => n.NurseId == nurse.NurseId))
            {
                ward.Nurses.Add(nurse);
            }

            foreach (var patient in patients)
            {
                patient.NurseId = nurse.NurseId;

                if (!nurse.Patients.Any(p => p.Id == patient.Id))
                {
                    nurse.Patients.Add(patient);
                }
            }

            _context.Nurse.Update(nurse);
            _context.Ward.Update(ward);

            foreach (var patient in patients)
            {
                _context.Patient.Update(patient);
            }

            await _context.SaveChangesAsync();

            var assignedPatientIds = patients
                .Select(p => p.Id)
                .ToList();

            var assignedPatientNames = string.Join(
                ", ",
                patients.Select(p => $"{p.FirstName} {p.LastName}")
            );

            var oldValue =
                $"WardId: {oldWardId?.ToString() ?? "None"}, " +
                $"PatientIds: {string.Join(",", oldPatientIds)}";

            var newValue =
                $"WardId: {ward.WardId}, " +
                $"PatientIds: {string.Join(",", assignedPatientIds)}";

            await LogAuditAsync(
                "Nurse Assigned",
                currentUser,
                entity: "Nurse",
                recordId: nurse.NurseId.ToString(),
                oldValue: oldValue,
                newValue: newValue,
                details:
                    $"Nurse {nurse.FirstName} {nurse.LastName} assigned to ward " +
                    $"{ward.WardName} and patients: {assignedPatientNames}."
            );

            TempData["Success"] =
                $"Nurse {nurse.FirstName} {nurse.LastName} assigned to " +
                $"{ward.WardName} with {patients.Count} patient(s).";

            return RedirectToAction("ManageNurses");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignPatient(int nurseId, int patientId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

            var nurse = await _context.Nurse
                .Include(n => n.Patients)
                .FirstOrDefaultAsync(n => n.NurseId == nurseId);

            if (nurse == null)
            {
                TempData["Error"] = "Nurse not found.";

                await LogAuditAsync(
                    "Unassign Nurse Failed",
                    currentUser,
                    entity: "Nurse",
                    recordId: nurseId.ToString(),
                    failureReason: "Nurse not found"
                );

                return RedirectToAction("ManageNurses");
            }

            var patient = await _context.Patient
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
            {
                TempData["Error"] = "Patient not found.";

                await LogAuditAsync(
                    "Unassign Nurse Failed",
                    currentUser,
                    entity: "Patient",
                    recordId: patientId.ToString(),
                    failureReason: "Patient not found"
                );

                return RedirectToAction("ManageNurses");
            }

            var oldValue = $"PatientId: {patient.Id}, NurseId: {patient.NurseId}";

            patient.NurseId = null;

            _context.Patient.Update(patient);
            _context.Nurse.Update(nurse);
            await _context.SaveChangesAsync();

            var newValue = $"PatientId: {patient.Id}, NurseId: None";

            await LogAuditAsync(
                "Nurse Unassigned from Patient",
                currentUser,
                entity: "Nurse",
                recordId: nurse.NurseId.ToString(),
                oldValue: oldValue,
                newValue: newValue,
                details: $"Nurse {nurse.FirstName} {nurse.LastName} unassigned from patient {patient.FirstName} {patient.LastName}."
            );

            TempData["Success"] = $"Nurse {nurse.FirstName} {nurse.LastName} unassigned from patient {patient.FirstName} {patient.LastName}.";

            return RedirectToAction("ManageNurses");
        }







        public async Task<IActionResult> DischargePatient(int id)
        {
            var patient = await _context.Patient
                .Include(p => p.DischargeInstructions)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (patient == null)
                return NotFound();

            
            var model = new Discharge
            {
                PatientId = patient.Id,
                Patient=patient

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DischargePatient(Discharge model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            var wardAdmin = await _context.WardAdmin
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Employee.UserId == user.Id);

            if (wardAdmin == null)
                return Unauthorized();

            var patient = await _context.Patient
                .Include(p => p.Room)
                .Include(p => p.Bed)
                .Include(p => p.AdmissionFolder)
                .FirstOrDefaultAsync(p => p.Id == model.PatientId && !p.IsDeleted);

            if (patient == null)
                return NotFound();

            var oldValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                patient.IsDischarged,
                patient.DateDischarged,
                patient.RoomId,
                patient.BedId,
                patient.DoctorId,
                patient.NurseId,
                patient.AdmissionFolderId
            });

            var bed = patient.Bed;
            patient.IsDischarged = true;
            patient.DateDischarged = DateTime.Now;
            patient.WardAdminId = wardAdmin.WardAdminId;

            patient.RoomId = null;
            patient.BedId = null;
            patient.DoctorId = null;
            patient.NurseId = null;
            patient.AdmissionFolderId = null;
            patient.LastDischargeDate = DateTime.Now;

            if (bed != null)
            {
                bed.IsOccupied = false;
                _context.Bed.Update(bed);
            }

            if (patient.AdmissionFolder != null)
            {
                patient.AdmissionFolder.IsActive = false;
                patient.AdmissionFolder.DateClosed = DateTime.UtcNow;
                patient.AdmissionFolder.BedId = null;
                _context.AdmissionFolder.Update(patient.AdmissionFolder);
            }

            var discharge = new Discharge
            {
                PatientId = patient.Id,
                WardAdminId = wardAdmin.WardAdminId,
                DischargeDate = DateTime.Now,
                TreatmentSummary = model.TreatmentSummary,
                FolloupInstructions = model.FolloupInstructions
            };

            _context.Patient.Update(patient);
            _context.Discharge.Add(discharge);

            var dischargeMovement = new PatientMovement
            {
                PatientId = patient.Id,
                WardAdminId = wardAdmin.WardAdminId,
                MovementType = "Discharge",
                FromBedId = bed?.BedId,
                FromRoomId = patient.RoomId,
                ToBedId = null,
                ToRoomId = null,
                MovementDate = DateTime.Now,
                MovementHistory = $"[{DateTime.Now}] Discharged from Bed {bed?.BedId} (Room {patient.RoomId}).",
                AdmissionFolderId = patient.AdmissionFolderId
            };
            _context.PatientMovement.Add(dischargeMovement);

            var pendingMoves = await _context.PatientMoveRequest
                .Where(m => m.PatientId == patient.Id && m.Status == "Pending")
                .ToListAsync();

            foreach (var move in pendingMoves)
            {
                move.Status = "Approved";
                move.ProcessedDate = DateTime.UtcNow;
                move.WardAdminId = wardAdmin.WardAdminId;
                _context.PatientMoveRequest.Update(move);
            }

            var instruction = await _context.DischargeInstruction
                .Where(d => d.PatientId == patient.Id && d.Status == "Pending")
                .FirstOrDefaultAsync();

            if (instruction != null)
            {
                instruction.Status = "Completed";
                instruction.ApprovedDate = DateTime.Now;
                instruction.ApprovedByAdminId = wardAdmin.WardAdminId;
                _context.DischargeInstruction.Update(instruction);
            }

            var admission = await _context.AdmissionFolder
                .Where(a => a.PatientId == patient.Id && a.IsActive)
                .FirstOrDefaultAsync();

            if (admission != null)
            {
                admission.IsActive = false;
                admission.DateClosed = DateTime.UtcNow;
                admission.HasPendingDischarge = false;
                _context.AdmissionFolder.Update(admission);
            }

            var history = await _context.PatientHistory
                .Where(h => h.PatientId == patient.Id && h.DischargeDate == null)
                .OrderByDescending(h => h.AdmissionDate)
                .FirstOrDefaultAsync();

            if (history != null)
            {
                history.DischargeDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var newValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                patient.IsDischarged,
                patient.DateDischarged,
                patient.RoomId,
                patient.BedId,
                patient.DoctorId,
                patient.NurseId,
                patient.AdmissionFolderId
            });

            await LogAuditAsync(
                actionTaken: "Discharge Patient",
                user: user,
                entity: "Patient",
                recordId: patient.Id.ToString(),
                oldValue: oldValue,
                newValue: newValue,
                details: $"WardAdmin {wardAdmin.Employee.FirstName} {wardAdmin.Employee.LastName} discharged patient {patient.FirstName} {patient.LastName}."
            );

            TempData["SuccessMessage"] = $"Patient {patient.FirstName} {patient.LastName} successfully discharged.";
            return RedirectToAction("WardAdminDashboard");
        }





        [HttpGet]
        public async Task<IActionResult> ReadmitPatient(int patientId)
        {
            var patient = await _context.Patient
                .Include(p => p.AdmissionFolder)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
                return NotFound();

            var wards = await _context.Ward.ToListAsync() ?? new List<Ward>();
            var rooms = await _context.Room.ToListAsync() ?? new List<Room>();
            var beds = await _context.Bed
                .Where(b => !b.IsOccupied && !b.IsDeleted)
                .ToListAsync() ?? new List<Bed>();
            var doctors = await _context.Doctor
                .Include(d => d.Employee)
                .ToListAsync();
            var nurses = await _context.Nurse
                .Include(n => n.Employee)
                .ToListAsync();

            var model = new CreateReadmissionViewModel
            {
                PatientId = patient.Id,
                PatientName = $"{patient.FirstName} {patient.LastName}",
                PreviousAdmissionFolderId = patient.AdmissionFolderId,
                PreviousAdmissionFolder = patient.AdmissionFolder,
                Rooms = rooms,
                Beds = beds,
                Doctors = doctors,
                Nurses = nurses,
                LastDischargeDate = patient.DateDischarged
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReadmitPatient(CreateReadmissionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var patient = await _context.Patient
                .Include(p => p.AdmissionFolder)
                .FirstOrDefaultAsync(p => p.Id == model.PatientId);

            if (patient == null)
                return NotFound();

            var oldValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                patient.BedId,
                patient.RoomId,
                patient.NurseId,
                patient.DoctorId,
                patient.AdmissionFolderId,
                patient.IsDischarged,
                patient.AdmissionCount,
                patient.AdmissionHistory
            });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wardAdmin = await _context.WardAdmin
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(w => w.Employee.UserId == userId);

            if (wardAdmin == null)
            {
                ModelState.AddModelError("", "Ward Admin not found.");
                return View(model);
            }

            var room = await _context.Room.FindAsync(model.RoomId);
            var bed = await _context.Bed.FindAsync(model.BedId);
            var doctor = await _context.Doctor.FindAsync(model.DoctorId);
            var nurse = await _context.Nurse.FindAsync(model.NurseId);

            if (room == null || bed == null || doctor == null || nurse == null)
            {
                ModelState.AddModelError("", "Invalid assignment selections.");
                return View(model);
            }

            patient.AdmissionCount += 1;

            var folder = new AdmissionFolder
            {
                PatientId = patient.Id,
                WardAdminId = wardAdmin.WardAdminId,
                BedId = bed.BedId,
                ReasonForAdmission = model.ReasonForReadmission,
                DateCreated = DateTime.UtcNow,
                IsActive = true,
                AdmissionCount = patient.AdmissionCount
            };
            _context.AdmissionFolder.Add(folder);

            patient.BedId = bed.BedId;
            patient.RoomId = room.RoomId;
            patient.NurseId = nurse.NurseId;
            patient.DoctorId = doctor.DoctorId;
            patient.AdmissionFolder = folder;
            patient.AdmissionFolderId = folder.AdmissionFolderId;
            patient.IsDischarged = false;
            patient.DateDischarged = null;

            patient.AdmissionHistory ??= "";
            patient.AdmissionHistory += $"Readmitted on {DateTime.UtcNow:dd/MM/yyyy HH:mm}; Reason: {model.ReasonForReadmission}\n";

            bed.IsOccupied = true;

            DateTime scheduledDate = DateTime.Today.AddHours(8);
            while (true)
            {
                bool conflict = await _context.Schedules
                    .AnyAsync(s => s.DoctorId == doctor.DoctorId &&
                                   s.ScheduledDate.Date == scheduledDate.Date &&
                                   s.ScheduledDate.Hour == scheduledDate.Hour &&
                                   !s.IsCompleted);
                if (!conflict) break;

                scheduledDate = scheduledDate.AddHours(1);
                if (scheduledDate.Hour > 17)
                    scheduledDate = scheduledDate.Date.AddDays(1).AddHours(8);
            }

            var schedule = new Schedule
            {
                DoctorId = doctor.DoctorId,
                PatientId = patient.Id,
                ScheduledDate = scheduledDate,
                VisitType = model.VisitType ?? "Consultation",
                Location = room.RoomName ?? room.RoomNumber.ToString(),
                IsCompleted = false
            };
            _context.Schedules.Add(schedule);

            _context.Patient.Update(patient);
            await _context.SaveChangesAsync();

            var newValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                patient.BedId,
                patient.RoomId,
                patient.NurseId,
                patient.DoctorId,
                patient.AdmissionFolderId,
                patient.IsDischarged,
                patient.AdmissionCount,
                patient.AdmissionHistory
            });


            await LogAuditAsync(
                actionTaken: "Readmit Patient",
                user: wardAdmin.Employee.User,
                entity: "Patient",
                recordId: patient.Id.ToString(),
                oldValue: oldValue,
                newValue: newValue,
                details: $"WardAdmin {wardAdmin.Employee.FirstName} {wardAdmin.Employee.LastName} readmitted patient {patient.FirstName} {patient.LastName}."
            );

            TempData["SuccessMessage"] = "Patient readmitted successfully and doctor assigned.";
            return RedirectToAction("WardAdminDashboard");
        }






        [HttpGet]
        public async Task<IActionResult> ManageBeds()
        {
            var beds = await _context.Bed
                .Include(b => b.Room)
                    .ThenInclude(r => r.Ward)
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Room.RoomNumber)
                .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wardAdmin = await _context.WardAdmin
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(w => w.Employee.UserId == userId);

            if (wardAdmin != null)
            {
                await LogAuditAsync(
                    actionTaken: "View Beds",
                    user: wardAdmin.Employee.User,
                    entity: "Bed",
                    details: $"WardAdmin {wardAdmin.Employee.FirstName} {wardAdmin.Employee.LastName} viewed the bed management page."
                );
            }

            return View(beds);
        }





        [HttpGet]
        public async Task<IActionResult> ManageWards()
        {
            var wards = await _context.Ward
                .Include(w => w.NurseSister)
                    .ThenInclude(ns => ns.Employee)
                .ToListAsync();

            var nurseSisters = await _context.NurseSister
                .Where(n => !n.IsDeleted && n.IsAvail)
                .Include(n => n.Employee)
                .ToListAsync();

            var nurses = await _context.Nurse
                .Include(n => n.Employee)
                .Include(n => n.Patients)
                    .ThenInclude(p => p.Room)
                .ToListAsync();

            var model = wards.Select(w => new ManageWardViewModel
            {
                WardId = w.WardId,
                WardName = w.WardName,
                Location = w.Location,
                Capacity = w.Capacity,
                Description = w.Description,

                NurseSisterId = w.NurseSisterId,
                NurseSisterName = w.NurseSister != null
                    ? $"{w.NurseSister.FirstName} {w.NurseSister.LastName}"
                    : "Unassigned",

                NurseSisters = nurseSisters.Select(n => new SelectListItem
                {
                    Value = n.NurseSisterId.ToString(),
                    Text = $"{n.FirstName} {n.LastName}"
                }),

                Nurses = nurses
                    .Where(n => n.Patients.Any(p =>
                        p.Room != null &&
                        p.Room.WardId == w.WardId &&
                        !p.IsDeleted &&
                        !p.IsDischarged))
                    .Distinct()
                    .ToList()
            }).ToList();

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignNurseSister(int wardId, int nurseSisterId)
        {
            var ward = await _context.Ward
                .Include(w => w.NurseSister)
                .FirstOrDefaultAsync(w => w.WardId == wardId);

            var nurse = await _context.NurseSister
                .FirstOrDefaultAsync(n => n.NurseSisterId == nurseSisterId && n.IsAvail && !n.IsDeleted);

            if (ward == null || nurse == null)
            {
                TempData["ErrorMessage"] = "Ward or Nurse Sister not found.";
                return RedirectToAction(nameof(ManageWards));
            }

            if (ward.NurseSisterId != null)
            {
                TempData["ErrorMessage"] = "This ward already has a Nurse Sister assigned.";
                return RedirectToAction(nameof(ManageWards));
            }

            ward.NurseSisterId = nurse.NurseSisterId;
            _context.Ward.Update(ward);

            var history = new WardNurseAssignment
            {
                WardId = ward.WardId,
                NurseSisterId = nurse.NurseSisterId,
                AssignedDate = DateTime.UtcNow
            };
            _context.WardNurseAssignment.Add(history);

            await _context.SaveChangesAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wardAdmin = await _context.WardAdmin
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(w => w.Employee.UserId == userId);

            if (wardAdmin != null)
            {
                await LogAuditAsync(
                    actionTaken: "Assign Nurse Sister",
                    user: wardAdmin.Employee.User,
                    entity: "Ward",
                    recordId: ward.WardId.ToString(),
                    newValue: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        WardId = ward.WardId,
                        WardName = ward.WardName,
                        NurseSisterId = nurse.NurseSisterId,
                        NurseSisterName = $"{nurse.FirstName} {nurse.LastName}"
                    }),
                    details: $"WardAdmin {wardAdmin.Employee.FirstName} {wardAdmin.Employee.LastName} assigned Nurse Sister {nurse.FirstName} {nurse.LastName} to Ward {ward.WardName}."
                );
            }

            TempData["SuccessMessage"] = $"Nurse Sister {nurse.FirstName} {nurse.LastName} assigned to Ward {ward.WardName}.";
            return RedirectToAction(nameof(ManageWards));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignNurseSister(int wardId)
        {
            var ward = await _context.Ward
                .Include(w => w.NurseSister)
                .FirstOrDefaultAsync(w => w.WardId == wardId);

            if (ward == null || ward.NurseSisterId == null)
            {
                TempData["ErrorMessage"] = "Ward has no Nurse Sister assigned.";
                return RedirectToAction(nameof(ManageWards));
            }

            var lastAssignment = await _context.WardNurseAssignment
                .Where(h => h.WardId == ward.WardId && h.NurseSisterId == ward.NurseSisterId && h.UnassignedDate == null)
                .OrderByDescending(h => h.AssignedDate)
                .FirstOrDefaultAsync();

            if (lastAssignment != null)
            {
                lastAssignment.UnassignedDate = DateTime.UtcNow;
                _context.WardNurseAssignment.Update(lastAssignment);
            }

            var nurse = ward.NurseSister; 

            ward.NurseSisterId = null;
            _context.Ward.Update(ward);

            await _context.SaveChangesAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wardAdmin = await _context.WardAdmin
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(w => w.Employee.UserId == userId);

            if (wardAdmin != null && nurse != null)
            {
                await LogAuditAsync(
                    actionTaken: "Unassign Nurse Sister",
                    user: wardAdmin.Employee.User,
                    entity: "Ward",
                    recordId: ward.WardId.ToString(),
                    oldValue: System.Text.Json.JsonSerializer.Serialize(new
                    {
                        WardId = ward.WardId,
                        WardName = ward.WardName,
                        NurseSisterId = nurse.NurseSisterId,
                        NurseSisterName = $"{nurse.FirstName} {nurse.LastName}"
                    }),
                    details: $"WardAdmin {wardAdmin.Employee.FirstName} {wardAdmin.Employee.LastName} unassigned Nurse Sister {nurse.FirstName} {nurse.LastName} from Ward {ward.WardName}."
                );
            }

            TempData["SuccessMessage"] = $"Nurse Sister unassigned from Ward {ward.WardName}.";
            return RedirectToAction(nameof(ManageWards));
        }




        [HttpGet]
        public async Task<IActionResult> ManageAllPatients(string? search)
        {
            var patientsQuery = _context.Patient
                .Include(p => p.Bed)
                    .ThenInclude(b => b.Room)
                        .ThenInclude(r => r.Ward)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                patientsQuery = patientsQuery.Where(p =>
                    p.FirstName.Contains(search) ||
                    p.LastName.Contains(search) ||
                    p.IdNumber.Contains(search));
            }
            var patients = await patientsQuery
                .OrderBy(p => p.LastName)
                .ToListAsync();

            ViewBag.Search = search;

            return View(patients);
        }


        [HttpGet]
        public async Task<IActionResult> ManagePatientMovement(int patientId)
        {
            var patient = await _context.Patient
                .Include(p => p.Room)
                .Include(p => p.Bed)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
                return NotFound();

            var wardAdminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wardAdmin = await _context.WardAdmin
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(w => w.Employee.UserId == wardAdminUserId);

            if (wardAdmin == null)
            {
                TempData["ErrorMessage"] = "Ward Admin not found.";
                return RedirectToAction("WardAdminDashboard");
            }

            var movementHistory = await _context.PatientMovement
                .Where(m => m.PatientId == patientId)
                .Include(m => m.FromBed)
                .Include(m => m.ToBed)
                .Include(m => m.FromRoom)
                .Include(m => m.ToRoom)
                .Include(m => m.WardAdmin)
                    .ThenInclude(w => w.Employee)
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();

            var rooms = (await _context.Room
                .Select(r => new
                {
                    RoomId = r.RoomId,
                    RoomName = r.RoomName,
                    RoomNumber = r.RoomNumber
                })
                .ToListAsync())
                .Cast<dynamic>()
                .ToList();

            var beds = (await _context.Bed
                .Where(b => !b.IsOccupied && !b.IsDeleted)
                .Select(b => new
                {
                    BedId = b.BedId,
                    BedNumber = b.BedNumber,
                    BedType = b.BedType,
                    RoomId = b.RoomId,
                    IsOccupied = b.IsOccupied,
                    IsDeleted = b.IsDeleted
                })
                .ToListAsync())
                .Cast<dynamic>()
                .ToList();

            ViewBag.Patient = patient;
            ViewBag.MovementHistory = movementHistory;
            ViewBag.Rooms = rooms;   
            ViewBag.Beds = beds;

            var model = new Imnandi_NovaCare_Hospital_System.Models.PatientMovement
            {
                PatientId = patientId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManagePatientMovement(PatientMovement model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input. Please check and try again.";
                return RedirectToAction(nameof(ManagePatientMovement), new { patientId = model.PatientId });
            }

            var patient = await _context.Patient
                .Include(p => p.Bed)
                .Include(p => p.Room)
                .Include(p => p.AdmissionFolder)
                .FirstOrDefaultAsync(p => p.Id == model.PatientId);

            if (patient == null)
                return NotFound();

            var wardAdminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wardAdmin = await _context.WardAdmin
                .Include(w => w.Employee)
                .FirstOrDefaultAsync(w => w.Employee.UserId == wardAdminUserId);

            if (wardAdmin == null)
            {
                TempData["ErrorMessage"] = "Ward Admin not found.";
                return RedirectToAction("WardAdminDashboard");
            }

            if (model.ToBedId.HasValue)
            {
                var targetBed = await _context.Bed.FindAsync(model.ToBedId.Value);
                if (targetBed == null || targetBed.IsOccupied || targetBed.IsDeleted)
                {
                    TempData["ErrorMessage"] = "Selected bed is not available.";
                    return RedirectToAction(nameof(ManagePatientMovement), new { patientId = model.PatientId });
                }
            }

            var movement = new PatientMovement
            {
                PatientId = patient.Id,
                WardAdminId = wardAdmin.WardAdminId,
                MovementType = model.MovementType,
                FromBedId = patient.BedId,
                FromRoomId = patient.RoomId,
                ToBedId = model.ToBedId,
                ToRoomId = model.ToRoomId,
                MovementDate = DateTime.Now,
                ReturnTime = model.IsTemporaryMove ? model.ReturnTime : null, 
                AdmissionFolderId = patient.AdmissionFolderId
            };

            if (patient.BedId.HasValue)
            {
                var oldBed = await _context.Bed.FindAsync(patient.BedId.Value);
                if (oldBed != null)
                    oldBed.IsOccupied = false;
            }

            if (model.ToBedId.HasValue)
            {
                var newBed = await _context.Bed.FindAsync(model.ToBedId.Value);
                if (newBed != null)
                    newBed.IsOccupied = true;
            }

            patient.BedId = model.ToBedId;
            patient.RoomId = model.ToRoomId;

            if (patient.AdmissionFolder != null)
            {
                patient.AdmissionFolder.BedId = model.ToBedId;
                patient.AdmissionFolder.WardAdminId = wardAdmin.WardAdminId;
            }

            movement.MovementHistory = $"[{DateTime.Now}] {movement.MovementType}: " +
                               $"Moved from Bed {movement.FromBedId} (Room {movement.FromRoomId}) " +
                               $"to Bed {movement.ToBedId} (Room {movement.ToRoomId}).";

            if (movement.IsTemporaryMove && movement.ReturnTime.HasValue)
            {
                movement.MovementHistory += $" Scheduled to return at {movement.ReturnTime.Value}.";
            }

            _context.PatientMovement.Add(movement);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Patient Movement",
                user: wardAdmin.Employee.User,
                entity: "Patient",
                recordId: patient.Id.ToString(),
                oldValue: System.Text.Json.JsonSerializer.Serialize(new
                {
                    BedId = movement.FromBedId,
                    RoomId = movement.FromRoomId
                }),
                newValue: System.Text.Json.JsonSerializer.Serialize(new
                {
                    BedId = movement.ToBedId,
                    RoomId = movement.ToRoomId
                }),
                details: $"Patient {patient.FirstName} {patient.LastName} moved by WardAdmin {wardAdmin.Employee.FirstName} {wardAdmin.Employee.LastName}. MovementType: {movement.MovementType}, Temporary: {movement.IsTemporaryMove}"
            );

            TempData["SuccessMessage"] = $"Patient {patient.FirstName} {patient.LastName} moved successfully.";
            return RedirectToAction(nameof(ManagePatientMovement), new { patientId = model.PatientId });
        }






        [HttpGet]
        public async Task<IActionResult> ManageWardStock(int? wardId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wardAdmin = await _context.WardAdmin
                .Include(wa => wa.Employee)
                .FirstOrDefaultAsync(wa =>
                    wa.Employee.UserId == user.Id &&
                    !wa.IsDeleted);

            if (wardAdmin == null)
            {
                TempData["Error"] = "Ward Admin not found.";
                return RedirectToAction(nameof(WardAdminDashboard));
            }

            var wards = await _context.Ward
                .Where(w => !w.IsDeleted)
                .OrderBy(w => w.WardName)
                .ToListAsync();

            if (!wardId.HasValue)
            {
                ViewBag.Wards = wards;
                ViewBag.SelectedWardId = null;
                ViewBag.SelectedWardName = "";

                ViewBag.WardStockTransactions =
                    new List<WardStockTransaction>();

                ViewBag.WardMedicationTransactions =
                    new List<WardMedicationTransaction>();

                return View(
                    (Consumables: new List<WardStock>(),
                     Medications: new List<WardMedicationStock>())
                );
            }

            var selectedWard = await _context.Ward
                .FirstOrDefaultAsync(w =>
                    w.WardId == wardId.Value &&
                    !w.IsDeleted);

            if (selectedWard == null)
            {
                TempData["Error"] = "The selected ward could not be found.";

                ViewBag.Wards = wards;
                ViewBag.SelectedWardId = null;
                ViewBag.SelectedWardName = "";

                ViewBag.WardStockTransactions =
                    new List<WardStockTransaction>();

                ViewBag.WardMedicationTransactions =
                    new List<WardMedicationTransaction>();

                return View(
                    (Consumables: new List<WardStock>(),
                     Medications: new List<WardMedicationStock>())
                );
            }

            var wardStocks = await _context.WardStocks
                .Include(ws => ws.Ward)
                .Include(ws => ws.Consumable)
                .Where(ws =>
                    ws.WardId == selectedWard.WardId &&
                    ws.Consumable != null &&
                    !ws.Consumable.IsDeleted)
                .OrderBy(ws => ws.Consumable.ConsumableName)
                .ToListAsync();

            var wardMedicationStocks = await _context.WardMedicationStocks
                .Include(wms => wms.Ward)
                .Include(wms => wms.Medication)
                .Where(wms =>
                    wms.WardId == selectedWard.WardId &&
                    wms.Medication != null &&
                    !wms.Medication.IsDeleted)
                .OrderBy(wms => wms.Medication.MedicationName)
                .ToListAsync();

            var wardStockTransactions = await _context.WardStockTransactions
                .Include(t => t.Consumable)
                .Include(t => t.Ward)
                .Where(t =>
                    t.WardId == selectedWard.WardId &&
                    !t.IsDeleted)
                .OrderByDescending(t => t.DateReceived)
                .ToListAsync();

            var wardMedicationTransactions = await _context.WardMedicationTransactions
                .Include(t => t.Medication)
                .Include(t => t.Ward)
                .Where(t =>
                    t.WardId == selectedWard.WardId &&
                    !t.IsDeleted)
                .OrderByDescending(t => t.DateReceived)
                .ToListAsync();

            ViewBag.Wards = wards;
            ViewBag.SelectedWardId = selectedWard.WardId;
            ViewBag.SelectedWardName = selectedWard.WardName;

            ViewBag.WardStockTransactions =
                wardStockTransactions;

            ViewBag.WardMedicationTransactions =
                wardMedicationTransactions;

            return View(
                (Consumables: wardStocks,
                 Medications: wardMedicationStocks)
            );
        }

    }
}