# ExpensePilot — Personal Expense Tracker (.NET 10 + WAMP MySQL)

A responsive personal finance dashboard built with ASP.NET Core MVC, C#, Entity Framework Core 10, and MySQL through WAMP.

## Features
- Dashboard with recorded income, expenses, net cash flow, and remaining budget.
- Separate monthly income setting saved by month and year.
- Add income and expenses with category, date, notes, and Need/Want classification.
- Monthly budget planner with progress bar and spending alerts.
- Six-month income-vs-spending chart.
- Spending category breakdown and smart insights.
- CSV export and transaction deletion.
- Responsive layout, modal transaction form, and input validation.

## Requirements
- .NET 10 SDK
- WAMP with MySQL/MariaDB service running
- A database named `expensepilot_db`

## Database setup
1. Start WAMP and ensure its icon is green.
2. Open phpMyAdmin at `http://localhost/phpmyadmin`.
3. Create the database with `CREATE DATABASE expensepilot_db;` if it does not already exist.
4. Check `appsettings.json`. The default connection assumes MySQL is at `127.0.0.1:3306`, username `root`, and an empty password. Change the connection string if your WAMP credentials differ.

On startup, the application creates tables for a new database and ensures the `MonthlyIncomes` table exists for an existing database too. The other existing tables are not migrated automatically if their schema changes.

## Run
From the folder containing `ExpensePilot.csproj`:

```powershell
dotnet restore
dotnet build
dotnet run
```

Open the localhost URL shown in the terminal.

## Monthly income
Choose the month using the dashboard's month selector. In the Monthly Budget panel, use the Monthly Income section to save or update the expected income for that month. It is stored separately from individual income transactions.
