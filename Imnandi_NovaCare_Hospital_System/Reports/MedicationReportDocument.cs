using Imnandi_NovaCare_Hospital_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Imnandi_NovaCare_Hospital_System.Reports
{
    public class MedicationReportDocument : IDocument
    {
        public List<Medication> Medications { get; set; } = new();

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = "Medication Management Report",
            Author = "Imnandi NovaCare Medical Center",
            Subject = "Medication Stock, Prescription and Administration Report",
            Keywords = "Medication, Stock, Prescriptions, Administration, Hospital"
        };

        public void Compose(IDocumentContainer container)
        {
            var activeMedications = Medications
                .Where(m => !m.IsDeleted)
                .ToList();

            var totalMedicationTypes = activeMedications.Count;

            var totalQuantityOnHand = activeMedications
                .Sum(m => m.QuantityOnHand ?? 0);

            var lowStockMedications = activeMedications
                .Where(m => (m.QuantityOnHand ?? 0) > 0 &&
                            (m.QuantityOnHand ?? 0) <= 5)
                .OrderBy(m => m.QuantityOnHand)
                .ThenBy(m => m.MedicationName)
                .ToList();

            var outOfStockMedications = activeMedications
                .Where(m => (m.QuantityOnHand ?? 0) <= 0)
                .OrderBy(m => m.MedicationName)
                .ToList();

            var expiredMedications = activeMedications
                .Where(m => m.ExpiryDate.HasValue &&
                            m.ExpiryDate.Value.Date < DateTime.Now.Date)
                .OrderBy(m => m.ExpiryDate)
                .ToList();

            var expiringSoonMedications = activeMedications
                .Where(m => m.ExpiryDate.HasValue &&
                            m.ExpiryDate.Value.Date >= DateTime.Now.Date &&
                            m.ExpiryDate.Value.Date <= DateTime.Now.Date.AddDays(90))
                .OrderBy(m => m.ExpiryDate)
                .ToList();

            var totalPrescriptions = activeMedications
                .Sum(m => m.Prescription?.Count ?? 0);

            var totalAdministrations = activeMedications
                .Sum(m => m.MedicationAdministration?.Count ?? 0);

            var medicationsWithPrescriptionActivity = activeMedications
                .Where(m => (m.Prescription?.Count ?? 0) > 0)
                .OrderByDescending(m => m.Prescription?.Count ?? 0)
                .ThenBy(m => m.MedicationName)
                .ToList();

            var medicationsWithAdministrationActivity = activeMedications
                .Where(m => (m.MedicationAdministration?.Count ?? 0) > 0)
                .OrderByDescending(m => m.MedicationAdministration?.Count ?? 0)
                .ThenBy(m => m.MedicationName)
                .ToList();

            container.Page(page =>
            {
                page.Margin(35);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.DefaultTextStyle(x =>
                    x.FontFamily("Arial")
                     .FontSize(9));

                page.Header()
                    .PaddingBottom(15)
                    .Column(header =>
                    {
                        header.Item()
                            .Row(row =>
                            {
                                row.ConstantColumn(70)
                                    .Height(55)
                                    .Image(
                                        "wwwroot/images/hospital_logo.jpg",
                                        ImageScaling.FitHeight);

                                row.RelativeColumn()
                                    .Column(col =>
                                    {
                                        col.Item()
                                            .AlignCenter()
                                            .Text("IMNANDI NOVACARE MEDICAL CENTER")
                                            .Bold()
                                            .FontSize(17)
                                            .FontColor(Colors.Green.Darken2);

                                        col.Item()
                                            .PaddingTop(3)
                                            .AlignCenter()
                                            .Text("Medication Management Report")
                                            .Bold()
                                            .FontSize(13)
                                            .FontColor(Colors.Green.Darken1);

                                        col.Item()
                                            .PaddingTop(4)
                                            .AlignCenter()
                                            .Text($"Generated: {DateTime.Now:dd MMMM yyyy, HH:mm}")
                                            .FontColor(Colors.Grey.Darken2);
                                    });
                            });

                        header.Item()
                            .PaddingTop(12)
                            .LineHorizontal(1)
                            .LineColor(Colors.Green.Darken1);
                    });

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        column.Spacing(12);

                        column.Item()
                            .Background(Colors.Green.Lighten5)
                            .Border(1)
                            .BorderColor(Colors.Green.Lighten2)
                            .Padding(12)
                            .Column(summary =>
                            {
                                summary.Item()
                                    .Text("EXECUTIVE SUMMARY")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(Colors.Green.Darken2);

                                summary.Item()
                                    .PaddingTop(8)
                                    .Row(row =>
                                    {
                                        row.RelativeColumn()
                                            .Element(container =>
                                                SummaryCard(
                                                    container,
                                                    "Medication Types",
                                                    totalMedicationTypes.ToString("N0")));

                                        row.RelativeColumn()
                                            .Element(container =>
                                                SummaryCard(
                                                    container,
                                                    "Units On Hand",
                                                    totalQuantityOnHand.ToString("N0")));

                                        row.RelativeColumn()
                                            .Element(container =>
                                                SummaryCard(
                                                    container,
                                                    "Prescriptions",
                                                    totalPrescriptions.ToString("N0")));

                                        row.RelativeColumn()
                                            .Element(container =>
                                                SummaryCard(
                                                    container,
                                                    "Administrations",
                                                    totalAdministrations.ToString("N0")));
                                    });
                            });

                        column.Item()
                            .Background(Colors.Grey.Lighten5)
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(12)
                            .Column(status =>
                            {
                                status.Item()
                                    .Text("STOCK AND EXPIRY ALERTS")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(Colors.Green.Darken2);

                                status.Item()
                                    .PaddingTop(8)
                                    .Row(row =>
                                    {
                                        row.RelativeColumn()
                                            .Text($"Low Stock: {lowStockMedications.Count:N0}")
                                            .Bold();

                                        row.RelativeColumn()
                                            .Text($"Out of Stock: {outOfStockMedications.Count:N0}")
                                            .Bold();

                                        row.RelativeColumn()
                                            .Text($"Expired: {expiredMedications.Count:N0}")
                                            .Bold();

                                        row.RelativeColumn()
                                            .Text($"Expiring ≤ 90 Days: {expiringSoonMedications.Count:N0}")
                                            .Bold();
                                    });
                            });

                        if (outOfStockMedications.Any())
                        {
                            column.Item()
                                .Column(section =>
                                {
                                    section.Item()
                                        .Text("OUT OF STOCK MEDICATION")
                                        .Bold()
                                        .FontSize(12)
                                        .FontColor(Colors.Red.Darken2);

                                    section.Item()
                                        .PaddingTop(6)
                                        .Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.RelativeColumn(2.5f);
                                                columns.RelativeColumn(2);
                                                columns.RelativeColumn(2);
                                                columns.RelativeColumn(1);
                                            });

                                            table.Header(header =>
                                            {
                                                HeaderCell(header.Cell(), "Medication");
                                                HeaderCell(header.Cell(), "Dosage Form");
                                                HeaderCell(header.Cell(), "Manufacturer");
                                                HeaderCell(header.Cell(), "Stock");
                                            });

                                            foreach (var medication in outOfStockMedications)
                                            {
                                                BodyCell(
                                                    table.Cell(),
                                                    medication.MedicationName);

                                                BodyCell(
                                                    table.Cell(),
                                                    medication.DosageForm);

                                                BodyCell(
                                                    table.Cell(),
                                                    medication.Manufacturer);

                                                BodyCell(
                                                    table.Cell(),
                                                    "0");
                                            }
                                        });
                                });
                        }

                        if (lowStockMedications.Any())
                        {
                            column.Item()
                                .Column(section =>
                                {
                                    section.Item()
                                        .Text("LOW STOCK MEDICATION")
                                        .Bold()
                                        .FontSize(12)
                                        .FontColor(Colors.Orange.Darken2);

                                    section.Item()
                                        .PaddingTop(6)
                                        .Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.RelativeColumn(2.5f);
                                                columns.RelativeColumn(2);
                                                columns.RelativeColumn(1.5f);
                                                columns.RelativeColumn(1);
                                            });

                                            table.Header(header =>
                                            {
                                                HeaderCell(header.Cell(), "Medication");
                                                HeaderCell(header.Cell(), "Dosage Form");
                                                HeaderCell(header.Cell(), "Manufacturer");
                                                HeaderCell(header.Cell(), "Quantity");
                                            });

                                            foreach (var medication in lowStockMedications)
                                            {
                                                BodyCell(
                                                    table.Cell(),
                                                    medication.MedicationName);

                                                BodyCell(
                                                    table.Cell(),
                                                    medication.DosageForm);

                                                BodyCell(
                                                    table.Cell(),
                                                    medication.Manufacturer);

                                                BodyCell(
                                                    table.Cell(),
                                                    (medication.QuantityOnHand ?? 0).ToString("N0"));
                                            }
                                        });
                                });
                        }

                        if (expiredMedications.Any() || expiringSoonMedications.Any())
                        {
                            column.Item()
                                .Column(section =>
                                {
                                    section.Item()
                                        .Text("EXPIRY MONITORING")
                                        .Bold()
                                        .FontSize(12)
                                        .FontColor(Colors.Green.Darken2);

                                    section.Item()
                                        .PaddingTop(6)
                                        .Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.RelativeColumn(2.5f);
                                                columns.RelativeColumn(1.5f);
                                                columns.RelativeColumn(1.5f);
                                                columns.RelativeColumn(1.5f);
                                            });

                                            table.Header(header =>
                                            {
                                                HeaderCell(header.Cell(), "Medication");
                                                HeaderCell(header.Cell(), "Expiry Date");
                                                HeaderCell(header.Cell(), "Quantity");
                                                HeaderCell(header.Cell(), "Status");
                                            });

                                            foreach (var medication in expiredMedications)
                                            {
                                                BodyCell(
                                                    table.Cell(),
                                                    medication.MedicationName);

                                                BodyCell(
                                                    table.Cell(),
                                                    medication.ExpiryDate.HasValue
                                                        ? medication.ExpiryDate.Value.ToString("dd MMM yyyy")
                                                        : "N/A");

                                                BodyCell(
                                                    table.Cell(),
                                                    (medication.QuantityOnHand ?? 0).ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    "EXPIRED");
                                            }

                                            foreach (var medication in expiringSoonMedications)
                                            {
                                                BodyCell(
                                                    table.Cell(),
                                                    medication.MedicationName);

                                                BodyCell(
                                                    table.Cell(),
                                                    medication.ExpiryDate.HasValue
                                                        ? medication.ExpiryDate.Value.ToString("dd MMM yyyy")
                                                        : "N/A");

                                                BodyCell(
                                                    table.Cell(),
                                                    (medication.QuantityOnHand ?? 0).ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    "EXPIRING SOON");
                                            }
                                        });
                                });
                        }

                        column.Item()
                            .Column(section =>
                            {
                                section.Item()
                                    .Text("MEDICATION INVENTORY")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(Colors.Green.Darken2);

                                section.Item()
                                    .PaddingTop(6)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2.5f);
                                            columns.RelativeColumn(1.4f);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1.2f);
                                        });

                                        table.Header(header =>
                                        {
                                            HeaderCell(header.Cell(), "Medication");
                                            HeaderCell(header.Cell(), "Form");
                                            HeaderCell(header.Cell(), "Manufacturer");
                                            HeaderCell(header.Cell(), "Expiry");
                                            HeaderCell(header.Cell(), "Stock");
                                            HeaderCell(header.Cell(), "Status");
                                        });

                                        foreach (var medication in activeMedications)
                                        {
                                            var quantity = medication.QuantityOnHand ?? 0;

                                            var status =
                                                quantity <= 0
                                                    ? "OUT OF STOCK"
                                                    : quantity <= 5
                                                        ? "LOW STOCK"
                                                        : "AVAILABLE";

                                            BodyCell(
                                                table.Cell(),
                                                medication.MedicationName);

                                            BodyCell(
                                                table.Cell(),
                                                medication.DosageForm);

                                            BodyCell(
                                                table.Cell(),
                                                medication.Manufacturer);

                                            BodyCell(
                                                table.Cell(),
                                                medication.ExpiryDate.HasValue
                                                    ? medication.ExpiryDate.Value.ToString("dd/MM/yyyy")
                                                    : "N/A");

                                            BodyCell(
                                                table.Cell(),
                                                quantity.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                status);
                                        }
                                    });
                            });

                        column.Item()
                            .Column(section =>
                            {
                                section.Item()
                                    .Text("MEDICATION USAGE AND CLINICAL ACTIVITY")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(Colors.Green.Darken2);

                                section.Item()
                                    .PaddingTop(6)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2.5f);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.5f);
                                        });

                                        table.Header(header =>
                                        {
                                            HeaderCell(header.Cell(), "Medication");
                                            HeaderCell(header.Cell(), "Stock");
                                            HeaderCell(header.Cell(), "Prescriptions");
                                            HeaderCell(header.Cell(), "Administered");
                                            HeaderCell(header.Cell(), "Activity");
                                        });

                                        foreach (var medication in activeMedications
                                            .OrderByDescending(m =>
                                                (m.Prescription?.Count ?? 0) +
                                                (m.MedicationAdministration?.Count ?? 0))
                                            .ThenBy(m => m.MedicationName))
                                        {
                                            var prescriptionCount =
                                                medication.Prescription?.Count ?? 0;

                                            var administrationCount =
                                                medication.MedicationAdministration?.Count ?? 0;

                                            var activity =
                                                prescriptionCount + administrationCount;

                                            BodyCell(
                                                table.Cell(),
                                                medication.MedicationName);

                                            BodyCell(
                                                table.Cell(),
                                                (medication.QuantityOnHand ?? 0).ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                prescriptionCount.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                administrationCount.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                activity > 0
                                                    ? "Active"
                                                    : "No Activity");
                                        }
                                    });
                            });

                        column.Item()
                            .Column(section =>
                            {
                                section.Item()
                                    .Text("PRESCRIPTION ACTIVITY")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(Colors.Green.Darken2);

                                section.Item()
                                    .PaddingTop(6)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2.5f);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.5f);
                                        });

                                        table.Header(header =>
                                        {
                                            HeaderCell(header.Cell(), "Medication");
                                            HeaderCell(header.Cell(), "Prescriptions");
                                            HeaderCell(header.Cell(), "Current Stock");
                                            HeaderCell(header.Cell(), "Usage Status");
                                        });

                                        foreach (var medication in medicationsWithPrescriptionActivity)
                                        {
                                            var prescriptionCount =
                                                medication.Prescription?.Count ?? 0;

                                            var quantity =
                                                medication.QuantityOnHand ?? 0;

                                            BodyCell(
                                                table.Cell(),
                                                medication.MedicationName);

                                            BodyCell(
                                                table.Cell(),
                                                prescriptionCount.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                quantity.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                quantity <= 0
                                                    ? "Requires Attention"
                                                    : quantity <= 5
                                                        ? "Monitor"
                                                        : "Available");
                                        }
                                    });
                            });

                        column.Item()
                            .Column(section =>
                            {
                                section.Item()
                                    .Text("MEDICATION ADMINISTRATION ACTIVITY")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(Colors.Green.Darken2);

                                section.Item()
                                    .PaddingTop(6)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(2.5f);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.5f);
                                        });

                                        table.Header(header =>
                                        {
                                            HeaderCell(header.Cell(), "Medication");
                                            HeaderCell(header.Cell(), "Administered");
                                            HeaderCell(header.Cell(), "Prescriptions");
                                            HeaderCell(header.Cell(), "Stock");
                                        });

                                        foreach (var medication in medicationsWithAdministrationActivity)
                                        {
                                            BodyCell(
                                                table.Cell(),
                                                medication.MedicationName);

                                            BodyCell(
                                                table.Cell(),
                                                (medication.MedicationAdministration?.Count ?? 0).ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                (medication.Prescription?.Count ?? 0).ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                (medication.QuantityOnHand ?? 0).ToString("N0"));
                                        }
                                    });
                            });

                        column.Item()
                            .PaddingTop(5)
                            .Background(Colors.Green.Lighten5)
                            .Border(1)
                            .BorderColor(Colors.Green.Lighten2)
                            .Padding(10)
                            .Column(note =>
                            {
                                note.Item()
                                    .Text("MANAGEMENT NOTES")
                                    .Bold()
                                    .FontSize(11)
                                    .FontColor(Colors.Green.Darken2);

                                note.Item()
                                    .PaddingTop(5)
                                    .Text(
                                        "This report provides an overview of medication availability, " +
                                        "expiry risks, prescription activity and medication administration " +
                                        "activity. Medicines identified as out of stock, low in stock, " +
                                        "expired or approaching expiry should be reviewed by the appropriate " +
                                        "medical store and clinical management personnel.");

                                note.Item()
                                    .PaddingTop(5)
                                    .Text(
                                        $"Report covers {totalMedicationTypes:N0} active medication type(s), " +
                                        $"with {totalQuantityOnHand:N0} total unit(s) currently recorded " +
                                        "in the medication system.");
                            });
                    });

                page.Footer()
                    .PaddingTop(10)
                    .BorderTop(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(8)
                    .Row(row =>
                    {
                        row.RelativeColumn()
                            .Text(text =>
                            {
                                text.Span(
                                    "Imnandi NovaCare Medical Center | Medication Management Report");
                            });

                        row.ConstantColumn(100)
                            .AlignRight()
                            .Text(text =>
                            {
                                text.Span("Page ");
                                text.CurrentPageNumber();
                                text.Span(" of ");
                                text.TotalPages();
                            });
                    });
            });
        }

        private static void SummaryCard(
            IContainer container,
            string title,
            string value)
        {
            container
                .Padding(5)
                .Background(Colors.White)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(8)
                .Column(column =>
                {
                    column.Item()
                        .AlignCenter()
                        .Text(value)
                        .Bold()
                        .FontSize(15)
                        .FontColor(Colors.Green.Darken2);

                    column.Item()
                        .PaddingTop(3)
                        .AlignCenter()
                        .Text(title)
                        .FontColor(Colors.Grey.Darken2);
                });
        }

        private static void HeaderCell(
            IContainer container,
            string text)
        {
            container
                .Background(Colors.Green.Darken2)
                .BorderBottom(1)
                .BorderColor(Colors.Green.Darken3)
                .Padding(6)
                .AlignCenter()
                .DefaultTextStyle(x =>
                    x.FontColor(Colors.White)
                     .Bold())
                .Text(text);
        }

        private static void BodyCell(
            IContainer container,
            string text)
        {
            container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .Padding(6)
                .Text(string.IsNullOrWhiteSpace(text) ? "N/A" : text);
        }
    }
}