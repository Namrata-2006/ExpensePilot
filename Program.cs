using ExpensePilot.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ExpenseDbContext>(options => options.UseMySQL(connectionString));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Create the database schema if the database is new. The SQL below also adds
// MonthlyIncomes when the database already existed before this feature was added.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
    db.Database.EnsureCreated();
    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS `MonthlyIncomes` (
            `Id` int NOT NULL AUTO_INCREMENT,
            `Month` int NOT NULL,
            `Year` int NOT NULL,
            `Amount` decimal(12,2) NOT NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_MonthlyIncomes_Year_Month` (`Year`, `Month`)
        ) ENGINE=InnoDB;
        """);
}

app.Run();
