CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(250) NULL,
    IsSystem BIT NOT NULL DEFAULT 0
);

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
    Location NVARCHAR(150) NULL,
    InsuranceType NVARCHAR(100) NULL,
    AssignedEmployeeId INT NULL REFERENCES Users(Id),
    CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE FollowUps (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL REFERENCES Customers(Id),
    FollowUpDate DATE NOT NULL,
    FollowUpNote NVARCHAR(1000) NULL,
    FollowUpStatus NVARCHAR(100) NULL,
    NextReminderDateTime DATETIME2 NULL
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

INSERT INTO Roles (Name, Description, IsSystem)
SELECT v.Name, v.Description, v.IsSystem
FROM (VALUES
    ('Admin', 'Full system access', 1),
    ('Manager', 'Manage teams and assignments', 0),
    ('Employee', 'Standard access for daily work', 0)
) AS v(Name, Description, IsSystem)
WHERE NOT EXISTS (
    SELECT 1 FROM Roles r WHERE LOWER(r.Name) = LOWER(v.Name)
);
