using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Imnandi_NovaCare_Hospital_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Patient> Patient { get; set; }
        public DbSet<Admin> Administrator { get; set; }
        public DbSet<WardAdmin> WardAdmin { get; set; }
        public DbSet<Bed> Bed { get; set; }
        public DbSet<Ward> Ward { get; set; }
        public DbSet<Medication> Medication { get; set; }
        public DbSet<Allergies> Allergy { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<VitalSign> VitalSign { get; set; }
        public DbSet<Treatment> Treatment { get; set; }
        public DbSet<Prescription> Prescription { get; set; }
        public DbSet<Consumable> Consumable { get; set; }
        public DbSet<Visit> Visit { get; set; }
        public DbSet<Discharge> Discharge { get; set; }
        public DbSet<Doctor> Doctor { get; set; }
        public DbSet<Nurse> Nurse { get; set; }
        public DbSet<NurseSister> NurseSister { get; set; }
        public DbSet<MedicationAdministration> MedicationAdministration { get; set; }
        public DbSet<StockManager> StockManager { get; set; }
        public DbSet<ScriptManager> ScriptManager { get; set; }
        public DbSet<Instruction> Instruction { get; set; }
        public DbSet<PatientMovement> PatientMovement { get; set; }
        public DbSet<Room> Room { get; set; }
        public DbSet<HospitalPharmarcy> HospitalPharmarcy { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItem { get; set; }
        public DbSet<MedicineOrder> MedicineOrder { get; set; }
        public DbSet<MedicineOrderItem> MedicineOrderItem { get; set; }
        public DbSet<StockTake> StockTake { get; set; }
        public DbSet<WardStock> WardStocks { get; set; }
        public DbSet<WardStockTransaction> WardStockTransactions { get; set; }
        public DbSet<WardMedicationTransaction> WardMedicationTransactions { get; set; }
        public DbSet<WardMedicationStock> WardMedicationStocks { get; set; }
        public DbSet<ChronicConditions> ChricConditions { get; set; }
        public DbSet<DischargeInstruction> DischargeInstruction { get; set; }
        public DbSet<PatientHistory> PatientHistory { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<AdmissionFolder> AdmissionFolder { get; set; }
        public DbSet<MedicalHistory> MedicalHistory { get; set; }
        public DbSet<HospitalStore> HospitalStore { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<AlertRead> AlertReads { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<Supplier> Supplier { get; set; }
        public DbSet<PatientMoveRequest> PatientMoveRequest { get; set; }
        public DbSet<WardNurseAssignment> WardNurseAssignment { get; set; }
        public DbSet<PeopleForgotPassword> PeopleForgotPassword { get; set; }
        public DbSet<ScriptPrescription> ScriptPrescription { get; set; }
        public DbSet<ScriptPrescriptionMedication> ScriptPrescriptionMedication { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NurseAlert> NurseAlerts { get; set; }
        public DbSet<NurseIncident> NurseIncidents { get; set; }
        public DbSet<NursePerformanceNote> NursePerformanceNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.AdmissionFolder)
                .WithOne(a => a.Patient)
                .HasForeignKey<Patient>(p => p.AdmissionFolderId);

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var fk in entity.GetForeignKeys())
                {
                    if (!fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade)
                    {
                        fk.DeleteBehavior = DeleteBehavior.Restrict;
                    }
                }
            }
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.DisplayName());
            }
        }

    }
}

