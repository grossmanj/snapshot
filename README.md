# Customer Snapshot – 15-Second Briefing

Lightweight desktop (WinForms) viewer that summarizes a customer's health and next steps for sales reps. The solution is split into Domain, Application, Infrastructure, Desktop, and Tests projects with ADO.NET data access and manual mapping.

## Projects
- **Domain** – POCO models used across layers.
- **Application** – Service interfaces and business logic (personality, talking points, next best action, snapshot orchestration).
- **Infrastructure** – ADO.NET repositories and KPI service using `System.Data.SqlClient`.
- **Tests** – xUnit tests for the service layer using mocked repositories.

> The former ASP.NET Core web host has been removed to streamline desktop-only deployment.

## Getting Started
1. Ensure the read-only database views exist with the expected column shapes:
   - `vwCustomers` (Id, CustomerCode, Name, Segment, Industry, LastRefreshedUtc)
   - `vwCustomerOrders` (Id, CustomerId, OrderDate, TotalAmount, MarginAmount)
   - `vwCustomerInvoices` (Id, CustomerId, InvoiceNumber, InvoiceDate, DueDate, Amount, PaidDate)
   - `vwCustomerQuotes` (Id, CustomerId, QuoteNumber, QuoteDate, Amount, Description, IsOpen)
   - `vwCustomerIssues` (Id, CustomerId, Severity, Status, Summary, CreatedOn)
   - `vwCustomerInteractions` (Id, CustomerId, InteractionDate, InteractionType, Subject, Owner)
   > The application performs read-only queries against these views; no schema changes or writes are required.
2. Restore and build:
   ```bash
   dotnet restore
   dotnet build
   ```

## Windows desktop viewer
For use as a lightweight overlay inside Visma Business (via BIG):

1. Update `src/Desktop/appsettings.json` with the ERP connection string (`ErpDatabase`).
2. On Windows, run the desktop project and optionally pass a starting customer ID:
   ```bash
   dotnet run --project src/Desktop/Desktop.csproj -- 1
   ```
3. The WinForms window can be launched on a background thread in BIG to float above the ERP client.

## Tests
Run the unit tests with:
```bash
dotnet test
```

## Notes
- Data access uses pure ADO.NET with manual mapping; no ORMs.
- Services are registered via DI in `Program.cs` for repositories and business logic.
