namespace FinanceManagerAspNet.Models;

public sealed class MonthlyCarryForward
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public decimal? OverrideAmount { get; set; }
    public string? OverrideReason { get; set; }
    public int? SourceYear { get; set; }
    public int? SourceMonth { get; set; }
    public DateTime UpdatedAt { get; set; }
}
