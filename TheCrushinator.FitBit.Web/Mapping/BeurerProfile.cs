using AutoMapper;
using System;
using TheCrushinator.Beurer.DTOs;
using TheCrushinator.FitBit.Web.Models;

namespace TheCrushinator.FitBit.Web.Mapping
{
    public class BeurerProfile : Profile
    {
        public BeurerProfile()
        {
            CreateMap<BeurerScaleMeasurementResponse, ScaleEntry>()
                .ForMember(d => d.EntryId, o => o.MapFrom(s => s.RecordIdentifier))
                .ForMember(d => d.UserId, o => o.MapFrom(s => s.UserID3))
                .ForMember(d => d.ImportDateTimeUtc, o => o.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.RecordDateTimeUtc, o => o.MapFrom(s => FindMeasurementTimeUtc(s)))
                .ForMember(d => d.WeightKg, o => o.MapFrom(s => s.WeightKg))
                .ForMember(d => d.BodyFatPct, o => o.MapFrom(s => s.BodyFatPct))
                .ForMember(d => d.WaterPct, o => o.MapFrom(s => s.WaterPct))
                .ForMember(d => d.MusclePct, o => o.MapFrom(s => s.MusclePct))
                .ForMember(d => d.BoneMassKg, o => o.MapFrom(s => s.BoneMassKg))
                .ForMember(d => d.BMI, o => o.MapFrom(s => s.BMI))
                .ForMember(d => d.BMRKCal, o => o.MapFrom(s => s.BMRKCal))
                .ForMember(d => d.AMRKCal, o => o.MapFrom(s => s.AMRKCal))
                .ForAllOtherMembers(o => o.Ignore());
        }

        private DateTime FindMeasurementTimeUtc(BeurerScaleMeasurementResponse s)
        {
            //var difference = s.CreatedDate.Subtract(s.GlobalTime);
            //var measurementDateTime = s.MeasurementTimeWithDate != DateTime.MinValue ? s.MeasurementTimeWithDate : s.MeasurementTime;
            //var shiftedDateTime = measurementDateTime.Subtract(difference);

            // Use MeasurementDateTime as many records seem to not align
            // - Eg. "MeasurementTime": "2016-09-17T12:26:24", "GlobalTime": "2017-02-28T01:35:30+00:00"
            // TODO: Allow for specification of TimeZone instead of relying on system time being AEST.
            var measurementDateTime = DateTime.SpecifyKind(s.MeasurementTime, DateTimeKind.Local);
            return measurementDateTime.ToUniversalTime();
        }
    }
}
