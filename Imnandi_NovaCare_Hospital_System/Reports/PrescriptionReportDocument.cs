using Imnandi_NovaCare_Hospital_System.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Imnandi_NovaCare_Hospital_System.Reports
{
    public class PrescriptionReportDocument : IDocument
    {
        private readonly Doctor _doctor;
        private readonly List<Prescription> _prescriptions;

        public PrescriptionReportDocument(
            Doctor doctor,
            List<Prescription> prescriptions)
        {
            _doctor = doctor;
            _prescriptions = prescriptions;
        }

        public DocumentMetadata GetMetadata()
        {
            return new DocumentMetadata
            {
                Title = "Doctor Prescription Report",
                Author = "Hospital Management System",
                Subject = "Prescription Monitoring Report",
                Keywords = "Doctor, Prescription, Medication, Patient, Hospital"
            };
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);

                page.DefaultTextStyle(x =>
                    x.FontFamily("Arial")
                     .FontSize(10));

                page.Header()
                    .Element(ComposeHeader);

                page.Content()
                    .Element(ComposeContent);

                page.Footer()
                    .Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .PaddingBottom(10)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Column(left =>
                            {
                                left.Item()
                                    .Text("HOSPITAL MANAGEMENT SYSTEM")
                                    .Bold()
                                    .FontSize(18)
                                    .FontColor(Colors.Blue.Darken2);

                                left.Item()
                                    .PaddingTop(3)
                                    .Text("Doctor Prescription Report")
                                    .Bold()
                                    .FontSize(14);
                            });

                        row.ConstantItem(150)
                            .AlignRight()
                            .Column(right =>
                            {
                                right.Item()
                                    .Text("PRESCRIPTION REPORT")
                                    .Bold()
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken2);

                                right.Item()
                                    .PaddingTop(4)
                                    .Text(DateTime.Now.ToString("dd MMMM yyyy"))
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                            });
                    });

                column.Item()
                    .LineHorizontal(1)
                    .LineColor(Colors.Grey.Lighten2);
            });
        }


        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(15);


                column.Item()
                    .Element(ComposeDoctorInformation);


                column.Item()
                    .Element(ComposeSummary);


                column.Item()
                    .Element(ComposePrescriptionTable);


                column.Item()
                    .Element(ComposeManagementSummary);
            });
        }

        private void ComposeDoctorInformation(IContainer container)
        {
            var doctorName =
                $"{_doctor.FirstName} {_doctor.LastName}";

            var employeeName = _doctor.Employee != null
                ? $"{_doctor.Employee.FirstName} {_doctor.Employee.LastName}"
                : doctorName;

            container
                .Background(Colors.Grey.Lighten4)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(15)
                .Column(column =>
                {
                    column.Item()
                        .Text("DOCTOR INFORMATION")
                        .Bold()
                        .FontSize(11)
                        .FontColor(Colors.Blue.Darken2);

                    column.Item()
                        .PaddingTop(8)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(left =>
                                {
                                    left.Item()
                                        .Text(text =>
                                        {
                                            text.Span("Doctor: ")
                                                .Bold();

                                            text.Span(doctorName);
                                        });

                                    left.Item()
                                        .PaddingTop(5)
                                        .Text(text =>
                                        {
                                            text.Span("Department: ")
                                                .Bold();

                                            text.Span(
                                                string.IsNullOrWhiteSpace(
                                                    _doctor.Department)
                                                    ? "Not specified"
                                                    : _doctor.Department);
                                        });
                                });

                            row.RelativeItem()
                                .Column(right =>
                                {
                                    right.Item()
                                        .Text(text =>
                                        {
                                            text.Span("Employee: ")
                                                .Bold();

                                            text.Span(employeeName);
                                        });

                                    right.Item()
                                        .PaddingTop(5)
                                        .Text(text =>
                                        {
                                            text.Span("Doctor ID: ")
                                                .Bold();

                                            text.Span(
                                                _doctor.DoctorId.ToString());
                                        });
                                });
                        });
                });
        }


        private void ComposeSummary(IContainer container)
        {
            var totalPrescriptions = _prescriptions.Count;

            var totalQuantity = _prescriptions
                .Sum(p => p.Quantity);

            var pending = _prescriptions.Count(p =>
                string.Equals(
                    p.Status,
                    "Pending",
                    StringComparison.OrdinalIgnoreCase));

            var processed = _prescriptions.Count(p =>
                string.Equals(
                    p.Status,
                    "Processed",
                    StringComparison.OrdinalIgnoreCase));

            container.Row(row =>
            {
                row.Spacing(10);

                row.RelativeItem()
                    .Element(c =>
                        ComposeSummaryCard(
                            c,
                            "TOTAL PRESCRIPTIONS",
                            totalPrescriptions.ToString(),
                            Colors.Blue.Darken2));

                row.RelativeItem()
                    .Element(c =>
                        ComposeSummaryCard(
                            c,
                            "TOTAL UNITS PRESCRIBED",
                            totalQuantity.ToString("N0"),
                            Colors.Green.Darken2));

                row.RelativeItem()
                    .Element(c =>
                        ComposeSummaryCard(
                            c,
                            "PENDING",
                            pending.ToString(),
                            Colors.Orange.Darken2));

                row.RelativeItem()
                    .Element(c =>
                        ComposeSummaryCard(
                            c,
                            "PROCESSED",
                            processed.ToString(),
                            Colors.Purple.Darken2));
            });
        }

        private void ComposeSummaryCard(
            IContainer container,
            string title,
            string value,
            string color)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.White)
                .Padding(10)
                .Column(column =>
                {
                    column.Item()
                        .Text(title)
                        .Bold()
                        .FontSize(7)
                        .FontColor(Colors.Grey.Darken1);

                    column.Item()
                        .PaddingTop(4)
                        .Text(value)
                        .Bold()
                        .FontSize(16)
                        .FontColor(color);
                });
        }


        private void ComposePrescriptionTable(IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .Text("RECENT PRESCRIPTION ACTIVITY")
                    .Bold()
                    .FontSize(12)
                    .FontColor(Colors.Blue.Darken2);

                column.Item()
                    .PaddingTop(8)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(25);
                            columns.RelativeColumn(1.4f);
                            columns.RelativeColumn(1.3f);
                            columns.ConstantColumn(45);
                            columns.RelativeColumn(1.5f);
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(65);
                        });

                        table.Header(header =>
                        {
                            header.Cell()
                                .Element(HeaderCell)
                                .Text("#");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("PATIENT");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("MEDICATION");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("QTY");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("DOSAGE / INSTRUCTIONS");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("DATE");

                            header.Cell()
                                .Element(HeaderCell)
                                .Text("STATUS");
                        });

                        var counter = 1;

                        foreach (var prescription in _prescriptions)
                        {
                            var patientName =
                                prescription.Patient != null
                                    ? $"{prescription.Patient.FirstName} {prescription.Patient.LastName}"
                                    : "Unknown Patient";

                            var medicationName =
                                prescription.Medication != null
                                    ? prescription.Medication.MedicationName
                                    : "Unknown Medication";

                            var dosage =
                                string.IsNullOrWhiteSpace(
                                    prescription.DosageInstructions)
                                    ? "Not specified"
                                    : prescription.DosageInstructions;

                            var status =
                                string.IsNullOrWhiteSpace(
                                    prescription.Status)
                                    ? "Unknown"
                                    : prescription.Status;

                            table.Cell()
                                .Element(BodyCell)
                                .Text(counter.ToString());

                            table.Cell()
                                .Element(BodyCell)
                                .Text(patientName);

                            table.Cell()
                                .Element(BodyCell)
                                .Text(medicationName);

                            table.Cell()
                                .Element(BodyCell)
                                .AlignCenter()
                                .Text(prescription.Quantity.ToString());

                            table.Cell()
                                .Element(BodyCell)
                                .Text(dosage);

                            table.Cell()
                                .Element(BodyCell)
                                .Text(
                                    prescription.IssueDate
                                        .ToString("dd/MM/yyyy"));

                            table.Cell()
                                .Element(BodyCell)
                                .AlignCenter()
                                .Element(statusContainer =>
                                {
                                    statusContainer
                                        .Background(
                                            GetStatusBackground(status))
                                        .PaddingHorizontal(5)
                                        .PaddingVertical(3)
                                        .AlignCenter()
                                        .Text(status)
                                        .Bold()
                                        .FontSize(7)
                                        .FontColor(
                                            GetStatusColor(status));
                                });

                            counter++;
                        }
                    });
            });
        }


        private IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(Colors.Blue.Darken2)
                .Padding(6)
                .AlignMiddle();
        }

        private IContainer BodyCell(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(6)
                .AlignMiddle();
        }


        private string GetStatusBackground(string status)
        {
            if (string.Equals(
                status,
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                return Colors.Orange.Lighten4;
            }

            if (string.Equals(
                status,
                "Processed",
                StringComparison.OrdinalIgnoreCase))
            {
                return Colors.Green.Lighten4;
            }

            if (string.Equals(
                status,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                return Colors.Green.Lighten4;
            }

            if (string.Equals(
                status,
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
            {
                return Colors.Red.Lighten4;
            }

            return Colors.Grey.Lighten3;
        }

        private string GetStatusColor(string status)
        {
            if (string.Equals(
                status,
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                return Colors.Orange.Darken3;
            }

            if (string.Equals(
                status,
                "Processed",
                StringComparison.OrdinalIgnoreCase))
            {
                return Colors.Green.Darken3;
            }

            if (string.Equals(
                status,
                "Completed",
                StringComparison.OrdinalIgnoreCase))
            {
                return Colors.Green.Darken3;
            }

            if (string.Equals(
                status,
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
            {
                return Colors.Red.Darken3;
            }

            return Colors.Grey.Darken2;
        }


        private void ComposeManagementSummary(IContainer container)
        {
            var medicationGroups = _prescriptions
                .Where(p => p.Medication != null)
                .GroupBy(p => p.Medication.MedicationName)
                .OrderByDescending(g => g.Sum(p => p.Quantity))
                .Take(5)
                .ToList();

            container
                .Background(Colors.Grey.Lighten4)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(14)
                .Column(column =>
                {
                    column.Item()
                        .Text("MANAGEMENT SUMMARY")
                        .Bold()
                        .FontSize(11)
                        .FontColor(Colors.Blue.Darken2);

                    column.Item()
                        .PaddingTop(7)
                        .Text(
                            "This report provides an overview of recent " +
                            "medication prescriptions issued by the doctor. " +
                            "It is intended to assist senior management with " +
                            "prescription monitoring, medication usage review " +
                            "and clinical oversight.")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Darken2);

                    if (medicationGroups.Any())
                    {
                        column.Item()
                            .PaddingTop(10)
                            .Text("Most Frequently Prescribed Medications")
                            .Bold()
                            .FontSize(9);

                        foreach (var group in medicationGroups)
                        {
                            column.Item()
                                .PaddingTop(4)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Text(group.Key)
                                        .FontSize(8);

                                    row.ConstantItem(100)
                                        .AlignRight()
                                        .Text(
                                            $"{group.Sum(p => p.Quantity):N0} unit(s)")
                                        .Bold()
                                        .FontSize(8);
                                });
                        }
                    }
                });
        }


        private void ComposeFooter(IContainer container)
        {
            container
                .PaddingTop(10)
                .BorderTop(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text(
                            "Hospital Management System - Confidential")
                        .FontSize(8)
                        .FontColor(Colors.Grey.Darken1);

                    row.ConstantItem(100)
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span("Page ")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);

                            text.CurrentPageNumber()
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);

                            text.Span(" of ")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);

                            text.TotalPages()
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1);
                        });
                });
        }
    }
}