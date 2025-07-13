using TourPlanner.BLL.DomainModels;
using TourPlanner.BLL.Interfaces;
using TourPlanner.DAL.Entities;

namespace TourPlanner.BLL.Mappers
{
    public class Mapper : IMapper
    {
        public TourDomain ToDomain(TourEntity tourEntity)
        {
            return new TourDomain(
                    tourEntity.Id,
                    tourEntity.Name,
                    tourEntity.Description,
                    tourEntity.From,
                    tourEntity.To,
                    tourEntity.GeoJson,
                    tourEntity.Directions,
                    tourEntity.Distance,
                    tourEntity.EstimatedTime,
                    tourEntity.TransportType,
                    tourEntity.TourLogs.Select(log => new TourLogDomain(
                        log.Id, log.Date, log.Comment, log.TotalDistance,
                        log.TotalTime, log.Difficulty, log.Rating, tourEntity.Id)).ToList()
            );
        }

        public TourEntity ToEntity(TourDomain tourDomain)
        {
            return new TourEntity
            {
                Id = tourDomain.Id,
                Name = tourDomain.Name,
                Description = tourDomain.Description,
                From = tourDomain.From,
                To = tourDomain.To,
                GeoJson = tourDomain.GeoJson,
                Directions = tourDomain.Directions,
                Distance = tourDomain.Distance,
                EstimatedTime = tourDomain.EstimatedTime,
                TransportType = tourDomain.TransportType,
                TourLogs = tourDomain.TourLogs.Select(ToEntity).ToList()
            };
        }

        public TourLogDomain ToDomain(TourLogEntity logEntity)
        {
            return new TourLogDomain(
                logEntity.Id,
                logEntity.Date,
                logEntity.Comment,
                logEntity.TotalDistance,
                logEntity.TotalTime,
                logEntity.Difficulty,
                logEntity.Rating,
                logEntity.TourId
            );
        }

        public TourLogEntity ToEntity(TourLogDomain logDomain)
        {
            return new TourLogEntity
            {
                Id = logDomain.Id,
                Date = logDomain.Date.ToUniversalTime(),
                Comment = logDomain.Comment,
                TotalDistance = logDomain.TotalDistance,
                TotalTime = logDomain.TotalTime,
                Difficulty = logDomain.Difficulty,
                Rating = logDomain.Rating,
                TourId = logDomain.TourId
            };
        }
    }
}
