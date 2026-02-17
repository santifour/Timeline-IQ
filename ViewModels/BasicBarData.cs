namespace ProjectTimeEstimator.ViewModels;

public class BasicBarData
{
    public string Label { get; set; } = string.Empty;
    public double Value1 { get; set; } // Estimated
    public double Value2 { get; set; } // Actual
    public bool IsOverestimated => Value2 < Value1; // Actual < Estimated
}
