# Insurance CRM (ASP.NET Core + Dapper)

An end-to-end customer relationship management system tailored for insurance teams. The application is built with ASP.NET Core 8 MVC, uses Dapper for data access, and focuses on day-to-day workflows such as lead intake, follow-up tracking, reminder automation, product management, and role-based administration.

## Feature Highlights
- **Role-aware authentication** – Cookie-based auth with Admin, Manager, and Employee roles plus automatic seeding of a default administrator (`admin@crm.local` / `Admin@123`).
- **Customer workspace** – Filterable customer lists, inline creation, CSV/Excel import templates, and PDF/Excel exports powered by CsvHelper, ExcelDataReader, ClosedXML, and QuestPDF.
- **Assignment workflows** – One-off or bulk assignment of customers to employees, plus visibility into ownership across the team.
- **Follow-up lifecycle** – Detailed forms capture budgets, insurance preferences, reminders, and conversion data that flow into sold-product tracking.
- **Reminder center** – Server-side reminder service with dashboards for "my" versus "all" reminders and lightweight acknowledgment endpoints.
- **Product & document library** – Product CRUD with commission metadata and secure upload/preview of collateral (PDF, DOC/DOCX, PNG/JPG up to 20 MB each).
- **Sales closure insights** – Sold policy tracker, employee insights, and admin exports in PDF/Excel formats.

## Solution Layout
```
InsuraceCRM_Dapper/
├── Controllers/               // MVC endpoints for auth, admin, customers, products, follow-ups, etc.
├── Data/                      // Dapper connection factory + schema script
├── Interfaces/                // Repository and service abstractions
├── Models/                    // Domain entities (Customer, Reminder, Product, User...)
├── Repositories/              // Dapper-based data access implementations
├── Services/                  // Business logic + seeding (e.g., EnsureDefaultAdminAsync)
├── ViewModels/                // Transport objects for strongly typed Razor views
├── Views/                     // Razor UI, layouts, and partials
└── wwwroot/                   // Static assets, uploaded product documents, compiled CSS/JS
```

## Technology Stack
- ASP.NET Core 8 MVC (`Microsoft.NET.Sdk.Web`)
- Dapper micro-ORM for SQL Server data access
- SQL Server (or Azure SQL) as the backing store
- CsvHelper, ExcelDataReader, ClosedXML for data import/export
- QuestPDF for PDF generation
- Cookie authentication + Identity password hashing

## Prerequisites
- .NET 8 SDK
- SQL Server instance reachable from your dev machine (LocalDB, SQL Express, Docker, or Azure SQL)
- Node/npm are **not** required (pure server-rendered Razor views)

## Database Setup
1. Create a database (default name used in samples: `InsuranceCRM`).
2. Execute `InsuraceCRM_Dapper/Data/schema.sql` against that database to provision tables, indexes, and constraints.
3. (Optional) Add seed data for products or customers using your preferred SQL scripts.

## Configuration
- Connection strings are read from the `DefaultConnection` entry in `appsettings.json` / `appsettings.Development.json`.
- Override locally via `dotnet user-secrets`, environment variables, or container secrets when deploying:
  ```bash
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=InsuranceCRM;User Id=..."
  ```
- Uploaded product documents are stored under `wwwroot/uploads/products`. Ensure the hosting environment grants write permissions to this directory or remap the path in `ProductController` if needed.

## Running the Application
```bash
# Restore dependencies
DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 dotnet restore InsuraceCRM_Dapper/InsuraceCRM_Dapper.csproj

# Launch in watch mode (optional)
dotnet watch --project InsuraceCRM_Dapper/InsuraceCRM_Dapper.csproj

# Or run once
dotnet run --project InsuraceCRM_Dapper/InsuraceCRM_Dapper.csproj
```
The site defaults to `https://localhost:5001` (or `http://localhost:5000`). The login page is the default route. Sign in with the seeded admin account on first run, then create roles/users from the Admin area.

## Production Notes
- Update `appsettings.Production.json` (not tracked) with the production connection string.
- Use `dotnet publish -c Release` to generate deployment assets.
- Configure reverse proxies (IIS, Nginx) to serve static files from `wwwroot` and secure the uploads directory as needed.
- Back up the `uploads/products` directory alongside your database, since uploaded documents are not stored in SQL Server.

## Troubleshooting
- **Unable to connect to SQL Server** → verify `DefaultConnection`, firewall rules, and TLS (`TrustServerCertificate=True` as needed).
- **Documents not saving** → confirm the process has write access to `wwwroot/uploads/products` and that files stay under 20 MB with allowed extensions.
- **Missing admin user** → `Program.cs` calls `EnsureDefaultAdminAsync` on startup. Check application logs if the seed fails (e.g., database unreachable).

## Next Steps
- Add automated tests (currently none) to cover services and repositories.
- Integrate a background worker for reminder notifications (email/SMS) if required.
- Extend analytics in `DashboardService` for richer conversion metrics.
