using System.Globalization;
using System.Text;
using ExpensePilot.Data;
using ExpensePilot.Models;
using ExpensePilot.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpensePilot.Controllers;

public class HomeController : Controller
{
    private readonly ExpenseDbContext _db;
    public HomeController(ExpenseDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(int? month, int? year)
    {
        var selected = new DateTime(year ?? DateTime.Today.Year, month ?? DateTime.Today.Month, 1);
        var start = selected;
        var end = start.AddMonths(1);
        var monthExpenses = await _db.Expenses.AsNoTracking()
            .Where(x => x.Date >= start && x.Date < end).ToListAsync();
        var budget = await _db.MonthlyBudgets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Month == selected.Month && x.Year == selected.Year);
        var monthlyIncome = await _db.MonthlyIncomes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Month == selected.Month && x.Year == selected.Year);
        var all = await _db.Expenses.AsNoTracking().ToListAsync();
        var expensesOnly = monthExpenses.Where(x => x.Type == "Expense").ToList();
        var incomeOnly = monthExpenses.Where(x => x.Type == "Income").ToList();
        var categoryTotals = expensesOnly.GroupBy(x => x.Category)
            .Select(g => new CategoryTotal { Category = g.Key, Amount = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Amount).ToList();
        var totalExpenses = expensesOnly.Sum(x => x.Amount);
        foreach (var category in categoryTotals)
            category.Percent = totalExpenses == 0 ? 0 : Math.Round(category.Amount / totalExpenses * 100, 1);

        var model = new DashboardViewModel
        {
            SelectedMonth = selected,
            IncomeThisMonth = incomeOnly.Sum(x => x.Amount),
            MonthlyIncomeAmount = monthlyIncome?.Amount ?? 0,
            ExpensesThisMonth = totalExpenses,
            BudgetLimit = budget?.Limit ?? 0,
            TransactionCount = monthExpenses.Count,
            RecentTransactions = monthExpenses.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).Take(8).ToList(),
            CategoryTotals = categoryTotals,
            MonthlyTotals = Enumerable.Range(0, 6).Select(offset => selected.AddMonths(offset - 5)).Select(m =>
            {
                var items = all.Where(x => x.Date.Year == m.Year && x.Date.Month == m.Month).ToList();
                return new MonthlyTotal
                {
                    Label = m.ToString("MMM", CultureInfo.InvariantCulture),
                    Income = items.Where(x => x.Type == "Income").Sum(x => x.Amount),
                    Expense = items.Where(x => x.Type == "Expense").Sum(x => x.Amount)
                };
            }).ToList()
        };

        if (model.BudgetLimit > 0)
        {
            if (model.ExpensesThisMonth >= model.BudgetLimit)
                model.Insights.Add("Budget alert: you've reached or exceeded your monthly spending limit.");
            else if (model.BudgetUsedPercent >= 80)
                model.Insights.Add($"Heads up: you've used {model.BudgetUsedPercent:0}% of this month's budget.");
            else
                model.Insights.Add($"You're within budget with {model.RemainingBudget:C} left to spend.");
        }
        else model.Insights.Add("Set a monthly budget to unlock budget progress and spending alerts.");

        var previous = selected.AddMonths(-1);
        var previousTotal = all.Where(x => x.Type == "Expense" && x.Date.Year == previous.Year && x.Date.Month == previous.Month).Sum(x => x.Amount);
        if (previousTotal > 0)
        {
            var change = (model.ExpensesThisMonth - previousTotal) / previousTotal * 100;
            model.Insights.Add(change > 0
                ? $"Spending is up {change:0}% compared with {previous.ToString("MMMM", CultureInfo.InvariantCulture)}."
                : $"Spending is down {Math.Abs(change):0}% compared with {previous.ToString("MMMM", CultureInfo.InvariantCulture)}.");
        }
        var top = categoryTotals.FirstOrDefault();
        if (top != null) model.Insights.Add($"Your biggest spending category is {top.Category} at {top.Amount:C}.");
        var wants = expensesOnly.Where(x => x.SpendingKind == "Want").Sum(x => x.Amount);
        if (totalExpenses > 0 && wants > 0) model.Insights.Add($"Wants account for {wants / totalExpenses * 100:0}% of your expenses this month.");

        ViewBag.Categories = Categories;
        ViewBag.SelectedMonthQuery = selected.ToString("yyyy-MM");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExpense(Expense expense)
    {
        if (expense.Date > DateTime.Today.AddDays(1)) ModelState.AddModelError(nameof(expense.Date), "Choose a date that is not in the future.");
        if (expense.Type == "Income") expense.SpendingKind = "Need";
        if (!Categories.Contains(expense.Category)) ModelState.AddModelError(nameof(expense.Category), "Choose a valid category.");
        if (expense.Type is not ("Expense" or "Income")) ModelState.AddModelError(nameof(expense.Type), "Choose Expense or Income.");
        if (expense.SpendingKind is not ("Need" or "Want")) ModelState.AddModelError(nameof(expense.SpendingKind), "Choose Need or Want.");

        if (!ModelState.IsValid)
        {
            TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index), new { month = expense.Date.Month, year = expense.Date.Year });
        }
        expense.Title = expense.Title.Trim();
        expense.Notes = string.IsNullOrWhiteSpace(expense.Notes) ? null : expense.Notes.Trim();
        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Transaction added successfully.";
        return RedirectToAction(nameof(Index), new { month = expense.Date.Month, year = expense.Date.Year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense != null)
        {
            var month = expense.Date.Month; var year = expense.Date.Year;
            _db.Expenses.Remove(expense);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Transaction deleted.";
            return RedirectToAction(nameof(Index), new { month, year });
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveBudget(int month, int year, decimal limit)
    {
        if (month is < 1 or > 12 || year is < 2000 or > 2100 || limit <= 0 || limit > 999999999)
        {
            TempData["Error"] = "Enter a valid month, year, and positive budget amount.";
            return RedirectToAction(nameof(Index));
        }
        var budget = await _db.MonthlyBudgets.FirstOrDefaultAsync(x => x.Month == month && x.Year == year);
        if (budget == null)
        {
            budget = new MonthlyBudget { Month = month, Year = year, Limit = limit };
            _db.MonthlyBudgets.Add(budget);
        }
        else budget.Limit = limit;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Monthly budget saved.";
        return RedirectToAction(nameof(Index), new { month, year });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveMonthlyIncome(int month, int year, decimal amount)
    {
        if (month is < 1 or > 12 || year is < 2000 or > 2100 || amount <= 0 || amount > 999999999)
        {
            TempData["Error"] = "Enter a valid month, year, and monthly income greater than zero.";
            return RedirectToAction(nameof(Index), new { month, year });
        }

        var income = await _db.MonthlyIncomes
            .FirstOrDefaultAsync(x => x.Month == month && x.Year == year);

        if (income == null)
        {
            income = new MonthlyIncome { Month = month, Year = year, Amount = amount };
            _db.MonthlyIncomes.Add(income);
        }
        else
        {
            income.Amount = amount;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Monthly income saved successfully.";
        return RedirectToAction(nameof(Index), new { month, year });
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(int? month, int? year)
    {
        var selected = new DateTime(year ?? DateTime.Today.Year, month ?? DateTime.Today.Month, 1);
        var items = await _db.Expenses.AsNoTracking().Where(x => x.Date.Year == selected.Year && x.Date.Month == selected.Month)
            .OrderBy(x => x.Date).ToListAsync();
        var sb = new StringBuilder("Date,Title,Type,Category,NeedOrWant,Amount,Notes\r\n");
        foreach (var x in items)
        {
            var fields = new[] { x.Date.ToString("yyyy-MM-dd"), x.Title, x.Type, x.Category, x.SpendingKind, x.Amount.ToString("0.00", CultureInfo.InvariantCulture), x.Notes ?? "" };
            sb.AppendLine(string.Join(",", fields.Select(CsvEscape)));
        }
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray(), "text/csv; charset=utf-8", $"ExpensePilot-{selected:yyyy-MM}.csv");
    }

    private static string CsvEscape(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";

    public IActionResult Error() => View("Error");

    public static readonly string[] Categories = { "Food", "Transport", "Shopping", "Bills", "Health", "Education", "Entertainment", "Travel", "Home", "Other" };
}
