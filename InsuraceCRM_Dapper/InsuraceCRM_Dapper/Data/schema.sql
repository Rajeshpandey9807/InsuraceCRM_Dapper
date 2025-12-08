CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    Mobile NVARCHAR(50) NULL,
    Role NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1
);

CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    MobileNumber NVARCHAR(50) NOT NULL,
    Location NVARCHAR(150) NOT NULL,
    InsuranceType NVARCHAR(100) NULL,
    Income DECIMAL(18,2) NULL,
    SourceOfIncome NVARCHAR(150) NULL,
    FamilyMembers INT NULL,
    AssignedEmployeeId INT NULL REFERENCES Users(Id),
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE FollowUps (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL REFERENCES Customers(Id),
    FollowUpDate DATE NOT NULL,
    InsuranceType NVARCHAR(100) NULL,
    Budget DECIMAL(18,2) NULL,
    HasExistingPolicy BIT NOT NULL DEFAULT 0,
    FollowUpNote NVARCHAR(1000) NULL,
    FollowUpStatus NVARCHAR(100) NULL,
    NextReminderDateTime DATETIME2 NULL,
    ReminderRequired BIT NOT NULL DEFAULT 0,
    IsConverted BIT NULL,
    ConversionReason NVARCHAR(500) NULL,
    SoldProductId INT NULL REFERENCES Products(Id),
    SoldProductName NVARCHAR(200) NULL,
    TicketSize DECIMAL(18,2) NULL,
    TenureInYears INT NULL,
    PolicyNumber NVARCHAR(100) NULL,
    PolicyEnforceDate DATE NULL
);

CREATE TABLE Reminders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL REFERENCES Customers(Id),
    EmployeeId INT NOT NULL REFERENCES Users(Id),
    ReminderDateTime DATETIME2 NOT NULL,
    Note NVARCHAR(500) NULL,
    IsShown BIT NOT NULL DEFAULT 0
);

CREATE NONCLUSTERED INDEX IX_Customers_AssignedEmployee
    ON Customers (AssignedEmployeeId);

CREATE NONCLUSTERED INDEX IX_Reminders_Employee_ReminderDate
    ON Reminders (EmployeeId, ReminderDateTime)
    INCLUDE (IsShown);

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    CommissionType NVARCHAR(50) NOT NULL,
    CommissionValue DECIMAL(18,2) NOT NULL,
    CommissionNotes NVARCHAR(500) NULL,
    CreatedOn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedOn DATETIME2 NULL
);

CREATE TABLE ProductDocuments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL REFERENCES Products(Id) ON DELETE CASCADE,
    FileName NVARCHAR(300) NOT NULL,
    OriginalFileName NVARCHAR(300) NOT NULL,
    ContentType NVARCHAR(150) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    UploadedOn DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE NONCLUSTERED INDEX IX_ProductDocuments_ProductId
    ON ProductDocuments (ProductId);
