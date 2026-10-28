using ExpensePilot.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpensePilot.Data;

public class ExpenseDbContext : DbContext
{
    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options) { }

    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<MonthlyBudget> MonthlyBudgets => Set<MonthlyBudget>();
    public DbSet<MonthlyIncome> MonthlyIncomes => Set<MonthlyIncome>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Expense>().Property(x => x.Amount).HasPrecision(12, 2);
        modelBuilder.Entity<MonthlyBudget>().Property(x => x.Limit).HasPrecision(12, 2);
        modelBuilder.Entity<MonthlyBudget>().HasIndex(x => new { x.Year, x.Month }).IsUnique();
        modelBuilder.Entity<MonthlyIncome>().Property(x => x.Amount).HasPrecision(12, 2);
        modelBuilder.Entity<MonthlyIncome>().HasIndex(x => new { x.Year, x.Month }).IsUnique();
    }
}
