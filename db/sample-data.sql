-- Sample schema and seed data for Customer Snapshot testing
CREATE TABLE Customers (
    Id INT IDENTITY PRIMARY KEY,
    CustomerCode NVARCHAR(50),
    Name NVARCHAR(200),
    Segment NVARCHAR(100),
    Industry NVARCHAR(100),
    LastRefreshedUtc DATETIME2
);

CREATE TABLE Orders (
    Id INT IDENTITY PRIMARY KEY,
    CustomerId INT,
    OrderDate DATETIME2,
    TotalAmount DECIMAL(18,2),
    MarginAmount DECIMAL(18,2)
);

CREATE TABLE Invoices (
    Id INT IDENTITY PRIMARY KEY,
    CustomerId INT,
    InvoiceNumber NVARCHAR(50),
    InvoiceDate DATETIME2,
    DueDate DATETIME2,
    Amount DECIMAL(18,2),
    PaidDate DATETIME2 NULL
);

CREATE TABLE Quotes (
    Id INT IDENTITY PRIMARY KEY,
    CustomerId INT,
    QuoteNumber NVARCHAR(50),
    QuoteDate DATETIME2,
    Amount DECIMAL(18,2),
    Description NVARCHAR(300),
    IsOpen BIT
);

CREATE TABLE Issues (
    Id INT IDENTITY PRIMARY KEY,
    CustomerId INT,
    Severity NVARCHAR(20),
    Status NVARCHAR(50),
    Summary NVARCHAR(300),
    CreatedOn DATETIME2
);

CREATE TABLE Interactions (
    Id INT IDENTITY PRIMARY KEY,
    CustomerId INT,
    InteractionDate DATETIME2,
    InteractionType NVARCHAR(50),
    Subject NVARCHAR(300),
    Owner NVARCHAR(100)
);

INSERT INTO Customers (CustomerCode, Name, Segment, Industry, LastRefreshedUtc)
VALUES ('C-1001', 'Contoso Retail', 'Enterprise', 'Retail', SYSUTCDATETIME());

DECLARE @CustomerId INT = SCOPE_IDENTITY();

INSERT INTO Orders (CustomerId, OrderDate, TotalAmount, MarginAmount) VALUES
(@CustomerId, DATEADD(day, -10, SYSUTCDATETIME()), 25000, 6500),
(@CustomerId, DATEADD(day, -70, SYSUTCDATETIME()), 18000, 4800),
(@CustomerId, DATEADD(day, -140, SYSUTCDATETIME()), 22000, 5200),
(@CustomerId, DATEADD(day, -280, SYSUTCDATETIME()), 16000, 4100);

INSERT INTO Invoices (CustomerId, InvoiceNumber, InvoiceDate, DueDate, Amount, PaidDate) VALUES
(@CustomerId, 'INV-100', DATEADD(day, -40, SYSUTCDATETIME()), DATEADD(day, -10, SYSUTCDATETIME()), 18000, DATEADD(day, -5, SYSUTCDATETIME())),
(@CustomerId, 'INV-101', DATEADD(day, -15, SYSUTCDATETIME()), DATEADD(day, 15, SYSUTCDATETIME()), 25000, NULL);

INSERT INTO Quotes (CustomerId, QuoteNumber, QuoteDate, Amount, Description, IsOpen) VALUES
(@CustomerId, 'Q-500', DATEADD(day, -7, SYSUTCDATETIME()), 12000, 'Annual replenishment', 1),
(@CustomerId, 'Q-501', DATEADD(day, -30, SYSUTCDATETIME()), 6000, 'Pilot for new region', 1);

INSERT INTO Issues (CustomerId, Severity, Status, Summary, CreatedOn) VALUES
(@CustomerId, 'High', 'In Progress', 'Late shipment on last PO', DATEADD(day, -3, SYSUTCDATETIME())),
(@CustomerId, 'Low', 'Open', 'Requesting updated spec sheet', DATEADD(day, -14, SYSUTCDATETIME()));

INSERT INTO Interactions (CustomerId, InteractionDate, InteractionType, Subject, Owner) VALUES
(@CustomerId, DATEADD(day, -1, SYSUTCDATETIME()), 'Call', 'Reviewed delivery status and next forecast', 'Alex R'),
(@CustomerId, DATEADD(day, -5, SYSUTCDATETIME()), 'Email', 'Shared updated pricing matrix', 'Alex R'),
(@CustomerId, DATEADD(day, -12, SYSUTCDATETIME()), 'Meeting', 'QBR prep with operations', 'Jamie L'),
(@CustomerId, DATEADD(day, -25, SYSUTCDATETIME()), 'Ticket', 'Support on EDI errors', 'Support Team'),
(@CustomerId, DATEADD(day, -40, SYSUTCDATETIME()), 'Quote', 'Scoped expansion bundle', 'Alex R');
