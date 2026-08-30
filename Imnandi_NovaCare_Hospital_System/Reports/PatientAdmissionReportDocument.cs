using Imnandi_NovaCare_Hospital_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Imnandi_NovaCare_Hospital_System.Reports
{
    public class PatientAdmissionReportDocument : IDocument
    {
        public Patient Patient { get; set; }
        public List<AdmissionFolder> Admissions { get; set; } = new List<AdmissionFolder>();

        public DocumentMetadata GetMetadata() => new DocumentMetadata
        {
            Title = "Patient Admission History Report",
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
                            .FontSize(22)
                            .FontColor(Colors.Blue.Darken2)
                            .AlignCenter();

                        col.Item().Text("Patient Admission History Report")
                            .SemiBold()
                            .FontSize(16)
                            .FontColor(Colors.Blue.Darken1)
                            .AlignCenter();

                        col.Item().Text($"Patient: {Patient?.FirstName} {Patient?.LastName ?? "-"}")
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken2)
                            .AlignCenter();
                    });
                });


                page.Content().PaddingVertical(10).Column(col =>
                {

                    col.Item().Text("Patient Information").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingBottom(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().Row(row =>
                    {
                        row.ConstantColumn(120).Text("Full Name:").Bold().FontSize(11);
                        row.RelativeColumn().Text($"{Patient?.FirstName} {Patient?.LastName ?? "-"}").FontSize(11);
                    });

                    col.Item().Row(row =>
                    {
                        row.ConstantColumn(120).Text("Patient ID:").Bold().FontSize(11);
                        row.RelativeColumn().Text(Patient?.IdNumber ?? "-").FontSize(11);
                    });

                    col.Item().Row(row =>
                    {
                        row.ConstantColumn(120).Text("Date of Birth:").Bold().FontSize(11);
                        row.RelativeColumn().Text(Patient?.DateOfBirth.ToString("yyyy-MM-dd") ?? "-").FontSize(11);
                    });

                    col.Item().Row(row =>
                    {
                        row.ConstantColumn(120).Text("Gender:").Bold().FontSize(11);
                        row.RelativeColumn().Text(Patient?.Gender ?? "-").FontSize(11);
                    });

                    col.Item().Row(row =>
                    {
                        row.ConstantColumn(120).Text("Contact Number:").Bold().FontSize(11);
                        row.RelativeColumn().Text(Patient?.PhoneNumber ?? "-").FontSize(11);
                    });

                    col.Item().PaddingTop(15).PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);


                    col.Item().Text("Admission History").Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    if (Admissions.Any())
                    {

                        col.Item().Element(AddPadding).Grid(grid =>
                        {
                            grid.Columns(2);

                            grid.Item().Row(statRow =>
                            {
                                statRow.ConstantColumn(100).Text("Total Admissions:").Bold();
                                statRow.RelativeColumn().Text(Admissions.Count.ToString());
                            });

                            grid.Item().Row(statRow =>
                            {
                                statRow.ConstantColumn(100).Text("Active Admissions:").Bold();
                                statRow.RelativeColumn().Text(Admissions.Count(a => a.IsActive).ToString());
                            });
                        });


                        col.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.5f); 
                                columns.RelativeColumn(2.5f); 
                                columns.RelativeColumn(2f); 
                                columns.RelativeColumn(3f); 
                                columns.RelativeColumn(3.5f); 
                            });


                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Admission Date");
                                header.Cell().Element(CellStyle).Text("Discharge Date");
                                header.Cell().Element(CellStyle).Text("Room");
                                header.Cell().Element(CellStyle).Text("Ward Admin");
                                header.Cell().Element(CellStyle).Text("Reason for Admission");

                                static IContainer CellStyle(IContainer container) => container
                                    .Background(Colors.Blue.Lighten2)
                                    .Border(1)
                                    .BorderColor(Colors.Blue.Darken1)
                                    .Padding(8)
                                    .AlignCenter()
                                    .DefaultTextStyle(TextStyle.Default.SemiBold().FontSize(10));
                            });


                            foreach (var admission in Admissions.OrderByDescending(a => a.DateCreated))
                            {
                                string wardAdminName = "-";
                                if (admission.WardAdmin != null && admission.WardAdmin.Employee != null)
                                {
                                    wardAdminName = $"{admission.WardAdmin.Employee.FirstName} {admission.WardAdmin.Employee.LastName}";
                                }

                                string dischargeDate = admission.DateClosed.HasValue
                                    ? admission.DateClosed.Value.ToString("yyyy-MM-dd HH:mm")
                                    : "Not discharged yet";

                                table.Cell().Element(CellStyle).Text(admission.DateCreated.ToString("yyyy-MM-dd HH:mm"));
                                table.Cell().Element(CellStyle).Text(dischargeDate);
                                table.Cell().Element(CellStyle).Text(admission.Bed?.Room?.RoomNumber ?? "-");
                                table.Cell().Element(CellStyle).Text(wardAdminName);
                                table.Cell().Element(CellStyle).Text(admission.ReasonForAdmission ?? "-");

                                static IContainer CellStyle(IContainer container) => container
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten3)
                                    .Padding(8)
                                    .AlignLeft()
                                    .DefaultTextStyle(TextStyle.Default.FontSize(10));
                            }
                        });
                    }
                    else
                    {
                        col.Item().Padding(20).AlignCenter().Text("No admission history found for this patient.")
                            .FontSize(11)
                            .FontColor(Colors.Grey.Darken2);
                    }
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

        static IContainer AddPadding(IContainer container) => container.PaddingBottom(15);
    }
}