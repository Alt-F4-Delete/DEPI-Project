# Quick Start Guide - Bookify Hotel Reservation System

## Getting Started in 5 Minutes

### Step 1: Navigate to Project Directory
```bash
cd C:\Users\FABRIKA\Downloads\DEPPI
```

### Step 2: Restore Packages (if needed)
```bash
dotnet restore
```

### Step 3: Update Database Connection (if needed)
Edit `Bookify.Web/appsettings.json` and verify the connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BookifyDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### Step 4: Run the Application
```bash
cd Bookify.Web
dotnet run
```

The application will:
- Automatically create the database
- Apply migrations
- Seed initial data (room types and rooms)
- Create user roles (Admin, Customer)

### Step 5: Access the Application
Open your browser and navigate to:
- `https://localhost:5001` or `http://localhost:5000`

### Step 6: Create Your First Account
1. Click "Register" in the navigation
2. Fill in your details
3. You'll be automatically assigned the "Customer" role

### Step 7: Create an Admin User (Optional)
After registering, you can manually assign the Admin role using SQL:

```sql
-- Connect to BookifyDb database
USE BookifyDb;

-- Find your user
SELECT Id, Email FROM AspNetUsers WHERE Email = 'your-email@example.com';

-- Assign Admin role (replace UserId and RoleId with actual values from above)
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.Email = 'your-email@example.com' AND r.Name = 'Admin';
```

## Testing the Application

### As a Customer:
1. **Browse Rooms**: Go to "Rooms" in the navigation
2. **Search**: Select check-in and check-out dates, optionally filter by room type
3. **View Details**: Click "View Details" on any room
4. **Add to Cart**: Click "Add to Cart" (requires login)
5. **Checkout**: Go to Cart and click "Proceed to Checkout"
6. **Complete Booking**: 
   - If Stripe is configured: Complete payment via Stripe
   - If not: Booking will be confirmed automatically
7. **View Bookings**: Go to "My Bookings" to see your reservations

### As an Admin:
1. **Login** with an admin account
2. **Access Admin Panel**: Click "Admin" dropdown in navigation
3. **Manage Rooms**: View all rooms with DataTables
4. **Manage Room Types**: View all room types
5. **View All Bookings**: See all customer bookings

## Key Features to Test

✅ **Room Search**: Filter by dates and room type  
✅ **Shopping Cart**: Session-based cart functionality  
✅ **Booking Flow**: Complete booking with transaction support  
✅ **Stripe Integration**: Payment processing (if configured)  
✅ **Admin Dashboard**: Role-based access control  
✅ **DataTables**: Sortable, searchable tables in admin panel  
✅ **Health Check**: Visit `/health` to check application status  
✅ **Logging**: Check `logs/` folder for application logs  

## Stripe Configuration (Optional)

To enable Stripe payments:

1. Get test keys from [Stripe Dashboard](https://dashboard.stripe.com/test/apikeys)
2. Update `Bookify.Web/appsettings.json`:
```json
{
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_..."
  }
}
```

**Note**: Without Stripe keys, bookings will still work but will be confirmed manually.

## Troubleshooting

### Database Issues
- Ensure SQL Server LocalDB is installed
- Check connection string in `appsettings.json`
- Try deleting the database and letting the app recreate it

### Build Errors
- Run `dotnet clean` then `dotnet build`
- Ensure .NET 8.0 SDK is installed

### Migration Issues
- Delete `Bookify.Data/Migrations` folder
- Run: `dotnet ef migrations add InitialCreate --project Bookify.Data/Bookify.Data.csproj`
- Run: `dotnet ef database update --project Bookify.Data/Bookify.Data.csproj`

## Next Steps

- Explore the codebase to understand the architecture
- Customize room types and pricing
- Add more features as needed
- Deploy to production (update connection strings and Stripe keys)

## Project Structure Overview

```
Bookify/
├── Bookify.Data/          # Data Access Layer (Repositories, Unit of Work)
├── Bookify.Services/      # Business Logic Layer (Services)
└── Bookify.Web/           # Presentation Layer (Controllers, Views)
```

Enjoy building with Bookify! 🏨

