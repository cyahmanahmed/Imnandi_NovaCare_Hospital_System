using Imnandi_NovaCare_Hospital_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Imnandi_NovaCare_Hospital_System.Reports
{
    public class StockTakeReportDocument : IDocument
    {
        public List<StockTake> StockTakes { get; set; } = new List<StockTake>();
        public DateTime Month { get; set; }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Stock Take Monthly Report - {Month:MMMM yyyy}",
            Author = "Imnandi NovaCare Medical Center"
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));


                page.Header().Padding(20).Background(Colors.Blue.Lighten5).Row(row =>
                {
                    row.ConstantColumn(100)
                        .Height(70)
                        .Image("wwwroot/images/hospital_logo.jpg", ImageScaling.FitHeight);

                    row.RelativeColumn().Column(col =>
                    {
                        col.Item().Text("Imnandi NovaCare Medical Center")
                            .Bold()
                            .FontSize(20)
                            .FontColor(Colors.Blue.Darken2)
                            .AlignCenter();

                        col.Item().Text($"Stock Take Monthly Report - {Month:MMMM yyyy}")
                            .SemiBold()
                            .FontSize(16)
                            .FontColor(Colors.Blue.Darken1)
                            .AlignCenter();

                        col.Item().Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2)
                            .AlignCenter();
                    });
                });


                page.Content().PaddingVertical(10).Column(col =>
                {
                    var totalStockTakes = StockTakes.Count;
                    var totalItemsCountered = StockTakes.Sum(st => st.QuantityCountered);
                    var distinctWards = StockTakes.Where(st => st.Ward != null).Select(st => st.Ward.WardName).Distinct().ToList();
                    var distinctStockManagers = StockTakes.Select(st => st.StockManager.FirstName).Distinct().ToList();

                    col.Item().Text("Executive Summary").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingBottom(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Row(row =>
                    {
                        row.RelativeColumn().Padding(8).Background(Colors.Grey.Lighten4).Column(summaryCol =>
                        {
                            summaryCol.Item().Text($"Total Stock Takes: {totalStockTakes}").Bold().FontSize(11);
                            summaryCol.Item().Text($"Total Items Countered: {totalItemsCountered:N0}").Bold().FontSize(11);
                            summaryCol.Item().Text($"Wards Covered: {distinctWards.Count}").Bold().FontSize(11);
                            summaryCol.Item().Text($"Active Stock Managers: {distinctStockManagers.Count}").Bold().FontSize(11);
                        });
                    });

                    col.Item().PaddingTop(15).PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Text("Stock Take Records").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1.5f); 
                            columns.RelativeColumn(2.5f); 
                            columns.RelativeColumn(2f); 
                            columns.RelativeColumn(1.5f); 
                            columns.RelativeColumn(2.5f); 
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Date").SemiBold().AlignCenter();
                            header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Stock Manager").SemiBold().AlignCenter();
                            header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Ward").SemiBold().AlignCenter();
                            header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Quantity Countered").SemiBold().AlignCenter();
                            header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Consumables").SemiBold().AlignCenter();
                        });

                        foreach (var stockTake in StockTakes.OrderBy(st => st.Date))
                        {
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(stockTake.Date.ToString("yyyy-MM-dd")).AlignCenter();
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(stockTake.StockManager.FirstName).AlignLeft();
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(stockTake.Ward?.WardName ?? "N/A").AlignLeft();
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(stockTake.QuantityCountered.ToString("N0")).AlignRight();
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(stockTake.Consumables.Sum(c => c.QuantityOnHand).ToString("N0")).AlignRight();
                        }
                    });


                    if (distinctWards.Any())
                    {
                        col.Item().PaddingTop(20).Text("Ward-wise Stock Take Analysis").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                        col.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3f); // Ward
                                columns.RelativeColumn(2f); // Total Stock Takes
                                columns.RelativeColumn(2f); // Total Items Countered
                                columns.RelativeColumn(2f); // Average Items per Take
                            });


                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Ward").SemiBold().AlignCenter();
                                header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Total Stock Takes").SemiBold().AlignCenter();
                                header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Total Items Countered").SemiBold().AlignCenter();
                                header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Avg Items per Take").SemiBold().AlignCenter();
                            });


                            foreach (var wardName in distinctWards.OrderBy(w => w))
                            {
                                var wardStockTakes = StockTakes.Where(st => st.Ward != null && st.Ward.WardName == wardName).ToList();
                                var totalWardTakes = wardStockTakes.Count;
                                var totalWardItems = wardStockTakes.Sum(st => st.QuantityCountered);
                                var avgItemsPerTake = totalWardTakes > 0 ? (double)totalWardItems / totalWardTakes : 0;

                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(wardName).AlignLeft();
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(totalWardTakes.ToString()).AlignRight();
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(totalWardItems.ToString("N0")).AlignRight();
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(avgItemsPerTake.ToString("N1")).AlignRight();
                            }


                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Background(Colors.Grey.Lighten3).Text("TOTAL").AlignLeft();
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Background(Colors.Grey.Lighten3).Text(totalStockTakes.ToString()).AlignRight();
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Background(Colors.Grey.Lighten3).Text(totalItemsCountered.ToString("N0")).AlignRight();
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Background(Colors.Grey.Lighten3).Text((totalItemsCountered / (double)totalStockTakes).ToString("N1")).AlignRight();
                        });
                    }


                    col.Item().PaddingTop(20).Text("Stock Manager Performance").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3f); 
                            columns.RelativeColumn(2f); 
                            columns.RelativeColumn(2f); 
                            columns.RelativeColumn(2f); 
                        });


                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Stock Manager").SemiBold().AlignCenter();
                            header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Total Stock Takes").SemiBold().AlignCenter();
                            header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Total Items Countered").SemiBold().AlignCenter();
                            header.Cell().Background(Colors.Blue.Lighten2).Border(1).BorderColor(Colors.Blue.Darken1).Padding(8).Text("Avg Items per Take").SemiBold().AlignCenter();
                        });


                        foreach (var managerName in distinctStockManagers.OrderBy(m => m))
                        {
                            var managerStockTakes = StockTakes.Where(st => st.StockManager.FirstName == managerName).ToList();
                            var totalManagerTakes = managerStockTakes.Count;
                            var totalManagerItems = managerStockTakes.Sum(st => st.QuantityCountered);
                            var avgItemsPerTake = totalManagerTakes > 0 ? (double)totalManagerItems / totalManagerTakes : 0;

                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(managerName).AlignLeft();
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(totalManagerTakes.ToString()).AlignRight();
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(totalManagerItems.ToString("N0")).AlignRight();
                            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(avgItemsPerTake.ToString("N1")).AlignRight();
                        }
                    });
                });


                page.Footer().AlignCenter().Padding(10).Row(row =>
                {
                    row.RelativeColumn().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken2));
                        text.Span("© Imnandi NovaCare Medical Center | Stock Management Department");
                    });

                    row.ConstantColumn(60).Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken2));
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
                });
            });
        }
    }
}