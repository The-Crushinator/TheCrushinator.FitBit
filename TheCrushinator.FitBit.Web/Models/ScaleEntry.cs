using System;
using System.ComponentModel.DataAnnotations;

namespace TheCrushinator.FitBit.Web.Models
{
    public class ScaleEntry: IEquatable<ScaleEntry>
    {
        [Key]
        public string EntryId { get; set; }

        public Guid UserId { get; set; }

        public DateTime ImportDateTimeUtc { get; set; }

        public DateTime RecordDateTimeUtc { get; set; }
        
        public float WeightKg { get; set; }
        
        public float BodyFatPct { get; set; }

        public float WaterPct { get; set; }

        public float MusclePct { get; set; }

        public float BoneMassKg { get; set; }

        public float BMI { get; set; }

        public int BMRKCal { get; set; }

        public int AMRKCal { get; set; }

        public DateTime FitBitUploadDateTimeUtc { get; set; }

        public bool Equals(ScaleEntry other)
        {
            return EntryId.Equals(other);
        }
    }
}
