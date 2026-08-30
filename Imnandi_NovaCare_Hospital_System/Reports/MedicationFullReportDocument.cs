using Imnandi_NovaCare_Hospital_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Imnandi_NovaCare_Hospital_System.Reports
{
    public class MedicationFullReportDocument : IDocument
    {
        public MedicationFullReportViewModel Medication { get; set; } = new MedicationFullReportViewModel();

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Medication Report - {Medication.MedicationName}",
            Author = "Imnandi NovaCare Medical Center",
            Subject = "Medication Full Audit Report",
            Keywords = "Medication, Pharmacy, Prescription, Stock, Administration, Hospital Report"
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.PageColor(Colors.White);

                page.DefaultTextStyle(x =>
                    x.FontSize(9)
                     .FontColor(Colors.Grey.Darken3));

                page.Header()
                    .Background(Colors.Blue.Darken2)
                    .Padding(15)
                    .Row(row =>
                    {
                        row.ConstantColumn(85)
                            .Height(65)
                            .AlignMiddle()
                            .Image("wwwroot/images/hospital_logo.jpg", ImageScaling.FitHeight);

                        row.RelativeColumn()
                            .PaddingLeft(15)
                            .Column(col =>
                            {
                                col.Item()
                                    .Text("IMNANDI NOVACARE MEDICAL CENTER")
                                    .Bold()
                                    .FontSize(17)
                                    .FontColor(Colors.White);

                                col.Item()
                                    .PaddingTop(4)
                                    .Text("Medication Full Audit Report")
                                    .SemiBold()
                                    .FontSize(12)
                                    .FontColor(Colors.Blue.Lighten4);

                                col.Item()
                                    .PaddingTop(5)
                                    .Text($"Medication: {Medication.MedicationName ?? "-"}")
                                    .FontSize(10)
                                    .FontColor(Colors.White);
                            });
                    });

                page.Content()
                    .PaddingVertical(15)
                    .Column(col =>
                    {
                        col.Spacing(12);

                        col.Item()
                            .Background(Colors.Grey.Lighten4)
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(12)
                            .Column(info =>
                            {
                                info.Item()
                                    .Text("REPORT OVERVIEW")
                                    .Bold()
                                    .FontSize(13)
                                    .FontColor(Colors.Blue.Darken2);

                                info.Item()
                                    .PaddingTop(8)
                                    .Row(row =>
                                    {
                                        row.RelativeColumn()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Medication").Bold();
                                                c.Item()
                                                    .PaddingTop(2)
                                                    .Text(Medication.MedicationName ?? "-")
                                                    .FontSize(10);
                                            });

                                        row.RelativeColumn()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Medication ID").Bold();
                                                c.Item()
                                                    .PaddingTop(2)
                                                    .Text(Medication.MedicationId.ToString())
                                                    .FontSize(10);
                                            });

                                        row.RelativeColumn()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Current Stock").Bold();
                                                c.Item()
                                                    .PaddingTop(2)
                                                    .Text((Medication.QuantityOnHand ?? 0).ToString())
                                                    .Bold()
                                                    .FontSize(11)
                                                    .FontColor(Colors.Blue.Darken2);
                                            });

                                        row.RelativeColumn()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Generated").Bold();
                                                c.Item()
                                                    .PaddingTop(2)
                                                    .Text(DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
                                                    .FontSize(10);
                                            });
                                    });
                            });

                        col.Item()
                            .Background(Colors.Blue.Lighten5)
                            .Border(1)
                            .BorderColor(Colors.Blue.Lighten2)
                            .Padding(12)
                            .Column(info =>
                            {
                                info.Item()
                                    .Text("MEDICATION INFORMATION")
                                    .Bold()
                                    .FontSize(13)
                                    .FontColor(Colors.Blue.Darken2);

                                info.Item()
                                    .PaddingTop(8)
                                    .Row(row =>
                                    {
                                        row.RelativeColumn()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Medication Name").Bold();
                                                c.Item().PaddingTop(2)
                                                    .Text(Medication.MedicationName ?? "-");
                                            });

                                        row.RelativeColumn()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Dosage Form").Bold();
                                                c.Item().PaddingTop(2)
                                                    .Text(Medication.DosageForm ?? "-");
                                            });

                                        row.RelativeColumn()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Manufacturer").Bold();
                                                c.Item().PaddingTop(2)
                                                    .Text(Medication.Manufacturer ?? "-");
                                            });
                                    });

                                info.Item()
                                    .PaddingTop(8)
                                    .Row(row =>
                                    {
                                        row.RelativeColumn()
                                            .Column(c =>
                                            {
                                                c.Item().Text("Expiry Date").Bold();
                                                c.Item().PaddingTop(2)
                                                    .Text(
                                                        Medication.ExpiryDate.HasValue
                                                            ? Medication.ExpiryDate.Value.ToString("yyyy-MM-dd")
                                                            : "-"
                                                    );
                                            });

                                        row.RelativeColumn(2)
                                            .Column(c =>
                                            {
                                                c.Item().Text("Description").Bold();
                                                c.Item().PaddingTop(2)
                                                    .Text(Medication.Description ?? "-");
                                            });
                                    });
                            });

                        col.Item()
                            .Background(Colors.Grey.Lighten4)
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(12)
                            .Column(summary =>
                            {
                                summary.Item()
                                    .Text("ACTIVITY SUMMARY")
                                    .Bold()
                                    .FontSize(13)
                                    .FontColor(Colors.Blue.Darken2);

                                summary.Item()
                                    .PaddingTop(10)
                                    .Row(row =>
                                    {
                                        row.RelativeColumn()
                                            .Background(Colors.White)
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("ORDERS")
                                                    .Bold()
                                                    .FontSize(8)
                                                    .FontColor(Colors.Grey.Darken1);

                                                c.Item()
                                                    .PaddingTop(3)
                                                    .Text(Medication.TotalOrders.ToString())
                                                    .Bold()
                                                    .FontSize(16)
                                                    .FontColor(Colors.Blue.Darken2);
                                            });

                                        row.RelativeColumn()
                                            .Background(Colors.White)
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("STOCK RECEIVED")
                                                    .Bold()
                                                    .FontSize(8)
                                                    .FontColor(Colors.Grey.Darken1);

                                                c.Item()
                                                    .PaddingTop(3)
                                                    .Text(Medication.TotalStockReceived.ToString())
                                                    .Bold()
                                                    .FontSize(16)
                                                    .FontColor(Colors.Green.Darken2);
                                            });

                                        row.RelativeColumn()
                                            .Background(Colors.White)
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("PRESCRIPTIONS")
                                                    .Bold()
                                                    .FontSize(8)
                                                    .FontColor(Colors.Grey.Darken1);

                                                c.Item()
                                                    .PaddingTop(3)
                                                    .Text(Medication.TotalPrescriptions.ToString())
                                                    .Bold()
                                                    .FontSize(16)
                                                    .FontColor(Colors.Blue.Darken2);
                                            });

                                        row.RelativeColumn()
                                            .Background(Colors.White)
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("SCRIPT ACTIONS")
                                                    .Bold()
                                                    .FontSize(8)
                                                    .FontColor(Colors.Grey.Darken1);

                                                c.Item()
                                                    .PaddingTop(3)
                                                    .Text(Medication.TotalScriptActions.ToString())
                                                    .Bold()
                                                    .FontSize(16)
                                                    .FontColor(Colors.Blue.Darken2);
                                            });

                                        row.RelativeColumn()
                                            .Background(Colors.White)
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("ADMINISTRATIONS")
                                                    .Bold()
                                                    .FontSize(8)
                                                    .FontColor(Colors.Grey.Darken1);

                                                c.Item()
                                                    .PaddingTop(3)
                                                    .Text(Medication.TotalAdministrations.ToString())
                                                    .Bold()
                                                    .FontSize(16)
                                                    .FontColor(Colors.Blue.Darken2);
                                            });
                                    });
                            });

                        col.Item()
                            .Background(Colors.Blue.Lighten5)
                            .Border(1)
                            .BorderColor(Colors.Blue.Lighten2)
                            .Padding(12)
                            .Column(stock =>
                            {
                                stock.Item()
                                    .Text("STOCK SUMMARY")
                                    .Bold()
                                    .FontSize(13)
                                    .FontColor(Colors.Blue.Darken2);

                                stock.Item()
                                    .PaddingTop(8)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                            columns.RelativeColumn();
                                        });

                                        table.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(7)
                                            .Text("Opening Stock")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        table.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(7)
                                            .Text("Received")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        table.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(7)
                                            .Text("Current / Closing")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        table.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(7)
                                            .Text("Recorded Usage")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .Text(Medication.OpeningStock.ToString())
                                            .Bold()
                                            .FontSize(11);

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .Text(Medication.TotalStockReceived.ToString())
                                            .Bold()
                                            .FontSize(11);

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .Text(Medication.ClosingStock.ToString())
                                            .Bold()
                                            .FontSize(11)
                                            .FontColor(Colors.Blue.Darken2);

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(8)
                                            .Text(Medication.TotalStockUsed.ToString())
                                            .Bold()
                                            .FontSize(11);
                                    });

                                stock.Item()
                                    .PaddingTop(6)
                                    .Text("Current stock reflects the quantity currently recorded against the medication in the system.")
                                    .Italic()
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken1);
                            });

                        if (Medication.StockReceived.Any())
                        {
                            col.Item()
                                .PaddingTop(8)
                                .Text("STOCK RECEIVED / PURCHASE HISTORY")
                                .Bold()
                                .FontSize(13)
                                .FontColor(Colors.Blue.Darken2);

                            col.Item()
                                .LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten2);

                            col.Item()
                                .PaddingTop(6)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.3f);
                                        columns.RelativeColumn(1.8f);
                                        columns.RelativeColumn(1.8f);
                                        columns.RelativeColumn(1.4f);
                                        columns.RelativeColumn(1.4f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Order").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Date").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Supplier").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Hospital Store").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Requested").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Received").Bold().FontColor(Colors.White);
                                    });

                                    foreach (var s in Medication.StockReceived.OrderByDescending(x => x.OrderDate))
                                    {
                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.OrderNumber ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.OrderDate.ToString("yyyy-MM-dd"));

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.SupplierName ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.HospitalStoreName ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.QuantityRequested.ToString());

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.QuantityReceived.ToString())
                                            .Bold();
                                    }
                                });

                            col.Item()
                                .PaddingTop(5)
                                .Text($"Total quantity received across recorded orders: {Medication.TotalStockReceived}")
                                .Bold()
                                .FontSize(9);
                        }

                        if (Medication.Prescriptions.Any())
                        {
                            col.Item()
                                .PaddingTop(15)
                                .Text("PRESCRIPTION HISTORY")
                                .Bold()
                                .FontSize(13)
                                .FontColor(Colors.Blue.Darken2);

                            col.Item()
                                .LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten2);

                            col.Item()
                                .PaddingTop(6)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2.2f);
                                        columns.RelativeColumn(2.2f);
                                        columns.RelativeColumn(1.3f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.4f);
                                        columns.RelativeColumn(2.8f);
                                        columns.RelativeColumn(2f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Doctor").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Patient").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Date").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Time").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Status").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Dosage Instructions").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Script Manager").Bold().FontColor(Colors.White);
                                    });

                                    foreach (var p in Medication.Prescriptions.OrderByDescending(x => x.IssueDate).ThenByDescending(x => x.IssueTime))
                                    {
                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text($"{p.DoctorName ?? "-"}\n{p.DoctorRole ?? "-"}");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(p.PatientName ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(p.IssueDate.ToString("yyyy-MM-dd"));

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(p.IssueTime.ToString("HH:mm"));

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(p.Status ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(p.DosageInstructions ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(p.ScriptManagerName ?? "-");
                                    }
                                });
                        }
                        else
                        {
                            col.Item()
                                .PaddingTop(15)
                                .Text("PRESCRIPTION HISTORY")
                                .Bold()
                                .FontSize(13)
                                .FontColor(Colors.Blue.Darken2);

                            col.Item()
                                .Background(Colors.Grey.Lighten4)
                                .Padding(10)
                                .Text("No prescription records were found for this medication.");
                        }

                        if (Medication.ScriptActions.Any())
                        {
                            col.Item()
                                .PaddingTop(15)
                                .Text("SCRIPT MANAGER ACTIVITY")
                                .Bold()
                                .FontSize(13)
                                .FontColor(Colors.Blue.Darken2);

                            col.Item()
                                .LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten2);

                            col.Item()
                                .PaddingTop(6)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2.2f);
                                        columns.RelativeColumn(1.4f);
                                        columns.RelativeColumn(1.4f);
                                        columns.RelativeColumn(1.4f);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.3f);
                                        columns.RelativeColumn(2f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Script Manager").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Status").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Assigned").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Processed").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Received").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Verified").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Verified By").Bold().FontColor(Colors.White);
                                    });

                                    foreach (var s in Medication.ScriptActions.OrderByDescending(x => x.AssignedDate))
                                    {
                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.ScriptManagerName ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.Status ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.AssignedDate?.ToString("yyyy-MM-dd") ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.ProcessedDate?.ToString("yyyy-MM-dd") ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.ReceivedDate?.ToString("yyyy-MM-dd") ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.IsVerified ? "Yes" : "No");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.VerifiedBy ?? "-");
                                    }
                                });

                            col.Item()
                                .PaddingTop(6)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(2f);
                                        columns.RelativeColumn(3f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Darken1).Padding(6).Text("Prescription ID").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Grey.Darken1).Padding(6).Text("Received From").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Grey.Darken1).Padding(6).Text("Verification Date").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Grey.Darken1).Padding(6).Text("Notes").Bold().FontColor(Colors.White);
                                    });

                                    foreach (var s in Medication.ScriptActions.OrderByDescending(x => x.AssignedDate))
                                    {
                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.PrescriptionId.ToString());

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.ReceivedFromUser ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.VerifiedDate?.ToString("yyyy-MM-dd HH:mm") ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(s.Notes ?? "-");
                                    }
                                });
                        }
                        else
                        {
                            col.Item()
                                .PaddingTop(15)
                                .Text("SCRIPT MANAGER ACTIVITY")
                                .Bold()
                                .FontSize(13)
                                .FontColor(Colors.Blue.Darken2);

                            col.Item()
                                .Background(Colors.Grey.Lighten4)
                                .Padding(10)
                                .Text("No script processing records were found for this medication.");
                        }

                        if (Medication.Administrations.Any())
                        {
                            col.Item()
                                .PaddingTop(15)
                                .Text("MEDICATION ADMINISTRATION HISTORY")
                                .Bold()
                                .FontSize(13)
                                .FontColor(Colors.Blue.Darken2);

                            col.Item()
                                .LineHorizontal(1)
                                .LineColor(Colors.Grey.Lighten2);

                            col.Item()
                                .PaddingTop(6)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.7f);
                                        columns.RelativeColumn(1.7f);
                                        columns.RelativeColumn(2f);
                                        columns.RelativeColumn(2f);
                                        columns.RelativeColumn(1.4f);
                                        columns.RelativeColumn(2.2f);
                                        columns.RelativeColumn(1.2f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Date").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Patient").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Doctor").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Nurse").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Nurse Sister").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Dosage / Purpose").Bold().FontColor(Colors.White);
                                        header.Cell().Background(Colors.Blue.Darken2).Padding(6).Text("Seen").Bold().FontColor(Colors.White);
                                    });

                                    foreach (var a in Medication.Administrations.OrderByDescending(x => x.DateAdministered))
                                    {
                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(a.DateAdministered.ToString("yyyy-MM-dd"));

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(a.PatientName ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(a.DoctorName ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(
                                                a.NurseName != null
                                                    ? $"{a.NurseName}\n{a.NurseRole ?? ""}"
                                                    : "-"
                                            );

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(a.NurseSisterName ?? "-");

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(
                                                $"Dosage: {a.Dosage ?? "-"}\n" +
                                                $"Purpose: {a.Purpose ?? "-"}"
                                            );

                                        table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                            .Text(a.IsSeen ? "Yes" : "No");
                                    }
                                });
                        }
                        else
                        {
                            col.Item()
                                .PaddingTop(15)
                                .Text("MEDICATION ADMINISTRATION HISTORY")
                                .Bold()
                                .FontSize(13)
                                .FontColor(Colors.Blue.Darken2);

                            col.Item()
                                .Background(Colors.Grey.Lighten4)
                                .Padding(10)
                                .Text("No medication administration records were found for this medication.");
                        }

                        col.Item()
                            .PaddingTop(18)
                            .Background(Colors.Grey.Lighten4)
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(10)
                            .Column(note =>
                            {
                                note.Item()
                                    .Text("REPORT NOTES")
                                    .Bold()
                                    .FontSize(11)
                                    .FontColor(Colors.Blue.Darken2);

                                note.Item()
                                    .PaddingTop(5)
                                    .Text(
                                        "This report provides a consolidated audit view of the selected medication, " +
                                        "including medication information, recorded stock orders, prescriptions, " +
                                        "script processing activities, verification activity, and medication administrations."
                                    )
                                    .FontSize(8.5f);

                                note.Item()
                                    .PaddingTop(4)
                                    .Text(
                                        "Stock quantities shown in this report are based on the quantities currently " +
                                        "recorded in the hospital medication system."
                                    )
                                    .FontSize(8.5f)
                                    .Italic();
                            });
                    });

                page.Footer()
                    .BorderTop(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .PaddingTop(8)
                    .Row(row =>
                    {
                        row.RelativeColumn()
                            .Column(col =>
                            {
                                col.Item()
                                    .Text("Imnandi NovaCare Medical Center")
                                    .Bold()
                                    .FontSize(8)
                                    .FontColor(Colors.Grey.Darken2);

                                col.Item()
                                    .PaddingTop(2)
                                    .Text($"Confidential Medication Report | Generated {DateTime.Now:yyyy-MM-dd HH:mm}")
                                    .FontSize(7.5f)
                                    .FontColor(Colors.Grey.Darken1);
                            });

                        row.ConstantColumn(70)
                            .AlignRight()
                            .Column(col =>
                            {
                                col.Item()
                                    .Text(text =>
                                    {
                                        text.DefaultTextStyle(x =>
                                            x.FontSize(8)
                                             .FontColor(Colors.Grey.Darken2));

                                        text.Span("Page ");
                                        text.CurrentPageNumber();
                                        text.Span(" of ");
                                        text.TotalPages();
                                    });
                            });
                    });
            });
        }
    }
}