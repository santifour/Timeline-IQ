using System;

namespace ProjectTimeEstimator.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double EstimatedHours { get; set; }
    public double ActualHours { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Planned"; // Planned, Active, Completed

    // Computed Properties
    // Computed Properties
    public double DeviationHours => ActualHours - EstimatedHours;

    public double PercentageDeviation
    {
        get
        {
            if (EstimatedHours <= 0) return 0;
            return (DeviationHours / EstimatedHours) * 100;
        }
    }

    public double AccuracyScore
    {
        get
        {
            if (EstimatedHours <= 0) return 0;
            var dev = Math.Abs(PercentageDeviation);
            return Math.Max(0, 100 - dev);
        }
    }

    public bool IsOverestimated => ActualHours < EstimatedHours && Status == "Completed";
    public bool IsUnderestimated => ActualHours > EstimatedHours;
    
    // Efficiency: (Estimated / Actual). If Actual is lower, efficiency > 1 (Good).
    // Or simpler: Just Deviation.
    
    // Project Size Category
    public string SizeCategory
    {
        get
        {
            if (EstimatedHours <= 10) return "Small (0-10h)";
            if (EstimatedHours <= 50) return "Medium (10-50h)";
            return "Large (50h+)";
        }
    }
}
