using Imnandi_NovaCare_Hospital_System.Models;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class ScriptManagerDashboardViewModel
    {
        public int ScriptManagerId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        public int TotalPrescriptions { get; set; }             
        public int PendingPrescriptionCount { get; set; }        
        public int CompletedPrescriptionCount { get; set; }       
        public int TotalMedicinesInStock { get; set; }            
        public List<ScriptPrescription> PendingScripts { get; set; } = new List<ScriptPrescription>();
        public List<ScriptPrescription> CompletedScripts { get; set; } = new List<ScriptPrescription>();
        public List<Medication> MedicineStock { get; set; } = new();        
        public List<StockTake> RecentStockTakes { get; set; } = new();   
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public string UserName { get; set; }
        public List<Medication> LowStockMedications { get; internal set; }
        public List<ScriptPrescription> ExpiringPrescriptions { get; internal set; }
        public int? LowStockCount { get; set; }
        public int? ExpiringCount { get; set; }
    }
}
