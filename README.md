# Customer Snapshot – 15-Second Briefing

ASP.NET Core 8 MVC dashboard that summarizes a customer's health and next steps for sales reps. The solution is split into Domain, Application, Infrastructure, Web, and Tests projects with ADO.NET data access and manual mapping.

## Projects
- **Domain** – POCO models used across layers.
- **Application** – Service interfaces and business logic (personality, talking points, next best action, snapshot orchestration).
- **Infrastructure** – ADO.NET repositories and KPI service using `System.Data.SqlClient`.
- **Web** – MVC UI that renders the single-page dashboard at `/CustomerSnapshot/{customerId}`.
- **Tests** – xUnit tests for the service layer using mocked repositories.

## Getting Started
1. Update `src/Web/appsettings.json` with your SQL Server connection string (`ErpDatabase`).
2. Ensure the read-only database views exist with the expected column shapes:
   - `vwCustomers` (Id, CustomerCode, Name, Segment, Industry, LastRefreshedUtc)
   - `vwCustomerOrders` (Id, CustomerId, OrderDate, TotalAmount, MarginAmount)
   - `vwCustomerInvoices` (Id, CustomerId, InvoiceNumber, InvoiceDate, DueDate, Amount, PaidDate)
   - `vwCustomerQuotes` (Id, CustomerId, QuoteNumber, QuoteDate, Amount, Description, IsOpen)
   - `vwCustomerIssues` (Id, CustomerId, Severity, Status, Summary, CreatedOn)
   - `vwCustomerInteractions` (Id, CustomerId, InteractionDate, InteractionType, Subject, Owner)
   > The application performs read-only queries against these views; no schema changes or writes are required.
3. Restore and build:
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project src/Web/Web.csproj
   ```
4. Navigate to `https://localhost:5001/CustomerSnapshot/1` (replace the ID as needed).

## Tests
Run the unit tests with:
```bash
dotnet test
```

## Notes
- Data access uses pure ADO.NET with manual mapping; no ORMs.
- Services are registered via DI in `Program.cs` for repositories and business logic.
