using Imnandi_NovaCare_Hospital_System.Models.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Imnandi_NovaCare_Hospital_System.Reports
{
    public class SupplierReportDocument : IDocument
    {
        private readonly SupplierReportViewModel Report;

        public SupplierReportDocument(SupplierReportViewModel report)
        {
            Report = report;
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = "Supplier Performance and Procurement Report",
            Author = "Imnandi NovaCare Medical Center",
            Subject = "Supplier Performance, Procurement, Delivery and Order History",
            Keywords = "Supplier, Procurement, Orders, Deliveries, Medicines, Consumables, Stock Management"
        };

        public void Compose(IDocumentContainer container)
        {
            var completedOrders = Report.Orders
                .Where(o => o.IsReceived)
                .ToList();

            var pendingOrders = Report.Orders
                .Where(o => !o.IsReceived)
                .ToList();

            var consumableItems = Report.ConsumableItems
                .OrderByDescending(i => i.OrderDate)
                .ToList();

            var medicineItems = Report.MedicineItems
                .OrderByDescending(i => i.OrderDate)
                .ToList();

            var totalConsumableRequested = consumableItems
                .Sum(i => i.QuantityRequested);

            var totalConsumableReceived = consumableItems
                .Sum(i => i.QuantityReceived);

            var totalMedicineRequested = medicineItems
                .Sum(i => i.QuantityRequested);

            var totalMedicineReceived = medicineItems
                .Sum(i => i.QuantityReceived);

            var consumableDeliveryRate = totalConsumableRequested > 0
                ? Math.Round(
                    (double)totalConsumableReceived /
                    totalConsumableRequested * 100,
                    2)
                : 0;

            var medicineDeliveryRate = totalMedicineRequested > 0
                ? Math.Round(
                    (double)totalMedicineReceived /
                    totalMedicineRequested * 100,
                    2)
                : 0;

            var ordersWithOutstandingQuantities = Report.Orders
                .Where(o => o.OutstandingQuantity > 0)
                .OrderByDescending(o => o.OutstandingQuantity)
                .ToList();

            var mostOrderedConsumables = consumableItems
                .GroupBy(i => i.ConsumableName)
                .Select(g => new
                {
                    Name = g.Key,
                    Orders = g.Select(x => x.OrderId)
                        .Distinct()
                        .Count(),
                    Requested = g.Sum(x => x.QuantityRequested),
                    Received = g.Sum(x => x.QuantityReceived),
                    Outstanding = g.Sum(x => x.QuantityOutstanding)
                })
                .OrderByDescending(x => x.Requested)
                .ThenBy(x => x.Name)
                .ToList();

            var mostOrderedMedicines = medicineItems
                .GroupBy(i => i.MedicationName)
                .Select(g => new
                {
                    Name = g.Key,
                    Orders = g.Select(x => x.MedicineOrderId)
                        .Distinct()
                        .Count(),
                    Requested = g.Sum(x => x.QuantityRequested),
                    Received = g.Sum(x => x.QuantityReceived),
                    Outstanding = g.Sum(x => x.QuantityOutstanding)
                })
                .OrderByDescending(x => x.Requested)
                .ThenBy(x => x.Name)
                .ToList();

            var stockManagers = Report.Orders
                .GroupBy(o => o.StockManagerName)
                .Select(g => new
                {
                    Name = g.Key,
                    Orders = g.Count(),
                    QuantityRequested = g.Sum(x => x.TotalQuantityRequested),
                    QuantityReceived = g.Sum(x => x.TotalQuantityReceived)
                })
                .OrderByDescending(x => x.Orders)
                .ToList();

            var hospitalStores = Report.Orders
                .GroupBy(o => o.HospitalStoreName)
                .Select(g => new
                {
                    Name = g.Key,
                    Location = g.First().HospitalStoreLocation,
                    Orders = g.Count(),
                    QuantityRequested = g.Sum(x => x.TotalQuantityRequested),
                    QuantityReceived = g.Sum(x => x.TotalQuantityReceived)
                })
                .OrderByDescending(x => x.Orders)
                .ToList();

            string supplierStatus;

            if (Report.TotalOrders == 0)
            {
                supplierStatus = "NO ORDER HISTORY";
            }
            else if (Report.OverallDeliveryRate >= 95)
            {
                supplierStatus = "STRONG DELIVERY PERFORMANCE";
            }
            else if (Report.OverallDeliveryRate >= 80)
            {
                supplierStatus = "SATISFACTORY DELIVERY PERFORMANCE";
            }
            else
            {
                supplierStatus = "REQUIRES MANAGEMENT REVIEW";
            }

            string deliveryAssessment;

            if (Report.TotalOrders == 0)
            {
                deliveryAssessment =
                    "No historical orders have been recorded for this supplier. " +
                    "Management should review supplier engagement before making procurement decisions.";
            }
            else if (Report.OverallDeliveryRate >= 95)
            {
                deliveryAssessment =
                    "The supplier demonstrates a strong historical delivery performance. " +
                    "Recorded quantities received are closely aligned with quantities requested. " +
                    "The supplier may be considered reliable based on the available procurement history.";
            }
            else if (Report.OverallDeliveryRate >= 80)
            {
                deliveryAssessment =
                    "The supplier demonstrates generally satisfactory delivery performance. " +
                    "However, outstanding quantities should continue to be monitored to ensure " +
                    "that procurement requirements are fulfilled within expected timeframes.";
            }
            else
            {
                deliveryAssessment =
                    "The supplier has a material difference between quantities requested and quantities received. " +
                    "Management should review outstanding deliveries, supplier reliability and procurement risks " +
                    "before increasing purchasing commitments.";
            }

            container.Page(page =>
            {
                page.Margin(35);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);

                page.DefaultTextStyle(x =>
                    x.FontFamily("Arial"));

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
                                            .Text("SUPPLIER PERFORMANCE & PROCUREMENT REPORT")
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
                        column.Spacing(13);

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
                                            .Element(c =>
                                                SummaryCard(
                                                    c,
                                                    "Total Orders",
                                                    Report.TotalOrders.ToString("N0")));

                                        row.RelativeColumn()
                                            .Element(c =>
                                                SummaryCard(
                                                    c,
                                                    "Items Ordered",
                                                    Report.TotalItemsOrdered.ToString("N0")));

                                        row.RelativeColumn()
                                            .Element(c =>
                                                SummaryCard(
                                                    c,
                                                    "Items Received",
                                                    Report.TotalItemsReceived.ToString("N0")));

                                        row.RelativeColumn()
                                            .Element(c =>
                                                SummaryCard(
                                                    c,
                                                    "Delivery Rate",
                                                    $"{Report.OverallDeliveryRate:N2}%"));
                                    });

                                summary.Item()
                                    .PaddingTop(8)
                                    .Row(row =>
                                    {
                                        row.RelativeColumn()
                                            .Element(c =>
                                                SummaryCard(
                                                    c,
                                                    "Completed Orders",
                                                    Report.CompletedOrders.ToString("N0")));

                                        row.RelativeColumn()
                                            .Element(c =>
                                                SummaryCard(
                                                    c,
                                                    "Pending Orders",
                                                    Report.PendingOrders.ToString("N0")));

                                        row.RelativeColumn()
                                            .Element(c =>
                                                SummaryCard(
                                                    c,
                                                    "Outstanding",
                                                    Report.TotalItemsOutstanding.ToString("N0")));

                                        row.RelativeColumn()
                                            .Element(c =>
                                                SummaryCard(
                                                    c,
                                                    "Supplier Status",
                                                    supplierStatus));
                                    });
                            });

                        column.Item()
                            .Background(Colors.Grey.Lighten5)
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(12)
                            .Column(profile =>
                            {
                                profile.Item()
                                    .Text("SUPPLIER PROFILE")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(Colors.Green.Darken2);

                                profile.Item()
                                    .PaddingTop(8)
                                    .Row(row =>
                                    {
                                        row.RelativeColumn()
                                            .Column(left =>
                                            {
                                                DetailLine(
                                                    left,
                                                    "Supplier",
                                                    Report.SupplierName);

                                                DetailLine(
                                                    left,
                                                    "Contact Person",
                                                    Report.ContactPerson);

                                                DetailLine(
                                                    left,
                                                    "Telephone",
                                                    Report.PhoneNumber);

                                                DetailLine(
                                                    left,
                                                    "Email",
                                                    Report.Email);
                                            });

                                        row.RelativeColumn()
                                            .Column(right =>
                                            {
                                                DetailLine(
                                                    right,
                                                    "Address",
                                                    Report.Address);

                                                DetailLine(
                                                    right,
                                                    "Supplier Status",
                                                    Report.IsActive
                                                        ? "Active"
                                                        : "Inactive");

                                                DetailLine(
                                                    right,
                                                    "Consumable Orders",
                                                    Report.TotalConsumableOrders.ToString("N0"));

                                                DetailLine(
                                                    right,
                                                    "Medicine Orders",
                                                    Report.TotalMedicineOrders.ToString("N0"));
                                            });
                                    });
                            });

                        column.Item()
                            .Column(section =>
                            {
                                section.Item()
                                    .Text("PROCUREMENT PERFORMANCE")
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
                                            HeaderCell(header.Cell(), "Supply Category");
                                            HeaderCell(header.Cell(), "Orders");
                                            HeaderCell(header.Cell(), "Requested");
                                            HeaderCell(header.Cell(), "Received");
                                            HeaderCell(header.Cell(), "Delivery Rate");
                                        });

                                        BodyCell(
                                            table.Cell(),
                                            "Consumables");

                                        BodyCell(
                                            table.Cell(),
                                            Report.TotalConsumableOrders.ToString("N0"));

                                        BodyCell(
                                            table.Cell(),
                                            totalConsumableRequested.ToString("N0"));

                                        BodyCell(
                                            table.Cell(),
                                            totalConsumableReceived.ToString("N0"));

                                        BodyCell(
                                            table.Cell(),
                                            $"{consumableDeliveryRate:N2}%");

                                        BodyCell(
                                            table.Cell(),
                                            "Medicines");

                                        BodyCell(
                                            table.Cell(),
                                            Report.TotalMedicineOrders.ToString("N0"));

                                        BodyCell(
                                            table.Cell(),
                                            totalMedicineRequested.ToString("N0"));

                                        BodyCell(
                                            table.Cell(),
                                            totalMedicineReceived.ToString("N0"));

                                        BodyCell(
                                            table.Cell(),
                                            $"{medicineDeliveryRate:N2}%");

                                        BodyCell(
                                            table.Cell(),
                                            "Overall");

                                        BodyCell(
                                            table.Cell(),
                                            Report.TotalOrders.ToString("N0"));

                                        BodyCell(
                                            table.Cell(),
                                            Report.TotalItemsOrdered.ToString("N0"));

                                        BodyCell(
                                            table.Cell(),
                                            Report.TotalItemsReceived.ToString("N0"));

                                        BodyCell(
                                            table.Cell(),
                                            $"{Report.OverallDeliveryRate:N2}%");
                                    });
                            });

                        column.Item()
                            .Background(Colors.Grey.Lighten5)
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(10)
                            .Column(assessment =>
                            {
                                assessment.Item()
                                    .Text("SUPPLIER PERFORMANCE ASSESSMENT")
                                    .Bold()
                                    .FontSize(11)
                                    .FontColor(Colors.Green.Darken2);

                                assessment.Item()
                                    .PaddingTop(5)
                                    .Text(deliveryAssessment);

                                assessment.Item()
                                    .PaddingTop(6)
                                    .Text(
                                        $"The supplier currently has {Report.TotalItemsOutstanding:N0} " +
                                        $"unit(s) outstanding across {ordersWithOutstandingQuantities.Count:N0} order(s).");
                            });

                        if (ordersWithOutstandingQuantities.Any())
                        {
                            column.Item()
                                .Column(section =>
                                {
                                    section.Item()
                                        .Text("OUTSTANDING PROCUREMENT")
                                        .Bold()
                                        .FontSize(12)
                                        .FontColor(Colors.Orange.Darken2);

                                    section.Item()
                                        .PaddingTop(6)
                                        .Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.RelativeColumn(1.8f);
                                                columns.RelativeColumn(1.8f);
                                                columns.RelativeColumn(1.2f);
                                                columns.RelativeColumn(1.2f);
                                                columns.RelativeColumn(1.2f);
                                                columns.RelativeColumn(1.5f);
                                            });

                                            table.Header(header =>
                                            {
                                                HeaderCell(header.Cell(), "Order Number");
                                                HeaderCell(header.Cell(), "Order Name");
                                                HeaderCell(header.Cell(), "Requested");
                                                HeaderCell(header.Cell(), "Received");
                                                HeaderCell(header.Cell(), "Outstanding");
                                                HeaderCell(header.Cell(), "Status");
                                            });

                                            foreach (var order in ordersWithOutstandingQuantities)
                                            {
                                                BodyCell(
                                                    table.Cell(),
                                                    order.OrderNumber);

                                                BodyCell(
                                                    table.Cell(),
                                                    order.OrderName);

                                                BodyCell(
                                                    table.Cell(),
                                                    order.TotalQuantityRequested.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    order.TotalQuantityReceived.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    order.OutstandingQuantity.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    order.Status);
                                            }
                                        });
                                });
                        }

                        if (mostOrderedConsumables.Any())
                        {
                            column.Item()
                                .Column(section =>
                                {
                                    section.Item()
                                        .Text("CONSUMABLE SUPPLY ANALYSIS")
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
                                                columns.RelativeColumn(1);
                                                columns.RelativeColumn(1.3f);
                                                columns.RelativeColumn(1.3f);
                                                columns.RelativeColumn(1.3f);
                                            });

                                            table.Header(header =>
                                            {
                                                HeaderCell(header.Cell(), "Consumable");
                                                HeaderCell(header.Cell(), "Orders");
                                                HeaderCell(header.Cell(), "Requested");
                                                HeaderCell(header.Cell(), "Received");
                                                HeaderCell(header.Cell(), "Outstanding");
                                            });

                                            foreach (var item in mostOrderedConsumables)
                                            {
                                                BodyCell(
                                                    table.Cell(),
                                                    item.Name);

                                                BodyCell(
                                                    table.Cell(),
                                                    item.Orders.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    item.Requested.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    item.Received.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    item.Outstanding.ToString("N0"));
                                            }
                                        });
                                });
                        }

                        if (mostOrderedMedicines.Any())
                        {
                            column.Item()
                                .Column(section =>
                                {
                                    section.Item()
                                        .Text("MEDICINE SUPPLY ANALYSIS")
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
                                                columns.RelativeColumn(1);
                                                columns.RelativeColumn(1.3f);
                                                columns.RelativeColumn(1.3f);
                                                columns.RelativeColumn(1.3f);
                                            });

                                            table.Header(header =>
                                            {
                                                HeaderCell(header.Cell(), "Medication");
                                                HeaderCell(header.Cell(), "Orders");
                                                HeaderCell(header.Cell(), "Requested");
                                                HeaderCell(header.Cell(), "Received");
                                                HeaderCell(header.Cell(), "Outstanding");
                                            });

                                            foreach (var item in mostOrderedMedicines)
                                            {
                                                BodyCell(
                                                    table.Cell(),
                                                    item.Name);

                                                BodyCell(
                                                    table.Cell(),
                                                    item.Orders.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    item.Requested.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    item.Received.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    item.Outstanding.ToString("N0"));
                                            }
                                        });
                                });
                        }

                        if (stockManagers.Any())
                        {
                            column.Item()
                                .Column(section =>
                                {
                                    section.Item()
                                        .Text("PROCUREMENT ACTIVITY BY STOCK MANAGER")
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
                                                columns.RelativeColumn(1);
                                                columns.RelativeColumn(1.5f);
                                                columns.RelativeColumn(1.5f);
                                            });

                                            table.Header(header =>
                                            {
                                                HeaderCell(header.Cell(), "Stock Manager");
                                                HeaderCell(header.Cell(), "Orders");
                                                HeaderCell(header.Cell(), "Requested");
                                                HeaderCell(header.Cell(), "Received");
                                            });

                                            foreach (var manager in stockManagers)
                                            {
                                                BodyCell(
                                                    table.Cell(),
                                                    manager.Name);

                                                BodyCell(
                                                    table.Cell(),
                                                    manager.Orders.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    manager.QuantityRequested.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    manager.QuantityReceived.ToString("N0"));
                                            }
                                        });
                                });
                        }

                        if (hospitalStores.Any())
                        {
                            column.Item()
                                .Column(section =>
                                {
                                    section.Item()
                                        .Text("HOSPITAL STORE PROCUREMENT ACTIVITY")
                                        .Bold()
                                        .FontSize(12)
                                        .FontColor(Colors.Green.Darken2);

                                    section.Item()
                                        .PaddingTop(6)
                                        .Table(table =>
                                        {
                                            table.ColumnsDefinition(columns =>
                                            {
                                                columns.RelativeColumn(2.3f);
                                                columns.RelativeColumn(2);
                                                columns.RelativeColumn(1);
                                                columns.RelativeColumn(1.4f);
                                                columns.RelativeColumn(1.4f);
                                            });

                                            table.Header(header =>
                                            {
                                                HeaderCell(header.Cell(), "Hospital Store");
                                                HeaderCell(header.Cell(), "Location");
                                                HeaderCell(header.Cell(), "Orders");
                                                HeaderCell(header.Cell(), "Requested");
                                                HeaderCell(header.Cell(), "Received");
                                            });

                                            foreach (var store in hospitalStores)
                                            {
                                                BodyCell(
                                                    table.Cell(),
                                                    store.Name);

                                                BodyCell(
                                                    table.Cell(),
                                                    store.Location);

                                                BodyCell(
                                                    table.Cell(),
                                                    store.Orders.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    store.QuantityRequested.ToString("N0"));

                                                BodyCell(
                                                    table.Cell(),
                                                    store.QuantityReceived.ToString("N0"));
                                            }
                                        });
                                });
                        }

                        column.Item()
                            .Column(section =>
                            {
                                section.Item()
                                    .Text("COMPLETE ORDER HISTORY")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(Colors.Green.Darken2);

                                section.Item()
                                    .PaddingTop(6)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1.6f);
                                            columns.RelativeColumn(1.1f);
                                            columns.RelativeColumn(1.8f);
                                            columns.RelativeColumn(1.1f);
                                            columns.RelativeColumn(1.1f);
                                            columns.RelativeColumn(1.1f);
                                            columns.RelativeColumn(1.1f);
                                        });

                                        table.Header(header =>
                                        {
                                            HeaderCell(header.Cell(), "Order");
                                            HeaderCell(header.Cell(), "Type");
                                            HeaderCell(header.Cell(), "Date");
                                            HeaderCell(header.Cell(), "Requested");
                                            HeaderCell(header.Cell(), "Received");
                                            HeaderCell(header.Cell(), "Outstanding");
                                            HeaderCell(header.Cell(), "Status");
                                        });

                                        foreach (var order in Report.Orders)
                                        {
                                            BodyCell(
                                                table.Cell(),
                                                order.OrderNumber);

                                            BodyCell(
                                                table.Cell(),
                                                order.IsMedicineOrder
                                                    ? "Medicine"
                                                    : "Consumable");

                                            BodyCell(
                                                table.Cell(),
                                                order.OrderDate.ToString("dd MMM yyyy"));

                                            BodyCell(
                                                table.Cell(),
                                                order.TotalQuantityRequested.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                order.TotalQuantityReceived.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                order.OutstandingQuantity.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                order.Status);
                                        }
                                    });
                            });

                        column.Item()
                            .Column(section =>
                            {
                                section.Item()
                                    .Text("DETAILED CONSUMABLE ORDER HISTORY")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(Colors.Green.Darken2);

                                section.Item()
                                    .PaddingTop(6)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.5f);
                                        });

                                        table.Header(header =>
                                        {
                                            HeaderCell(header.Cell(), "Order");
                                            HeaderCell(header.Cell(), "Consumable");
                                            HeaderCell(header.Cell(), "Date");
                                            HeaderCell(header.Cell(), "Requested");
                                            HeaderCell(header.Cell(), "Received");
                                            HeaderCell(header.Cell(), "Stock Manager");
                                            HeaderCell(header.Cell(), "Store");
                                        });

                                        foreach (var item in consumableItems)
                                        {
                                            BodyCell(
                                                table.Cell(),
                                                item.OrderNumber);

                                            BodyCell(
                                                table.Cell(),
                                                item.ConsumableName);

                                            BodyCell(
                                                table.Cell(),
                                                item.OrderDate.ToString("dd/MM/yyyy"));

                                            BodyCell(
                                                table.Cell(),
                                                item.QuantityRequested.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                item.QuantityReceived.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                item.StockManagerName);

                                            BodyCell(
                                                table.Cell(),
                                                item.HospitalStoreName);
                                        }
                                    });
                            });

                        column.Item()
                            .Column(section =>
                            {
                                section.Item()
                                    .Text("DETAILED MEDICINE ORDER HISTORY")
                                    .Bold()
                                    .FontSize(12)
                                    .FontColor(Colors.Green.Darken2);

                                section.Item()
                                    .PaddingTop(6)
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(2);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1);
                                            columns.RelativeColumn(1.5f);
                                            columns.RelativeColumn(1.5f);
                                        });

                                        table.Header(header =>
                                        {
                                            HeaderCell(header.Cell(), "Order");
                                            HeaderCell(header.Cell(), "Medication");
                                            HeaderCell(header.Cell(), "Date");
                                            HeaderCell(header.Cell(), "Requested");
                                            HeaderCell(header.Cell(), "Received");
                                            HeaderCell(header.Cell(), "Stock Manager");
                                            HeaderCell(header.Cell(), "Store");
                                        });

                                        foreach (var item in medicineItems)
                                        {
                                            BodyCell(
                                                table.Cell(),
                                                item.OrderNumber);

                                            BodyCell(
                                                table.Cell(),
                                                item.MedicationName);

                                            BodyCell(
                                                table.Cell(),
                                                item.OrderDate.ToString("dd/MM/yyyy"));

                                            BodyCell(
                                                table.Cell(),
                                                item.QuantityRequested.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                item.QuantityReceived.ToString("N0"));

                                            BodyCell(
                                                table.Cell(),
                                                item.StockManagerName);

                                            BodyCell(
                                                table.Cell(),
                                                item.HospitalStoreName);
                                        }
                                    });
                            });

                        column.Item()
                            .Background(Colors.Green.Lighten5)
                            .Border(1)
                            .BorderColor(Colors.Green.Lighten2)
                            .Padding(10)
                            .Column(conclusion =>
                            {
                                conclusion.Item()
                                    .Text("MANAGEMENT & INVESTMENT REVIEW")
                                    .Bold()
                                    .FontSize(11)
                                    .FontColor(Colors.Green.Darken2);

                                conclusion.Item()
                                    .PaddingTop(5)
                                    .Text(
                                        $"Supplier: {Report.SupplierName}");

                                conclusion.Item()
                                    .PaddingTop(4)
                                    .Text(
                                        $"Historical orders recorded: {Report.TotalOrders:N0}. " +
                                        $"Total requested quantity: {Report.TotalItemsOrdered:N0}. " +
                                        $"Total received quantity: {Report.TotalItemsReceived:N0}. " +
                                        $"Outstanding quantity: {Report.TotalItemsOutstanding:N0}.");

                                conclusion.Item()
                                    .PaddingTop(4)
                                    .Text(
                                        $"Overall recorded delivery performance: " +
                                        $"{Report.OverallDeliveryRate:N2}%.");

                                conclusion.Item()
                                    .PaddingTop(5)
                                    .Text(deliveryAssessment);

                                conclusion.Item()
                                    .PaddingTop(5)
                                    .Text(
                                        "Management should consider the supplier's historical delivery " +
                                        "performance, outstanding quantities, procurement frequency and " +
                                        "the operational importance of supplied medicines and consumables " +
                                        "when evaluating future procurement commitments.");

                                conclusion.Item()
                                    .PaddingTop(5)
                                    .Text(
                                        "This report is based on procurement and stock records currently " +
                                        "captured within the hospital management system and should be " +
                                        "considered alongside contractual, financial and supplier evaluation records.");
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
                                    "Imnandi NovaCare Medical Center | Supplier Performance Report");
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
                .Padding(4)
                .Background(Colors.White)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(7)
                .Column(column =>
                {
                    column.Item()
                        .AlignCenter()
                        .Text(value)
                        .Bold()
                        .FontSize(13)
                        .FontColor(Colors.Green.Darken2);

                    column.Item()
                        .PaddingTop(3)
                        .AlignCenter()
                        .Text(title)
                        .FontColor(Colors.Grey.Darken2);
                });
        }

        private static void DetailLine(
            ColumnDescriptor column,
            string label,
            string value)
        {
            column.Item()
                .PaddingBottom(5)
                .Row(row =>
                {
                    row.ConstantColumn(95)
                        .Text(label)
                        .Bold()
                        .FontColor(Colors.Grey.Darken2);

                    row.RelativeColumn()
                        .Text(string.IsNullOrWhiteSpace(value)
                            ? "N/A"
                            : value);
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
                .Padding(5)
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
                .Padding(5)
                .Text(string.IsNullOrWhiteSpace(text)
                    ? "N/A"
                    : text);
        }
    }
}