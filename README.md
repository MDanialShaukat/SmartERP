# SmartERP - Business Management System

## Phase 1: Setup - COMPLETED ?

### Database Schema
The following database entities have been created:

1. **Users** - Authentication and user management
   - Fields: Id, Username, PasswordHash, Role (Admin/User), FullName, Email, PhoneNumber, IsActive, CreatedDate, LastLoginDate
   - Default Admin: username: `admin`, password: `admin123`
   - Default Employee: username: `employee`, password: `employee123`

2. **Inventory** - Inventory management
   - Fields: Id, ItemName, Description, Category, Unit, PurchasePrice, QuantityPurchased, QuantityUsed, QuantityRemaining, TotalPurchaseAmount, Supplier, PurchaseDate, CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy, Notes

3. **Customers** - Customer management
   - Fields: Id, CustomerName, CustomerCode, PhoneNumber, Email, Address, City, PinCode, PackageType, PackageAmount, ConnectionType, ConnectionDate, IsActive, AdditionalDetails, OutstandingBalance, CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy, Notes

4. **Billing** - Billing management
   - Fields: Id, BillNumber, CustomerId, BillingMonth, BillingYear, BillAmount, PreviousDue, TotalAmount, AmountPaid, BalanceAmount, PaymentStatus, BillDate, DueDate, PaymentDate, PaymentMethod, TransactionReference, Notes, CreatedDate, LastModifiedDate, CreatedBy, LastModifiedBy

### Architecture

#### Technology Stack
- **Framework**: .NET 10, WPF (Windows Presentation Foundation)
- **Database**: SQLite (local database)
- **ORM**: Entity Framework Core 9.0
- **Authentication**: BCrypt for password hashing
- **Patterns**: Repository Pattern, Unit of Work, Dependency Injection

#### Project Structure
```
SmartERP/
??? SmartERP.Models/          # Entity models
?   ??? Entities/
?       ??? User.cs
?       ??? Inventory.cs
?       ??? Customer.cs
?       ??? Billing.cs
?
??? SmartERP.Data/            # Data access layer
?   ??? SmartERPDbContext.cs
?   ??? DatabaseInitializer.cs
?   ??? IUnitOfWork.cs
?   ??? UnitOfWork.cs
?   ??? Repositories/
?       ??? IRepository.cs
?       ??? Repository.cs
?       ??? IUserRepository.cs
?       ??? UserRepository.cs
?       ??? IInventoryRepository.cs
?       ??? InventoryRepository.cs
?       ??? ICustomerRepository.cs
?       ??? CustomerRepository.cs
?       ??? IBillingRepository.cs
?       ??? BillingRepository.cs
?
??? SmartERP.Core/            # Business logic layer
?   ??? Services/
?       ??? IAuthenticationService.cs
?       ??? AuthenticationService.cs
?
??? SmartERP.UI/              # Presentation layer
    ??? Views/
    ?   ??? LoginWindow.xaml
    ?   ??? LoginWindow.xaml.cs
    ??? MainWindow.xaml
    ??? MainWindow.xaml.cs
    ??? App.xaml
    ??? App.xaml.cs
    ??? appsettings.json
```

### Features Implemented

#### 1. Authentication System
- **Login Window**: Secure login interface
- **Password Hashing**: BCrypt-based password encryption
- **Role-Based Access Control**: Admin and User roles with different permissions
- **Session Management**: Track current user and last login

#### 2. Repository Pattern
- Generic repository with common CRUD operations
- Specialized repositories for each entity:
  - **UserRepository**: User authentication, password management
  - **InventoryRepository**: Inventory tracking, low stock alerts, quantity updates
  - **CustomerRepository**: Customer search, active customer filtering
  - **BillingRepository**: Bill generation, payment tracking, overdue bills

#### 3. Unit of Work Pattern
- Transaction management
- Coordinated save operations across multiple repositories
- Rollback support for failed operations

#### 4. Database
- SQLite database for local storage
- Automatic database creation and migration
- Seed data with default admin and employee users

### Permissions

#### Admin Role (Full Access)
- Inventory: View, Add, Update, Delete
- Customers: View, Add, Update, Delete
- Billing: View, Add, Update, Delete
- Reports: All reports

#### User Role (Limited Access)
- Inventory: View, Update (quantity usage)
- Customers: View only
- Billing: View, Add, Update
- Reports: No access

### Configuration

The `appsettings.json` file contains:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=SmartERP.db"
  },
  "AppSettings": {
    "CompanyName": "SmartERP",
    "LowStockThreshold": 10,
    "BillDueDays": 30
  }
}
```

### How to Run

1. **Build the solution**: 
   ```
   dotnet build
   ```

2. **Run the application**:
   ```
   dotnet run --project SmartERP.UI
   ```

3. **Login with default credentials**:
   - Admin: `admin` / `admin123`
   - Employee: `employee` / `employee123`

### Database Location
The SQLite database file `SmartERP.db` will be created in the application's output directory (e.g., `SmartERP.UI/bin/Debug/net10.0-windows/`).

## Next Steps (Phase 2)

1. **UI Development**:
   - Create Dashboard/Main window with navigation
   - Inventory management screens
   - Customer management screens
   - Billing management screens
   - Reports screens

2. **Business Logic**:
   - Inventory tracking and alerts
   - Customer package management
   - Automatic billing generation
   - Payment processing

3. **Reports**:
   - Inventory reports (stock levels, usage history)
   - Billing reports (monthly, outstanding, paid)
   - Customer reports (active, inactive, package-wise)

4. **Additional Features**:
   - Search and filter functionality
   - Data validation
   - Export to Excel/PDF
   - Backup and restore functionality
