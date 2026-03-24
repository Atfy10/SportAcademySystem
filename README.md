# AURA - Athlete, Unified, Resistence, Ambitious

## Sports Academy Management SaaS for Modern Clubs

---

## Vision

AURA exists to transform how sports academies operate — building strong relationships and deep trust between clubs and their trainees. We believe technology should amplify the human connection in sports, not replace it.

## Mission

Empowering sports academies with smart tools that connect athletes, coaches, and administrators in one seamless platform.

---

## Business Overview

AURA is an enterprise-grade sports academy management system designed for multi-branch operations. It provides comprehensive tools for managing trainees, coaches, schedules, enrollments, payments, and communications — all powered by AI for smarter decision-making.

---

## Core Business Modules

| Module | Description |
|--------|-------------|
| **Branch Management** | Multi-location support with location-specific pricing and configurations |
| **Trainee Management** | Complete trainee lifecycle: registration, profiles, sports assignment, progress tracking |
| **Employee & Coach Management** | Staff onboarding, role assignment (Coach, Manager, HR, Accountant, IT), performance tracking |
| **Class Scheduling** | Recurring group schedules with weekly patterns and session occurrences |
| **Attendance Tracking** | Per-session attendance with status (Present, Late, Absent) and notes |
| **Subscriptions & Payments** | Multiple tiers (Monthly, Quarterly, Yearly, Gold, Platinum, Silver, Basic) with branch-specific pricing |
| **AI ChatBot** | OpenAI-powered assistant for trainee inquiries and support |
| **Video Analysis** | MediaPipe-based pose analysis with AI recommendations for technique improvement |

---

## User Roles

| Role | Access Level |
|------|---------------|
| **Admin** | Full system access, user management, branch settings |
| **Manager** | Branch operations, reports, staff oversight |
| **Coach** | Trainee groups, attendance, session management |
| **HR** | Employee management, recruitment |
| **Accountant** | Financial reports, payment tracking |
| **IT** | System configuration, integrations |
| **Trainee** | Self-service profile, view schedules, attendance |

---

## API Overview

### Authentication & Users
- `POST /api/auth/sign-up` - User registration
- `POST /api/auth/login` - User login (returns JWT)
- `GET /api/auth/roles` - Get available roles
- `GET /api/user/me` - Current user profile

### Trainees
- `GET /api/trainee` - List trainees (paginated)
- `GET /api/trainee/search` - Search trainees
- `GET /api/trainee/{id}` - Get trainee details
- `POST /api/trainee` - Create trainee
- `PUT /api/trainee` - Update trainee
- `DELETE /api/trainee/{id}` - Delete trainee

### Employees & Coaches
- `GET /api/employee` - List employees (paginated)
- `GET /api/employee/search` - Search employees
- `GET /api/employee/active/coaches` - Get active coaches
- `POST /api/employee` - Create employee
- `PUT /api/employee` - Update employee

### Enrollments
- `GET /api/enrollment` - List enrollments
- `POST /api/enrollment` - Create enrollment
- `PUT /api/enrollment` - Update enrollment

### Attendance
- `GET /api/attendance` - List attendance records
- `GET /api/attendance/rate` - Get attendance rate
- `POST /api/attendance` - Record attendance

### Sessions & Groups
- `GET /api/sessionoccurrence` - List session occurrences
- `GET /api/traineegroup` - List trainee groups
- `POST /api/traineegroup` - Create group
- `PUT /api/traineegroup` - Update group

### Sports & Branches
- `GET /api/sport` - List sports
- `GET /api/branch` - List branches
- `GET /api/sportprice` - Get pricing by sport/branch

### Subscriptions & Payments
- `GET /api/subscriptiondetails` - List subscription details
- `POST /api/subscriptiondetails` - Create subscription
- `GET /api/payment` - List payments

### AI Features
- `POST /api/chatbot/message` - Send message to AI chatbot
- `POST /api/videoanalysis/analyze` - Analyze video for pose/movement

### Notifications
- `GET /api/notification` - Get user notifications
- SignalR Hub: `/hubs/notification` - Real-time notifications

> **Full API Documentation**: Visit `/swagger` when running in Development mode

---

## Multi-Tenancy Architecture

The system is built on a **Branch-based architecture** where each branch operates independently with its own:
- Trainees and employees
- Sports and pricing
- Schedules and sessions

This design supports future **multi-tenant isolation** for SaaS deployment, enabling different organizations to host their data separately while using the same platform.

---

## Technology Stack

| Layer | Technology |
|-------|------------|
| **Runtime** | .NET 9.0 |
| **Framework** | ASP.NET Core Web API |
| **ORM** | Entity Framework Core 9 |
| **Database** | Microsoft SQL Server |
| **Authentication** | JWT Bearer Tokens |
| **Real-time** | SignalR |
| **AI Integration** | OpenAI GPT-4 / OpenRouter |
| **Architecture** | Clean Architecture + CQRS + MediatR |
| **Testing** | xUnit, FluentAssertions, Moq |

---

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (local or container)
- Visual Studio 2022 or VS Code

### Build & Run

```powershell
# Build the entire solution
dotnet build SportAcademy.sln

# Run the Web API
dotnet run --project SportAcademy.Web/SportAcademy.Web.csproj
```

> Swagger UI is available at the default launch URL in Development mode

### Run Tests

```powershell
dotnet test SportAcademy.Tests/SportAcademy.Tests.csproj
```

### Database Migrations

```powershell
# Add migration
dotnet ef migrations add <MigrationName> --project SportAcademy.Infrastructure --startup-project SportAcademy.Web

# Update database
dotnet ef database update --project SportAcademy.Infrastructure --startup-project SportAcademy.Web
```

---

## Configuration

Required settings in `appsettings.json`:

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` | JWT authentication config |
| `OpenAiSettings:ApiKey` | OpenAI API key for ChatBot |

---

## Roadmap

| Priority | Feature | Description |
|----------|---------|-------------|
| **High** | Multi-Tenancy Support | Add tenant isolation at Branch level for SaaS |
| **High** | Financial Reports | Revenue dashboards, branch performance analytics |
| **Medium** | Payment Gateway Integration | Stripe/PayTabs integration for online payments |
| **Medium** | Mobile Apps | React Native / Flutter companion apps |
| **Low** | Advanced AI Features | Performance prediction, injury prevention alerts |

---

## Project Structure

```
SportAcademy/
├── SportAcademy.Domain/         # Entities, Enums, Value Objects, Domain Services
├── SportAcademy.Application/    # CQRS Commands/Queries, Validators, DTOs
├── SportAcademy.Infrastructure/ # Repositories, External Services, DB Context
├── SportAcademy.Web/            # Controllers, Middleware, Configuration
└── SportAcademy.Tests/          # Unit & Integration Tests
```

---

## License

MIT License