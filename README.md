# Predictive Guard - Industrial Equipment Health Monitoring

A real-time predictive maintenance system that monitors industrial equipment health and predicts failures before they occur.

## 📋 Project Overview

Predictive Guard is a multi-tier application designed to monitor the health of critical industrial equipment (wind turbines, transformers, substations) using real-time sensor data. The system automatically detects anomalies, generates maintenance alerts, and provides maintenance leads with an intuitive control center to manage repairs.

### Key Features

- **Real-Time Sensor Monitoring**: Continuous ingestion of temperature, vibration, and load data
- **Predictive Alerts**: Threshold-based and trend analysis alerts for equipment health
- **Maintenance Ticket Management**: Escalate, assign, and track maintenance tasks
- **Admin Control Center**: Comprehensive dashboard for maintenance leads
- **Offline Data Caching**: Last 48 hours of sensor history cached locally for remote sites
- **Role-Based Access**: Engineer, Technician, and Lead roles with different permissions
- **Concurrency Handling**: Optimistic concurrency control for simultaneous updates

---

## 🏗️ Architecture

### System Components
┌─────────────────────────────────────────────────────────────────┐
│                    Predictive Guard System                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────┐      ┌──────────────────┐                │
│  │  Blazor Web App  │      │   Windows Forms   │                │
│  │   (User UI)      │      │  (Admin Desktop)  │                │
│  └────────┬─────────┘      └────────┬─────────┘                │
│           │                         │                            │
│           └────────────┬────────────┘                            │
│                        │                                         │
│           ┌────────────▼────────────┐                           │
│           │   ASP.NET Core API      │                           │
│           │  (Ingestion Engine)     │                           │
│           └────────────┬────────────┘                           │
│                        │                                         │
│           ┌────────────▼────────────┐                           │
│           │   SQLite Database       │                           │
│           │  (Time-Series Storage)  │                           │
│           └─────────────────────────┘                           │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

### Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Frontend** | Blazor Server, Bootstrap 5 | User interface & admin dashboard |
| **Backend** | ASP.NET Core Web API | REST API for data ingestion & management |
| **Database** | SQLite (dev), SQL Server (prod) | Time-series sensor data + metadata |
| **Authentication** | Google OAuth 2.0 | Secure user login |
| **ORM** | Entity Framework Core | Database abstraction & queries |

### Data Flow

1. **Sensor Ingestion** → API receives temperature, vibration, load data
2. **Alert Logic** → Evaluates thresholds + trends, creates maintenance tickets
3. **UI Visualization** → Engineers view asset health in real-time
4. **Ticket Escalation** → Maintenance lead assigns & tracks repairs
5. **Offline Sync** → Admin caches last 48 hours for remote access

---

## 🗄️ Database Schema

### Core Entities
Users
├── GoogleId (unique)
├── Email
├── FullName
└── CreatedAt
Assets (Operational Units)
├── Name (e.g., "Wind Turbine #1")
├── Type (e.g., "Wind Turbine")
├── Location
├── IsActive
└── CreatedAt
AssetTeamMembers (Group Membership)
├── AssetId (FK)
├── UserId (FK)
├── Role ("Engineer", "Technician", "Lead")
└── JoinedAt
SensorReadings (Time-Series Data)
├── AssetId (FK)
├── Timestamp
├── Temperature (°C)
├── Vibration (m/s²)
├── Load (%)
└── [Indexed on AssetId + Timestamp for query performance]
MaintenanceTickets (Work Orders)
├── AssetId (FK)
├── AssignedToUserId (FK, nullable)
├── Status ("Reported", "Assigned", "In Progress", "Completed")
├── AlertType ("Temperature", "Vibration", "Trend")
├── Description
├── CreatedAt
├── CompletedAt
└── Version (Concurrency token)

---

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Google OAuth credentials (from Google Cloud Console)
- Visual Studio Code or Visual Studio 2022

### Setup Instructions

#### 1. Clone & Navigate

```bash
git clone https://github.com/yourusername/PredictiveGuard.git
cd PredictiveGuard
```

#### 2. Configure Google OAuth

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project
3. Enable Google+ API
4. Create OAuth 2.0 credentials (Web application)
5. Add authorized redirect URI: `https://localhost:7001/signin-google`
6. Copy **Client ID** and **Client Secret**

#### 3. Update Configuration

Open `PredictiveGuard.Web/appsettings.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_CLIENT_SECRET"
    }
  },
  "ApiBaseUrl": "https://localhost:5001"
}
```

#### 4. Run Database Migration

```bash
cd PredictiveGuard.API
dotnet ef database update
cd ..
```

#### 5. Start the Application

**Terminal 1 - API Server:**
```bash
cd PredictiveGuard.API
dotnet run
```
API runs on `https://localhost:5001`

**Terminal 2 - Blazor Web App:**
```bash
cd PredictiveGuard.Web
dotnet run
```
Web app runs on `https://localhost:7001`

#### 6. Access the Application

- **User Dashboard**: `https://localhost:7001`
- **Admin Control Center**: `https://localhost:7001/admin`
- **Offline Cache**: `https://localhost:7001/admin/offline-cache`
- **Swagger API Docs**: `https://localhost:5001/swagger`

---

## 📊 Alert Logic

The system generates maintenance tickets based on two mechanisms:

### 1. Threshold Breach (Immediate Alert)

- **Temperature > 80°C** → Creates "Temperature" alert
- **Vibration > 5.0 m/s²** → Creates "Vibration" alert

### 2. Trend Analysis (Predictive Alert)

- **Temperature rising > 2°C/hour** → Creates "Trend" alert
- Analyzes last 10 sensor readings to detect upward trend
- Predicts failure window based on rate of change

### Ticket Lifecycle
Reported → Assigned → In Progress → Completed
↓         ↓           ↓             ↓
Auto-     Lead       Engineer      Lead
created   assigns    works on      closes

---

## 🔐 Role-Based Access Control

| Role | Permissions |
|------|------------|
| **Engineer** | View asset health, acknowledge alerts, view sensor data |
| **Technician** | View sensor history (read-only), cannot modify tickets |
| **Lead** | Full admin access, assign tickets, escalate, view all data |

---

## 🛠️ API Endpoints

### Assets

- `GET /api/asset` - List all assets
- `GET /api/asset/{id}` - Get asset details
- `POST /api/asset` - Create new asset

### Sensor Readings

- `GET /api/sensorreading/{assetId}?hours=24` - Get readings for asset
- `POST /api/sensorreading` - Ingest single reading
- `POST /api/sensorreading/batch` - Bulk ingest readings

### Maintenance Tickets

- `GET /api/maintenanceticket` - List all tickets (filter by status)
- `GET /api/maintenanceticket/{id}` - Get ticket details
- `PATCH /api/maintenanceticket/{id}/assign` - Assign ticket to engineer
- `PATCH /api/maintenanceticket/{id}/status` - Update ticket status

### Users

- `GET /api/user` - List all users
- `POST /api/user` - Create user (from Google Auth)
- `GET /api/user/{id}` - Get user details

### Team Members

- `GET /api/assetteammember/{assetId}` - Get team members for asset
- `POST /api/assetteammember` - Add user to asset team

---

## 📱 Frontend Pages

### User Pages

- **Dashboard** (`/`) - Overview of all assets, quick access to asset details
- **Asset Details** (`/asset/{id}`) - Sensor data, maintenance tickets, team members for single asset

### Admin Pages

- **Control Center** (`/admin`) - Health dashboard, alert escalation, user management
- **Offline Cache** (`/admin/offline-cache`) - Manage cached sensor data for offline access

---

## 🔄 Concurrency Control

The system implements **Optimistic Concurrency** for maintenance tickets:

- Each `MaintenanceTicket` has a `Version` property
- When updating a ticket, the version is checked
- If ticket was modified by another user → **409 Conflict** response
- This prevents lost updates in high-concurrency scenarios

Example error response:
```json
{
  "error": "Ticket was modified by another user. Please refresh and try again."
}
```

---

## 💾 Offline Data Cache

The admin can cache the last 48 hours of sensor data locally for remote sites:

1. Admin navigates to `/admin/offline-cache`
2. Clicks "Cache Last 48 Hours"
3. System downloads all sensor data for all assets
4. Data stored in browser (IndexedDB / localStorage)
5. Even without internet, technician can view historical data

**Use Case**: Technician walks into remote wind farm, opens dashboard offline, reviews sensor history from last 2 days to diagnose issues.

---

## 🧪 Testing the System

### Seed Sample Data

On first API startup, the system seeds:
- 2 sample users (Alice Engineer, Bob Technician)
- 1 wind turbine asset
- 24 hours of synthetic sensor readings

### Generate Alerts

Use Swagger to inject sensor data that triggers alerts:

```bash
curl -X POST https://localhost:5001/api/sensorreading \
  -H "Content-Type: application/json" \
  -d '{
    "assetId": 1,
    "temperature": 85,
    "vibration": 6.0,
    "load": 75
  }'
```

This creates:
- Temperature alert (85 > 80)
- Vibration alert (6.0 > 5.0)

View in Admin Dashboard `/admin` → "Alert Escalation" tab.

---

## 📈 Performance Considerations

### Database Indexing

- `SensorReadings`: Composite index on `(AssetId, Timestamp)` for time-range queries
- `Users`: Unique index on `GoogleId` and `Email`
- `MaintenanceTickets`: Index on `Status` for filtering open tickets

### API Optimization

- Sensor endpoint limits to last 100 readings per asset (pagination TBD)
- Team members lazy-loaded when viewing asset details
- Batch endpoint for bulk sensor ingestion

### Future Improvements

- Implement pagination for sensor readings
- Add caching layer (Redis) for frequently accessed assets
- Compress historical data older than 90 days
- Time-series database migration (InfluxDB, TimescaleDB) for scale

---

## 🐛 Troubleshooting

### Database Connection Issues

**Error**: `SqliteConnection: unable to open the database file`

**Solution**: Ensure `PredictiveGuard.db` is in `PredictiveGuard.API/` folder after migration:
```bash
cd PredictiveGuard.API
dotnet ef database update
```

### Google OAuth Redirect Error

**Error**: `The redirect URI in the request does not match the registered redirect URIs`

**Solution**: Add exact URI to Google Cloud Console:
- Authorized redirect URI: `https://localhost:7001/signin-google`

### API CORS Errors

**Error**: `Access to XMLHttpRequest blocked by CORS policy`

**Solution**: Ensure `appsettings.json` has correct `ApiBaseUrl`:
```json
"ApiBaseUrl": "https://localhost:5001"
```

---

## 📝 Commit History

Key commits demonstrate understanding of system architecture:
Initial project structure setup
Add database models and schema
Switch to SQLite for Mac development
Add API endpoints (assets, sensors, tickets, users)
Link projects and create solution
Add Blazor Web app with Google Auth and dashboard
Add admin dashboard with alert escalation and offline cache
Polish: Add error handling, validation, and documentation

---

## 🎓 Learning Outcomes

By completing this project, you've demonstrated:

1. **Database Design** - Time-series schema with proper indexing
2. **REST API Development** - Full CRUD operations with validation
3. **Frontend Development** - Responsive Blazor UI with real-time updates
4. **Authentication** - OAuth 2.0 integration for secure login
5. **Business Logic** - Predictive alert logic (threshold + trend analysis)
6. **Concurrency Control** - Optimistic locking for multi-user scenarios
7. **System Architecture** - Service-oriented design with clear separation of concerns
8. **Offline Capabilities** - Local data caching for remote access

---

## 📜 License

MIT License - See LICENSE file for details

---

## 👨‍💻 Author

[Your Name] - Semester Project 2024

---

## 🔗 References

- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/)
- [Google OAuth 2.0](https://developers.google.com/identity/protocols/oauth2)
