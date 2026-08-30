using Imnandi_NovaCare_Hospital_System.Data;
using Imnandi_NovaCare_Hospital_System.Models;
using Imnandi_NovaCare_Hospital_System.Models.Reports;
using Imnandi_NovaCare_Hospital_System.Reports;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Security.Claims;

namespace Imnandi_NovaCare_Hospital_System.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> EmployeeReport()
        {
            var employees = await _context.Employee
                .Include(e => e.User)
                .Where(e => !e.IsDeleted && e.User.IsActive)
                .ToListAsync();

            var document = new EmployeeReportDocument
            {
                Employees = employees
            };
            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", "EmployeeReport.pdf");
        }









        public async Task<IActionResult> MedicationReport()
        {
            var medications = await _context.Medication
                .Where(m => !m.IsDeleted)
                .Include(m => m.Prescription)
                .Include(m => m.MedicationAdministration)
                .OrderBy(m => m.MedicationName)
                .AsNoTracking()
                .ToListAsync();

            if (!medications.Any())
            {
                TempData["Error"] = "No medication records were found.";
                return RedirectToAction("Index", "Admin");
            }

            var document = new MedicationReportDocument
            {
                Medications = medications
            };

            var pdfBytes = document.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"Medication_Management_Report_{DateTime.Now:yyyyMMdd}.pdf"
            );
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserAuditReport(string userId, string month)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(month))
                return BadRequest("User and month must be provided.");

            if (!DateTime.TryParse($"{month}-01", out var selectedMonth))
                return BadRequest("Invalid month format.");

            var firstDay = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound("User not found.");

            var auditLogs = await _context.AuditLogs
                .Where(a => a.UserId == userId && a.Timestamp >= firstDay && a.Timestamp <= lastDay)
                .OrderBy(a => a.Timestamp)
                .ToListAsync();

            var document = new UserAuditReportDocument
            {
                User = user,
                AuditLogs = auditLogs,
                Month = firstDay
            };
            var pdfBytes = document.GeneratePdf();
            var fileName = $"AuditReport_{user.FirstName}_{user.LastName}_{firstDay:yyyy_MM}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }




        public async Task<IActionResult> DoctorScheduleReport(string month)
        {
            if (string.IsNullOrEmpty(month))
                return BadRequest("Month is required.");

            if (!DateTime.TryParse(month + "-01", out var selectedMonth))
                return BadRequest("Invalid month format.");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(); 
            var doctor = await _context.Doctor
                .Include(d => d.Employee)
                .ThenInclude(e => e.User) 
                .FirstOrDefaultAsync(d => d.Employee.UserId == userId);

            if (doctor == null)
            {
                TempData["ErrorMessage"] = "Only doctors can generate schedule reports.";
                return RedirectToAction("DoctorDashboard");
            }

            var schedules = await _context.Schedule
                .Include(s => s.Patient)
                .Where(s => s.DoctorId == doctor.DoctorId &&
                            s.ScheduledDate.Year == selectedMonth.Year &&
                            s.ScheduledDate.Month == selectedMonth.Month &&
                            !s.IsDeleted)
                .ToListAsync();

            var doc = new DoctorScheduleReportDocument
            {
                Doctor = doctor,
                Schedules = schedules,
                Month = selectedMonth
            };

            var pdfBytes = doc.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"ScheduleReport-{selectedMonth:yyyy-MM}.pdf");
        }



        [HttpGet]
        public async Task<IActionResult> DoctorPrescriptionReport(int doctorId)
        {
            var doctor = await _context.Doctor
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d =>
                    d.DoctorId == doctorId &&
                    !d.IsDeleted);

            if (doctor == null)
            {
                TempData["Error"] = "Doctor not found.";
                return RedirectToAction("DoctorDashboard", "Doctor");
            }

            var prescriptions = await _context.Prescription
                .Include(p => p.Patient)
                .Include(p => p.Medication)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.Employee)
                .Where(p =>
                    p.DoctorId == doctorId &&
                    p.Patient != null &&
                    !p.Patient.IsDeleted)
                .OrderByDescending(p => p.IssueDate)
                .ThenByDescending(p => p.IssueTime)
                .ToListAsync();

            if (!prescriptions.Any())
            {
                TempData["Error"] =
                    "No prescriptions were found for this doctor.";

                return RedirectToAction("DoctorDashboard", "Doctor");
            }

            var document = new PrescriptionReportDocument(
                doctor,
                prescriptions
            );

            var pdf = document.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Doctor_Prescription_Report_{doctor.FirstName}_{doctor.LastName}_{DateTime.Now:yyyyMMdd}.pdf"
            );
        }








        [HttpGet]
        public async Task<IActionResult> GeneratePatientAdmissionReport(int patientId)
        {
            var patient = await _context.Patient
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null)
                return NotFound("Patient not found.");

            var admissions = await _context.AdmissionFolder
                .Include(a => a.Bed)
                .ThenInclude(b => b.Room)
                .ThenInclude(r => r.Ward)
                .Include(a => a.WardAdmin)
                    .ThenInclude(wa => wa.Employee)
                .Where(a => a.PatientId == patientId)
                .OrderBy(a => a.DateCreated)
                .ToListAsync();

            if (!admissions.Any())
                return BadRequest("No admissions found for this patient.");

            var document = new PatientAdmissionReportDocument
            {
                Patient = patient,
                Admissions = admissions
            };
            var pdfBytes = document.GeneratePdf();

            var fileName = $"AdmissionHistory_{patient.FirstName}_{patient.LastName}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }







        public async Task<IActionResult> MedicationFullReport(int medicationId)
        {
            var medication = await _context.Medication
                .Where(m => m.MedicationId == medicationId && !m.IsDeleted)
                .FirstOrDefaultAsync();

            if (medication == null)
                return NotFound();

            var viewModel = new MedicationFullReportViewModel
            {
                MedicationId = medication.MedicationId,
                MedicationName = medication.MedicationName,
                Description = medication.Description,
                QuantityOnHand = medication.QuantityOnHand,
                DosageForm = medication.DosageForm,
                Manufacturer = medication.Manufacturer,
                ExpiryDate = medication.ExpiryDate
            };

            viewModel.StockReceived = await _context.MedicineOrderItem
                .Where(i =>
                    i.MedicationId == medicationId &&
                    !i.IsDeleted &&
                    !i.MedicineOrder.IsDeleted)
                .Select(i => new MedicationStockRecord
                {
                    MedicineOrderItemId = i.MedicineOrderItemId,
                    MedicineOrderId = i.MedicineOrderId,
                    OrderNumber = i.MedicineOrder.OrderNumber,
                    OrderName = i.MedicineOrder.OrderName,
                    OrderDate = i.MedicineOrder.Date,
                    SupplierId = i.MedicineOrder.SupplierId,
                    SupplierName = i.MedicineOrder.Supplier != null
                        ? i.MedicineOrder.Supplier.SupplierName
                        : null,
                    HospitalStoreId = i.MedicineOrder.HospitalStoreId,
                    HospitalStoreName = i.MedicineOrder.HospitalStore != null
                        ? i.MedicineOrder.HospitalStore.HospitalStoreName
                        : null,
                    StockManagerId = i.MedicineOrder.StockManagerId,
                    StockManagerName = i.MedicineOrder.StockManager != null
                        ? i.MedicineOrder.StockManager.Employee.FirstName + " " +
                          i.MedicineOrder.StockManager.Employee.LastName
                        : null,
                    QuantityRequested = i.QuantityRequested,
                    QuantityReceived = i.QuantityReceived,
                    IsReceived = i.MedicineOrder.IsReceived
                })
                .OrderByDescending(i => i.OrderDate)
                .ToListAsync();

            viewModel.TotalOrders = viewModel.StockReceived
                .Select(x => x.MedicineOrderId)
                .Distinct()
                .Count();

            viewModel.TotalStockReceived = viewModel.StockReceived
                .Where(x => x.IsReceived)
                .Sum(x => x.QuantityReceived);

            viewModel.Prescriptions = await _context.Prescription
                .Where(p =>
                    p.MedicationId == medicationId &&
                    !p.IsDeleted)
                .Include(p => p.Doctor)
                    .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e.User)
                .Include(p => p.Patient)
                .Include(p => p.ScriptManager)
                    .ThenInclude(sm => sm.Employee)
                    .ThenInclude(e => e.User)
                .Select(p => new PrescriptionRecord
                {
                    PrescriptionId = p.PrescriptionId,
                    DoctorId = p.DoctorId,
                    DoctorName = p.Doctor.Employee.FirstName + " " +
                                 p.Doctor.Employee.LastName,
                    DoctorRole = p.Doctor.Employee.JobTitle,
                    PatientId = p.PatientId,
                    PatientName = p.Patient.FirstName + " " +
                                  p.Patient.LastName,
                    IssueDate = p.IssueDate,
                    IssueTime = p.IssueTime,
                    Status = p.Status,
                    DosageInstructions = p.DosageInstructions,
                    Level = p.Level,
                    ScriptManagerId = p.ScriptManagerId,
                    ScriptManagerName = p.ScriptManager != null
                        ? p.ScriptManager.Employee.FirstName + " " +
                          p.ScriptManager.Employee.LastName
                        : null
                })
                .OrderByDescending(p => p.IssueDate)
                .ThenByDescending(p => p.IssueTime)
                .ToListAsync();

            viewModel.TotalPrescriptions = viewModel.Prescriptions.Count;

            viewModel.ScriptActions = await _context.ScriptPrescription
                .Where(sp =>
                    sp.Prescription.MedicationId == medicationId &&
                    !sp.IsDeleted)
                .Include(sp => sp.ScriptManager)
                    .ThenInclude(sm => sm.Employee)
                    .ThenInclude(e => e.User)
                .Include(sp => sp.ReceivedFrom)
                .Select(sp => new ScriptPrescriptionRecord
                {
                    ScriptPrescriptionId = sp.ScriptPrescriptionId,
                    PrescriptionId = sp.PrescriptionId,
                    ScriptManagerId = sp.ScriptManagerId,
                    ScriptManagerName = sp.ScriptManager != null
                        ? sp.ScriptManager.Employee.FirstName + " " +
                          sp.ScriptManager.Employee.LastName
                        : null,
                    Status = sp.Status,
                    ProcessedDate = sp.ProcessedDate,
                    ReceivedDate = sp.ReceivedDate,
                    Notes = sp.Notes,
                    IsVerified = sp.IsVerified,
                    ReceivedFromUser = sp.ReceivedFrom != null
                        ? sp.ReceivedFrom.FirstName + " " +
                          sp.ReceivedFrom.LastName
                        : null,
                    VerifiedDate = sp.VerifiedDate,
                    VerifiedBy = sp.VerifiedBy,
                    AssignedDate = sp.AssignedDate
                })
                .OrderByDescending(sp => sp.AssignedDate)
                .ToListAsync();

            viewModel.TotalScriptActions = viewModel.ScriptActions.Count;

            viewModel.Administrations = await _context.MedicationAdministration
                .Where(a => a.MedicationId == medicationId)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e.User)
                .Include(a => a.Nurse)
                    .ThenInclude(n => n.Employee)
                    .ThenInclude(e => e.User)
                .Include(a => a.NurseSister)
                    .ThenInclude(ns => ns.Employee)
                    .ThenInclude(e => e.User)
                .Include(a => a.Patient)
                .Select(a => new AdministrationRecord
                {
                    MedicationAdministrationId = a.MedicationAdministrationId,
                    MedicationId = a.MedicationId,
                    PrescriptionId = a.PrescriptionId,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor != null
                        ? a.Doctor.Employee.FirstName + " " +
                          a.Doctor.Employee.LastName
                        : null,
                    NurseId = a.NurseId,
                    NurseName = a.Nurse != null
                        ? a.Nurse.Employee.FirstName + " " +
                          a.Nurse.Employee.LastName
                        : null,
                    NurseRole = a.Nurse != null
                        ? a.Nurse.Employee.JobTitle
                        : null,
                    NurseSisterId = a.NurseSisterId,
                    NurseSisterName = a.NurseSister != null
                        ? a.NurseSister.Employee.FirstName + " " +
                          a.NurseSister.Employee.LastName
                        : null,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.FirstName + " " +
                                  a.Patient.LastName,
                    DateAdministered = a.AdministrationTime,
                    Dosage = a.Dosage,
                    Purpose = a.Purpose,
                    IsSeen = a.IsSeen
                })
                .OrderByDescending(a => a.DateAdministered)
                .ToListAsync();

            viewModel.TotalAdministrations = viewModel.Administrations.Count;

            viewModel.TotalStockAvailable = medication.QuantityOnHand ?? 0;
            viewModel.ClosingStock = medication.QuantityOnHand ?? 0;
            viewModel.TotalStockUsed = 0;
            viewModel.OpeningStock = 0;

            var document = new MedicationFullReportDocument
            {
                Medication = viewModel
            };

            var pdfBytes = document.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"Medication_Full_Report_{medication.MedicationName}_{DateTime.Now:yyyyMMdd}.pdf"
            );
        }



        public async Task<IActionResult> WardMedicationFullReport(int wardId)
        {
            var ward = await _context.Ward
                .Where(w => w.WardId == wardId && !w.IsDeleted)
                .Include(w => w.NurseSister)
                    .ThenInclude(ns => ns.Employee)
                .FirstOrDefaultAsync();

            if (ward == null)
                return NotFound();

            var viewModel = new WardMedicationReportViewModel
            {
                WardId = ward.WardId,
                WardName = ward.WardName,
                Location = ward.Location,
                Description = ward.Description,
                Capacity = ward.Capacity,
                ReportGenerated = DateTime.Now
            };

            var currentStock = await _context.WardMedicationStocks
                .Where(ws => ws.WardId == wardId)
                .Include(ws => ws.Medication)
                .Where(ws => !ws.Medication.IsDeleted)
                .Select(ws => new WardMedicationStockRecord
                {
                    WardMedicationStockId = ws.WardMedicationStockId,
                    MedicationId = ws.MedicationId,
                    MedicationName = ws.Medication.MedicationName,
                    DosageForm = ws.Medication.DosageForm,
                    Manufacturer = ws.Medication.Manufacturer,
                    ExpiryDate = ws.Medication.ExpiryDate,
                    QuantityInWard = ws.QuantityInWard
                })
                .OrderBy(ws => ws.MedicationName)
                .ToListAsync();

            viewModel.CurrentStock = currentStock;

            var transactions = await _context.WardMedicationTransactions
                .Where(t =>
                    t.WardId == wardId &&
                    !t.IsDeleted &&
                    !t.Medication.IsDeleted)
                .Include(t => t.Medication)
                .Select(t => new WardMedicationTransactionRecord
                {
                    WardMedicationTransactionId = t.WardMedicationTransactionId,
                    MedicationId = t.MedicationId,
                    MedicationName = t.Medication.MedicationName,
                    DosageForm = t.Medication.DosageForm,
                    Quantity = t.Quantity,
                    DateReceived = t.DateReceived,
                    TransactionType = t.TransactionType
                })
                .OrderByDescending(t => t.DateReceived)
                .ToListAsync();

            viewModel.Transactions = transactions;

            viewModel.TotalMedicationTypes = currentStock.Count;

            viewModel.TotalQuantityInWard = currentStock
                .Sum(x => x.QuantityInWard);

            viewModel.TotalReceived = transactions
                .Where(x =>
                    x.TransactionType != null &&
                    x.TransactionType.ToLower() == "received")
                .Sum(x => x.Quantity);

            viewModel.TotalIssued = transactions
                .Where(x =>
                    x.TransactionType != null &&
                    x.TransactionType.ToLower() == "issued")
                .Sum(x => x.Quantity);

            viewModel.TotalAdjusted = transactions
                .Where(x =>
                    x.TransactionType != null &&
                    x.TransactionType.ToLower() == "adjusted")
                .Sum(x => x.Quantity);

            var stockManager = await _context.StockManager
                .Where(sm =>
                    !sm.IsDeleted &&
                    sm.Department != null &&
                    sm.Department.ToLower().Contains("medicine"))
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync();

            if (stockManager != null)
            {
                viewModel.StockManagerName =
                    stockManager.Employee.FirstName + " " +
                    stockManager.Employee.LastName;

                viewModel.Department = stockManager.Department;
            }

            var document = new WardMedicationReportDocument
            {
                Ward = viewModel
            };

            var pdfBytes = document.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"Ward_Medication_Report_{ward.WardName}_{DateTime.Now:yyyyMMdd}.pdf"
            );
        }











        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StockTakeMonthlyReport(int month, int year)
        {
            if (month < 1 || month > 12 || year < 2000)
                return BadRequest("Invalid month or year.");

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var stockTakes = await _context.StockTake
                .Include(st => st.StockManager)
                .Include(st => st.Ward)
                .Include(st => st.Consumables)
                .Where(st =>
                    !st.IsDeleted &&
                    st.Date >= DateOnly.FromDateTime(startDate) &&
                    st.Date <= DateOnly.FromDateTime(endDate))
                .OrderBy(st => st.Date)
                .ToListAsync();

            if (!stockTakes.Any())
            {
                TempData["Error"] = $"No stock take report exists for {startDate:MMMM yyyy}.";

                return RedirectToAction("ManageStock", "StockManager");
            }
            var document = new StockTakeReportDocument
            {
                StockTakes = stockTakes,
                Month = startDate
            };

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"StockTakeReport_{month:D2}_{year}.pdf");
        }














        public async Task<IActionResult> WardInventoryFullReport(int wardId)
        {
            var ward = await _context.Ward
                .Where(w => w.WardId == wardId && !w.IsDeleted)
                .Include(w => w.NurseSister)
                    .ThenInclude(ns => ns.Employee)
                .FirstOrDefaultAsync();

            if (ward == null)
                return NotFound();

            var viewModel = new WardInventoryReportViewModel
            {
                WardId = ward.WardId,
                WardName = ward.WardName,
                Location = ward.Location,
                Description = ward.Description,
                Capacity = ward.Capacity,
                ReportDate = DateTime.Now,
                NurseSisterName =
                    ward.NurseSister != null &&
                    ward.NurseSister.Employee != null
                        ? ward.NurseSister.Employee.FirstName + " " +
                          ward.NurseSister.Employee.LastName
                        : "Not Assigned"
            };

            var currentMedicationStock = await _context.WardMedicationStocks
                .Where(ws =>
                    ws.WardId == wardId &&
                    ws.Medication != null &&
                    !ws.Medication.IsDeleted)
                .Include(ws => ws.Medication)
                .Select(ws => new WardInventoryMedicationRecord
                {
                    MedicationId = ws.MedicationId,
                    MedicationName = ws.Medication.MedicationName,
                    Description = ws.Medication.Description,
                    DosageForm = ws.Medication.DosageForm,
                    Manufacturer = ws.Medication.Manufacturer,
                    ExpiryDate = ws.Medication.ExpiryDate,
                    QuantityInWard = ws.QuantityInWard,
                    TotalReceived = 0,
                    TotalIssued = 0,
                    TotalAdjusted = 0,
                    QuantityAtStartOfPeriod = ws.QuantityInWard,
                    QuantityAtEndOfPeriod = ws.QuantityInWard,
                    StockStatus =
                        ws.QuantityInWard <= 0
                            ? "Out of Stock"
                            : ws.QuantityInWard <= 5
                                ? "Low Stock"
                                : "Available"
                })
                .OrderBy(ws => ws.MedicationName)
                .ToListAsync();

            viewModel.Medications = currentMedicationStock;

            var medicationTransactions = await _context.WardMedicationTransactions
                .Where(t =>
                    t.WardId == wardId &&
                    !t.IsDeleted &&
                    t.Medication != null &&
                    !t.Medication.IsDeleted)
                .Include(t => t.Medication)
                .Select(t => new WardInventoryMedicationTransactionRecord
                {
                    TransactionId = t.WardMedicationTransactionId,
                    MedicationId = t.MedicationId,
                    MedicationName = t.Medication.MedicationName,
                    Quantity = t.Quantity,
                    Date = t.DateReceived,
                    TransactionType = t.TransactionType
                })
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            viewModel.MedicationTransactions = medicationTransactions;

            foreach (var medication in viewModel.Medications)
            {
                medication.TotalReceived = medicationTransactions
                    .Where(t =>
                        t.MedicationId == medication.MedicationId &&
                        t.TransactionType != null &&
                        t.TransactionType.ToLower() == "received")
                    .Sum(t => t.Quantity);

                medication.TotalIssued = medicationTransactions
                    .Where(t =>
                        t.MedicationId == medication.MedicationId &&
                        t.TransactionType != null &&
                        t.TransactionType.ToLower() == "issued")
                    .Sum(t => t.Quantity);

                medication.TotalAdjusted = medicationTransactions
                    .Where(t =>
                        t.MedicationId == medication.MedicationId &&
                        t.TransactionType != null &&
                        t.TransactionType.ToLower() == "adjusted")
                    .Sum(t => t.Quantity);

                medication.QuantityAtEndOfPeriod =
                    medication.QuantityInWard;

                medication.QuantityAtStartOfPeriod =
                    medication.QuantityInWard
                    - medication.TotalReceived
                    + medication.TotalIssued
                    - medication.TotalAdjusted;
            }

            var currentConsumableStock = await _context.WardStocks
                .Where(ws =>
                    ws.WardId == wardId &&
                    ws.Consumable != null &&
                    !ws.Consumable.IsDeleted)
                .Include(ws => ws.Consumable)
                .OrderBy(ws => ws.Consumable.ConsumableName)
                .ToListAsync();

            viewModel.Consumables = currentConsumableStock
                .Select(ws =>
                {
                    var consumable = ws.Consumable;

                    var isExpired =
                        consumable.ExpiryDate <
                        DateOnly.FromDateTime(DateTime.Now);

                    var isBelowMinimum =
                        ws.QuantityInWard <
                        consumable.MinimumConsumables;

                    var status = "Available";

                    if (isExpired)
                    {
                        status = "Expired";
                    }
                    else if (ws.QuantityInWard <= 0)
                    {
                        status = "Out of Stock";
                    }
                    else if (isBelowMinimum)
                    {
                        status = "Below Minimum";
                    }

                    return new WardInventoryConsumableRecord
                    {
                        ConsumableId = consumable.ConsumableId,
                        ConsumableName = consumable.ConsumableName,
                        Description = consumable.Description,
                        Unit = consumable.Unit,
                        ExpiryDate = consumable.ExpiryDate,
                        SystemQuantity = ws.QuantityInWard,
                        PhysicalQuantity = 0,
                        Difference = 0,
                        StockStatus = status,
                        MinimumConsumables = consumable.MinimumConsumables,
                        IsBelowMinimum = isBelowMinimum,
                        IsExpired = isExpired
                    };
                })
                .ToList();

            var stockTakes = await _context.StockTake
                .Where(st =>
                    st.WardId == wardId &&
                    !st.IsDeleted)
                .Include(st => st.StockManager)
                .Include(st => st.Consumables)
                .OrderByDescending(st => st.Date)
                .ToListAsync();

            viewModel.StockTakes = stockTakes
                .Select(st =>
                {
                    var stockTakeConsumables = st.Consumables != null
                        ? st.Consumables
                            .Where(c => !c.IsDeleted)
                            .Select(c => new WardInventoryStockTakeConsumableRecord
                            {
                                ConsumableId = c.ConsumableId,
                                ConsumableName = c.ConsumableName,
                                Unit = c.Unit,
                                SystemQuantity = c.QuantityOnHand,
                                PhysicalQuantity = 0,
                                Difference = 0,
                                Status = "Physical count recorded in stock take"
                            })
                            .ToList()
                        : new List<WardInventoryStockTakeConsumableRecord>();

                    return new WardInventoryStockTakeRecord
                    {
                        StockTakeId = st.StockTakeId,
                        Date = st.Date,
                        StockManagerId = st.StockManagerId,
                        StockManagerName =
                            st.StockManager != null
                                ? st.StockManager.FirstName + " " +
                                  st.StockManager.LastName
                                : "Unknown",
                        QuantityCounted = st.QuantityCountered,
                        TotalConsumablesCounted = stockTakeConsumables.Count,
                        TotalShortages = 0,
                        TotalSurpluses = 0,
                        TotalMatching = 0,
                        Consumables = stockTakeConsumables
                    };
                })
                .ToList();

            var stockManagerIds = stockTakes
                .Select(st => st.StockManagerId)
                .Distinct()
                .ToList();

            if (stockManagerIds.Any())
            {
                var stockManagers = await _context.StockManager
                    .Where(sm =>
                        stockManagerIds.Contains(sm.StockManagerId) &&
                        !sm.IsDeleted)
                    .ToListAsync();

                viewModel.StockManagers = stockManagers
                    .Select(sm => new WardInventoryStockManagerRecord
                    {
                        StockManagerId = sm.StockManagerId,
                        StockManagerName =
                            sm.FirstName + " " + sm.LastName,
                        Department = sm.Department
                    })
                    .OrderBy(sm => sm.StockManagerName)
                    .ToList();
            }

            viewModel.TotalMedicationTypes =
                viewModel.Medications.Count;

            viewModel.TotalConsumableTypes =
                viewModel.Consumables.Count;

            viewModel.TotalMedicationQuantityInWard =
                viewModel.Medications.Sum(m => m.QuantityInWard);

            viewModel.TotalConsumableQuantityInWard =
                viewModel.Consumables.Sum(c => c.SystemQuantity);

            viewModel.TotalMedicationReceived =
                medicationTransactions
                    .Where(t =>
                        t.TransactionType != null &&
                        t.TransactionType.ToLower() == "received")
                    .Sum(t => t.Quantity);

            viewModel.TotalMedicationIssued =
                medicationTransactions
                    .Where(t =>
                        t.TransactionType != null &&
                        t.TransactionType.ToLower() == "issued")
                    .Sum(t => t.Quantity);

            viewModel.TotalMedicationAdjusted =
                medicationTransactions
                    .Where(t =>
                        t.TransactionType != null &&
                        t.TransactionType.ToLower() == "adjusted")
                    .Sum(t => t.Quantity);

            viewModel.TotalStockTakes =
                viewModel.StockTakes.Count;

            viewModel.TotalItemsCounted =
                viewModel.StockTakes.Sum(st => st.QuantityCounted);

            viewModel.TotalShortages =
                viewModel.StockTakes.Sum(st => st.TotalShortages);

            viewModel.TotalSurpluses =
                viewModel.StockTakes.Sum(st => st.TotalSurpluses);

            viewModel.TotalMatchingItems =
                viewModel.StockTakes.Sum(st => st.TotalMatching);

            var document = new WardInventoryReportDocument
            {
                Ward = viewModel
            };

            var pdfBytes = document.GeneratePdf();

            return File(
                pdfBytes,
                "application/pdf",
                $"Ward_Inventory_Report_{ward.WardName}_{DateTime.Now:yyyyMMdd}.pdf"
            );
        }






        [HttpGet]
        public async Task<IActionResult> SupplierReport(int supplierId)
        {
            var supplier = await _context.Supplier
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    s.SupplierId == supplierId &&
                    !s.IsDeleted);

            if (supplier == null)
            {
                return NotFound("Supplier not found.");
            }

            var consumableOrders = await _context.Order
                .AsNoTracking()
                .Where(o =>
                    o.SupplierId == supplierId &&
                    !o.IsDeleted)
                .Include(o => o.StockManager)
                .Include(o => o.HospitalStore)
                .Include(o => o.Ward)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Consumable)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            var medicineOrders = await _context.MedicineOrder
                .AsNoTracking()
                .Where(o =>
                    o.SupplierId == supplierId &&
                    !o.IsDeleted)
                .Include(o => o.StockManager)
                .Include(o => o.HospitalStore)
                .Include(o => o.MedicineOrderItems)
                    .ThenInclude(mi => mi.Medication)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            var report = new SupplierReportViewModel
            {
                SupplierId = supplier.SupplierId,
                SupplierName = supplier.SupplierName,
                ContactPerson = supplier.ContactPerson,
                PhoneNumber = supplier.PhoneNumber,
                Email = supplier.Email,
                Address = supplier.Address,
                IsActive = supplier.IsActive
            };

            foreach (var order in consumableOrders)
            {
                foreach (var item in order.OrderItems
                             .Where(oi => !oi.IsDeleted))
                {
                    int requested = item.QuantityRequested;
                    int received = item.QuantityReceived ?? 0;

                    int outstanding = Math.Max(
                        requested - received,
                        0);

                    report.ConsumableItems.Add(
                        new SupplierConsumableReportItem
                        {
                            OrderId = order.OrderId,
                            OrderNumber = order.OrderNumber,
                            OrderName = order.OrderName,
                            OrderDate = order.Date,

                            ConsumableName =
                                item.Consumable?.ConsumableName
                                ?? "Unknown Consumable",

                            Description =
                                item.Consumable?.Description
                                ?? "",

                            Unit =
                                item.Consumable?.Unit
                                ?? "",

                            QuantityRequested = requested,
                            QuantityReceived = received,
                            QuantityOutstanding = outstanding,

                            IsReceived = order.IsReceived,

                            StockManagerName =
                                order.StockManager == null
                                    ? "Unknown"
                                    : $"{order.StockManager.FirstName} {order.StockManager.LastName}",

                            HospitalStoreName =
                                order.HospitalStore?.HospitalStoreName
                                ?? "Unknown",

                            HospitalStoreLocation =
                                order.HospitalStore?.Location
                                ?? "Unknown",

                            WardName =
                                order.Ward?.WardName
                                ?? "Main Hospital Store"
                        });
                }
            }

            foreach (var order in medicineOrders)
            {
                foreach (var item in order.MedicineOrderItems
                             .Where(mi => !mi.IsDeleted))
                {
                    int requested = item.QuantityRequested;
                    int received = item.QuantityReceived;

                    int outstanding = Math.Max(
                        requested - received,
                        0);

                    report.MedicineItems.Add(
                        new SupplierMedicineReportItem
                        {
                            MedicineOrderId =
                                order.MedicineOrderId,

                            OrderNumber =
                                order.OrderNumber,

                            OrderName =
                                order.OrderName,

                            OrderDate =
                                order.Date,

                            MedicationName =
                                item.Medication?.MedicationName
                                ?? "Unknown Medication",

                            Description =
                                item.Medication?.Description
                                ?? "",

                            DosageForm =
                                item.Medication?.DosageForm
                                ?? "",

                            Manufacturer =
                                item.Medication?.Manufacturer
                                ?? "",

                            QuantityRequested =
                                requested,

                            QuantityReceived =
                                received,

                            QuantityOutstanding =
                                outstanding,

                            IsReceived =
                                order.IsReceived,

                            StockManagerName =
                                order.StockManager == null
                                    ? "Unknown"
                                    : $"{order.StockManager.FirstName} {order.StockManager.LastName}",

                            HospitalStoreName =
                                order.HospitalStore?.HospitalStoreName
                                ?? "Unknown",

                            HospitalStoreLocation =
                                order.HospitalStore?.Location
                                ?? "Unknown"
                        });
                }
            }

            foreach (var order in consumableOrders)
            {
                var activeItems = order.OrderItems
                    .Where(oi => !oi.IsDeleted)
                    .ToList();

                int totalRequested = activeItems
                    .Sum(oi => oi.QuantityRequested);

                int totalReceived = activeItems
                    .Sum(oi => oi.QuantityReceived ?? 0);

                int outstanding = Math.Max(
                    totalRequested - totalReceived,
                    0);

                double deliveryRate = totalRequested > 0
                    ? Math.Round(
                        (double)totalReceived /
                        totalRequested * 100,
                        2)
                    : 0;

                report.Orders.Add(
                    new SupplierOrderReportItem
                    {
                        OrderId = order.OrderId,

                        IsMedicineOrder = false,

                        OrderNumber = order.OrderNumber,

                        OrderName = order.OrderName,

                        Description = order.Description,

                        OrderDate = order.Date,

                        IsReceived = order.IsReceived,

                        Status = order.IsReceived
                            ? "Received"
                            : "Pending",

                        TotalItems = activeItems.Count,

                        TotalQuantityRequested =
                            totalRequested,

                        TotalQuantityReceived =
                            totalReceived,

                        OutstandingQuantity =
                            outstanding,

                        DeliveryRate =
                            deliveryRate,

                        StockManagerName =
                            order.StockManager == null
                                ? "Unknown"
                                : $"{order.StockManager.FirstName} {order.StockManager.LastName}",

                        HospitalStoreName =
                            order.HospitalStore?.HospitalStoreName
                            ?? "Unknown",

                        HospitalStoreLocation =
                            order.HospitalStore?.Location
                            ?? "Unknown",

                        WardName =
                            order.Ward?.WardName
                            ?? "Main Hospital Store"
                    });
            }

            foreach (var order in medicineOrders)
            {
                var activeItems = order.MedicineOrderItems
                    .Where(mi => !mi.IsDeleted)
                    .ToList();

                int totalRequested = activeItems
                    .Sum(mi => mi.QuantityRequested);

                int totalReceived = activeItems
                    .Sum(mi => mi.QuantityReceived);

                int outstanding = Math.Max(
                    totalRequested - totalReceived,
                    0);

                double deliveryRate = totalRequested > 0
                    ? Math.Round(
                        (double)totalReceived /
                        totalRequested * 100,
                        2)
                    : 0;

                report.Orders.Add(
                    new SupplierOrderReportItem
                    {
                        OrderId =
                            order.MedicineOrderId,

                        IsMedicineOrder = true,

                        OrderNumber =
                            order.OrderNumber,

                        OrderName =
                            order.OrderName,

                        Description =
                            order.Description,

                        OrderDate =
                            order.Date,

                        IsReceived =
                            order.IsReceived,

                        Status =
                            order.IsReceived
                                ? "Received"
                                : "Pending",

                        TotalItems =
                            activeItems.Count,

                        TotalQuantityRequested =
                            totalRequested,

                        TotalQuantityReceived =
                            totalReceived,

                        OutstandingQuantity =
                            outstanding,

                        DeliveryRate =
                            deliveryRate,

                        StockManagerName =
                            order.StockManager == null
                                ? "Unknown"
                                : $"{order.StockManager.FirstName} {order.StockManager.LastName}",

                        HospitalStoreName =
                            order.HospitalStore?.HospitalStoreName
                            ?? "Unknown",

                        HospitalStoreLocation =
                            order.HospitalStore?.Location
                            ?? "Unknown",

                        WardName = "N/A"
                    });
            }

            report.Orders = report.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            report.TotalConsumableOrders =
                consumableOrders.Count;

            report.TotalMedicineOrders =
                medicineOrders.Count;

            report.TotalOrders =
                report.TotalConsumableOrders +
                report.TotalMedicineOrders;

            report.CompletedOrders =
                report.Orders.Count(o => o.IsReceived);

            report.PendingOrders =
                report.Orders.Count(o => !o.IsReceived);

            report.TotalItemsOrdered =
                report.Orders.Sum(o =>
                    o.TotalQuantityRequested);

            report.TotalItemsReceived =
                report.Orders.Sum(o =>
                    o.TotalQuantityReceived);

            report.TotalItemsOutstanding =
                report.Orders.Sum(o =>
                    o.OutstandingQuantity);

            report.OverallDeliveryRate =
                report.TotalItemsOrdered > 0
                    ? Math.Round(
                        (double)report.TotalItemsReceived /
                        report.TotalItemsOrdered * 100,
                        2)
                    : 0;

            var document = new SupplierReportDocument(report);

            var pdf = document.GeneratePdf();

            return File(
                pdf,
                "application/pdf",
                $"Supplier_Report_{supplier.SupplierName}_{DateTime.Now:yyyyMMdd}.pdf");
        }


    }
}