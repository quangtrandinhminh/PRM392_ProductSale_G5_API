-- Tạo database
USE master;

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'SalesAppDB')
    DROP DATABASE SalesAppDB;
GO

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
('imadmin', '$2a$12$ZaFmQWz3JOBrko3ZKmLZ7u3AuS9m1DSGSWQ.pMNDv5fXGCHdbxviG', 'john@example.com', '1234567890', '123 Main St', 'Admin'),
('imcustomer', '$2a$12$T/Mb68jp.voF62cDC2uWp.p0P8AFmkdTXVsxg7boCj97yjkt3zN1C', 'jane@example.com', '0987654321', '456 Market St', 'Customer')

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

INSERT INTO Products 
(ProductName, BriefDescription, FullDescription, TechnicalSpecifications, Price, ImageURL, CategoryID)
VALUES
('MacBook Pro', 'Apple Laptop', 'High performance laptop by Apple', '16GB RAM, 512GB SSD', 20000000.00, 'https://minhtuanmobile.com/uploads/products/m4-250206040356.png', 1),
('iPhone 14', 'Apple Smartphone', 'Latest iPhone model', '128GB Storage, A15 Bionic', 19000000.00, 'https://encrypted-tbn0.gstatic.com/shopping?q=tbn:ANd9GcQWcUaJZQBvGvFQISKE1YTcIwt4UxPuSHThH0-L9peVsDn0e9T3TnbMX7RRANJ88_4oXKclnTD4MSGMKradiomFm4hNntmS5xn3JiieprJEGozMKnALAuucixk0Y0ac5FYAUpoN&usqp=CAc', 2),
('iPad Air', 'Apple Tablet', 'Lightweight and powerful', '10.9-inch, A14 Bionic', 18000000.00, 'https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcSfLV_AXHWgC1R3NDaErniKQ92u4hft3bX03FGeX50Jt3__CZU-g237wLpUnsmIbxf5MJG0AqzFKaCCCNtc969Lxd-g0riUZuGC0XMmpBPixV3oNQcuItE2nqqNzQxjo-0RhTcf&usqp=CAc', 3),
('Wireless Mouse', 'Accessory', 'Ergonomic wireless mouse', 'Bluetooth connectivity', 769000.00, 'https://encrypted-tbn2.gstatic.com/shopping?q=tbn:ANd9GcTabKl47dzTMiyuMRRId_P2JOwukEDF929brcX5t8G0cakKC5BLEHF2qwzEa7ByLhra4YRkgf8RDoVRMqG1MBahP5Tip2iUZ-7L3yRF_7O8pJGDIrqZWikkvFXjhiuCXNzOcg&usqp=CAc', 4),
('AirPods Pro', 'Wireless Earbuds', 'Noise cancelling earbuds', 'Bluetooth 5.0', 5000000.00, 'https://encrypted-tbn0.gstatic.com/shopping?q=tbn:ANd9GcTxUJeb_iVfT_5akua1XWP_JwS6FqXjz5wbRZF3q--O9Q1lE5OPmBbr4O5IxXTgYxbzeCB6ZkrtQa20wJBk-w810AtQaIySvoXIKPWIGEfM_HN3J8UeW2TVr2VCWbOXXA46&usqp=CAc', 5),
('PlayStation 5', 'Gaming Console', 'Latest Sony gaming console', 'Ultra HD, SSD storage', 13000000.00, 'https://encrypted-tbn2.gstatic.com/shopping?q=tbn:ANd9GcQVfXpc0i0zI06aoSobo-_B4lLShTwyv36LcyneB0QQFb3RKhDet_r0uV4bq3QXb10eP0NLjJAKN5v34UVafrnEkOGxtSIWRbi6lbKn8w4nQWhtggtrf8kSf-2ebQnBceS4Dw&usqp=CAc', 6),
('Apple Watch', 'Smartwatch', 'Health and fitness smartwatch', 'GPS, Heart Rate Monitor', 8600000.00, 'https://encrypted-tbn0.gstatic.com/shopping?q=tbn:ANd9GcRT1E-q9yyXBX9qn-dBfUhr_SggM0idafRx5fets2_XP5lcpch55-hJP9SNNb8nbtqCedzG6uYA0Yu2GaH8El7YULXZfjKnEsDkDm3XQKZZX_QxekYL0J5KMyv7dVhuk41y&usqp=CAc', 7),
('Dell Monitor', 'Computer Monitor', '27-inch LED Monitor', 'Full HD, 144Hz', 4999000.00, 'https://encrypted-tbn2.gstatic.com/shopping?q=tbn:ANd9GcTUNZmdSDNIwWzVYR5_5F5SRa1rcHgY_6KPrEV0SK25l2Y_tes-S348hK1hbLY5ifuCW9ru99yYEArAtwz2BLMxXR8FsB9fWBXpqwn46v5R3sf4J7bd7oGDN-0vAacHd5W1jw&usqp=CAc', 8),
('HP Laser Printer', 'Laser Printer', 'Fast and high-quality printing', 'Wireless, Duplex', 2700000.00, 'https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcShzBzEIjjAY-H6ODrnp-LU6PCr6WfS5N8LdAULNHs7tpRD714fpo05zaysHMQtK3z4HAYHwMvLPzLoCsFw7PdIv3pgMIvA2sPG7YaADLN5ZZD22eGeaAgD7n5K8NGgEnT3&usqp=CAc', 9),
('Mechanical Keyboard', 'Mechanical Gaming Keyboard', 'RGB backlit keys', 'USB, Mechanical switches', 2500000.00, 'https://encrypted-tbn1.gstatic.com/shopping?q=tbn:ANd9GcQKngWYjxZIwBPkUFcqdTgj_V_KH701jFFXTimgKX8vpsRn_JlBrdmwm8NjmRFQu3VnfmuzzJZBxmX1dGJ-WouCanBIVs76t-groXooo6caKuR6uenmHwip_4yl_0RlrUK9Ng&usqp=CAc', 10);

INSERT INTO [dbo].[StoreLocations]
(Latitude, Longitude, Address)
VALUES
(10.842287, 106.801944, 'Lô E2a-7, Đường D1, Khu Công nghệ cao, Long Thạnh Mỹ, TP. Thủ Đức, TP. Hồ Chí Minh.');

-- Tạo bảng UserDevices để lưu thông tin thiết bị của người dùng
CREATE TABLE [dbo].[UserDevices] (
    [UserDeviceId] INT IDENTITY(1,1) NOT NULL,
    [UserId] INT NULL,
    [DeviceToken] NVARCHAR(255) NOT NULL, 
    [DeviceType] NVARCHAR(50) NOT NULL,
    [LastUsed] DATETIME DEFAULT GETDATE() NOT NULL,
    CONSTRAINT [PK_UserDevices] PRIMARY KEY CLUSTERED ([UserDeviceId] ASC),
    CONSTRAINT [FK_UserDevices_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
);

-- Tạo unique index cho cặp UserId và DeviceToken
CREATE UNIQUE INDEX [IX_UserDevices_UserId_DeviceToken] 
ON [dbo].[UserDevices] ([UserId], [DeviceToken]) 
WHERE [UserId] IS NOT NULL;

ALTER TABLE ChatMessages
ADD IsRead BIT NOT NULL DEFAULT 0;


--thêm cột receiverid cho message---
ALTER TABLE [SalesAppDB].[dbo].[ChatMessages]  
ADD ReceiverID INT NULL;

ALTER TABLE [SalesAppDB].[dbo].[ChatMessages]  
ADD CONSTRAINT FK_ChatMessages_Receiver  
FOREIGN KEY (ReceiverID) REFERENCES [SalesAppDB].[dbo].[Users](UserID);

TRUNCATE TABLE [SalesAppDB].[dbo].[ChatMessages];
