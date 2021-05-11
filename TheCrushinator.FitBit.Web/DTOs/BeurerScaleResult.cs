using System;

public class BeurerScaleResult
{
    public Scale[] Scale { get; set; }
}

public class Scale
{
    public int ScaleMeasurementID { get; set; }
    public DateTime MeasurementDate { get; set; }
    public DateTime MeasurementTime { get; set; }
    public DateTime MeasurementTimeWithDate { get; set; }
    public float WeightKg { get; set; }
    public float WeightPound { get; set; }
    public float BodyFatPct { get; set; }
    public float WaterPct { get; set; }
    public float MusclePct { get; set; }
    public float BoneMassKg { get; set; }
    public float BoneMassPound { get; set; }
    public float BMI { get; set; }
    public int BMRKCal { get; set; }
    public int AMRKCal { get; set; }
    public int ActivityGrade { get; set; }
    public bool IncludeInGraph { get; set; }
    public bool IsAddedManually { get; set; }
    public bool IsUpdatedManually { get; set; }
    public int DeviceId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string UpdatedDate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime GlobalTime { get; set; }
    public int Impedance { get; set; }
    public int DeviceClientRelationshipId { get; set; }
    public string UserID3 { get; set; }
    public string RecordIdentifier { get; set; }
    public string ChangeType { get; set; }
    public int Counter { get; set; }
    public DateTime MT { get; set; }
    public string Source { get; set; }
    public string KeyIdentifier { get; set; }
    public string CommentWithMedication { get; set; }
}