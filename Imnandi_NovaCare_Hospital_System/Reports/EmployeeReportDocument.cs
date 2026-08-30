using Imnandi_NovaCare_Hospital_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace Imnandi_NovaCare_Hospital_System.Reports
{
    public class EmployeeReportDocument : IDocument
    {
        public List<Employee> Employees { get; set; } = new List<Employee>();

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = "Employee Report",
            Author = "Imnandi NovaCare Medical Center"
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(9));


                page.Header().Padding(20).Background(Colors.Blue.Lighten5).Row(row =>
                {
                    row.ConstantColumn(80)
                       .Height(60)
                       .Image("wwwroot/images/hospital_logo.jpg", ImageScaling.FitHeight);

                    row.RelativeColumn().Column(col =>
                    {
                        col.Item().Text("Imnandi NovaCare Medical Center")
                            .Bold()
                            .FontSize(20)
                            .FontColor(Colors.Blue.Darken2)
                            .AlignCenter();

                        col.Item().Text("Employee Report")
                            .SemiBold()
                            .FontSize(16)
                            .FontColor(Colors.Blue.Darken1)
                            .AlignCenter();

                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken3)
                            .AlignCenter();
                    });
                });


                page.Content().PaddingVertical(20).Column(column =>
                {

                    column.Item()
                        .LineHorizontal(1)
                        .LineColor(Colors.Grey.Lighten2);


                    column.Item().Table(table =>
                    {

                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(3); 
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2); 
                            columns.RelativeColumn(2); 
                        });


                        table.Header(header =>
                        {
                            header.Cell()
                                .Background(Colors.Blue.Lighten4)
                                .BorderBottom(1)
                                .BorderColor(Colors.Blue.Darken1)
                                .Padding(8)
                                .AlignCenter()
                                .Text("Name");

                            header.Cell()
                                .Background(Colors.Blue.Lighten4)
                                .BorderBottom(1)
                                .BorderColor(Colors.Blue.Darken1)
                                .Padding(8)
                                .AlignCenter()
                                .Text("Job Title");

                            header.Cell()
                                .Background(Colors.Blue.Lighten4)
                                .BorderBottom(1)
                                .BorderColor(Colors.Blue.Darken1)
                                .Padding(8)
                                .AlignCenter()
                                .Text("Department");

                            header.Cell()
                                .Background(Colors.Blue.Lighten4)
                                .BorderBottom(1)
                                .BorderColor(Colors.Blue.Darken1)
                                .Padding(8)
                                .AlignCenter()
                                .Text("Email");

                            header.Cell()
                                .Background(Colors.Blue.Lighten4)
                                .BorderBottom(1)
                                .BorderColor(Colors.Blue.Darken1)
                                .Padding(8)
                                .AlignCenter()
                                .Text("Phone");

                            header.Cell()
                                .Background(Colors.Blue.Lighten4)
                                .BorderBottom(1)
                                .BorderColor(Colors.Blue.Darken1)
                                .Padding(8)
                                .AlignCenter()
                                .Text("Tax Number");

                            header.Cell()
                                .Background(Colors.Blue.Lighten4)
                                .BorderBottom(1)
                                .BorderColor(Colors.Blue.Darken1)
                                .Padding(8)
                                .AlignCenter()
                                .Text("Bank");
                        });


                        bool alternate = false;
                        foreach (var emp in Employees.Where(e => !e.IsDeleted && e.User != null && e.User.IsActive))
                        {
                            var bgColor = alternate ? Colors.Grey.Lighten5 : Colors.White;
                            alternate = !alternate;


                            table.Cell()
                                .Background(bgColor)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .PaddingVertical(8)
                                .PaddingHorizontal(6)
                                .Text($"{emp.FirstName} {emp.LastName}");


                            table.Cell()
                                .Background(bgColor)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .PaddingVertical(8)
                                .PaddingHorizontal(6)
                                .Text(emp.JobTitle ?? "-");


                            table.Cell()
                                .Background(bgColor)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .PaddingVertical(8)
                                .PaddingHorizontal(6)
                                .Text(emp.Department ?? "-");


                            table.Cell()
                                .Background(bgColor)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .PaddingVertical(8)
                                .PaddingHorizontal(6)
                                .Text(emp.Email ?? "-");


                            table.Cell()
                                .Background(bgColor)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .PaddingVertical(8)
                                .PaddingHorizontal(6)
                                .Text(emp.PhoneNumber ?? "-");


                            table.Cell()
                                .Background(bgColor)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .PaddingVertical(8)
                                .PaddingHorizontal(6)
                                .Text(emp.TaxNumber ?? "-");


                            table.Cell()
                                .Background(bgColor)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten3)
                                .PaddingVertical(8)
                                .PaddingHorizontal(6)
                                .Text($"{emp.BankName ?? "-"} / {emp.BankAccountNumber ?? "-"}");
                        }
                    });
                });


                page.Footer().AlignCenter().Padding(10).Row(row =>
                {
                    row.RelativeColumn().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken2));
                        text.Span("© Imnandi NovaCare Medical Center | Generated on ");
                        text.Span($"{DateTime.Now:yyyy-MM-dd HH:mm}");
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
