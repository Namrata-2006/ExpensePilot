using System.ComponentModel.DataAnnotations;

namespace ExpensePilot.Models;

public class Expense
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Title { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999999")]
    public decimal Amount { get; set; }

    [Required, StringLength(40)]
    public string Category { get; set; } = "Other";

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required, StringLength(10)]
    public string Type { get; set; } = "Expense";

    [Required, StringLength(20)]
    public string SpendingKind { get; set; } = "Need";

    [StringLength(250)]
    public string? Notes { get; set; }
}
