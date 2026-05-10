<div align="center">
  <img src="Masar/wwwroot/Logo.png" alt="Logo" width="150" height="150">

  <h3 align="center">Masar</h3>

  <p align="center">
    A comprehensive internship and employment platform designed to connect students with companies.
    <br />
    <br />
    <a href="#features">Explore Features</a>
    ·
    <a href="#architecture">View Architecture</a>
    ·
    <a href="#getting-started">Getting Started</a>
  </p>
</div>

---
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![EF Core](https://img.shields.io/badge/EF%20Core-8.0-68217A)](https://docs.microsoft.com/ef)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Project Structure](#project-structure)
- [Screenshots](#screenshots)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

Masar is an open-source career platform built with ASP.NET Core 8 MVC, designed to bridge the gap between job seekers and employers. It supports two user roles — **Candidates** and **Companies** — each with a tailored experience for finding opportunities or discovering talent.

---

## Features

### For Candidates
- **Profile builder** — personal info, education, skills, resume upload, and professional links
- **Job browsing** — search and filter by keyword, location, job type, industry, and salary range
- **One-click apply** — apply with an existing profile resume or upload a new one
- **Custom application questions** — answer job-specific questions (essay or yes/no) during the apply flow
- **Cover letter support** — write and submit cover letters for positions that require them
- **Application tracking** — view the status of every application (Applied → Under Review → Accepted/Rejected)
- **Saved jobs** — bookmark jobs and manage them with search and sort
- **Dashboard** — profile completion hints, stats, recent applications, and recommended jobs

### For Companies
- **Company profile** — logo, description, industry, size, contact info, and social links
- **Job postings** — create and manage full-time, part-time, and internship listings
- **Custom application questions** — attach structured questions to any job posting
- **Applicant management** — filter, sort, search, and paginate applicants per job
- **Review workflow** — view full candidate profiles, resumes, cover letters, and answers, then accept or reject
- **Dashboard** — stats on active jobs, applicants, new applicants, and pending reviews

### Platform
- **Google OAuth** — sign in with Google alongside traditional email/password auth
- **Password reset** — server-side token flow (no email dependency required)
- **Role selection** — new users choose Candidate or Company after registration
- **Job lifecycle** — jobs automatically close when their application deadline passes
- **Responsive UI** — mobile-friendly sidebar navigation with hamburger menu
- **File uploads** — local disk storage for resumes, avatars, and company logos

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 MVC |
| ORM | Entity Framework Core 8 |
| Database | SQL Server |
| Auth | ASP.NET Identity + Google OAuth |
| Front-end | Bootstrap 5, Iconify, Sora + DM Sans fonts |
| File storage | Local disk (wwwroot/uploads) |
| Environment | DotNetEnv |
| CI/CD | GitHub Actions → MonsterASP (Web Deploy) |

---

## Architecture

The solution follows a **layered architecture** split across four projects:

```
Masar.sln
├── Masar                   # ASP.NET Core MVC web app (controllers, views, wwwroot)
├── Masar.Core              # Application services and interfaces
├── Masar.Domain            # Domain models, view models, enums, validation attributes
└── Masar.Infrastructure    # EF Core context, migrations, repository, constants
```

### Dependency flow

```
Masar → Masar.Core → Masar.Domain
Masar → Masar.Infrastructure → Masar.Domain
Masar.Core → Masar.Infrastructure
```

### Key services

| Interface | Responsibility |
|---|---|
| `IAuthService` | Registration, login, Google OAuth, password reset/change |
| `IProfileService` | Candidate and company profile CRUD, file uploads |
| `IJobService` | Job posting, editing, browsing, filtering |
| `IApplicationService` | Apply, track, save jobs, company review workflow |
| `IDashboardService` | Aggregated stats and recent activity for both roles |
| `IJobLifecycleService` | Auto-close expired job listings |
| `IFileService` | Resume, avatar, and logo upload/delete |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [SQL Server](https://www.microsoft.com/sql-server) (or SQL Server Express / LocalDB)
- A Google Cloud project with OAuth 2.0 credentials *(optional — only needed for Google sign-in)*

### Clone and run

```bash
git clone https://github.com/your-org/masar.git
cd masar
```

Create a `.env` file in the `Masar/` directory (see [Configuration](#configuration)), then:

```bash
cd Masar.Infrastructure
dotnet ef database update

cd ../Masar
dotnet run
```

The app will be available at `https://localhost:7152` (or `http://localhost:5278`).

---

## Configuration

Create `Masar/.env` with the following keys:

```env
# SQL Server connection string
CONN_STRING=Server=localhost;Database=MasarDb;Trusted_Connection=True;TrustServerCertificate=True;

# Google OAuth (leave blank to disable Google sign-in)
GOOGLE_CLIENT_ID=your-google-client-id
GOOGLE_CLIENT_SECRET=your-google-client-secret
```

For production deployments, set these as environment variables or secrets rather than committing the `.env` file. The `.gitignore` already excludes it.

### GitHub Actions (CI/CD)

The included `deploy.yml` workflow deploys to MonsterASP on every push. Set the following repository secrets:

| Secret | Description |
|---|---|
| `PUBLISH_URL` | Web Deploy endpoint URL |
| `PUBLISH_USERNAME` | Web Deploy username |
| `PUBLISH_PASSWORD` | Web Deploy password |

---

## Database Setup

Migrations live in `Masar.Infrastructure/Migrations`. To apply them:

```bash
cd Masar.Infrastructure
dotnet ef database update
```

To add a new migration after modifying domain models:

```bash
dotnet ef migrations add <MigrationName>
```

On first startup, the application automatically:
1. Runs any pending migrations
2. Seeds the `Admin`, `Candidate`, and `Company` roles

---

## Project Structure

```
Masar/
├── Controllers/
│   ├── AuthController.cs         # Registration, login, Google OAuth, password reset
│   ├── CandidateController.cs    # Candidate dashboard, profile, applications, saved jobs
│   ├── CompanyController.cs      # Company dashboard, profile, job posting, applicant review
│   ├── JobsController.cs         # Public job browsing and apply flow
│   └── HomeController.cs         # Landing page
├── Views/
│   ├── Auth/                     # Login, register, role selection, password reset
│   ├── Candidate/                # Dashboard, profile, applications, saved jobs
│   ├── Company/                  # Dashboard, profile, job posting, applicant management
│   ├── Jobs/                     # Job index, detail, apply
│   └── Shared/                   # Layout, auth layout, sidebars, footer
├── wwwroot/
│   ├── css/                      # site.css (main), auth.css
│   ├── js/                       # site.js (phone input widget, general utils)
│   └── uploads/                  # Runtime file uploads (gitignored)
└── Program.cs                    # DI registration, middleware, DB seeding

Masar.Core/
├── IService/                     # Service interfaces
└── Services/                     # Service implementations

Masar.Domain/
├── Models/                       # EF entity classes
├── Enums/                        # ApplicationStatus, JobType, WorkMode, etc.
├── ViewModels/                   # DTOs for all controllers
└── Validation/                   # Custom validation attributes

Masar.Infrastructure/
├── Config/                       # EF Fluent API configurations
├── Context/                      # AppDbContext
├── Migrations/                   # EF Core migrations
├── Constants/                    # Role name constants
└── Repository.cs                 # Generic repository (optional base)
```

---

## Screenshots

| Landing Page | Candidate Dashboard | Job Listings |
|:---:|:---:|:---:|
| <img width="1366" height="3547" alt="home" src="https://github.com/user-attachments/assets/3c6ea5a5-0fc0-4e2c-b44e-919d87933fc1" /> | <img width="1366" height="803" alt="candidate-dashboard" src="https://github.com/user-attachments/assets/fb614914-cd26-4391-b3f3-4a3987ef9c02" /> | <img width="1366" height="1259" alt="jobs" src="https://github.com/user-attachments/assets/fc4a67aa-4aed-495d-8eaf-ddcc46f38bf3" /> |

| Company Dashboard | Applicant Review | Job Posting |
|:---:|:---:|:---:|
| <img width="1366" height="1043" alt="company-dashboard" src="https://github.com/user-attachments/assets/4e021deb-e2a1-48cf-99ec-32f9494e02d2" /> | <img width="1366" height="1212" alt="applicant-review" src="https://github.com/user-attachments/assets/67af120a-3bff-47c9-9a12-1aedf43dadcd" /> | <img width="1366" height="2021" alt="job-posting" src="https://github.com/user-attachments/assets/e5057d2f-c4da-4a53-a061-2cdad62a127d" /> |

---

## Contributing

Contributions are welcome! Here's how to get involved:

1. **Fork** the repository
2. **Create a branch** for your feature or fix: `git checkout -b feature/my-feature`
3. **Commit** your changes with a clear message: `git commit -m "feat: add email notifications"`
4. **Push** to your fork: `git push origin feature/my-feature`
5. **Open a Pull Request** describing what you changed and why

### Development tips

- Follow the existing layered architecture — new features belong in `Masar.Core/Services`, not controllers
- Add new entities to `Masar.Domain/Models`, configure them in `Masar.Infrastructure/Config`, and create a migration
- Keep controllers thin — delegate business logic to services via interfaces
- Run `dotnet build` before opening a PR to catch compile errors

### Reporting issues

Please open a GitHub issue with a clear title, steps to reproduce, and the expected vs actual behavior.

---

## License

This project is licensed under the [MIT License](LICENSE). You are free to use, modify, and distribute it for personal or commercial purposes with attribution.
