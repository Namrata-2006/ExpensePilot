using System.ComponentModel.DataAnnotations;

namespace ExpensePilot.Models;

public class MonthlyBudget
{
    public int Id { get; set; }
    [Range(1, 12)] public int Month { get; set; }
    [Range(2000, 2100)] public int Year { get; set; }
    [Range(typeof(decimal), "1", "999999999")] public decimal Limit { get; set; }
}
