using TourPlanner.API.DTOs;
using TourPlanner.BLL.DomainModels;

namespace TourPlanner.API.Mapping
{
    /// <summary>
    /// Maps between the API contract (DTOs) and the BLL domain models.
    /// </summary>
    public static class TourDtoMapping
    {
        public static TourDomain ToDomain(this TourRequest request) => new()
        {
            Name = request.Name,
            Description = request.Description,
            From = request.From,
            To = request.To,
            TransportType = request.TransportType
        };

        public static TourDomain ToDomain(this ImportTourRequest request) => new()
        {
            Name = request.Name,
            Description = request.Description,
            From = request.From,
            To = request.To,
            TransportType = request.TransportType,
            GeoJson = request.GeoJson,
            Directions = request.Directions,
            Distance = request.Distance,
            EstimatedTime = request.EstimatedTime,
            TourLogs = request.TourLogs.Select(log => new TourLogDomain
            {
                Date = log.Date,
                Comment = log.Comment,
                TotalDistance = log.TotalDistance,
                TotalTime = log.TotalTime,
                Difficulty = log.Difficulty,
                Rating = log.Rating
            }).ToList()
        };

        public static TourResponse ToResponse(this TourDomain domain) => new()
        {
            Id = domain.Id,
            Name = domain.Name,
            Description = domain.Description,
            From = domain.From,
            To = domain.To,
            TransportType = domain.TransportType,
            Distance = domain.Distance,
            EstimatedTime = domain.EstimatedTime,
            Directions = domain.Directions,
            GeoJson = domain.GeoJson,
            TourLogs = domain.TourLogs.Select(log => new TourLogResponse
            {
                Id = log.Id,
                Date = log.Date,
                Comment = log.Comment,
                TotalDistance = log.TotalDistance,
                TotalTime = log.TotalTime,
                Difficulty = log.Difficulty,
                Rating = log.Rating,
                TourId = log.TourId
            }).ToList()
        };
    }
}
