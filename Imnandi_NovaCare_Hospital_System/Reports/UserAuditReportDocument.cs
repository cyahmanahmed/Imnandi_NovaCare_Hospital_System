using global::Imnandi_NovaCare_Hospital_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Imnandi_NovaCare_Hospital_System.Reports
{
    public class UserAuditReportDocument : IDocument
    {
        public User User { get; set; }
        public List<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public DateTime Month { get; set; } 

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = "User Audit Report",
            Author = "Imnandi NovaCare Medical Center"
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));


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

                        col.Item().Text("User Audit Report")
                            .SemiBold()
                            .FontSize(16)
                            .FontColor(Colors.Blue.Darken1)
                            .AlignCenter();

                        col.Item().Text($"Month: {Month:MMMM yyyy}")
                            .FontSize(11)
                            .FontColor(Colors.Grey.Darken2)
                            .AlignCenter();
                    });
                });


                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Text("User Information").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);

                    col.Item().Row(row =>
                    {
                        row.ConstantColumn(80).Text("Name:").Bold();
                        row.RelativeColumn().Text($"{User.FirstName} {User.LastName}");
                    });

                    col.Item().Row(row =>
                    {
                        row.ConstantColumn(80).Text("Email:").Bold();
                        row.RelativeColumn().Text(User.Email ?? "-");
                    });

                    col.Item().Row(row =>
                    {
                        row.ConstantColumn(80).Text("Role:").Bold();
                        row.RelativeColumn().Text(User.Role ?? "-");
                    });

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);


                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); 
                            columns.RelativeColumn(3); 
                            columns.RelativeColumn(3); 
                            columns.RelativeColumn(4); 
                        });


                        table.Header(header =>
                        {
                            string[] headers = { "Date/Time", "Session ID", "IP Address", "Login / Logout (Date & Time)" };
                            foreach (var h in headers)
                            {
                                header.Cell()
                                    .Background(Colors.Blue.Lighten4)
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Blue.Darken1)
                                    .Padding(6)
                                    .AlignCenter()
                                    .Text(h)
                                    .FontSize(10)
                                    .SemiBold();
                            }
                        });


                        bool alternate = false;
                        foreach (var log in AuditLogs)
                        {
                            var bgColor = alternate ? Colors.Grey.Lighten5 : Colors.White;
                            alternate = !alternate;

                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                .Text(log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                .Text(log.SessionId ?? "-");

                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                .Text(log.IpAddress ?? "-");

                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6)
                                .Text($"Login: {log.LoginDateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-"} / Logout: {log.LogoutDateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-"}");
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