# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

- **Build the solution**: `dotnet build`
- **Run the application**: `dotnet run` (from the Masar project directory)
- **Run tests**: No test project is currently configured. When tests are added, use `dotnet test`.
- **Apply migrations**: `dotnet ef database update` (from Masar.Infrastructure project directory)
- **Add a migration**: `dotnet ef migrations add <MigrationName>` (from Masar.Infrastructure project directory)

## Project Structure

The solution follows a layered architecture with four projects:

1. **Masar** (Web Application) - ASP.NET Core MVC project containing:
   - Controllers: Handle HTTP requests (Home, Auth, Candidate, Jobs, Company)
   - Views: Razor views for rendering HTML
   - wwwroot: Static assets (CSS, JavaScript, images)
   - Program.cs: Application entry point and middleware configuration

2. **Masar.Core** - Contains core interfaces and services:
   - IService: Application service interfaces (e.g., IApplicationService)
   - Services: Implementation of application services

3. **Masar.Domain** - Contains domain models and view models:
   - Models: Entity classes (Job, JobApplication, ApplicationUser, etc.)
   - Enums: Enumerations for job types, work modes, industries, etc.
   - ViewModels: DTOs for data transfer between layers (e.g., ApplicantsViewDto, CompanyDtos)

4. **Masar.Infrastructure** - Contains data access and configuration:
   - Config: Entity Framework Core configuration classes
   - Context: AppDbContext (database context)
   - Migrations: EF Core migration files
   - Repository: Generic repository implementation
   - Constants: Role definitions and other constants

## Key Features

- **Authentication**: Google authentication integrated via Microsoft.AspNetCore.Authentication.Google
- **Authorization**: Role-based access control (constants defined in Masar.Infrastructure.Constants.Roles)
- **File Uploads**: AWS S3 integration for storing candidate resumes and company logos (AWSSDK.S3 package)
- **Environment Variables**: DotNetEnv package for managing environment variables
- **Database**: SQL Server with Entity Framework Core ORM

## Common Tasks

- **Adding a new controller**: Create in Masar/Controllers folder, inherit from Controller base class
- **Adding a new entity**: Create model in Masar.Domain/Models, configure in Masar.Infrastructure/Config, add migration
- **Adding a new service**: Create interface in Masar.Core/IService, implementation in Masar.Core/Services
- **Modifying views**: Razor views located in Masar/Views folder corresponding to controller names