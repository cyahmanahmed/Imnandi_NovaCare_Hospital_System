using Imnandi_NovaCare_Hospital_System.Data;
using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Imnandi_NovaCare_Hospital_System.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public DoctorController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> DoctorDashboard(string? search)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctor
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Employee.UserId == user.Id);

            if (doctor == null)
                return Unauthorized();
            var patientsQuery = _context.Patient
                .Where(p => p.DoctorId == doctor.DoctorId && !p.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                bool isId = int.TryParse(search, out int patientId);

                patientsQuery = patientsQuery.Where(p =>
                    p.IdNumber.Contains(search) ||
                    p.FirstName.Contains(search) ||
                    p.LastName.Contains(search)
                );
            }

            var patients = await patientsQuery.ToListAsync();
            var totalPatients = patients.Count;

            var pendingDischarges = await _context.DischargeInstruction
                .Include(di => di.Patient)
                .Where(di => di.Status == "Pending")
                .OrderBy(di => di.IssuedDate)
                .ToListAsync();

            var model = new DoctorDashboardViewModel
            {
                DoctorId = doctor.DoctorId,
                EmployeeId = doctor.EmployeeId,
                FirstName = doctor.Employee.FirstName,
                LastName = doctor.Employee.LastName,
                JobTitle = doctor.Employee.JobTitle,
                Specialty = doctor.Department,
                Department = doctor.Employee.Department,
                PhoneNumber = doctor.Employee.PhoneNumber,
                Email = user.Email,

                PatientCount = totalPatients,
                Patients = patients,
                PendingDischarges = pendingDischarges
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
            var model = new DoctorDashboardViewModel
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
        public async Task<IActionResult> Profile(DoctorDashboardViewModel model)
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
            var doctor = await _context.Doctor
                .FirstOrDefaultAsync(d => d.EmployeeId == employee.Id);

            if (doctor != null)
            {
                doctor.FirstName = model.FirstName;
                doctor.LastName = model.LastName;

                _context.Doctor.Update(doctor);
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
                    details: $"Dr. {user.FirstName} {user.LastName} updated their profile."
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
                    details: $"Dr. {user.FirstName} {user.LastName} successfully changed their password."
                );

                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction("DoctorDashboard");
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
        public async Task<IActionResult> Schedule()
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return Unauthorized();

            var upcomingVisits = await _context.Schedule
                .Include(s => s.Patient)
                .Where(s => s.DoctorId == doctor.DoctorId && !s.IsCompleted && s.ScheduledDate.Date >= DateTime.Today)
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();

            ViewBag.StartDate = null;
            ViewBag.EndDate = null;
            return View(upcomingVisits);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(DateTime? startDate, DateTime? endDate)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return Unauthorized();

            var oldFilter = new
            {
                StartDate = ViewBag.StartDate,
                EndDate = ViewBag.EndDate
            };

            var query = _context.Schedule
                .Include(s => s.Patient)
                .Where(s => s.DoctorId == doctor.DoctorId && !s.IsCompleted && !s.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(s => s.ScheduledDate.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(s => s.ScheduledDate.Date <= endDate.Value.Date);

            var filteredVisits = await query
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();

            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await LogAuditAsync(
                    actionTaken: "Schedule Filtered",
                    user: user,
                    entity: "Schedule",
                    recordId: doctor.DoctorId.ToString(),
                    oldValue: System.Text.Json.JsonSerializer.Serialize(oldFilter),
                    newValue: $"StartDate={ViewBag.StartDate}, EndDate={ViewBag.EndDate}",
                    details: $"Dr. {user.FirstName} {user.LastName} filtered their schedule."
                );
            }

            return View(filteredVisits);
        }
        private async Task<Doctor> GetCurrentDoctorAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            return await _context.Doctor
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Employee.UserId == user.Id);
        }










        [HttpGet]
        public async Task<IActionResult> VisitHistory()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var doctor = await _context.Doctor
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Employee.UserId == user.Id);

            if (doctor == null)
                return NotFound("Doctor not found.");

            var visits = await _context.Visit
                .Include(v => v.Patient)
                .Where(v => v.DoctorId == doctor.DoctorId)
                .OrderByDescending(v => v.VisitDateTime)
                .ToListAsync();

            return View(visits);
        }











        public async Task<IActionResult> RecentPrescription()
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctor
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Employee.UserId == user.Id);

            if (doctor == null)
            {
                await LogAuditAsync(
                    actionTaken: "Access Denied",
                    user: user,
                    entity: "Prescription",
                    details: "Doctor attempted to access RecentPrescription but was not found."
                );
                return Unauthorized();
            }

            var prescriptions = await _context.Prescription
                .Include(p => p.Patient)
                .Include(p => p.Medication)
                .Include(p => p.ScriptManager)
                    .ThenInclude(s => s.Employee)
                .Where(p => p.DoctorId == doctor.DoctorId && !p.IsDeleted)
                .OrderByDescending(p => p.IssueDate)
                .ThenByDescending(p => p.IssueTime)
                .ToListAsync();

            await LogAuditAsync(
                actionTaken: "View Prescriptions",
                user: user,
                entity: "Prescription",
                details: $"Doctor {doctor.Employee.FirstName} {doctor.Employee.LastName} viewed their prescriptions."
            );

            return View(prescriptions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecentPrescription(int? patientId, string? status)
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctor
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Employee.UserId == user.Id);

            if (doctor == null)
            {
                await LogAuditAsync(
                    actionTaken: "View Prescriptions Failed",
                    user: user,
                    entity: "Prescription",
                    details: "Doctor not found when attempting to filter prescriptions."
                );
                return Unauthorized();
            }

            var query = _context.Prescription
                .Include(p => p.Patient)
                .Include(p => p.Medication)
                .Include(p => p.ScriptManager)
                    .ThenInclude(s => s.Employee)
                .Where(p => p.DoctorId == doctor.DoctorId && !p.IsDeleted)
                .AsQueryable();

            string patientName = "All patients";

            if (patientId.HasValue)
            {
                var patient = await _context.Patient.FirstOrDefaultAsync(p => p.Id == patientId.Value);
                if (patient != null)
                {
                    patientName = patient.FirstName + " " + patient.LastName;
                    query = query.Where(p => p.PatientId == patientId.Value);
                }
            }

            if (!string.IsNullOrEmpty(status))
                query = query.Where(p => p.Status == status);

            var prescriptions = await query
                .OrderByDescending(p => p.IssueDate)
                .ThenByDescending(p => p.IssueTime)
                .ToListAsync();
 
            await LogAuditAsync(
                actionTaken: "View Prescriptions",
                user: user,
                entity: "Prescription",
                details: $"Doctor viewed prescriptions for {patientName}" +
                         (string.IsNullOrEmpty(status) ? "" : $" with status '{status}'")
            );

            return View(prescriptions);
        }












        public async Task<IActionResult> PatientsFolder(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var patient = await _context.Patient
                .Include(p => p.AdmissionFolder)
                    .ThenInclude(f => f.Bed)
                        .ThenInclude(b => b.Room)
                            .ThenInclude(r => r.Ward)
                .Include(p => p.Allergies)
                .Include (p => p.MedicalHistory)
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
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (patient == null)
                return NotFound();
            var user = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "View Patient Folder",
                user: user,
                entity: "Patient",
                recordId: patient.Id.ToString(),
                details: $"Doctor viewed patient folder: {patient.FirstName} {patient.LastName}"
            );
            if (patient.AdmissionFolder == null)
            {
                TempData["Info"] = "This patient does not have an active admission folder.";
            }
            var admissionFolder = patient.AdmissionFolder;
            admissionFolder.Patient = patient;
            return View(admissionFolder);
        }












        [HttpGet]
        public async Task<IActionResult> ScheduleVisit(int id) 
        {
            var patient = await _context.Patient
                .Include(p => p.Doctor)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var user = await _context.User
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctor
                .FirstOrDefaultAsync(d => d.EmployeeId == user.Employee.Id);

            if (doctor == null)
                return Unauthorized();

            var rooms = await _context.Room
                .Include(r => r.Ward)
                .OrderBy(r => r.Ward.WardName)
                .ThenBy(r => r.RoomName) 
                .Select(r => new SelectListItem
                {
                    Value = r.RoomName, 
                    Text = r.Ward != null
                           ? $"{r.Ward.WardName} - {r.RoomName}"
                           : r.RoomName
                })
                .ToListAsync();

            ViewBag.Rooms = rooms;

            var model = new Schedule
            {
                DoctorId = doctor.DoctorId,
                PatientId = patient.Id,
                ScheduledDate = DateTime.Now.Date.AddDays(1).AddHours(8),
                Location = patient.Room != null ? $"Room {patient.Room.RoomNumber}" : "Ward A",
                VisitType = "Follow-up"
            };

            var schedule = await _context.Schedule
                .Where(s => s.DoctorId == doctor.DoctorId)
                .Include(s => s.Patient)
                .Select(s => new
                {
                    ScheduledDate = s.ScheduledDate,
                    s.VisitType,
                    PatientName = s.Patient.FirstName + " " + s.Patient.LastName
                })
                .ToListAsync();

            ViewBag.Schedule = schedule;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleVisit(Schedule model, int selectedHour)
        {
            if (!ModelState.IsValid)
                return View(model);

            var doctor = await _context.Doctor
                .Include(d => d.Employee)  
                .FirstOrDefaultAsync(d => d.DoctorId == model.DoctorId);
            var patient = await _context.Patient.FindAsync(model.PatientId);

            if (doctor == null || patient == null)
            {
                ModelState.AddModelError("", "Invalid doctor or patient.");
                return View(model);
            }

            if (model.ScheduledDate.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError("", "You cannot schedule a visit in the past.");
                return View(model);
            }

            var selectedDate = model.ScheduledDate.Date;

            var dailySchedule = await _context.Schedule
                .Where(s => s.DoctorId == doctor.DoctorId && s.ScheduledDate.Date == selectedDate)
                .ToListAsync();

            var allSlots = Enumerable.Range(8, 10).ToList(); 
            var bookedSlots = dailySchedule.Select(s => s.ScheduledDate.Hour).ToList();
            var freeSlots = allSlots.Except(bookedSlots).ToList();

            if (!freeSlots.Contains(selectedHour))
            {
                ModelState.AddModelError("", $"Selected time slot ({selectedHour}:00) is not available. Please choose another slot.");
                
                var existingSchedules = await _context.Schedule
                    .Where(s => s.DoctorId == doctor.DoctorId)
                    .Include(s => s.Patient)
                    .Select(s => new
                    {
                        ScheduledDate = s.ScheduledDate,
                        s.VisitType,
                        PatientName = s.Patient.FirstName + " " + s.Patient.LastName
                    })
                    .ToListAsync();
                ViewBag.Schedule = existingSchedules;
                return View(model);
            }

            model.ScheduledDate = selectedDate.AddHours(selectedHour);
            model.IsCompleted = false;

            _context.Schedule.Add(model);

            doctor.IsAvail = false;

            patient.DateDischarged = null;

            _context.Doctor.Update(doctor);
            _context.Patient.Update(patient);

            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            await LogAuditAsync(
                actionTaken: "Scheduled Visit",
                user: user,
                entity: "Schedule",
                recordId: model.ScheduleId.ToString(),
                details: $"Doctor {doctor.Employee.FirstName} {doctor.Employee.LastName} scheduled a visit for patient {patient.FirstName} {patient.LastName} on {model.ScheduledDate:yyyy-MM-dd HH:mm}"
            );

            TempData["SuccessMessage"] = $"Visit for {patient.FirstName} {patient.LastName} scheduled on {model.ScheduledDate:yyyy-MM-dd HH:mm}";
            return RedirectToAction("ScheduleVisit", new { id = model.PatientId });
        }














        public async Task<IActionResult> GiveInstruction(int id)
        {
            var patient = await _context.Patient
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.Employee)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (patient == null)
                return NotFound();

            var instruction = new Instruction
            {
                PatientId = patient.Id,
                CreatedAt = DateTime.UtcNow
            };

            ViewBag.Patient = patient;
            ViewBag.Message = $"Creating instruction for patient: {patient.FirstName} {patient.LastName}";

            return View(instruction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiveInstruction(Instruction instruction)
        {
            if (!ModelState.IsValid)
                return View(instruction);

            var doctor = await _userManager.GetUserAsync(User);
            var doctorEntity = await _context.Doctor
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Employee.UserId == doctor.Id);

            if (doctorEntity == null)
                return Unauthorized();

            instruction.DoctorId = doctorEntity.DoctorId;
            instruction.CreatedAt = DateTime.UtcNow;
            instruction.isActive = true;

            _context.Instruction.Add(instruction);
            await _context.SaveChangesAsync();

            var patient = await _context.Patient
                .FirstOrDefaultAsync(p => p.Id == instruction.PatientId);

            var oldValue = "{}"; 
            var newValue = new
            {
                DoctorName = doctorEntity.Employee.FirstName + " " + doctorEntity.Employee.LastName,
                PatientName = patient != null ? patient.FirstName + " " + patient.LastName : "Unknown",
                instruction.Title,
                instruction.CreatedAt
            };

            await LogAuditAsync(
                actionTaken: "Instruction Created",
                user: doctor,
                entity: "Instruction",
                recordId: instruction.InstructionId.ToString(),
                oldValue: oldValue,
                newValue: System.Text.Json.JsonSerializer.Serialize(newValue),
                details: $"Doctor {doctorEntity.Employee.FirstName} {doctorEntity.Employee.LastName} added an instruction for patient {patient?.FirstName} {patient?.LastName}."
            );

            TempData["SuccessMessage"] = "Instruction created successfully.";
            return RedirectToAction("PatientsFolder", "Doctor", new { id = instruction.PatientId });
        }















        public async Task<IActionResult> PrescribeMedication(int id)
        {
            var patient = await _context.Patient
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Medication)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.Employee)
                .Include(p => p.Allergies)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (patient == null)
                return NotFound();

            var medications = await _context.Medication
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.MedicationName)
                .ToListAsync();

            ViewBag.Patient = patient;
            ViewBag.Medications = medications.Select(m => new SelectListItem
            {
                Value = m.MedicationId.ToString(),
                Text = m.MedicationName
            }).ToList();

            var now = DateTime.Now; 
            var prescription = new Prescription
            {
                PatientId = patient.Id,
                DoctorId = patient.DoctorId ?? 0,
                IssueDate = DateOnly.FromDateTime(now),
                IssueTime = TimeOnly.FromDateTime(now)
            };

            return View(prescription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrescribeMedication(Prescription model)
        {
            if (model.Quantity < 1)
            {
                ModelState.AddModelError(
                    nameof(model.Quantity),
                    "Quantity must be at least 1."
                );
            }

            if (!ModelState.IsValid)
            {
                var medications = await _context.Medication
                    .Where(m => !m.IsDeleted)
                    .OrderBy(m => m.MedicationName)
                    .ToListAsync();

                ViewBag.Medications = medications.Select(m => new SelectListItem
                {
                    Value = m.MedicationId.ToString(),
                    Text = m.MedicationName
                }).ToList();

                var patient = await _context.Patient
                    .FirstOrDefaultAsync(
                        p => p.Id == model.PatientId && !p.IsDeleted
                    );

                ViewBag.Patient = patient;

                return View(model);
            }

            var now = DateTime.Now;

            if (model.IssueDate == default)
                model.IssueDate = DateOnly.FromDateTime(now);

            if (model.IssueTime == default)
                model.IssueTime = TimeOnly.FromDateTime(now);

            model.Status = "Pending";

            _context.Prescription.Add(model);
            await _context.SaveChangesAsync();

            var md = new MedicationAdministration
            {
                PrescriptionId = model.PrescriptionId,
                MedicationId = model.MedicationId,
                Purpose = model.DosageInstructions,
                Dosage = model.DosageInstructions,
                AdministrationTime = model.IssueDate,
                DoctorId = model.DoctorId,
                PatientId = model.PatientId
            };

            _context.MedicationAdministration.Add(md);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);

            var doctor = await _context.Doctor
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Employee.UserId == user.Id);

            var patientEntity = await _context.Patient
                .FirstOrDefaultAsync(p => p.Id == model.PatientId);

            var oldValue = "{}"; 
            var newValue = new
            {
                DoctorName = doctor != null
                    ? doctor.Employee.FirstName + " " + doctor.Employee.LastName
                    : "Unknown",

                PatientName = patientEntity != null
                    ? patientEntity.FirstName + " " + patientEntity.LastName
                    : "Unknown",

                model.MedicationId,
                model.Quantity,
                model.DosageInstructions,
                model.Status,
                model.IssueDate,
                model.IssueTime
            };

            await LogAuditAsync(
                actionTaken: "Medication Prescribed",
                user: user,
                entity: "Prescription",
                recordId: model.PrescriptionId.ToString(),
                oldValue: oldValue,
                newValue: System.Text.Json.JsonSerializer.Serialize(newValue),
                details:
                    $"Doctor {doctor?.Employee.FirstName} {doctor?.Employee.LastName} " +
                    $"prescribed {model.Quantity} unit(s) of medication to patient " +
                    $"{patientEntity?.FirstName} {patientEntity?.LastName}."
            );

            TempData["SuccessMessage"] = "Medication prescribed successfully.";

            return RedirectToAction(
                "PatientsFolder",
                "Doctor",
                new { id = model.PatientId }
            );
        }














        [HttpGet]
        public async Task<IActionResult> TreatPatient(int id)
        {
            var patient = await _context.Patient
                .Include(p => p.Treatments)
                .Include(p => p.AdmissionFolder)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TreatPatient(int patientId, string treatmentType, string description)
        {
            if (string.IsNullOrWhiteSpace(treatmentType))
            {
                ModelState.AddModelError("TreatmentType", "Treatment type is required.");
            }

            if (!ModelState.IsValid)
            {
                var patient = await _context.Patient
                    .Include(p => p.Treatments)
                    .Include(p => p.Doctor)
                        .ThenInclude(d => d.Employee)
                    .FirstOrDefaultAsync(p => p.Id == patientId);
                return View(patient);
            }

            var pdoctor = await _context.Patient
                .Include(p => p.Doctor)
                .ThenInclude(d => d.Employee)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (pdoctor == null || pdoctor.Doctor == null)
            {
                return NotFound();
            }

            var treatment = new Treatment
            {
                PatientId = patientId,
                DoctorId = pdoctor.DoctorId.Value,
                TreatmentDate = DateOnly.FromDateTime(DateTime.Now),
                TreatmentType = treatmentType,
                Description = description
            };

            var visit = new Visit
            {
                PatientId = patientId,
                DoctorId = pdoctor.DoctorId.Value,
                VisitDateTime = DateTime.Now,
                Purpose = treatmentType
            };

            _context.Visit.Add(visit);
            _context.Treatment.Add(treatment);
            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctor
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.DoctorId == pdoctor.DoctorId);

            var patientEntity = await _context.Patient
                .FirstOrDefaultAsync(p => p.Id == patientId);

            var newValue = new
            {
                DoctorName = doctor != null ? doctor.Employee.FirstName + " " + doctor.Employee.LastName : "Unknown",
                PatientName = patientEntity != null ? patientEntity.FirstName + " " + patientEntity.LastName : "Unknown",
                treatment.TreatmentType,
                treatment.Description,
                treatment.TreatmentDate
            };

            await LogAuditAsync(
                actionTaken: "Patient Treated",
                user: user,
                entity: "Treatment",
                recordId: treatment.TreatmentID.ToString(),
                oldValue: "{}",
                newValue: System.Text.Json.JsonSerializer.Serialize(newValue),
                details: $"Doctor {doctor?.Employee.FirstName} {doctor?.Employee.LastName} treated patient {patientEntity?.FirstName} {patientEntity?.LastName}."
            );

            return RedirectToAction("TreatPatient", new { id = patientId });
        }










        public async Task<IActionResult> IssueDischarge(int id)
        {
            var patient = await _context.Patient
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (patient == null)
                return NotFound();

            var model = new DischargeInstruction
            {
                PatientId = patient.Id,
                Patient = patient,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueDischarge(DischargeInstruction model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctor
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Employee.UserId == user.Id);

            if (doctor == null)
                return Unauthorized();

            var patient = await _context.Patient.FirstOrDefaultAsync(p => p.Id == model.PatientId && !p.IsDeleted);
            if (patient == null)
                return NotFound();

            var discharge = new DischargeInstruction
            {
                DoctorId = doctor.DoctorId,
                PatientId = patient.Id,
                Reason = model.Reason,
                Status = "Pending",
                IssuedDate = DateTime.Now
            };

            var admissionFolder = await _context.AdmissionFolder.FirstOrDefaultAsync(f => f.PatientId == patient.Id && f.IsActive);
            if (admissionFolder != null)
            {
                admissionFolder.HasPendingDischarge = true;
                _context.AdmissionFolder.Update(admissionFolder);
            }

            _context.DischargeInstruction.Add(discharge);
            await _context.SaveChangesAsync();

            var userEntity = await _userManager.GetUserAsync(User);

            var newValue = new
            {
                DoctorName = doctor.Employee.FirstName + " " + doctor.Employee.LastName,
                PatientName = patient.FirstName + " " + patient.LastName,
                discharge.Reason,
                discharge.Status,
                discharge.IssuedDate
            };

            await LogAuditAsync(
                actionTaken: "Discharge Issued",
                user: userEntity,
                entity: "DischargeInstruction",
                recordId: discharge.Id.ToString(),
                oldValue: "{}",
                newValue: System.Text.Json.JsonSerializer.Serialize(newValue),
                details: $"Doctor {doctor.Employee.FirstName} {doctor.Employee.LastName} issued a discharge instruction for patient {patient.FirstName} {patient.LastName}."
            );

            TempData["SuccessMessage"] = $"Discharge instruction issued for patient {patient.FirstName} {patient.LastName}.";
            return RedirectToAction("DoctorDashboard");
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

                        (a.UserId == null && a.TargetRole == user.Role) || (a.UserId == null && a.TargetRole == null)
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











        [HttpGet]
        public async Task<IActionResult> TrackMovement(int id)
        {
            var patient = await _context.Patient
                .Include(p => p.Room)
                .Include(p => p.Bed)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            await LogAuditAsync(
                actionTaken: "ViewPatientMovements",
                user: currentUser,
                entity: "Patient",
                recordId: patient.Id.ToString(),
                oldValue: null,
                newValue: System.Text.Json.JsonSerializer.Serialize(new
                {
                    PatientId = patient.Id,
                    PatientName = $"{patient.FirstName} {patient.LastName}",
                    CurrentRoom = patient.Room?.RoomNumber ?? "N/A",
                    CurrentBed = patient.Bed?.BedNumber ?? "N/A"
                }),
                details: $"User {currentUser.FirstName} {currentUser.LastName} viewed movement history for patient {patient.FirstName} {patient.LastName}."
            );

            var movements = await _context.PatientMovement
                .Where(m => m.PatientId == id)
                .Include(m => m.FromBed).ThenInclude(b => b.Room)
                .Include(m => m.ToBed).ThenInclude(b => b.Room)
                .Include(m => m.WardAdmin).ThenInclude(w => w.Employee)
                .OrderByDescending(m => m.MovementDate)
                .ToListAsync();

            var treatments = await _context.Treatment
                .Where(t => t.PatientId == id)
                .Include(t => t.Doctor).ThenInclude(d => d.Employee)
                .OrderByDescending(t => t.TreatmentDate)
                .ToListAsync();

            var viewModel = new PatientFullHistoryViewModel
            {
                PatientId = patient.Id,
                FullName = patient.FirstName + " " + patient.LastName,
                IdNumber = patient.IdNumber,
                DateAdmitted = patient.DateAdmitted,
                DateDischarged = patient.DateDischarged,
                CurrentRoom = patient.Room?.RoomNumber ?? "N/A",
                CurrentBed = patient.Bed?.BedNumber ?? "N/A",
                Movements = movements,
                Treatments = treatments
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> SendMoveRequest(int patientId)
        {
            var patient = await _context.Patient
                .Include(p => p.Bed)
                    .ThenInclude(b => b.Room)
                        .ThenInclude(r => r.Ward)
                .Include(p => p.AdmissionFolder)
                .Include(p => p.Doctor)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
                return NotFound();

            if (patient.Doctor == null)
            {
                TempData["TempError"] = "Patient has no assigned doctor. Cannot send move request.";
                return RedirectToAction("DoctorDashboard");
            }

            if (patient.IsDeleted)
            {
                TempData["TempError"] = "Cannot move a deleted patient.";
                return RedirectToAction("DoctorDashboard");
            }

            if (patient.IsDischarged)
            {
                TempData["TempError"] = "Cannot move a discharged patient.";
                return RedirectToAction("DoctorDashboard");
            }

            if (patient.AdmissionFolder == null || !patient.AdmissionFolder.IsActive)
            {
                TempData["TempError"] = "Cannot move patient without an active admission folder.";
                return RedirectToAction("DoctorDashboard");
            }

            var wards = await _context.Ward.ToListAsync();
            ViewBag.Patient = patient;
            ViewBag.Wards = new SelectList(wards, "WardId", "WardName");

            var model = new PatientMoveRequest
            {
                PatientId = patientId,
                DoctorId = patient.Doctor.DoctorId 
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMoveRequest(PatientMoveRequest model)
        {
            var patient = await _context.Patient
                .Include(p => p.Bed)
                    .ThenInclude(b => b.Room)
                        .ThenInclude(r => r.Ward)
                .Include(p => p.AdmissionFolder)
                .Include(p => p.Doctor) 
                .FirstOrDefaultAsync(p => p.Id == model.PatientId);

            var wards = await _context.Ward.ToListAsync();
            
            ViewBag.Patient = patient;
            ViewBag.Wards = new SelectList(wards, "WardId", "WardName");

            if (!ModelState.IsValid)
                return View(model);

            if (patient == null)
            {
                TempData["TempError"] = "Patient not found.";
                return NotFound();
            }

            if (patient.IsDeleted)
            {
                TempData["TempError"] = "Cannot move a deleted patient.";
                return NotFound();
            }

            if (patient.Doctor == null)
            {
                TempData["TempError"] = "Patient has no assigned doctor. Cannot send move request.";
                return RedirectToAction("DoctorDashboard");
            }

            if (patient.IsDischarged)
            {
                TempData["TempError"] = "Cannot move a discharged patient.";
                return NotFound();
            }

            if (patient.AdmissionFolder == null || !patient.AdmissionFolder.IsActive)
            {
                TempData["TempError"] = "Cannot move patient without an active admission folder.";
                return NotFound();
            }

            var targetWard = await _context.Ward.FindAsync(model.TargetWardId);
            if (targetWard == null)
            {
                TempData["TempError"] = "Selected ward does not exist.";
                return NotFound();
            }

            bool duplicateRequest = await _context.PatientMoveRequest
                .AnyAsync(r => r.PatientId == patient.Id &&
                               r.TargetWardId == model.TargetWardId &&
                               r.Status == "Pending");

            if (duplicateRequest)
            {
                TempData["TempError"] = "A pending move request for this patient to the selected ward already exists.";
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            model.DoctorId = patient.Doctor.DoctorId;
            model.RequestDate = DateTime.UtcNow;
            model.Status = "Pending";
            _context.PatientMoveRequest.Add(model);

            await _context.SaveChangesAsync();

            
            await LogAuditAsync(
                actionTaken: "SendMoveRequest",
                user: currentUser,
                entity: "PatientMoveRequest",
                recordId: model.RequestId.ToString(),
                oldValue: null,
                newValue: System.Text.Json.JsonSerializer.Serialize(new
                {
                    PatientId = patient.Id,
                    PatientName = $"{patient.FirstName} {patient.LastName}",
                    DoctorId = currentUser.Id,
                    DoctorName = $"{currentUser.FirstName} {currentUser.LastName}",
                    TargetWardId = targetWard.WardId,
                    TargetWardName = targetWard.WardName,
                    Status = model.Status,
                    RequestDate = model.RequestDate
                }),
                details: $"Doctor {currentUser.FirstName} {currentUser.LastName} sent a move request for patient {patient.FirstName} {patient.LastName} to ward {targetWard.WardName}."
            );

            TempData["TempSuccess"] = $"Move request for {patient.FirstName} {patient.LastName} sent successfully to Ward Admin.";
            return RedirectToAction("PatientsFolder", "Doctor", new { id = patient.Id });
        }
    }
}