# Event Registration System - Windows Setup

This guide explains how to run the project on another Windows PC.

## 1. Requirements

- Windows 10 or Windows 11
- .NET 8 SDK
- Git
- PostgreSQL access

Download:

- .NET 8 SDK: <https://dotnet.microsoft.com/en-us/download/dotnet/8.0>
- Git: <https://git-scm.com/download/win>
- PostgreSQL: <https://www.postgresql.org/download/windows/>

## 2. Copy the Project

Move or clone the project onto the Windows PC.

Example:

```powershell
git clone <your-repository-url>
cd EventRegistrationSystem
```

If you are copying the folder manually, just open terminal in the `EventRegistrationSystem` folder.

## 3. Configure the Database

Open:

- [appsettings.Development.json](./appsettings.Development.json)

Update this value if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=YOUR_HOST;Port=5432;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD"
}
```

Notes:

- In `Development`, the app is already set to run migrations automatically.
- In `Development`, the app is already set to seed demo users and demo event data automatically.

Current flags:

```json
"Database": {
  "RunMigrationsOnStartup": true,
  "SeedDemoDataOnStartup": true
}
```

## 4. Restore and Build

Run:

```powershell
dotnet restore
dotnet build
```

The project targets `.NET 8` in [EventRegistrationSystem.csproj](./EventRegistrationSystem.csproj).

## 5. Run the Project

Default run:

```powershell
dotnet run
```

If you want a custom port:

```powershell
dotnet run --no-build --urls http://127.0.0.1:5082
```

Default launch profile ports from [launchSettings.json](./Properties/launchSettings.json):

- HTTP: `http://localhost:5149`
- HTTPS: `https://localhost:7278`

## 6. Open in Browser

After startup, open:

- `http://localhost:5149`

Or your custom port, for example:

- `http://127.0.0.1:5082`

## 7. Demo Login Accounts

These are seeded automatically in development:

- Super Admin
  - Email: `superadmin@semistash.io`
  - Password: `Admin@123`
- Organizer
  - Email: `organizer@semistash.io`
  - Password: `Organizer@123`
- Attendee
  - Email: `attendee@semistash.io`
  - Password: `Attendee@123`

## 8. Useful Commands

Build only:

```powershell
dotnet build
```

Run with hot reload:

```powershell
dotnet watch
```

Run on a fixed port:

```powershell
dotnet run --no-build --urls http://127.0.0.1:5082
```

## 9. Troubleshooting

### Port already in use

Use another port:

```powershell
dotnet run --no-build --urls http://127.0.0.1:5090
```

### Database connection failed

Check:

- host
- port
- database name
- username
- password
- firewall or hosting access rules

### HTTPS certificate warning

Use the HTTP profile for quick local testing:

```powershell
dotnet run --launch-profile http
```

### First startup is slower

This is normal when:

- restoring packages
- applying migrations
- seeding demo data

## 10. Summary

Quick start:

```powershell
dotnet restore
dotnet build
dotnet run
```

Then open:

```text
http://localhost:5149
```
