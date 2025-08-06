# Stock Management System

A comprehensive ASP.NET Core MVC application for managing stock inventory with role-based access control, real-time dashboard, and activity logging.

## Features

### 🔐 Authentication & Authorization
- **User Registration & Login**: Secure authentication with email and password
- **Role-Based Access Control**: Admin and Staff roles with different permissions
- **Password Security**: Hashed passwords with validation requirements
- **Session Management**: Secure session handling with remember me functionality

### 📦 Stock Management
- **CRUD Operations**: Create, Read, Update, Delete stock items
- **Category Management**: Organize items by categories (Furniture, Electronics, Goods, Technology)
- **Search & Filter**: Search by name, supplier, or category with advanced filtering
- **Pagination**: Efficient data display with pagination
- **Sorting**: Sort by name, date, quantity, or price

### 📊 Admin Dashboard
- **Summary Statistics**: Total items, low stock alerts, total value
- **Category Analytics**: Items per category with total values
- **Recent Activities**: Latest system activities and user actions
- **Low Stock Alerts**: Items with quantity < 5 highlighted
- **Quick Actions**: Direct access to common operations

### 👥 User Management (Admin Only)
- **User List**: View all registered users with roles
- **Role Management**: Change user roles (Admin/Staff)
- **User Status**: Activate/deactivate users
- **Activity Logs**: Track user actions and system events

### 🔍 Advanced Features
- **Activity Logging**: Comprehensive audit trail of all actions
- **Responsive Design**: Modern UI with Bootstrap 5
- **Real-time Alerts**: Low stock notifications
- **Data Validation**: Client and server-side validation
- **Error Handling**: Graceful error handling and user feedback

## Technology Stack

- **Framework**: ASP.NET Core 9.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **UI Framework**: Bootstrap 5 with Bootstrap Icons
- **Language**: C# with Razor Views

## Prerequisites

- .NET 9.0 SDK
- SQL Server (LocalDB or SQL Server Express)
- Visual Studio 2022 or VS Code

## Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd StockManagementSystem
```

### 2. Database Setup
The application uses LocalDB by default. If you have SQL Server installed, update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StockManagementSystem;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Install Dependencies
```bash
dotnet restore
```

### 4. Run the Application
```bash
dotnet run
```

The application will automatically:
- Create the database if it doesn't exist
- Seed initial data (categories and default admin user)
- Start the web server

### 5. Access the Application
Navigate to `https://localhost:5001` or `http://localhost:5000`

## Default Credentials

The system creates a default admin user on first run:

- **Email**: admin@stockmanagement.com
- **Password**: Admin123!

## User Roles & Permissions

### Admin Role
- Full access to all features
- Manage stock items (Create, Read, Update, Delete)
- Manage categories
- Manage users
- View dashboard and analytics
- View activity logs

### Staff Role
- View stock items
- Add new stock items
- Search and filter stock
- Limited access to system features

## Database Schema

### Core Tables
- **AspNetUsers**: Extended user table with custom fields
- **Categories**: Stock item categories
- **StockItems**: Main stock inventory table
- **ActivityLogs**: System activity tracking

### Relationships
- StockItems → Categories (Many-to-One)
- StockItems → Users (Created/Updated by)
- ActivityLogs → Users (Action performed by)

## API Endpoints

### Authentication
- `GET /Account/Login` - Login page
- `POST /Account/Login` - Authenticate user
- `GET /Account/Register` - Registration page
- `POST /Account/Register` - Create new user
- `POST /Account/Logout` - Logout user

### Stock Management
- `GET /Stock` - List all stock items
- `GET /Stock/Create` - Create new item form
- `POST /Stock/Create` - Save new item
- `GET /Stock/Edit/{id}` - Edit item form
- `POST /Stock/Edit/{id}` - Update item
- `GET /Stock/Delete/{id}` - Delete confirmation
- `POST /Stock/Delete/{id}` - Delete item
- `GET /Stock/Details/{id}` - View item details

### Admin Features
- `GET /Dashboard` - Admin dashboard
- `GET /Dashboard/ActivityLogs` - Activity logs
- `GET /Category` - Manage categories
- `GET /User` - Manage users

## Configuration

### Connection String
Update the connection string in `appsettings.json` for your database:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=StockManagementSystem;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Identity Configuration
Password requirements and lockout settings can be modified in `Program.cs`:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
```

## Deployment

### Local Development
```bash
dotnet run
```

### Production Deployment
1. Update connection string for production database
2. Configure HTTPS certificates
3. Set environment variables for production settings
4. Use `dotnet publish` to create deployment package

## Security Features

- **Password Hashing**: Secure password storage using ASP.NET Core Identity
- **CSRF Protection**: Anti-forgery tokens on all forms
- **Input Validation**: Client and server-side validation
- **Role-Based Authorization**: Secure access control
- **Activity Logging**: Audit trail for security monitoring

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.

## Support

For support and questions, please create an issue in the repository or contact the development team.

---

**Note**: This is a demonstration application. For production use, ensure proper security measures, database backups, and regular updates are implemented. 