using System.Globalization;
using System.IO;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using TourPlanner.UI.Models;
using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.Services
{
    public class TourReportPdfService : ITourReportPdfService
    {
        private readonly IMapService _mapService;
        private readonly IFileDialogService _fileDialogService;

        public TourReportPdfService(IMapService MapService, IFileDialogService fileDialogService)
        {
            _mapService = MapService;
            _fileDialogService = fileDialogService;
        }

        public async Task<bool> GenerateTourReportPdfAsync(Tour tour)
        {
            if (tour == null)
            {
                throw new ArgumentNullException(nameof(tour), "Tour cannot be null");
            }

            string filePath = await GetSaveFilePath(tour);
            if (string.IsNullOrEmpty(filePath))
            {
                return false; // User canceled the save dialog
            }

            if (!_mapService.IsInitialized)
            {
                throw new InvalidOperationException("Map service is not initialized");
            }

            var mapImageStream = await _mapService.CaptureMapScreenshotAsync();

            using (var pdfWriter = new PdfWriter(filePath))
            using (var pdf = new PdfDocument(pdfWriter))
            using (var document = new Document(pdf))
            {
                AddDocumentTitle(document, tour);
                AddMapImage(document, mapImageStream);
                AddTourDetails(document, tour);
                AddTourLogs(document, tour);
                AddReportFooter(document);
            }

            return true;
        }

        private async Task<string> GetSaveFilePath(Tour tour)
        {
            string defaultFileName = $"{tour.Name}_Summary_{DateTime.Now:yyyyMMdd}.pdf";
            const string filter = "PDF files (*.pdf)|*.pdf";

            return await _fileDialogService.SaveFileDialogAsync(defaultFileName, filter, "pdf") ?? string.Empty;
        }

        private void AddDocumentTitle(Document document, Tour tour)
        {
            var title = new Paragraph($"Tour Report: {tour.Name}")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER);

            document.Add(title);
            document.Add(new Paragraph("\n"));
        }

        private void AddTourDetails(Document document, Tour tour)
        {
            document.Add(new Paragraph("Tour Details:")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(14));

            var infoTable = new Table(2).UseAllAvailableWidth();
            infoTable.AddCell("Name:").AddCell(tour.Name);
            infoTable.AddCell("Description:").AddCell(tour.Description);
            infoTable.AddCell("From:").AddCell(tour.From);
            infoTable.AddCell("To:").AddCell(tour.To);
            infoTable.AddCell("Transport Type:").AddCell(tour.TransportType);
            infoTable.AddCell("Distance:").AddCell($"{tour.Distance} km");
            infoTable.AddCell("Estimated Time:").AddCell(FormatTimeToHoursAndMinutes(tour.EstimatedTime));
            infoTable.AddCell("Popularity:").AddCell(tour.Popularity);
            infoTable.AddCell("Is Child Friendly:").AddCell(tour.IsChildFriendly ? "Yes" : "No");
            document.Add(infoTable);

            document.Add(new Paragraph("\n"));
        }

        private void AddMapImage(Document document, MemoryStream? mapImageStream)
        {
            document.Add(new Paragraph("Tour Map:")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(14));

            if (mapImageStream != null && mapImageStream.Length > 0)
            {
                var mapImage = ImageDataFactory.Create(mapImageStream.ToArray());
                var img = new Image(mapImage);
                img.SetWidth(450); // TODO: Adjust Width accordingly
                img.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                document.Add(img);
            }

            document.Add(new Paragraph("\n"));
        }

        private void AddTourLogs(Document document, Tour tour)
        {
            document.Add(new Paragraph("Tour Logs:")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(14));

            if (tour.TourLogs.Count > 0)
            {
                AddLogsTable(document, tour.TourLogs);
                AddStatistics(document, tour.TourLogs);
            }
            else
            {
                document.Add(new Paragraph("No logs available for this tour."));
            }
        }

        private void AddLogsTable(Document document, ICollection<TourLog> logs)
        {
            var logsTable = new Table(6).UseAllAvailableWidth();
            logsTable.AddHeaderCell("Date");
            logsTable.AddHeaderCell("Distance");
            logsTable.AddHeaderCell("Time");
            logsTable.AddHeaderCell("Difficulty");
            logsTable.AddHeaderCell("Rating");
            logsTable.AddHeaderCell("Comment");

            foreach (var log in logs)
            {
                logsTable.AddCell(log.Date.ToString("yyyy-MM-dd"));
                logsTable.AddCell($"{log.TotalDistance} km");
                logsTable.AddCell(FormatTimeToHoursAndMinutes(log.TotalTime));
                logsTable.AddCell(log.Difficulty.ToString(CultureInfo.InvariantCulture));
                logsTable.AddCell(log.Rating.ToString(CultureInfo.InvariantCulture));
                logsTable.AddCell(log.Comment ?? "");
            }

            document.Add(logsTable);
        }

        private void AddStatistics(Document document, ICollection<TourLog> logs)
        {
            document.Add(new Paragraph("\nStatistics:")
                .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
                .SetFontSize(14));

            double avgTime = logs.Average(l => l.TotalTime);
            double avgDistance = logs.Average(l => l.TotalDistance);
            double avgRating = logs.Average(l => l.Rating);

            var statsTable = new Table(2).UseAllAvailableWidth();
            statsTable.AddCell("Average Time:").AddCell(FormatTimeToHoursAndMinutes(avgTime));
            statsTable.AddCell("Average Distance:").AddCell($"{avgDistance:F2} km");
            statsTable.AddCell("Average Rating:").AddCell($"{avgRating:F2}");
            document.Add(statsTable);
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
