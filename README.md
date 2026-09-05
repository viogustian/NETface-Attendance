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
