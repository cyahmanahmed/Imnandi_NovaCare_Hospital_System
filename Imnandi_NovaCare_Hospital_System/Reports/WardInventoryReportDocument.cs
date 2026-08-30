using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace Imnandi_NovaCare_Hospital_System.Models
{
    public class WardInventoryReportDocument : IDocument
    {
        public WardInventoryReportViewModel Ward { get; set; }

        public DocumentMetadata GetMetadata()
        {
            return new DocumentMetadata
            {
                Title = $"Ward Inventory Report - {Ward?.WardName ?? "Unknown Ward"}",
                Author = "Imnandi NovaCare Medical Center",
                Subject = "Ward Inventory, Medication, Consumables and Stock Take Report",
                Keywords = "Ward Inventory, Medication, Consumables, Stock Take, Hospital",
                Creator = "Imnandi NovaCare Medical Center"
            };
        }

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(x =>
                        x.FontFamily("Arial")
                         .FontSize(9)
                         .FontColor(Colors.Grey.Darken3));

                    page.Header()
                        .Element(ComposeHeader);

                    page.Content()
                        .PaddingTop(15)
                        .Element(ComposeContent);

                    page.Footer()
                        .Element(ComposeFooter);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container
                .Background(Colors.Blue.Darken2)
                .Padding(18)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("IMNANDI NOVACARE MEDICAL CENTER")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.White);

                            column.Item()
                                .PaddingTop(4)
                                .Text("WARD INVENTORY REPORT")
                                .FontSize(12)
                                .SemiBold()
                                .FontColor(Colors.Blue.Lighten4);

                            column.Item()
                                .PaddingTop(8)
                                .Text($"Ward: {Ward?.WardName ?? "N/A"}")
                                .FontSize(10)
                                .FontColor(Colors.White);
                        });

                    row.ConstantItem(150)
                        .AlignRight()
                        .Column(column =>
                        {
                            column.Item()
                                .AlignRight()
                                .Text("INTERNAL REPORT")
                                .FontSize(9)
                                .Bold()
                                .FontColor(Colors.White);

                            column.Item()
                                .PaddingTop(5)
                                .AlignRight()
                                .Text($"Generated: {Ward?.ReportDate:dd MMM yyyy HH:mm}")
                                .FontSize(8)
                                .FontColor(Colors.White);
                        });
                });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(12);

                column.Item()
                    .Element(ComposeWardInformation);

                column.Item()
                    .Element(ComposeSummary);

                if (Ward?.StockManagers?.Any() == true)
                {
                    column.Item()
                        .Element(ComposeStockManagers);
                }

                if (Ward?.Medications?.Any() == true)
                {
                    column.Item()
                        .Element(ComposeMedicationStock);
                }

                if (Ward?.Consumables?.Any() == true)
                {
                    column.Item()
                        .Element(ComposeConsumableStock);
                }

                if (Ward?.MedicationTransactions?.Any() == true)
                {
                    column.Item()
                        .Element(ComposeMedicationTransactions);
                }

                if (Ward?.Prescriptions?.Any() == true)
                {
                    column.Item()
                        .Element(ComposePrescriptions);
                }

                if (Ward?.MedicationAdministrations?.Any() == true)
                {
                    column.Item()
                        .Element(ComposeMedicationAdministrations);
                }

                if (Ward?.StockTakes?.Any() == true)
                {
                    column.Item()
                        .Element(ComposeStockTakes);
                }

                column.Item()
                    .Element(ComposeReportNotes);
            });
        }

        private void ComposeWardInformation(IContainer container)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten5)
                .Padding(12)
                .Column(column =>
                {
                    column.Item()
                        .Text("WARD INFORMATION")
                        .FontSize(13)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item()
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

                            InfoCell(table, "Ward", Ward?.WardName);
                            InfoCell(table, "Location", Ward?.Location);
                            InfoCell(table, "Capacity", $"{Ward?.Capacity:N0} beds");
                            InfoCell(
                                table,
                                "Report Date",
                                Ward?.ReportDate.ToString("dd MMM yyyy HH:mm"));

                            InfoCell(
                                table,
                                "Nurse Sister",
                                string.IsNullOrWhiteSpace(Ward?.NurseSisterName)
                                    ? "Not assigned"
                                    : Ward.NurseSisterName);

                            InfoCell(
                                table,
                                "Reporting Period",
                                BuildReportingPeriod());

                            InfoCell(
                                table,
                                "Description",
                                string.IsNullOrWhiteSpace(Ward?.Description)
                                    ? "No description provided."
                                    : Ward.Description);

                            InfoCell(
                                table,
                                "Medication Types",
                                $"{Ward?.TotalMedicationTypes ?? 0:N0}");
                        });
                });
        }

        private void ComposeSummary(IContainer container)
        {
            container
                .Border(1)
                .BorderColor(Colors.Blue.Lighten2)
                .Background(Colors.Blue.Lighten5)
                .Padding(12)
                .Column(column =>
                {
                    column.Item()
                        .Text("INVENTORY SUMMARY")
                        .FontSize(13)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item()
                        .PaddingTop(8)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            SummaryCell(
                                table,
                                "MEDICATION TYPES",
                                $"{Ward?.TotalMedicationTypes ?? 0:N0}");

                            SummaryCell(
                                table,
                                "MEDICATION UNITS IN WARD",
                                $"{Ward?.TotalMedicationQuantityInWard ?? 0:N0}");

                            SummaryCell(
                                table,
                                "CONSUMABLE TYPES",
                                $"{Ward?.TotalConsumableTypes ?? 0:N0}");

                            SummaryCell(
                                table,
                                "CONSUMABLE UNITS IN WARD",
                                $"{Ward?.TotalConsumableQuantityInWard ?? 0:N0}");

                            SummaryCell(
                                table,
                                "STOCK TAKES",
                                $"{Ward?.TotalStockTakes ?? 0:N0}");

                            SummaryCell(
                                table,
                                "MEDICATION RECEIVED",
                                $"{Ward?.TotalMedicationReceived ?? 0:N0}");

                            SummaryCell(
                                table,
                                "MEDICATION ISSUED",
                                $"{Ward?.TotalMedicationIssued ?? 0:N0}");

                            SummaryCell(
                                table,
                                "ADJUSTMENTS",
                                $"{Ward?.TotalMedicationAdjusted ?? 0:N0}");

                            SummaryCell(
                                table,
                                "ITEMS COUNTED",
                                $"{Ward?.TotalItemsCounted ?? 0:N0}");

                            SummaryCell(
                                table,
                                "MATCHING ITEMS",
                                $"{Ward?.TotalMatchingItems ?? 0:N0}");
                        });

                    column.Item()
                        .PaddingTop(8)
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            SummaryStatusCell(
                                table,
                                "SHORTAGES",
                                $"{Ward?.TotalShortages ?? 0:N0}");

                            SummaryStatusCell(
                                table,
                                "SURPLUSES",
                                $"{Ward?.TotalSurpluses ?? 0:N0}");

                            SummaryStatusCell(
                                table,
                                "MATCHING",
                                $"{Ward?.TotalMatchingItems ?? 0:N0}");
                        });
                });
        }

        private void ComposeStockManagers(IContainer container)
        {
            SectionTitle(container, "STOCK MANAGERS RESPONSIBLE FOR INVENTORY");

            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                    });

                    TableHeader(table, "#");
                    TableHeader(table, "STOCK MANAGER");
                    TableHeader(table, "DEPARTMENT");

                    var index = 1;

                    foreach (var manager in Ward.StockManagers)
                    {
                        BodyCell(table, index.ToString());
                        BodyCell(
                            table,
                            string.IsNullOrWhiteSpace(manager.StockManagerName)
                                ? "N/A"
                                : manager.StockManagerName);
                        BodyCell(
                            table,
                            string.IsNullOrWhiteSpace(manager.Department)
                                ? "N/A"
                                : manager.Department);

                        index++;
                    }
                });
        }

        private void ComposeMedicationStock(IContainer container)
        {
            SectionTitle(container, "CURRENT MEDICATION STOCK IN WARD");

            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.ConstantColumn(55);
                        columns.ConstantColumn(55);
                        columns.ConstantColumn(60);
                        columns.ConstantColumn(65);
                    });

                    TableHeader(table, "MEDICATION");
                    TableHeader(table, "DOSAGE FORM");
                    TableHeader(table, "MANUFACTURER");
                    TableHeader(table, "EXPIRY");
                    TableHeader(table, "IN WARD");
                    TableHeader(table, "RECEIVED");
                    TableHeader(table, "ISSUED");
                    TableHeader(table, "STATUS");

                    foreach (var medication in Ward.Medications)
                    {
                        BodyCell(
                            table,
                            medication.MedicationName ?? "N/A");

                        BodyCell(
                            table,
                            medication.DosageForm ?? "N/A");

                        BodyCell(
                            table,
                            medication.Manufacturer ?? "N/A");

                        BodyCell(
                            table,
                            medication.ExpiryDate.HasValue
                                ? medication.ExpiryDate.Value.ToString("dd MMM yyyy")
                                : "N/A");

                        BodyCell(
                            table,
                            medication.QuantityInWard.ToString("N0"));

                        BodyCell(
                            table,
                            medication.TotalReceived.ToString("N0"));

                        BodyCell(
                            table,
                            medication.TotalIssued.ToString("N0"));

                        StatusCell(
                            table,
                            medication.StockStatus);
                    }
                });
        }

        private void ComposeConsumableStock(IContainer container)
        {
            SectionTitle(container, "CURRENT CONSUMABLE STOCK IN WARD");

            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.ConstantColumn(65);
                        columns.ConstantColumn(65);
                        columns.ConstantColumn(65);
                        columns.RelativeColumn();
                    });

                    TableHeader(table, "CONSUMABLE");
                    TableHeader(table, "UNIT");
                    TableHeader(table, "EXPIRY");
                    TableHeader(table, "SYSTEM");
                    TableHeader(table, "PHYSICAL");
                    TableHeader(table, "DIFFERENCE");
                    TableHeader(table, "STATUS");

                    foreach (var consumable in Ward.Consumables)
                    {
                        BodyCell(
                            table,
                            consumable.ConsumableName ?? "N/A");

                        BodyCell(
                            table,
                            consumable.Unit ?? "N/A");

                        BodyCell(
                            table,
                            consumable.ExpiryDate.ToString("dd MMM yyyy"));

                        BodyCell(
                            table,
                            consumable.SystemQuantity.ToString("N0"));

                        BodyCell(
                            table,
                            consumable.PhysicalQuantity.ToString("N0"));

                        BodyCell(
                            table,
                            consumable.Difference.ToString("+0;-0;0"));

                        StatusCell(
                            table,
                            consumable.StockStatus);
                    }
                });

            container
                .PaddingTop(6)
                .Text(
                    "System Quantity represents the quantity recorded by the system. " +
                    "Physical Quantity represents the quantity physically counted during stock verification.")
                .FontSize(7)
                .Italic()
                .FontColor(Colors.Grey.Darken1);
        }

        private void ComposeMedicationTransactions(IContainer container)
        {
            SectionTitle(container, "MEDICATION STOCK TRANSACTIONS");

            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(65);
                        columns.RelativeColumn(2);
                        columns.ConstantColumn(65);
                        columns.ConstantColumn(85);
                        columns.RelativeColumn();
                    });

                    TableHeader(table, "DATE");
                    TableHeader(table, "MEDICATION");
                    TableHeader(table, "QUANTITY");
                    TableHeader(table, "TYPE");
                    TableHeader(table, "TRANSACTION");

                    foreach (var transaction in Ward.MedicationTransactions)
                    {
                        BodyCell(
                            table,
                            transaction.Date.ToString("dd MMM yyyy HH:mm"));

                        BodyCell(
                            table,
                            transaction.MedicationName ?? "N/A");

                        BodyCell(
                            table,
                            transaction.Quantity.ToString("N0"));

                        StatusCell(
                            table,
                            transaction.TransactionType);

                        BodyCell(
                            table,
                            BuildTransactionDescription(transaction));
                    }
                });
        }

        private void ComposePrescriptions(IContainer container)
        {
            SectionTitle(container, "MEDICATION PRESCRIPTIONS");

            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Column(column =>
                {
                    foreach (var prescription in Ward.Prescriptions)
                    {
                        column.Item()
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten3)
                            .Padding(8)
                            .Column(item =>
                            {
                                item.Item()
                                    .Text(text =>
                                    {
                                        text.Span(
                                            $"Prescription #{prescription.PrescriptionId}: ")
                                            .Bold()
                                            .FontSize(8);

                                        text.Span(
                                            $"{prescription.MedicationName ?? "N/A"} — " +
                                            $"{prescription.PatientName ?? "N/A"} — " +
                                            $"{prescription.Quantity:N0} unit(s)")
                                            .FontSize(8);
                                    });

                                item.Item()
                                    .PaddingTop(3)
                                    .Text(text =>
                                    {
                                        text.Span("Doctor: ")
                                            .Bold()
                                            .FontSize(7);

                                        text.Span(
                                            prescription.DoctorName ?? "N/A")
                                            .FontSize(7);

                                        text.Span("    Issue Date: ")
                                            .Bold()
                                            .FontSize(7);

                                        text.Span(
                                            $"{prescription.IssueDate:dd MMM yyyy} {prescription.IssueTime:HH:mm}")
                                            .FontSize(7);

                                        text.Span("    Status: ")
                                            .Bold()
                                            .FontSize(7);

                                        text.Span(
                                            prescription.Status ?? "N/A")
                                            .FontSize(7);
                                    });

                                if (!string.IsNullOrWhiteSpace(
                                    prescription.DosageInstructions))
                                {
                                    item.Item()
                                        .PaddingTop(3)
                                        .Text(text =>
                                        {
                                            text.Span("Instructions: ")
                                                .Bold()
                                                .FontSize(7);

                                            text.Span(
                                                prescription.DosageInstructions)
                                                .FontSize(7);
                                        });
                                }

                                if (!string.IsNullOrWhiteSpace(
                                    prescription.ScriptManagerName))
                                {
                                    item.Item()
                                        .PaddingTop(2)
                                        .Text(text =>
                                        {
                                            text.Span("Script Manager: ")
                                                .Bold()
                                                .FontSize(7);

                                            text.Span(
                                                prescription.ScriptManagerName)
                                                .FontSize(7);
                                        });
                                }
                            });
                    }
                });
        }

        private void ComposeMedicationAdministrations(IContainer container)
        {
            SectionTitle(container, "MEDICATION ADMINISTRATION RECORD");

            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(65);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn();
                        columns.ConstantColumn(70);
                    });

                    TableHeader(table, "DATE");
                    TableHeader(table, "MEDICATION");
                    TableHeader(table, "PATIENT");
                    TableHeader(table, "NURSE");
                    TableHeader(table, "DOSAGE");
                    TableHeader(table, "PRESCRIPTION");

                    foreach (var administration in Ward.MedicationAdministrations)
                    {
                        BodyCell(
                            table,
                            administration.AdministrationDate
                                .ToString("dd MMM yyyy"));

                        BodyCell(
                            table,
                            administration.MedicationName ?? "N/A");

                        BodyCell(
                            table,
                            administration.PatientName ?? "N/A");

                        BodyCell(
                            table,
                            administration.NurseName ?? "N/A");

                        BodyCell(
                            table,
                            administration.Dosage ?? "N/A");

                        BodyCell(
                            table,
                            administration.PrescriptionId.ToString());
                    }
                });
        }

        private void ComposeStockTakes(IContainer container)
        {
            SectionTitle(container, "STOCK TAKE HISTORY");

            foreach (var stockTake in Ward.StockTakes)
            {
                container
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(10)
                    .Column(column =>
                    {
                        column.Item()
                            .Text(text =>
                            {
                                text.Span(
                                    $"Stock Take #{stockTake.StockTakeId} — ")
                                    .Bold()
                                    .FontSize(9);

                                text.Span(
                                    $"{stockTake.Date:dd MMM yyyy}")
                                    .FontSize(9);
                            });

                        column.Item()
                            .PaddingTop(4)
                            .Text(text =>
                            {
                                text.Span("Stock Manager: ")
                                    .Bold()
                                    .FontSize(7);

                                text.Span(
                                    stockTake.StockManagerName ?? "N/A")
                                    .FontSize(7);

                                text.Span("    Quantity Counted: ")
                                    .Bold()
                                    .FontSize(7);

                                text.Span(
                                    stockTake.QuantityCounted.ToString("N0"))
                                    .FontSize(7);

                                text.Span("    Consumable Types: ")
                                    .Bold()
                                    .FontSize(7);

                                text.Span(
                                    stockTake.TotalConsumablesCounted.ToString("N0"))
                                    .FontSize(7);
                            });

                        column.Item()
                            .PaddingTop(6)
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(65);
                                    columns.ConstantColumn(65);
                                    columns.ConstantColumn(65);
                                    columns.RelativeColumn();
                                });

                                TableHeader(table, "CONSUMABLE");
                                TableHeader(table, "UNIT");
                                TableHeader(table, "SYSTEM");
                                TableHeader(table, "PHYSICAL");
                                TableHeader(table, "DIFFERENCE");
                                TableHeader(table, "STATUS");

                                foreach (var item in stockTake.Consumables)
                                {
                                    BodyCell(
                                        table,
                                        item.ConsumableName ?? "N/A");

                                    BodyCell(
                                        table,
                                        item.Unit ?? "N/A");

                                    BodyCell(
                                        table,
                                        item.SystemQuantity.ToString("N0"));

                                    BodyCell(
                                        table,
                                        item.PhysicalQuantity.ToString("N0"));

                                    BodyCell(
                                        table,
                                        item.Difference.ToString("+0;-0;0"));

                                    StatusCell(
                                        table,
                                        item.Status);
                                }
                            });

                        column.Item()
                            .PaddingTop(6)
                            .Text(text =>
                            {
                                text.Span("Shortages: ")
                                    .Bold()
                                    .FontSize(7);

                                text.Span(
                                    stockTake.TotalShortages.ToString("N0"))
                                    .FontSize(7);

                                text.Span("    Surpluses: ")
                                    .Bold()
                                    .FontSize(7);

                                text.Span(
                                    stockTake.TotalSurpluses.ToString("N0"))
                                    .FontSize(7);

                                text.Span("    Matching: ")
                                    .Bold()
                                    .FontSize(7);

                                text.Span(
                                    stockTake.TotalMatching.ToString("N0"))
                                    .FontSize(7);
                            });
                    });
            }
        }

        private void ComposeReportNotes(IContainer container)
        {
            container
                .BorderTop(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingTop(10)
                .Column(column =>
                {
                    column.Item()
                        .Text("REPORT NOTES")
                        .FontSize(10)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    column.Item()
                        .PaddingTop(5)
                        .Text(
                            "This report provides a consolidated view of medication and consumable " +
                            "inventory associated with the selected ward. Medication quantities reflect " +
                            "the quantities recorded in the ward medication stock records. Consumable " +
                            "physical quantities reflect stock physically counted during stock takes. " +
                            "Differences between system and physical quantities should be reviewed by " +
                            "the responsible stock management personnel.")
                        .FontSize(7)
                        .FontColor(Colors.Grey.Darken1);
                });
        }

        private void ComposeFooter(IContainer container)
        {
            container
                .BorderTop(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingTop(6)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("Imnandi NovaCare Medical Center")
                        .FontSize(7)
                        .FontColor(Colors.Grey.Darken1);

                    row.RelativeItem()
                        .AlignCenter()
                        .Text("Ward Inventory Report")
                        .FontSize(7)
                        .FontColor(Colors.Grey.Darken1);

                    row.RelativeItem()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span("Page ")
                                .FontSize(7);

                            text.CurrentPageNumber()
                                .FontSize(7);

                            text.Span(" of ")
                                .FontSize(7);

                            text.TotalPages()
                                .FontSize(7);
                        });
                });
        }

        private void SectionTitle(IContainer container, string title)
        {
            container
                .Background(Colors.Blue.Darken2)
                .PaddingVertical(7)
                .PaddingHorizontal(10)
                .Text(title)
                .FontSize(11)
                .Bold()
                .FontColor(Colors.White);
        }

        private void InfoCell(
            TableDescriptor table,
            string label,
            string value)
        {
            table.Cell()
                .Padding(5)
                .Column(column =>
                {
                    column.Item()
                        .Text(label.ToUpperInvariant())
                        .FontSize(6)
                        .Bold()
                        .FontColor(Colors.Grey.Darken1);

                    column.Item()
                        .PaddingTop(2)
                        .Text(string.IsNullOrWhiteSpace(value) ? "N/A" : value)
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken3);
                });
        }

        private void SummaryCell(
            TableDescriptor table,
            string label,
            string value)
        {
            table.Cell()
                .Border(1)
                .BorderColor(Colors.Blue.Lighten3)
                .Background(Colors.White)
                .Padding(7)
                .Column(column =>
                {
                    column.Item()
                        .Text(label)
                        .FontSize(6)
                        .Bold()
                        .FontColor(Colors.Grey.Darken1);

                    column.Item()
                        .PaddingTop(3)
                        .Text(value)
                        .FontSize(13)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);
                });
        }

        private void SummaryStatusCell(
            TableDescriptor table,
            string label,
            string value)
        {
            table.Cell()
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.White)
                .Padding(7)
                .Column(column =>
                {
                    column.Item()
                        .Text(label)
                        .FontSize(6)
                        .Bold()
                        .FontColor(Colors.Grey.Darken1);

                    column.Item()
                        .PaddingTop(3)
                        .Text(value)
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);
                });
        }

        private void TableHeader(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .Background(Colors.Grey.Darken3)
                .PaddingVertical(6)
                .PaddingHorizontal(5)
                .Text(text)
                .FontSize(6.5f)
                .Bold()
                .FontColor(Colors.White);
        }

        private void BodyCell(
            TableDescriptor table,
            string text)
        {
            table.Cell()
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .PaddingVertical(5)
                .PaddingHorizontal(5)
                .Text(string.IsNullOrWhiteSpace(text) ? "N/A" : text)
                .FontSize(7);
        }

        private void StatusCell(
            TableDescriptor table,
            string status)
        {
            var displayStatus =
                string.IsNullOrWhiteSpace(status)
                    ? "N/A"
                    : status.Trim();

            table.Cell()
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .PaddingVertical(5)
                .PaddingHorizontal(5)
                .Text(displayStatus)
                .FontSize(7)
                .Bold();
        }

        private string BuildReportingPeriod()
        {
            if (Ward?.FromDate.HasValue == true &&
                Ward?.ToDate.HasValue == true)
            {
                return $"{Ward.FromDate.Value:dd MMM yyyy} - {Ward.ToDate.Value:dd MMM yyyy}";
            }

            if (Ward?.FromDate.HasValue == true)
            {
                return $"From {Ward.FromDate.Value:dd MMM yyyy}";
            }

            if (Ward?.ToDate.HasValue == true)
            {
                return $"Until {Ward.ToDate.Value:dd MMM yyyy}";
            }

            return "All available records";
        }

        private string BuildTransactionDescription(
            WardInventoryMedicationTransactionRecord transaction)
        {
            if (string.IsNullOrWhiteSpace(transaction.TransactionType))
            {
                return "Medication stock transaction";
            }

            var type = transaction.TransactionType.Trim().ToLowerInvariant();

            return type switch
            {
                "received" =>
                    "Medication received into the ward",

                "issued" =>
                    "Medication issued from ward stock",

                "adjusted" =>
                    "Ward medication stock adjustment",

                _ =>
                    $"Medication stock transaction: {transaction.TransactionType}"
            };
        }
    }
}