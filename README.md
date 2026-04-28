# PredictiveGuard: Industrial Maintenance & Asset Monitoring

PredictiveGuard is a high-performance industrial asset management platform designed for real-time telemetry monitoring and predictive maintenance. Built with **Blazor Server** and **ASP.NET Core**, it provides a premium OLED-dark dashboard for monitoring equipment health across multiple industrial sectors.

## 🚀 Features

- **OLED-Dark UI**: A high-fidelity, high-contrast interface designed for professional monitoring environments.
- **Real-Time Telemetry**: Live tracking of Temperature, Vibration, and Load across all industrial assets.
- **Predictive Analytics**: Integrated sensor data simulation for testing RUL (Remaining Useful Life) algorithms.
- **Asset Management**: Full CRUD operations for industrial equipment with interactive creation modals.
- **Admin Console**: Escalation management for health alerts and system-wide monitoring.
- **Google Authentication**: Secure access control for authorized personnel.

## 🛠️ Technology Stack

- **Frontend**: Blazor Server (.NET 8/10) with Vanilla CSS and Bootstrap Icons.
- **Backend**: ASP.NET Core Web API with Entity Framework Core.
- **Database**: SQLite (OLED-optimized local data storage).
- **Security**: ASP.NET Core Antiforgery and Google OAuth.
- **Graphics**: Three.js for generative background effects.

## 📂 Project Structure

- `PredictiveGuard.Web`: The Blazor Server web application (UI & Dashboard).
- `PredictiveGuard.API`: The backend service providing telemetry and asset data.
- `PredictiveGuard.Data`: Shared project containing database models and EF Core context.

## ⚙️ Getting Started

### Prerequisites
- .NET 8.0 or later
- Visual Studio 2022 or VS Code

### Setup & Execution
1. **Clone the repository**:
   ```bash
   git clone https://github.com/abdullaah17/PredictiveGuard.git
   cd PredictiveGuard
   ```

2. **Run the API Backend**:
   ```bash
   cd PredictiveGuard.API
   dotnet run
   ```

3. **Run the Web Dashboard**:
   ```bash
   cd ../PredictiveGuard.Web
   dotnet run
   ```

4. **Access the Dashboard**:
   Navigate to `https://localhost:7001` in your browser.

## 🔒 Security Notes
For local development, the application uses self-signed certificates. If you encounter a browser warning, ensure you trust the .NET dev-certs:
```bash
dotnet dev-certs https --trust
```

---
*PredictiveGuard - Smarter Maintenance, Reduced Downtime.*
