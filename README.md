# Bookify Hotel Reservation System

A comprehensive hotel booking platform built with ASP.NET Core MVC, following N-Tier Architecture, Repository Pattern, and Unit of Work design patterns.

## Features

### Week 1: Architecture Setup and Room Listings
- ✅ N-Tier Architecture (Presentation, Business Logic, Data Access layers)
- ✅ Entity Framework Core with SQL Server
- ✅ Repository Pattern and Unit of Work implementation
- ✅ ASP.NET Identity for user authentication and role management
- ✅ Public room listing page with search functionality

### Week 2: Reservation Flow, Roles, and Admin Panel
- ✅ Session-based reservation cart
- ✅ Role-based access control (Admin, Customer)
- ✅ Admin dashboard with DataTables integration
- ✅ Room and booking management interfaces

### Week 3: Booking Confirmation and Stripe Integration
- ✅ Booking confirmation with transaction support (Unit of Work)
- ✅ Stripe payment gateway integration
- ✅ User profile and booking history pages

### Week 4: Health Checks, Logging, and Final Polish
- ✅ ASP.NET Core Health Checks endpoint (`/health`)
- ✅ Serilog structured logging
- ✅ Enhanced UI with jQuery, DataTables, and Toaster JS
- ✅ Responsive Bootstrap design

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server (LocalDB)
- **ORM**: Entity Framework Core 8.0
- **Authentication**: ASP.NET Identity
- **Payment**: Stripe.net
- **Logging**: Serilog
- **Frontend**: Bootstrap 5, jQuery, DataTables, Toaster JS

## Project Structure

```
Bookify/
├── Bookify.Data/              # Data Access Layer
│   ├── Models/                # Entity models
│   ├── Repositories/          # Repository pattern implementation
│   ├── UnitOfWork/            # Unit of Work pattern
│   └── ApplicationDbContext.cs
├── Bookify.Services/          # Business Logic Layer
│   ├── Interfaces/            # Service interfaces
│   └── Services/              # Service implementations
└── Bookify.Web/               # Presentation Layer
    ├── Controllers/           # MVC Controllers
    ├── Views/                 # Razor views
    ├── Models/                # View models
    └── wwwroot/               # Static files
```

## Prerequisites

- .NET 8.0 SDK or later
- SQL Server (LocalDB is used by default)
- Visual Studio 2022 or VS Code (optional)

## Setup Instructions

### 1. Clone or Download the Project

Navigate to the project directory:
```bash
cd C:\Users\FABRIKA\Downloads\DEPPI
```

### 2. Configure Database Connection

Update the connection string in `Bookify.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BookifyDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Configure Stripe (Optional)

If you want to use Stripe payments, update the Stripe keys in `appsettings.json`:

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_your_publishable_key_here",
    "SecretKey": "sk_test_your_secret_key_here"
  }
}
```

**Note**: If Stripe keys are not configured, bookings will be confirmed manually without payment processing.

### 4. Apply Database Migrations

The application will automatically create the database and apply migrations on first run. Alternatively, you can run:

```bash
cd Bookify.Web
dotnet ef database update --project ../Bookify.Data/Bookify.Data.csproj
```

### 5. Run the Application

```bash
cd Bookify.Web
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.

## Default Data

On first run, the application will seed:
- **Roles**: Admin, Customer
- **Room Types**: Standard Room ($99.99/night), Deluxe Room ($149.99/night), Suite ($249.99/night)
- **Rooms**: 6 sample rooms (2 of each type)

## Creating an Admin User

To create an admin user, you can:

1. Register a new account through the web interface
2. Use the Package Manager Console or SQL to assign the "Admin" role:

```sql
-- Find the user ID
SELECT Id, Email FROM AspNetUsers WHERE Email = 'your-email@example.com';

-- Assign Admin role (replace UserId and RoleId with actual values)
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.Email = 'your-email@example.com' AND r.Name = 'Admin';
```

Or use the following PowerShell script in Package Manager Console:

```powershell
# In Package Manager Console (after running the app once)
$userManager = Get-Service UserManager<ApplicationUser>
$user = await $userManager.FindByEmailAsync("admin@bookify.com")
await $userManager.AddToRoleAsync($user, "Admin")
```

## Key Features

### For Customers
- Browse available rooms with date filtering
- View room details and pricing
- Add rooms to cart
- Complete booking with Stripe payment
- View booking history
- Cancel bookings

### For Administrators
- Access admin dashboard
- View all rooms and room types
- Manage bookings
- View all customer bookings with DataTables

## API Endpoints

- `/health` - Health check endpoint
- `/Rooms` - Browse available rooms
- `/Cart` - Shopping cart
- `/Bookings` - User bookings
- `/Admin` - Admin dashboard (requires Admin role)

## Logging

Logs are written to:
- Console (during development)
- File: `logs/bookify-YYYYMMDD.txt`

## Health Checks

The application includes a health check endpoint at `/health` that verifies database connectivity.

## Design Patterns Used

1. **N-Tier Architecture**: Clear separation between Presentation, Business Logic, and Data Access layers
2. **Repository Pattern**: Abstracts data access operations
3. **Unit of Work Pattern**: Ensures transactional consistency
4. **Dependency Injection**: Loose coupling between components

## Testing

To test the application:

1. Register a new user account
2. Browse rooms and select check-in/check-out dates
3. Add a room to cart
4. Complete the booking (Stripe test mode or manual confirmation)
5. View your bookings

For admin features:
1. Create an admin user (see instructions above)
2. Login as admin
3. Access the admin dashboard
4. Manage rooms and view all bookings

## Troubleshooting

### Database Connection Issues
- Ensure SQL Server LocalDB is installed
- Check the connection string in `appsettings.json`
- Verify the database server is running

### Migration Issues
- Delete the `Migrations` folder in `Bookify.Data` and recreate migrations
- Ensure Entity Framework tools are installed: `dotnet tool install --global dotnet-ef`

### Stripe Payment Issues
- Verify Stripe keys are correctly configured
- Use test mode keys for development
- Check Stripe dashboard for payment logs

## Future Enhancements

- Email notifications for bookings
- Room availability calendar
- Advanced search and filtering
- Customer reviews and ratings
- Multi-room booking support
- Admin CRUD operations for rooms and room types
- Booking modification functionality
- Refund processing

## License

This project is created for educational purposes.

## Support

For issues or questions, please check the code comments or refer to the ASP.NET Core documentation.

