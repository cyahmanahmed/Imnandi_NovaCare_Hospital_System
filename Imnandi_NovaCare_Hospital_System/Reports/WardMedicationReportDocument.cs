using Imnandi_NovaCare_Hospital_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Imnandi_NovaCare_Hospital_System.Reports
{
    public class WardMedicationReportDocument : IDocument
    {
        public WardMedicationReportViewModel Ward { get; set; } = new WardMedicationReportViewModel();

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Ward Medication Report - {Ward.WardName}",
            Author = "Imnandi NovaCare Medical Center",
            Subject = "Ward Medication Stock and Transaction Report",
            Keywords = "Ward, Medication, Stock, Medicine Store, Stock Manager, Transactions"
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
                            .Image(
                                "wwwroot/images/hospital_logo.jpg",
                                ImageScaling.FitHeight
                            );

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
                                    .Text("Ward Medication Stock Report")
                                    .SemiBold()
                                    .FontSize(12)
                                    .FontColor(Colors.Blue.Lighten4);

                                col.Item()
                                    .PaddingTop(5)
                                    .Text($"Ward: {Ward.WardName ?? "-"}")
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
                                    .Text("WARD INFORMATION")
                                    .Bold()
                                    .FontSize(13)
                                    .FontColor(Colors.Blue.Darken2);

                                info.Item()
                                    .PaddingTop(9)
                                    .Row(row =>
                                    {
                                        row.RelativeColumn()
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("Ward")
                                                    .Bold();

                                                c.Item()
                                                    .PaddingTop(2)
                                                    .Text(Ward.WardName ?? "-")
                                                    .FontSize(10);
                                            });

                                        row.RelativeColumn()
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("Location")
                                                    .Bold();

                                                c.Item()
                                                    .PaddingTop(2)
                                                    .Text(Ward.Location ?? "-")
                                                    .FontSize(10);
                                            });


                                        row.RelativeColumn()
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("Report Date")
                                                    .Bold();

                                                c.Item()
                                                    .PaddingTop(2)
                                                    .Text(
                                                        Ward.ReportGenerated
                                                            .ToString("yyyy-MM-dd HH:mm")
                                                    )
                                                    .FontSize(10);
                                            });
                                    });

                                info.Item()
                                    .PaddingTop(9)
                                    .Row(row =>
                                    {
                                        row.RelativeColumn(2)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("Description")
                                                    .Bold();

                                                c.Item()
                                                    .PaddingTop(2)
                                                    .Text(Ward.Description ?? "-")
                                                    .FontSize(10);
                                            });
                                    });
                            });

                        col.Item()
                            .Background(Colors.Blue.Lighten5)
                            .Border(1)
                            .BorderColor(Colors.Blue.Lighten2)
                            .Padding(12)
                            .Column(summary =>
                            {
                                summary.Item()
                                    .Text("STOCK SUMMARY")
                                    .Bold()
                                    .FontSize(13)
                                    .FontColor(Colors.Blue.Darken2);

                                summary.Item()
                                    .PaddingTop(9)
                                    .Row(row =>
                                    {
                                        row.RelativeColumn()
                                            .Background(Colors.White)
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(9)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("MEDICATION TYPES")
                                                    .Bold()
                                                    .FontSize(8)
                                                    .FontColor(Colors.Grey.Darken1);

                                                c.Item()
                                                    .PaddingTop(3)
                                                    .Text(
                                                        Ward.TotalMedicationTypes.ToString()
                                                    )
                                                    .Bold()
                                                    .FontSize(17)
                                                    .FontColor(Colors.Blue.Darken2);
                                            });

                                        row.RelativeColumn()
                                            .Background(Colors.White)
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(9)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("QUANTITY IN WARD")
                                                    .Bold()
                                                    .FontSize(8)
                                                    .FontColor(Colors.Grey.Darken1);

                                                c.Item()
                                                    .PaddingTop(3)
                                                    .Text(
                                                        Ward.TotalQuantityInWard.ToString()
                                                    )
                                                    .Bold()
                                                    .FontSize(17)
                                                    .FontColor(Colors.Blue.Darken2);
                                            });

                                        row.RelativeColumn()
                                            .Background(Colors.White)
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .Padding(9)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text("TOTAL RECEIVED")
                                                    .Bold()
                                                    .FontSize(8)
                                                    .FontColor(Colors.Grey.Darken1);

                                                c.Item()
                                                    .PaddingTop(3)
                                                    .Text(
                                                        Ward.TotalReceived.ToString()
                                                    )
                                                    .Bold()
                                                    .FontSize(17)
                                                    .FontColor(Colors.Green.Darken2);
                                            });
                                    });
                            });

                        col.Item()
                            .PaddingTop(4)
                            .Text("CURRENT MEDICATION STOCK IN WARD")
                            .Bold()
                            .FontSize(13)
                            .FontColor(Colors.Blue.Darken2);

                        col.Item()
                            .LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);

                        if (Ward.CurrentStock.Any())
                        {
                            col.Item()
                                .PaddingTop(6)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2.2f);
                                        columns.RelativeColumn(1.3f);
                                        columns.RelativeColumn(1.8f);
                                        columns.RelativeColumn(1.3f);
                                        columns.RelativeColumn(1.2f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(6)
                                            .Text("Medication")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(6)
                                            .Text("Dosage Form")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(6)
                                            .Text("Manufacturer")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(6)
                                            .Text("Expiry")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(6)
                                            .Text("Quantity")
                                            .Bold()
                                            .FontColor(Colors.White);
                                    });

                                    foreach (var medication in Ward.CurrentStock)
                                    {
                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(medication.MedicationName ?? "-")
                                            .Bold();

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(medication.DosageForm ?? "-");

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(medication.Manufacturer ?? "-");

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(
                                                medication.ExpiryDate.HasValue
                                                    ? medication.ExpiryDate.Value.ToString("yyyy-MM-dd")
                                                    : "-"
                                            );

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(
                                                medication.QuantityInWard.ToString()
                                            )
                                            .Bold()
                                            .FontColor(
                                                medication.QuantityInWard <= 0
                                                    ? Colors.Red.Darken2
                                                    : Colors.Blue.Darken2
                                            );
                                    }
                                });
                        }
                        else
                        {
                            col.Item()
                                .Background(Colors.Grey.Lighten4)
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .Text("No medication stock is currently recorded for this ward.");
                        }

                        col.Item()
                            .PaddingTop(15)
                            .Text("MEDICATION STOCK MOVEMENT HISTORY")
                            .Bold()
                            .FontSize(13)
                            .FontColor(Colors.Blue.Darken2);

                        col.Item()
                            .LineHorizontal(1)
                            .LineColor(Colors.Grey.Lighten2);

                        if (Ward.Transactions.Any())
                        {
                            col.Item()
                                .PaddingTop(6)
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2.1f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.3f);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.4f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(6)
                                            .Text("Medication")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(6)
                                            .Text("Dosage Form")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(6)
                                            .Text("Date")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(6)
                                            .Text("Transaction")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Blue.Darken2)
                                            .Padding(6)
                                            .Text("Quantity")
                                            .Bold()
                                            .FontColor(Colors.White);
                                    });

                                    foreach (var transaction in Ward.Transactions)
                                    {
                                        var transactionType =
                                            transaction.TransactionType ?? "-";

                                        var transactionColor =
                                            transactionType.ToLower() == "received"
                                                ? Colors.Green.Darken2
                                                : transactionType.ToLower() == "issued"
                                                    ? Colors.Red.Darken2
                                                    : Colors.Orange.Darken2;

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(transaction.MedicationName ?? "-")
                                            .Bold();

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(transaction.DosageForm ?? "-");

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(
                                                transaction.DateReceived
                                                    .ToString("yyyy-MM-dd HH:mm")
                                            );

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(transactionType)
                                            .Bold()
                                            .FontColor(transactionColor);

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(
                                                transaction.Quantity.ToString()
                                            )
                                            .Bold();
                                    }
                                });
                        }
                        else
                        {
                            col.Item()
                                .Background(Colors.Grey.Lighten4)
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(10)
                                .Text("No medication stock transactions have been recorded for this ward.");
                        }

                        if (Ward.Transactions.Any())
                        {
                            col.Item()
                                .PaddingTop(15)
                                .Text("MEDICATION MOVEMENT SUMMARY")
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
                                        columns.RelativeColumn(2.5f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(1.5f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Background(Colors.Grey.Darken1)
                                            .Padding(6)
                                            .Text("Medication")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Grey.Darken1)
                                            .Padding(6)
                                            .Text("Received")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Grey.Darken1)
                                            .Padding(6)
                                            .Text("Issued")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Grey.Darken1)
                                            .Padding(6)
                                            .Text("Adjusted")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Grey.Darken1)
                                            .Padding(6)
                                            .Text("Current Stock")
                                            .Bold()
                                            .FontColor(Colors.White);
                                    });

                                    var medicationGroups = Ward.Transactions
                                        .GroupBy(x => new
                                        {
                                            x.MedicationId,
                                            x.MedicationName
                                        })
                                        .OrderBy(x => x.Key.MedicationName);

                                    foreach (var group in medicationGroups)
                                    {
                                        var received = group
                                            .Where(x =>
                                                x.TransactionType != null &&
                                                x.TransactionType.ToLower() == "received")
                                            .Sum(x => x.Quantity);

                                        var issued = group
                                            .Where(x =>
                                                x.TransactionType != null &&
                                                x.TransactionType.ToLower() == "issued")
                                            .Sum(x => x.Quantity);

                                        var adjusted = group
                                            .Where(x =>
                                                x.TransactionType != null &&
                                                x.TransactionType.ToLower() == "adjusted")
                                            .Sum(x => x.Quantity);

                                        var currentStock = Ward.CurrentStock
                                            .FirstOrDefault(x =>
                                                x.MedicationId == group.Key.MedicationId);

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(group.Key.MedicationName ?? "-")
                                            .Bold();

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(received.ToString());

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(issued.ToString());

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(adjusted.ToString());

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(
                                                currentStock != null
                                                    ? currentStock.QuantityInWard.ToString()
                                                    : "0"
                                            )
                                            .Bold()
                                            .FontColor(Colors.Blue.Darken2);
                                    }
                                });
                        }

                        var expiringMedications = Ward.CurrentStock
                            .Where(x =>
                                x.ExpiryDate.HasValue &&
                                x.ExpiryDate.Value.Date <= DateTime.Now.Date.AddDays(30))
                            .OrderBy(x => x.ExpiryDate)
                            .ToList();

                        if (expiringMedications.Any())
                        {
                            col.Item()
                                .PaddingTop(15)
                                .Text("EXPIRY ALERTS")
                                .Bold()
                                .FontSize(13)
                                .FontColor(Colors.Red.Darken2);

                            col.Item()
                                .LineHorizontal(1)
                                .LineColor(Colors.Red.Lighten2);

                            col.Item()
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
                                        header.Cell()
                                            .Background(Colors.Red.Darken2)
                                            .Padding(6)
                                            .Text("Medication")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Red.Darken2)
                                            .Padding(6)
                                            .Text("Expiry Date")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Red.Darken2)
                                            .Padding(6)
                                            .Text("Quantity")
                                            .Bold()
                                            .FontColor(Colors.White);

                                        header.Cell()
                                            .Background(Colors.Red.Darken2)
                                            .Padding(6)
                                            .Text("Status")
                                            .Bold()
                                            .FontColor(Colors.White);
                                    });

                                    foreach (var medication in expiringMedications)
                                    {
                                        var daysRemaining =
                                            (medication.ExpiryDate.Value.Date -
                                             DateTime.Now.Date).Days;

                                        string status;

                                        if (daysRemaining < 0)
                                        {
                                            status = "EXPIRED";
                                        }
                                        else if (daysRemaining == 0)
                                        {
                                            status = "EXPIRES TODAY";
                                        }
                                        else
                                        {
                                            status = $"{daysRemaining} days remaining";
                                        }

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(medication.MedicationName ?? "-")
                                            .Bold();

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(
                                                medication.ExpiryDate.Value
                                                    .ToString("yyyy-MM-dd")
                                            );

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(
                                                medication.QuantityInWard.ToString()
                                            );

                                        table.Cell()
                                            .Border(1)
                                            .BorderColor(Colors.Grey.Lighten3)
                                            .Padding(6)
                                            .Text(status)
                                            .Bold()
                                            .FontColor(Colors.Red.Darken2);
                                    }
                                });
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
                                        "This report provides a consolidated view of medication stock " +
                                        "currently held by the selected ward and the medication stock " +
                                        "movements recorded against the ward."
                                    )
                                    .FontSize(8.5f);

                                note.Item()
                                    .PaddingTop(4)
                                    .Text(
                                        "Current quantities are based on the WardMedicationStock records. " +
                                        "Transaction totals are based on recorded WardMedicationTransaction " +
                                        "entries."
                                    )
                                    .FontSize(8.5f)
                                    .Italic();

                                note.Item()
                                    .PaddingTop(4)
                                    .Text(
                                        "Expiry information is based on the expiry date currently recorded " +
                                        "against each medication."
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
                                    .Text(
                                        $"Confidential Ward Medication Report | Generated {Ward.ReportGenerated:yyyy-MM-dd HH:mm}"
                                    )
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