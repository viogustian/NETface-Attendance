# NETFace Attendance System

A modern employee attendance system with face recognition capabilities, built with a .NET Core Backend and a React + Vite Frontend.

## Features
- **Face Recognition Attendance:** Clock in and clock out seamlessly using facial recognition embeddings.
- **Admin Dashboard:** Manage employees, view active sessions, and monitor attendance metrics in real-time.
- **Session Management:** Create, track, and manage attendance sessions by department and date.
- **CSV Export:** Download attendance records for external processing and payroll systems.

## Technology Stack
### Backend
- ASP.NET Core 8 Web API
- Entity Framework Core
- PostgreSQL Database
- Clean Architecture (Api, Application, Domain, Infrastructure layers)

### Frontend
- React 18
- Vite
- Vanilla CSS for styling
- Lucide React (Icons)

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18+)
- [PostgreSQL](https://www.postgresql.org/) (v14+)

## Getting Started

### 1. Database Setup
Ensure PostgreSQL is running and update the connection string in `src/backend/NETFace.Attendance.Api/appsettings.json` (or via .NET user secrets). Then apply the Entity Framework Core migrations to initialize the database:
```bash
cd src/backend
dotnet ef database update --project NETFace.Attendance.Infrastructure --startup-project NETFace.Attendance.Api
```

### 2. Running the Backend
Start the ASP.NET Core Web API:
```bash
cd src/backend/NETFace.Attendance.Api
dotnet run
```
The API will be available at `http://localhost:5000` (or the port specified in your launch settings).

### 3. Running the Frontend
Start the Vite development server for the React application:
```bash
cd src/frontend/netface-attendance-web
npm install
npm run dev
```
The web application will be accessible at `http://localhost:5173`.

## Architecture & Structure
The backend follows Clean Architecture principles:
- `Api`: RESTful endpoints, controllers, and dependency injection setup.
- `Application`: Business logic, services, and interfaces.
- `Domain`: Core entities, enums, and domain exceptions.
- `Infrastructure`: Data access (EF Core), database migrations, and external service adapters.

The frontend is a single-page application (SPA) focused on providing an intuitive and modern administrative experience.

```text
src/
  backend/
    NETFace.Attendance.Api/
    NETFace.Attendance.Application/
    NETFace.Attendance.Domain/
    NETFace.Attendance.Infrastructure/
  frontend/
    netface-attendance-web/
tests/
  NETFace.Attendance.Api.Tests/
docs/
  adr/
```

## Contributing

We welcome contributions from the community! To ensure a smooth collaboration process, please follow these guidelines.

### Getting Started

Before you start contributing, you **MUST** fork and star this repository! ⭐

1. **Star the repository** to show your support.
2. **Fork the repository** to your own GitHub account.
3. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/NETface-Attendance.git
   ```
4. Add the original repository as an upstream remote:
   ```bash
   git remote add upstream https://github.com/viogustian/NETface-Attendance.git
   ```

### Development Workflow

Please follow this standard GitHub workflow for all contributions:

1. **Create a branch** for your feature or bug fix:
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-bug-fix
   ```
2. **Implement your changes**. Be sure to follow the existing code style.
3. **Run tests locally**. All existing tests must pass before you submit your changes.
4. **Push your branch** to your forked repository.
5. **Open a Pull Request (PR)** against the `main` branch of the original repository.

### Branch Naming Convention

- `feature/...` for new features or enhancements.
- `fix/...` for bug fixes.
- `docs/...` for documentation updates.
- `test/...` for adding or fixing tests.

### Testing Requirements

**CRITICAL:** You must run all relevant tests locally **BEFORE** opening a Pull Request.
If your PR introduces new features or fixes a bug, please include tests that verify your changes.

Local verification flow:
`Code` → `Test Locally` → `Pass` → `Push Branch` → `Open PR`

### Pull Request Requirements

When opening a Pull Request, please ensure:
- Your PR description clearly explains the changes and references any related issue(s) (e.g., `Fixes #123`).
- You do not include unrelated changes in your PR.
- All required Continuous Integration (CI) checks pass. (If CI fails, please fix the issues and update your PR).

### Good First Issues & Help Wanted

If you are new to the project, look for issues labeled `good first issue` or `help wanted`. These are specifically curated to be approachable and have clear scopes. If you need clarification on any requirements, please feel free to ask in the issue comments before starting work!
