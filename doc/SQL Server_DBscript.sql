-- Tạo database
CREATE DATABASE SalesAppDB;
GO

USE SalesAppDB;
GO

-- Tạo bảng User
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(15),
    Address NVARCHAR(255),
    Role NVARCHAR(50) NOT NULL
);
GO

-- Tạo bảng Category
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL
);
GO

-- Tạo bảng Product
CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(100) NOT NULL,
    BriefDescription NVARCHAR(255),
    FullDescription NVARCHAR(MAX),
    TechnicalSpecifications NVARCHAR(MAX),
    Price DECIMAL(18, 2) NOT NULL,
    ImageURL NVARCHAR(255),
    CategoryID INT,
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);
GO

-- Tạo bảng Cart
CREATE TABLE Carts (
    CartID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    TotalPrice DECIMAL(18, 2) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- Tạo bảng CartItem
CREATE TABLE CartItems (
    CartItemID INT PRIMARY KEY IDENTITY(1,1),
    CartID INT,
    ProductID INT,
    Quantity INT NOT NULL,
    Price DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (CartID) REFERENCES Carts(CartID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- Tạo bảng Order
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    CartID INT,
    UserID INT,
    PaymentMethod NVARCHAR(50) NOT NULL,
    BillingAddress NVARCHAR(255) NOT NULL,
    OrderStatus NVARCHAR(50) NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (CartID) REFERENCES Carts(CartID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- Tạo bảng Payment
CREATE TABLE Payments (
    PaymentID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT,
    Amount DECIMAL(18, 2) NOT NULL,
    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
    PaymentStatus NVARCHAR(50) NOT NULL,
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);
GO

-- Tạo bảng Notification
CREATE TABLE Notifications (
    NotificationID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    Message NVARCHAR(255),
    IsRead BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- Tạo bảng ChatMessage
CREATE TABLE ChatMessages (
    ChatMessageID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    Message NVARCHAR(MAX),
    SentAt DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- Tạo bảng StoreLocation
CREATE TABLE StoreLocations (
    LocationID INT PRIMARY KEY IDENTITY(1,1),
    Latitude DECIMAL(9, 6) NOT NULL,
    Longitude DECIMAL(9, 6) NOT NULL,
    Address NVARCHAR(255) NOT NULL
);
GO

use SalesAppDB

-- Insert sample data into Users
INSERT INTO Users (Username, PasswordHash, Email, PhoneNumber, Address, Role)
VALUES
('john_doe', 'hashedpassword1', 'john@example.com', '1234567890', '123 Main St', 'Customer'),
('jane_smith', 'hashedpassword2', 'jane@example.com', '0987654321', '456 Market St', 'Customer'),
('admin', 'hashedpassword3', 'admin@example.com', '1122334455', 'Admin HQ', 'Admin'),
('alice_johnson', 'hashedpassword4', 'alice@example.com', '2233445566', '789 Elm St', 'Customer'),
('bob_miller', 'hashedpassword5', 'bob@example.com', '3344556677', '101 Oak St', 'Customer'),
('charlie_wilson', 'hashedpassword6', 'charlie@example.com', '4455667788', '202 Pine St', 'Customer'),
('david_clark', 'hashedpassword7', 'david@example.com', '5566778899', '303 Maple St', 'Customer'),
('eva_lee', 'hashedpassword8', 'eva@example.com', '6677889900', '404 Cedar St', 'Customer'),
('frank_hall', 'hashedpassword9', 'frank@example.com', '7788990011', '505 Birch St', 'Customer'),
('grace_white', 'hashedpassword10', 'grace@example.com', '8899001122', '606 Spruce St', 'Customer');

-- Insert sample data into Categories
INSERT INTO Categories (CategoryName)
VALUES
('Laptops'),
('Smartphones'),
( 'Tablets'),
( 'Accessories'),
( 'Headphones'),
( 'Gaming Consoles'),
( 'Smartwatches'),
( 'Monitors'),
( 'Printers'),
( 'Keyboards');

-- Insert sample data into Products
INSERT INTO Products (ProductName, BriefDescription, FullDescription, TechnicalSpecifications, Price, ImageURL, CategoryID)
VALUES
( 'MacBook Pro', 'Apple Laptop', 'High performance laptop by Apple', '16GB RAM, 512GB SSD', 2000, 'macbook.jpg', 1),
('iPhone 14', 'Apple Smartphone', 'Latest iPhone model', '128GB Storage, A15 Bionic', 999, 'iphone.jpg', 2),
( 'iPad Air', 'Apple Tablet', 'Lightweight and powerful', '10.9-inch, A14 Bionic', 699, 'ipadair.jpg', 3),
( 'Wireless Mouse', 'Accessory', 'Ergonomic wireless mouse', 'Bluetooth connectivity', 50, 'mouse.jpg', 4),
( 'AirPods Pro', 'Wireless Earbuds', 'Noise cancelling earbuds', 'Bluetooth 5.0', 249, 'airpods.jpg', 5),
( 'PlayStation 5', 'Gaming Console', 'Latest Sony gaming console', 'Ultra HD, SSD storage', 499, 'ps5.jpg', 6),
( 'Apple Watch', 'Smartwatch', 'Health and fitness smartwatch', 'GPS, Heart Rate Monitor', 399, 'watch.jpg', 7),
( 'Dell Monitor', 'Computer Monitor', '27-inch LED Monitor', 'Full HD, 144Hz', 299, 'monitor.jpg', 8),
( 'HP Laser Printer', 'Laser Printer', 'Fast and high-quality printing', 'Wireless, Duplex', 199, 'printer.jpg', 9),
( 'Mechanical Keyboard', 'Mechanical Gaming Keyboard', 'RGB backlit keys', 'USB, Mechanical switches', 129, 'keyboard.jpg', 10);

-- Insert sample data into Carts
INSERT INTO Carts ( UserID, TotalPrice, Status)
VALUES
( 1, 2999, 'Pending'),
( 2, 749, 'Pending'),
( 3, 249, 'Completed'),
( 4, 999, 'Pending'),
( 5, 129, 'Completed'),
( 6, 699, 'Pending'),
( 7, 499, 'Completed'),
( 8, 50, 'Pending'),
( 9, 399, 'Pending'),
( 10, 299, 'Completed');

-- Insert sample data into CartItems
INSERT INTO CartItems ( CartID, ProductID, Quantity, Price)
VALUES
( 1, 1, 1, 2000),
( 1, 2, 1, 999),
( 2, 3, 1, 699),
( 2, 4, 1, 50),
( 3, 5, 1, 249),
( 4, 6, 1, 999),
( 5, 10, 1, 129),
( 6, 3, 1, 699),
( 7, 6, 1, 499),
( 8, 4, 1, 50);

-- Insert sample data into Orders
INSERT INTO Orders ( CartID, UserID, PaymentMethod, BillingAddress, OrderStatus, OrderDate)
VALUES
( 1, 1, 'Credit Card', '123 Main St', 'Processing', GETDATE()),
( 2, 2, 'PayPal', '456 Market St', 'Shipped', GETDATE()),
( 3, 3, 'Credit Card', '789 Elm St', 'Delivered', GETDATE()),
( 4, 4, 'Debit Card', '101 Oak St', 'Processing', GETDATE()),
( 5, 5, 'PayPal', '303 Maple St', 'Shipped', GETDATE()),
( 6, 6, 'Credit Card', '404 Cedar St', 'Pending', GETDATE()),
( 7, 7, 'Debit Card', '505 Birch St', 'Processing', GETDATE()),
( 8, 8, 'Credit Card', '606 Spruce St', 'Shipped', GETDATE()),
( 9, 9, 'PayPal', '707 Willow St', 'Pending', GETDATE()),
( 10, 10, 'Debit Card', '808 Redwood St', 'Delivered', GETDATE());
