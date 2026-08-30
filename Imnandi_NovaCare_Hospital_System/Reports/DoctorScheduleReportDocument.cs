using Imnandi_NovaCare_Hospital_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Imnandi_NovaCare_Hospital_System.Reports
{
    public class DoctorScheduleReportDocument : IDocument
    {
        public Doctor Doctor { get; set; } = new Doctor();
        public List<Schedule> Schedules { get; set; } = new List<Schedule>();
        public DateTime Month { get; set; }

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = $"Schedule Report - {Month:MMMM yyyy}",
            Author = "Imnandi NovaCare Medical Center"
        };

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));
                page.PageColor(Colors.White);


                page.Header().Row(row =>
                {
                    row.RelativeColumn().Column(col =>
                    {
                        col.Item().Text("Imnandi NovaCare Medical Center")
                            .Bold().FontSize(18).FontColor(Colors.Blue.Darken1);

                        col.Item().Text("Doctor Schedule Report")
                            .SemiBold().FontSize(14);

                        var doctorName = Doctor?.Employee != null
                            ? $"{Doctor.Employee.FirstName} {Doctor.Employee.LastName}"
                            : "Unknown Doctor";

                        col.Item().Text($"Doctor: {doctorName}")
                            .FontSize(11);

                        col.Item().Text($"Month: {Month:MMMM yyyy}")
                            .FontSize(11).FontColor(Colors.Grey.Darken2);

                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                    });
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    if (!Schedules.Any())
                    {
                        column.Item().Text("No scheduled appointments found for this month.")
                            .FontSize(12)
                            .FontColor(Colors.Red.Darken1)
                            .AlignCenter();
                    }
                    else
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); 
                                columns.RelativeColumn(2); 
                                columns.RelativeColumn(2); 
                                columns.RelativeColumn(2); 
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Padding(6).BorderBottom(1).Background(Colors.Blue.Lighten4).Text("Date & Time").SemiBold().AlignCenter();
                                header.Cell().Padding(6).BorderBottom(1).Background(Colors.Blue.Lighten4).Text("Patient").SemiBold().AlignCenter();
                                header.Cell().Padding(6).BorderBottom(1).Background(Colors.Blue.Lighten4).Text("Location").SemiBold().AlignCenter();
                                header.Cell().Padding(6).BorderBottom(1).Background(Colors.Blue.Lighten4).Text("Visit Type").SemiBold().AlignCenter();
                                header.Cell().Padding(6).BorderBottom(1).Background(Colors.Blue.Lighten4).Text("Completed").SemiBold().AlignCenter();
                            });

                            bool alternate = false;
                            foreach (var s in Schedules.OrderBy(x => x.ScheduledDate))
                            {
                                var bgColor = alternate ? Colors.Grey.Lighten4 : Colors.White;
                                alternate = !alternate;

                                table.Cell().Background(bgColor).Padding(6)
                                    .Text(s.ScheduledDate.ToString("yyyy-MM-dd HH:mm"));

                                var patientName = s.Patient != null
                                    ? $"{s.Patient.FirstName} {s.Patient.LastName}"
                                    : "Unknown Patient";
                                table.Cell().Background(bgColor).Padding(6).Text(patientName);

                                table.Cell().Background(bgColor).Padding(6)
                                    .Text(s.Location ?? "-");

                                table.Cell().Background(bgColor).Padding(6)
                                    .Text(s.VisitType ?? "-");

                                table.Cell().Background(bgColor).Padding(6).AlignCenter()
                                    .Text(s.IsCompleted ? "Yes" : "No");
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text("© Imnandi NovaCare Medical Center | Confidential")
                    .FontSize(10).FontColor(Colors.Grey.Darken2);
            });
        }
    }
}
