using ExpensePilot.Models;

namespace ExpensePilot.ViewModels;

public class DashboardViewModel
{
    public DateTime SelectedMonth { get; set; }
    public decimal IncomeThisMonth { get; set; }
    public decimal MonthlyIncomeAmount { get; set; }
    public decimal ExpensesThisMonth { get; set; }
    public decimal BalanceThisMonth => IncomeThisMonth - ExpensesThisMonth;
    public decimal BudgetLimit { get; set; }
    public decimal BudgetUsedPercent => BudgetLimit <= 0 ? 0 : Math.Min(100, ExpensesThisMonth / BudgetLimit * 100);
    public decimal RemainingBudget => Math.Max(0, BudgetLimit - ExpensesThisMonth);
    public int TransactionCount { get; set; }
    public List<Expense> RecentTransactions { get; set; } = new();
    public List<CategoryTotal> CategoryTotals { get; set; } = new();
    public List<MonthlyTotal> MonthlyTotals { get; set; } = new();
    public List<string> Insights { get; set; } = new();
}

public class CategoryTotal
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percent { get; set; }
}

public class MonthlyTotal
{
    public string Label { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}
