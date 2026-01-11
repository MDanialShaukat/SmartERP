-- SmartERP Database Schema
-- SQLite Database

-- ============================================
-- Users Table
-- ============================================
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL DEFAULT 'User',
    FullName TEXT NOT NULL,
    Email TEXT,
    PhoneNumber TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedDate TEXT NOT NULL,
    LastLoginDate TEXT
);

-- ============================================
-- Inventory Table
-- ============================================
CREATE TABLE Inventories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ItemName TEXT NOT NULL,
    Description TEXT,
    Category TEXT NOT NULL,
    Unit TEXT,
    PurchasePrice REAL NOT NULL,
    QuantityPurchased INTEGER NOT NULL,
    QuantityUsed INTEGER NOT NULL,
    QuantityRemaining INTEGER NOT NULL,
    TotalPurchaseAmount REAL,
    Supplier TEXT,
    PurchaseDate TEXT NOT NULL,
    CreatedDate TEXT NOT NULL,
    LastModifiedDate TEXT,
    CreatedBy INTEGER NOT NULL,
    LastModifiedBy INTEGER,
    Notes TEXT
);

-- ============================================
-- Customers Table
-- ============================================
CREATE TABLE Customers (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CustomerName TEXT NOT NULL,
    CustomerCode TEXT NOT NULL UNIQUE,
    PhoneNumber TEXT,
    Email TEXT,
    Address TEXT NOT NULL,
    City TEXT,
    PinCode TEXT,
    PackageType TEXT,
    PackageAmount REAL,
    ConnectionType TEXT,
    ConnectionDate TEXT NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    AdditionalDetails TEXT,
    OutstandingBalance REAL DEFAULT 0,
    CreatedDate TEXT NOT NULL,
    LastModifiedDate TEXT,
    CreatedBy INTEGER NOT NULL,
    LastModifiedBy INTEGER,
    Notes TEXT
);

-- ============================================
-- Billing Table
-- ============================================
CREATE TABLE Billings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BillNumber TEXT NOT NULL UNIQUE,
    CustomerId INTEGER NOT NULL,
    BillingMonth INTEGER NOT NULL,
    BillingYear INTEGER NOT NULL,
    BillAmount REAL NOT NULL,
    PreviousDue REAL DEFAULT 0,
    TotalAmount REAL NOT NULL,
    AmountPaid REAL DEFAULT 0,
    BalanceAmount REAL NOT NULL,
    PaymentStatus TEXT NOT NULL DEFAULT 'Pending',
    BillDate TEXT NOT NULL,
    DueDate TEXT NOT NULL,
    PaymentDate TEXT,
    PaymentMethod TEXT,
    TransactionReference TEXT,
    Notes TEXT,
    CreatedDate TEXT NOT NULL,
    LastModifiedDate TEXT,
    CreatedBy INTEGER NOT NULL,
    LastModifiedBy INTEGER,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

-- ============================================
-- Indexes
-- ============================================
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Customers_CustomerCode ON Customers(CustomerCode);
CREATE INDEX IX_Billings_BillNumber ON Billings(BillNumber);
CREATE INDEX IX_Billings_CustomerId ON Billings(CustomerId);
CREATE INDEX IX_Billings_PaymentStatus ON Billings(PaymentStatus);

-- ============================================
-- Seed Data
-- ============================================
-- Admin User (password: admin123)
INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, IsActive, CreatedDate)
VALUES ('admin', '$2a$11$encrypted_password_hash', 'Admin', 'System Administrator', 'admin@smarterp.local', 1, datetime('now'));

-- Employee User (password: employee123)
INSERT INTO Users (Username, PasswordHash, Role, FullName, Email, IsActive, CreatedDate)
VALUES ('employee', '$2a$11$encrypted_password_hash', 'User', 'Employee User', 'employee@smarterp.local', 1, datetime('now'));
