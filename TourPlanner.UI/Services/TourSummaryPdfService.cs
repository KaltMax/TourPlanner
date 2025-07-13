using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.Services
{
    public class TourSummaryPdfService : ITourSummaryPdfService
    {
        private readonly IFileDialogService _fileDialogService;

        public TourSummaryPdfService(IFileDialogService fileDialogService)
        {
            _fileDialogService = fileDialogService;
        }

        public async Task<bool> GenerateTourSummaryPdfAsync(IEnumerable<Tour> tours)
        {
            if (tours == null)
            {
                throw new ArgumentNullException(nameof(tours), "Tours collection cannot be null.");
            }

            var toursList = tours.ToList();

            if (!toursList.Any())
            {
                throw new ArgumentException("No tours provided.", nameof(tours));
            }

            string filePath = await GetSaveFilePath();
            if (string.IsNullOrEmpty(filePath))
            {
                return false; // User canceled the save dialog
            }

            using (var pdfWriter = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(pdfWriter))
            using (var document = new Document(pdf))
            {
                AddDocumentTitle(document, toursList.Count);
                AddTourSummaryTables(document, toursList);
                AddReportFooter(document);
            }

            return true;
        }

        private async Task<string> GetSaveFilePath()
        {
            string defaultFileName = $"All_Tours_Summary_{DateTime.Now:yyyyMMdd}.pdf";
            const string filter = "PDF files (*.pdf)|*.pdf";
            return await _fileDialogService.SaveFileDialogAsync(defaultFileName, filter, "pdf") ?? string.Empty;
        }

        private void AddDocumentTitle(Document document, int tourCount)
        {
            var title = new Paragraph("All Tours Summary")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER);

            document.Add(title);
            document.Add(new Paragraph("\n"));
        }

        private void AddTourSummaryTables(Document document, IEnumerable<Tour> tours)
        {
            foreach (var tour in tours)
            {
                // Add tour title
                document.Add(new Paragraph($"Tour: {tour.Name}")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.LEFT));

                if (tour.TourLogs.Count > 0)
                {
                    // Calculate statistics
                    double avgTime = tour.TourLogs.Average(l => l.TotalTime);
                    double avgDistance = tour.TourLogs.Average(l => l.TotalDistance);
                    double avgRating = tour.TourLogs.Average(l => l.Rating);
                    double avgDifficulty = tour.TourLogs.Average(l => l.Difficulty);
                    int logCount = tour.TourLogs.Count;

                    // Create stats table
                    var statsTable = new Table(2).UseAllAvailableWidth();
                    statsTable.AddCell("Log Entries:").AddCell(logCount.ToString());
                    statsTable.AddCell("Average Time:").AddCell(FormatTimeToHoursAndMinutes(avgTime));
                    statsTable.AddCell("Average Distance:").AddCell($"{avgDistance:F2} km");
                    statsTable.AddCell("Average Rating:").AddCell($"{avgRating:F1} / 5,0");
                    statsTable.AddCell("Average Difficulty:").AddCell($"{avgDifficulty:F1} / 5,0");
                    statsTable.AddCell("Child Friendly:").AddCell(tour.IsChildFriendly ? "Yes" : "No");
                    statsTable.AddCell("Popularity:").AddCell(tour.Popularity);

                    document.Add(statsTable);
                }
                else
                {
                    document.Add(new Paragraph("No logs available for this tour.")
                        .SetFontSize(12));
                }

                document.Add(new Paragraph("\n"));

                // Add a separator line between tours
                var separatorLine = new LineSeparator(new SolidLine(0.5f))
                    .SetMarginTop(5)
                    .SetMarginBottom(10);
                document.Add(separatorLine);
            }
        }

        private void AddReportFooter(Document document)
        {
            document.Add(new Paragraph($"\nReport generated on: {DateTime.Now}")
                .SetFontSize(10));
        }

        private static string FormatTimeToHoursAndMinutes(double timeInMinutes)
        {
            int hours = (int)(timeInMinutes / 60);
            int minutes = (int)(timeInMinutes % 60);
            return $"{hours}h {minutes:D2}min";
        }
    }
}
