using System.ComponentModel.DataAnnotations;

namespace ExpensePilot.Models;

public class MonthlyIncome
{
    public int Id { get; set; }

    [Range(1, 12)]
    public int Month { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; }

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal Amount { get; set; }
}
