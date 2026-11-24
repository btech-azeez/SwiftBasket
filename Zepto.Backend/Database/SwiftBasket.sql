IF DB_ID(N'ZeptoDb') IS NULL
BEGIN
    CREATE DATABASE [ZeptoDb];
END;
GO

USE [ZeptoDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        FullName nvarchar(100) NOT NULL,
        Email nvarchar(150) NOT NULL,
        PasswordHash nvarchar(max) NOT NULL,
        Role nvarchar(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT N'Customer',
        PhoneNumber nvarchar(20) NULL,
        Address nvarchar(250) NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'Customer', N'Admin'))
    );
    CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);
END;

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
        Name nvarchar(100) NOT NULL,
        Description nvarchar(500) NULL,
        ImageUrl nvarchar(500) NULL,
        Icon nvarchar(50) NULL
    );
END;

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        Name nvarchar(150) NOT NULL,
        Description nvarchar(1000) NULL,
        Price decimal(18,2) NOT NULL,
        DiscountPrice decimal(18,2) NULL,
        StockQuantity int NOT NULL CONSTRAINT DF_Products_StockQuantity DEFAULT 50,
        Unit nvarchar(50) NOT NULL CONSTRAINT DF_Products_Unit DEFAULT N'1 unit',
        ImageUrl nvarchar(500) NULL,
        InStock bit NOT NULL CONSTRAINT DF_Products_InStock DEFAULT 1,
        IsFeatured bit NOT NULL CONSTRAINT DF_Products_IsFeatured DEFAULT 0,
        CategoryId int NOT NULL,
        CreatedAt datetime2(7) NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Products_Categories_CategoryId FOREIGN KEY (CategoryId)
            REFERENCES dbo.Categories(Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);
END;

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
        UserId int NOT NULL,
        OrderDate datetime2(7) NOT NULL CONSTRAINT DF_Orders_OrderDate DEFAULT SYSUTCDATETIME(),
        TotalAmount decimal(18,2) NOT NULL,
        Status nvarchar(50) NOT NULL CONSTRAINT DF_Orders_Status DEFAULT N'Placed',
        ShippingAddress nvarchar(250) NOT NULL,
        PhoneNumber nvarchar(20) NOT NULL,
        PaymentMethod nvarchar(50) NOT NULL CONSTRAINT DF_Orders_PaymentMethod DEFAULT N'Cash on Delivery',
        PaymentStatus nvarchar(50) NOT NULL CONSTRAINT DF_Orders_PaymentStatus DEFAULT N'Pending',
        CONSTRAINT FK_Orders_Users_UserId FOREIGN KEY (UserId)
            REFERENCES dbo.Users(Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_Orders_UserId ON dbo.Orders(UserId);
END;

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderItems PRIMARY KEY,
        OrderId int NOT NULL,
        ProductId int NOT NULL,
        Quantity int NOT NULL,
        UnitPrice decimal(18,2) NOT NULL,
        TotalPrice decimal(18,2) NOT NULL,
        CONSTRAINT FK_OrderItems_Orders_OrderId FOREIGN KEY (OrderId)
            REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Products_ProductId FOREIGN KEY (ProductId)
            REFERENCES dbo.Products(Id) ON DELETE NO ACTION
    );
    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
    CREATE INDEX IX_OrderItems_ProductId ON dbo.OrderItems(ProductId);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@swiftbasket.com')
   AND EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@zepto.com')
BEGIN
    UPDATE dbo.Users
    SET Email = N'admin@swiftbasket.com',
        FullName = N'SwiftBasket Admin',
        Address = N'SwiftBasket Headquarters, 100 Express Blvd',
        PhoneNumber = N'+91 80000 93786',
        Role = N'Admin'
    WHERE Email = N'admin@zepto.com';
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'customer@swiftbasket.com')
   AND EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'customer@zepto.com')
BEGIN
    UPDATE dbo.Users
    SET Email = N'customer@swiftbasket.com',
        Role = N'Customer',
        PhoneNumber = N'+91 98765 01928',
        Address = N'42 Willow Street, Apt 3B, Bengaluru, Karnataka'
    WHERE Email = N'customer@zepto.com';
END;

UPDATE dbo.Users SET Role = N'Customer' WHERE Role NOT IN (N'Customer', N'Admin');

IF NOT EXISTS
(
    SELECT 1 FROM sys.check_constraints
    WHERE name = N'CK_Users_Role' AND parent_object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    ALTER TABLE dbo.Users WITH CHECK
        ADD CONSTRAINT CK_Users_Role CHECK (Role IN (N'Customer', N'Admin'));
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'admin@swiftbasket.com')
BEGIN
    INSERT dbo.Users (FullName, Email, PasswordHash, Role, PhoneNumber, Address)
    VALUES
    (
        N'SwiftBasket Admin', N'admin@swiftbasket.com',
        N'$2a$11$HlAyjmauptxRM2.RB6lntu9mT8byHC4rgQy/j7.llR2XP9gAj86.W',
        N'Admin', N'+1 800-937-8623', N'SwiftBasket Headquarters, 100 Express Blvd'
    );
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Email = N'customer@swiftbasket.com')
BEGIN
    INSERT dbo.Users (FullName, Email, PasswordHash, Role, PhoneNumber, Address)
    VALUES
    (
        N'John Customer', N'customer@swiftbasket.com',
        N'$2a$11$dz55nakbA4uwhkNIiPDGgeqkXF.8Mg2.sB.poEKmByn7u9TVB/B7y',
        N'Customer', N'+91 55501 92834', N'42 Willow Street, Apt 3B, Bengaluru, Karnataka'
    );
END;

DECLARE @CategorySeed TABLE
(
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500) NOT NULL,
    Icon nvarchar(50) NOT NULL,
    ImageUrl nvarchar(500) NOT NULL
);

INSERT @CategorySeed (Name, Description, Icon, ImageUrl)
VALUES
(N'Fruits & Vegetables', N'Farm-fresh organic fruits and crisp vegetables', N'apple-alt', N'https://images.unsplash.com/photo-1610832958506-aa56368176cf?w=600&auto=format&fit=crop&q=80'),
(N'Dairy, Bread & Eggs', N'Fresh dairy, artisanal bread, and farm eggs', N'egg', N'https://images.unsplash.com/photo-1550583724-b2692b85b150?w=600&auto=format&fit=crop&q=80'),
(N'Snacks & Munchies', N'Crispy chips, nuts, cookies, and savory bites', N'cookie-bite', N'https://images.unsplash.com/photo-1599490659213-e2b9527bd087?w=600&auto=format&fit=crop&q=80'),
(N'Cold Drinks & Juices', N'Chilled sodas, fresh pressed juices, and iced teas', N'wine-bottle', N'https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=600&auto=format&fit=crop&q=80'),
(N'Instant & Frozen Food', N'Ready-to-eat meals, quick noodles, and appetizers', N'utensils', N'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=600&auto=format&fit=crop&q=80'),
(N'Chocolates & Desserts', N'Rich chocolates, gourmet candies, and sweet treats', N'birthday-cake', N'https://images.unsplash.com/photo-1549007994-cb92caebd54b?w=600&auto=format&fit=crop&q=80'),
(N'Personal Care', N'Daily skincare, hygiene essentials, and grooming', N'pump-soap', N'https://images.unsplash.com/photo-1556228720-195a672e8a03?w=600&auto=format&fit=crop&q=80'),
(N'Cleaning & Household', N'Disinfectants, detergents, and home cleaning supplies', N'spray-can', N'https://images.unsplash.com/photo-1585421514738-01798e348b17?w=600&auto=format&fit=crop&q=80');

INSERT dbo.Categories (Name, Description, Icon, ImageUrl)
SELECT seed.Name, seed.Description, seed.Icon, seed.ImageUrl
FROM @CategorySeed AS seed
WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories AS existing WHERE existing.Name = seed.Name);

DECLARE @ProductSeed TABLE
(
    Name nvarchar(150) NOT NULL,
    Description nvarchar(1000) NOT NULL,
    Price decimal(18,2) NOT NULL,
    DiscountPrice decimal(18,2) NULL,
    StockQuantity int NOT NULL,
    Unit nvarchar(50) NOT NULL,
    ImageUrl nvarchar(500) NOT NULL,
    InStock bit NOT NULL,
    IsFeatured bit NOT NULL,
    CategoryName nvarchar(100) NOT NULL
);

INSERT @ProductSeed (Name, Description, Price, DiscountPrice, StockQuantity, Unit, ImageUrl, InStock, IsFeatured, CategoryName)
VALUES
(N'Organic Royal Gala Apples', N'Crisp, sweet, and hand-picked premium organic gala apples.', 399, 349, 80, N'4 pcs (approx. 500g)', N'https://images.unsplash.com/photo-1560806887-1e4cd0b6cbd6?w=600&auto=format&fit=crop&q=80', 1, 1, N'Fruits & Vegetables'),
(N'Fresh Cavendish Bananas', N'Naturally ripened sweet bananas packed with potassium.', 199, 149, 120, N'1 bunch (6 pcs)', N'https://images.unsplash.com/photo-1571771894821-ce9b6c11b08e?w=600&auto=format&fit=crop&q=80', 1, 0, N'Fruits & Vegetables'),
(N'Hydroponic Baby Spinach', N'Pre-washed tender baby spinach leaves ready for healthy salads.', 249, NULL, 50, N'200 g pack', N'https://images.unsplash.com/photo-1576045057995-568f588f82fb?w=600&auto=format&fit=crop&q=80', 1, 0, N'Fruits & Vegetables'),
(N'Vine Ripe Roma Tomatoes', N'Plump and juicy vine-ripened red tomatoes ideal for cooking and salads.', 219, 189, 95, N'500 g', N'https://images.unsplash.com/photo-1592924357228-91a4daadcfea?w=600&auto=format&fit=crop&q=80', 1, 1, N'Fruits & Vegetables'),
(N'Fresh Hass Avocados', N'Creamy, ready-to-eat Hass avocados perfect for guacamole and toast.', 429, 389, 45, N'2 pcs', N'https://images.unsplash.com/photo-1523049673857-eb18f1d7b578?w=600&auto=format&fit=crop&q=80', 1, 1, N'Fruits & Vegetables'),
(N'Whole Organic Milk', N'Fresh pasteurized whole milk from grass-fed cows.', 379, 329, 60, N'1 Gallon', N'https://images.unsplash.com/photo-1563636619-e9143da7973b?w=600&auto=format&fit=crop&q=80', 1, 1, N'Dairy, Bread & Eggs'),
(N'Artisan Sourdough Bread', N'Slow-fermented artisan sourdough bread loaf with crunchy crust.', 449, NULL, 35, N'1 loaf (450g)', N'https://images.unsplash.com/photo-1589367920969-ab8e050bbb04?w=600&auto=format&fit=crop&q=80', 1, 1, N'Dairy, Bread & Eggs'),
(N'Cage-Free Brown Eggs', N'Grade A brown eggs enriched with Omega-3 fatty acids.', 499, 429, 75, N'12 count', N'https://images.unsplash.com/photo-1582722872445-44dc5f7e3c8f?w=600&auto=format&fit=crop&q=80', 1, 1, N'Dairy, Bread & Eggs'),
(N'Salted Creamery Butter', N'Rich European-style churned sweet cream butter with pure sea salt.', 389, 339, 40, N'250 g', N'https://images.unsplash.com/photo-1589985270826-4b7bb135bc9d?w=600&auto=format&fit=crop&q=80', 1, 0, N'Dairy, Bread & Eggs'),
(N'Greek Plain Yogurt', N'Thick, high-protein strained Greek yogurt with zero added sugar.', 349, NULL, 50, N'500 g tub', N'https://images.unsplash.com/photo-1488477181946-6428a0291777?w=600&auto=format&fit=crop&q=80', 1, 0, N'Dairy, Bread & Eggs'),
(N'Classic Sea Salt Kettle Chips', N'Golden batch-cooked potato chips dusted with fine sea salt.', 299, 249, 100, N'150 g bag', N'https://images.unsplash.com/photo-1566478989037-eec170784d0b?w=600&auto=format&fit=crop&q=80', 1, 1, N'Snacks & Munchies'),
(N'Roasted Almonds & Cashews Mix', N'Lightly salted premium roasted nuts loaded with healthy fats and protein.', 699, 599, 60, N'250 g pouch', N'https://images.unsplash.com/photo-1599599810769-bcde5a160d32?w=600&auto=format&fit=crop&q=80', 1, 1, N'Snacks & Munchies'),
(N'Spicy Nacho Cheese Tortillas', N'Crunchy corn tortilla chips with bold zesty cheddar cheese flavor.', 329, NULL, 80, N'180 g bag', N'https://images.unsplash.com/photo-1513456852971-30c0b8199d4d?w=600&auto=format&fit=crop&q=80', 1, 0, N'Snacks & Munchies'),
(N'Sparkling Lime Mineral Water', N'Zero-calorie refreshing carbonated water with real lime essence.', 179, 129, 90, N'1 L bottle', N'https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=600&auto=format&fit=crop&q=80', 1, 0, N'Cold Drinks & Juices'),
(N'Cold Pressed Orange Juice', N'100% pure cold-pressed Valencia orange juice with pulp.', 449, 399, 45, N'500 ml', N'https://images.unsplash.com/photo-1613478223719-2ab802602423?w=600&auto=format&fit=crop&q=80', 1, 1, N'Cold Drinks & Juices'),
(N'Iced Peach Black Tea', N'Brewed black tea infused with natural juicy peach sweetness.', 249, 199, 70, N'450 ml', N'https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=600&auto=format&fit=crop&q=80', 1, 0, N'Cold Drinks & Juices'),
(N'Spicy Ramen Noodles Bowl', N'Instant gourmet noodle bowl with fiery chili broth and vegetables.', 229, 189, 110, N'115 g bowl', N'https://images.unsplash.com/photo-1612927601601-6638404737ce?w=600&auto=format&fit=crop&q=80', 1, 1, N'Instant & Frozen Food'),
(N'Artisan Margherita Frozen Pizza', N'Stone-baked thin crust pizza with San Marzano tomatoes and mozzarella.', 749, 649, 30, N'400 g', N'https://images.unsplash.com/photo-1513104890138-7c749659a591?w=600&auto=format&fit=crop&q=80', 1, 1, N'Instant & Frozen Food'),
(N'Dark Belgian Chocolate 70%', N'Smooth velvety dark chocolate bar crafted with single-origin cocoa.', 399, 329, 65, N'100 g bar', N'https://images.unsplash.com/photo-1548907040-4baa42d10919?w=600&auto=format&fit=crop&q=80', 1, 1, N'Chocolates & Desserts'),
(N'Madagascar Vanilla Bean Ice Cream', N'Ultra-rich creamy churned ice cream flecked with pure vanilla bean.', 599, 499, 25, N'473 ml pint', N'https://images.unsplash.com/photo-1570197788417-0e82375c9371?w=600&auto=format&fit=crop&q=80', 1, 1, N'Chocolates & Desserts'),
(N'Herbal Tea Tree Shampoo', N'Clarifying and invigorating shampoo enriched with organic tea tree oil.', 899, 749, 40, N'300 ml', N'https://images.unsplash.com/photo-1535585209827-a15fcdbc4c2d?w=600&auto=format&fit=crop&q=80', 1, 0, N'Personal Care'),
(N'Hydrating Shea Butter Body Lotion', N'Deep moisture formula with raw shea butter and vitamin E.', 749, NULL, 35, N'400 ml', N'https://images.unsplash.com/photo-1608248597359-5f21226b9195?w=600&auto=format&fit=crop&q=80', 1, 0, N'Personal Care'),
(N'Eco-Friendly Dishwashing Gel', N'Tough on grease, gentle on hands with natural lemon blossom extracts.', 399, 349, 50, N'750 ml', N'https://images.unsplash.com/photo-1585421514738-01798e348b17?w=600&auto=format&fit=crop&q=80', 1, 0, N'Cleaning & Household'),
(N'Multi-Surface Lavender Disinfectant Spray', N'Kills 99.9% of bacteria and viruses with calming French lavender aroma.', 449, 399, 60, N'500 ml', N'https://images.unsplash.com/photo-1584362917165-526a968579e8?w=600&auto=format&fit=crop&q=80', 1, 1, N'Cleaning & Household');

UPDATE product
SET Price = seed.Price,
    DiscountPrice = seed.DiscountPrice
FROM dbo.Products AS product
INNER JOIN @ProductSeed AS seed ON seed.Name = product.Name
WHERE product.Price = seed.Price / 100.00;

INSERT dbo.Products (Name, Description, Price, DiscountPrice, StockQuantity, Unit, ImageUrl, InStock, IsFeatured, CategoryId)
SELECT seed.Name, seed.Description, seed.Price, seed.DiscountPrice, seed.StockQuantity, seed.Unit, seed.ImageUrl,
       seed.InStock, seed.IsFeatured, category.Id
FROM @ProductSeed AS seed
INNER JOIN dbo.Categories AS category ON category.Name = seed.CategoryName
WHERE NOT EXISTS (SELECT 1 FROM dbo.Products AS existing WHERE existing.Name = seed.Name);

DECLARE @MigratedOrderIds TABLE (Id int NOT NULL);

UPDATE item
SET UnitPrice = item.UnitPrice * 100.00,
    TotalPrice = item.UnitPrice * item.Quantity * 100.00
OUTPUT inserted.OrderId INTO @MigratedOrderIds (Id)
FROM dbo.OrderItems AS item
INNER JOIN dbo.Products AS product ON product.Id = item.ProductId
INNER JOIN @ProductSeed AS seed ON seed.Name = product.Name
WHERE item.UnitPrice = seed.Price / 100.00
   OR (seed.DiscountPrice IS NOT NULL AND item.UnitPrice = seed.DiscountPrice / 100.00);

;WITH MigratedOrderTotals AS
(
    SELECT item.OrderId, SUM(item.TotalPrice) AS Subtotal
    FROM dbo.OrderItems AS item
    WHERE item.OrderId IN (SELECT Id FROM @MigratedOrderIds)
    GROUP BY item.OrderId
)
UPDATE orders
SET TotalAmount = totals.Subtotal + CASE WHEN totals.Subtotal > 0 AND totals.Subtotal < 2000 THEN 199 ELSE 0 END
FROM dbo.Orders AS orders
INNER JOIN MigratedOrderTotals AS totals ON totals.OrderId = orders.Id;

IF NOT EXISTS (SELECT 1 FROM dbo.Orders)
BEGIN
    DECLARE @CustomerId int = (SELECT TOP (1) Id FROM dbo.Users WHERE Email = N'customer@swiftbasket.com');
    DECLARE @AppleId int = (SELECT TOP (1) Id FROM dbo.Products WHERE Name = N'Organic Royal Gala Apples');
    DECLARE @MilkId int = (SELECT TOP (1) Id FROM dbo.Products WHERE Name = N'Whole Organic Milk');
    DECLARE @ChipsId int = (SELECT TOP (1) Id FROM dbo.Products WHERE Name = N'Classic Sea Salt Kettle Chips');
    DECLARE @OrderId int;

    INSERT dbo.Orders (UserId, OrderDate, TotalAmount, Status, ShippingAddress, PhoneNumber, PaymentMethod, PaymentStatus)
    VALUES (@CustomerId, DATEADD(hour, -2, SYSUTCDATETIME()), 1475.00, N'Processing',
            N'42 Willow Street, Apt 3B, Bengaluru, Karnataka', N'+91 98765 01928', N'Cash on Delivery', N'Pending');
    SET @OrderId = SCOPE_IDENTITY();

    INSERT dbo.OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice)
    VALUES
    (@OrderId, @AppleId, 2, 349.00, 698.00),
    (@OrderId, @MilkId, 1, 329.00, 329.00),
    (@OrderId, @ChipsId, 1, 249.00, 249.00);
END;

COMMIT TRANSACTION;
GO

SELECT N'Customer' AS RoleName
UNION ALL SELECT N'Admin' AS RoleName;

SELECT COUNT(*) AS CategoryCount FROM dbo.Categories;
SELECT COUNT(*) AS ProductCount FROM dbo.Products;
SELECT COUNT(*) AS UserCount FROM dbo.Users;
