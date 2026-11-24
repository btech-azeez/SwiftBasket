using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Zepto.Backend.Models;

namespace Zepto.Backend.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ZeptoDbContext context)
        {
            context.Database.EnsureCreated();

            var adminUser = context.Users.FirstOrDefault(user =>
                user.Email == "admin@swiftbasket.com" || user.Email == "admin@zepto.com");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    FullName = "SwiftBasket Admin",
                    Email = "admin@swiftbasket.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = UserRoles.Admin,
                    PhoneNumber = "+91 80000 93786",
                    Address = "SwiftBasket Headquarters, 100 Express Blvd",
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(adminUser);
            }
            else
            {
                adminUser.Email = "admin@swiftbasket.com";
                adminUser.FullName = "SwiftBasket Admin";
                adminUser.Address = "SwiftBasket Headquarters, 100 Express Blvd";
                adminUser.Role = UserRoles.Admin;
                adminUser.PhoneNumber = "+91 80000 93786";
            }

            var customerUser = context.Users.FirstOrDefault(user =>
                user.Email == "customer@swiftbasket.com" || user.Email == "customer@zepto.com");
            if (customerUser == null)
            {
                customerUser = new User
                {
                    FullName = "John Customer",
                    Email = "customer@swiftbasket.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                    Role = UserRoles.Customer,
                    PhoneNumber = "+91 98765 01928",
                    Address = "42 Willow Street, Apt 3B, Bengaluru, Karnataka",
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(customerUser);
            }
            else
            {
                customerUser.Email = "customer@swiftbasket.com";
                customerUser.Role = UserRoles.Customer;
                customerUser.PhoneNumber = "+91 98765 01928";
                customerUser.Address = "42 Willow Street, Apt 3B, Bengaluru, Karnataka";
            }

            foreach (var user in context.Users.Where(user => user.Role != UserRoles.Customer && user.Role != UserRoles.Admin))
            {
                user.Role = UserRoles.Customer;
            }
            context.SaveChanges();

            // 2. Seed Categories
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Fruits & Vegetables",
                    Description = "Farm-fresh organic fruits and crisp vegetables",
                    Icon = "apple-alt",
                    ImageUrl = "https://images.unsplash.com/photo-1610832958506-aa56368176cf?w=600&auto=format&fit=crop&q=80"
                },
                new Category
                {
                    Name = "Dairy, Bread & Eggs",
                    Description = "Fresh dairy, artisanal bread, and farm eggs",
                    Icon = "egg",
                    ImageUrl = "https://images.unsplash.com/photo-1550583724-b2692b85b150?w=600&auto=format&fit=crop&q=80"
                },
                new Category
                {
                    Name = "Snacks & Munchies",
                    Description = "Crispy chips, nuts, cookies, and savory bites",
                    Icon = "cookie-bite",
                    ImageUrl = "https://images.unsplash.com/photo-1599490659213-e2b9527bd087?w=600&auto=format&fit=crop&q=80"
                },
                new Category
                {
                    Name = "Cold Drinks & Juices",
                    Description = "Chilled sodas, fresh pressed juices, and iced teas",
                    Icon = "wine-bottle",
                    ImageUrl = "https://images.unsplash.com/photo-1622483767028-3f66f32aef97?w=600&auto=format&fit=crop&q=80"
                },
                new Category
                {
                    Name = "Instant & Frozen Food",
                    Description = "Ready-to-eat meals, quick noodles, and appetizers",
                    Icon = "utensils",
                    ImageUrl = "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=600&auto=format&fit=crop&q=80"
                },
                new Category
                {
                    Name = "Chocolates & Desserts",
                    Description = "Rich chocolates, gourmet candies, and sweet treats",
                    Icon = "birthday-cake",
                    ImageUrl = "https://images.unsplash.com/photo-1549007994-cb92caebd54b?w=600&auto=format&fit=crop&q=80"
                },
                new Category
                {
                    Name = "Personal Care",
                    Description = "Daily skincare, hygiene essentials, and grooming",
                    Icon = "pump-soap",
                    ImageUrl = "https://images.unsplash.com/photo-1556228720-195a672e8a03?w=600&auto=format&fit=crop&q=80"
                },
                new Category
                {
                    Name = "Cleaning & Household",
                    Description = "Disinfectants, detergents, and home cleaning supplies",
                    Icon = "spray-can",
                    ImageUrl = "https://images.unsplash.com/photo-1585421514738-01798e348b17?w=600&auto=format&fit=crop&q=80"
                }
            };

            var storedCategories = context.Categories
                .ToList()
                .GroupBy(category => category.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            var missingCategories = new List<Category>();
            for (var index = 0; index < categories.Count; index++)
            {
                var seedCategory = categories[index];
                if (storedCategories.TryGetValue(seedCategory.Name, out var storedCategory))
                {
                    categories[index] = storedCategory;
                }
                else
                {
                    missingCategories.Add(seedCategory);
                    storedCategories.Add(seedCategory.Name, seedCategory);
                }
            }

            context.Categories.AddRange(missingCategories);
            context.SaveChanges();

            // 3. Seed Products
            var catFV = categories[0].Id;
            var catDairy = categories[1].Id;
            var catSnacks = categories[2].Id;
            var catDrinks = categories[3].Id;
            var catInstant = categories[4].Id;
            var catSweets = categories[5].Id;
            var catCare = categories[6].Id;
            var catClean = categories[7].Id;

            var products = new List<Product>
            {
                // Fruits & Veg
                new Product
                {
                    Name = "Organic Royal Gala Apples",
                    Description = "Crisp, sweet, and hand-picked premium organic gala apples.",
                    Price = 399m,
                    DiscountPrice = 349m,
                    StockQuantity = 80,
                    Unit = "4 pcs (approx. 500g)",
                    ImageUrl = "https://images.unsplash.com/photo-1560806887-1e4cd0b6cbd6?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catFV
                },
                new Product
                {
                    Name = "Fresh Cavendish Bananas",
                    Description = "Naturally ripened sweet bananas packed with potassium.",
                    Price = 199m,
                    DiscountPrice = 149m,
                    StockQuantity = 120,
                    Unit = "1 bunch (6 pcs)",
                    ImageUrl = "https://images.unsplash.com/photo-1571771894821-ce9b6c11b08e?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = false,
                    CategoryId = catFV
                },
                new Product
                {
                    Name = "Hydroponic Baby Spinach",
                    Description = "Pre-washed tender baby spinach leaves ready for healthy salads.",
                    Price = 249m,
                    DiscountPrice = null,
                    StockQuantity = 50,
                    Unit = "200 g pack",
                    ImageUrl = "https://images.unsplash.com/photo-1576045057995-568f588f82fb?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = false,
                    CategoryId = catFV
                },
                new Product
                {
                    Name = "Vine Ripe Roma Tomatoes",
                    Description = "Plump and juicy vine-ripened red tomatoes ideal for cooking and salads.",
                    Price = 219m,
                    DiscountPrice = 189m,
                    StockQuantity = 95,
                    Unit = "500 g",
                    ImageUrl = "https://images.unsplash.com/photo-1592924357228-91a4daadcfea?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catFV
                },
                new Product
                {
                    Name = "Fresh Hass Avocados",
                    Description = "Creamy, ready-to-eat Hass avocados perfect for guacamole and toast.",
                    Price = 429m,
                    DiscountPrice = 389m,
                    StockQuantity = 45,
                    Unit = "2 pcs",
                    ImageUrl = "https://images.unsplash.com/photo-1523049673857-eb18f1d7b578?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catFV
                },

                // Dairy & Bread
                new Product
                {
                    Name = "Whole Organic Milk",
                    Description = "Fresh pasteurized whole milk from grass-fed cows.",
                    Price = 379m,
                    DiscountPrice = 329m,
                    StockQuantity = 60,
                    Unit = "1 Gallon",
                    ImageUrl = "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catDairy
                },
                new Product
                {
                    Name = "Artisan Sourdough Bread",
                    Description = "Slow-fermented artisan sourdough bread loaf with crunchy crust.",
                    Price = 449m,
                    DiscountPrice = null,
                    StockQuantity = 35,
                    Unit = "1 loaf (450g)",
                    ImageUrl = "https://images.unsplash.com/photo-1589367920969-ab8e050bbb04?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catDairy
                },
                new Product
                {
                    Name = "Cage-Free Brown Eggs",
                    Description = "Grade A brown eggs enriched with Omega-3 fatty acids.",
                    Price = 499m,
                    DiscountPrice = 429m,
                    StockQuantity = 75,
                    Unit = "12 count",
                    ImageUrl = "https://images.unsplash.com/photo-1582722872445-44dc5f7e3c8f?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catDairy
                },
                new Product
                {
                    Name = "Salted Creamery Butter",
                    Description = "Rich European-style churned sweet cream butter with pure sea salt.",
                    Price = 389m,
                    DiscountPrice = 339m,
                    StockQuantity = 40,
                    Unit = "250 g",
                    ImageUrl = "https://images.unsplash.com/photo-1589985270826-4b7bb135bc9d?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = false,
                    CategoryId = catDairy
                },
                new Product
                {
                    Name = "Greek Plain Yogurt",
                    Description = "Thick, high-protein strained Greek yogurt with zero added sugar.",
                    Price = 349m,
                    DiscountPrice = null,
                    StockQuantity = 50,
                    Unit = "500 g tub",
                    ImageUrl = "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = false,
                    CategoryId = catDairy
                },

                // Snacks
                new Product
                {
                    Name = "Classic Sea Salt Kettle Chips",
                    Description = "Golden batch-cooked potato chips dusted with fine sea salt.",
                    Price = 299m,
                    DiscountPrice = 249m,
                    StockQuantity = 100,
                    Unit = "150 g bag",
                    ImageUrl = "https://images.unsplash.com/photo-1566478989037-eec170784d0b?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catSnacks
                },
                new Product
                {
                    Name = "Roasted Almonds & Cashews Mix",
                    Description = "Lightly salted premium roasted nuts loaded with healthy fats and protein.",
                    Price = 699m,
                    DiscountPrice = 599m,
                    StockQuantity = 60,
                    Unit = "250 g pouch",
                    ImageUrl = "https://images.unsplash.com/photo-1599599810769-bcde5a160d32?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catSnacks
                },
                new Product
                {
                    Name = "Spicy Nacho Cheese Tortillas",
                    Description = "Crunchy corn tortilla chips with bold zesty cheddar cheese flavor.",
                    Price = 329m,
                    DiscountPrice = null,
                    StockQuantity = 80,
                    Unit = "180 g bag",
                    ImageUrl = "https://images.unsplash.com/photo-1513456852971-30c0b8199d4d?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = false,
                    CategoryId = catSnacks
                },

                // Drinks
                new Product
                {
                    Name = "Sparkling Lime Mineral Water",
                    Description = "Zero-calorie refreshing carbonated water with real lime essence.",
                    Price = 179m,
                    DiscountPrice = 129m,
                    StockQuantity = 90,
                    Unit = "1 L bottle",
                    ImageUrl = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = false,
                    CategoryId = catDrinks
                },
                new Product
                {
                    Name = "Cold Pressed Orange Juice",
                    Description = "100% pure cold-pressed Valencia orange juice with pulp.",
                    Price = 449m,
                    DiscountPrice = 399m,
                    StockQuantity = 45,
                    Unit = "500 ml",
                    ImageUrl = "https://images.unsplash.com/photo-1613478223719-2ab802602423?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catDrinks
                },
                new Product
                {
                    Name = "Iced Peach Black Tea",
                    Description = "Brewed black tea infused with natural juicy peach sweetness.",
                    Price = 249m,
                    DiscountPrice = 199m,
                    StockQuantity = 70,
                    Unit = "450 ml",
                    ImageUrl = "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = false,
                    CategoryId = catDrinks
                },

                // Instant Food
                new Product
                {
                    Name = "Spicy Ramen Noodles Bowl",
                    Description = "Instant gourmet noodle bowl with fiery chili broth and vegetables.",
                    Price = 229m,
                    DiscountPrice = 189m,
                    StockQuantity = 110,
                    Unit = "115 g bowl",
                    ImageUrl = "https://images.unsplash.com/photo-1612927601601-6638404737ce?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catInstant
                },
                new Product
                {
                    Name = "Artisan Margherita Frozen Pizza",
                    Description = "Stone-baked thin crust pizza with San Marzano tomatoes and mozzarella.",
                    Price = 749m,
                    DiscountPrice = 649m,
                    StockQuantity = 30,
                    Unit = "400 g",
                    ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catInstant
                },

                // Chocolates & Sweets
                new Product
                {
                    Name = "Dark Belgian Chocolate 70%",
                    Description = "Smooth velvety dark chocolate bar crafted with single-origin cocoa.",
                    Price = 399m,
                    DiscountPrice = 329m,
                    StockQuantity = 65,
                    Unit = "100 g bar",
                    ImageUrl = "https://images.unsplash.com/photo-1548907040-4baa42d10919?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catSweets
                },
                new Product
                {
                    Name = "Madagascar Vanilla Bean Ice Cream",
                    Description = "Ultra-rich creamy churned ice cream flecked with pure vanilla bean.",
                    Price = 599m,
                    DiscountPrice = 499m,
                    StockQuantity = 25,
                    Unit = "473 ml pint",
                    ImageUrl = "https://images.unsplash.com/photo-1570197788417-0e82375c9371?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catSweets
                },

                // Personal Care
                new Product
                {
                    Name = "Herbal Tea Tree Shampoo",
                    Description = "Clarifying and invigorating shampoo enriched with organic tea tree oil.",
                    Price = 899m,
                    DiscountPrice = 749m,
                    StockQuantity = 40,
                    Unit = "300 ml",
                    ImageUrl = "https://images.unsplash.com/photo-1535585209827-a15fcdbc4c2d?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = false,
                    CategoryId = catCare
                },
                new Product
                {
                    Name = "Hydrating Shea Butter Body Lotion",
                    Description = "Deep moisture formula with raw shea butter and vitamin E.",
                    Price = 749m,
                    DiscountPrice = null,
                    StockQuantity = 35,
                    Unit = "400 ml",
                    ImageUrl = "https://images.unsplash.com/photo-1608248597359-5f21226b9195?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = false,
                    CategoryId = catCare
                },

                // Cleaning
                new Product
                {
                    Name = "Eco-Friendly Dishwashing Gel",
                    Description = "Tough on grease, gentle on hands with natural lemon blossom extracts.",
                    Price = 399m,
                    DiscountPrice = 349m,
                    StockQuantity = 50,
                    Unit = "750 ml",
                    ImageUrl = "https://images.unsplash.com/photo-1585421514738-01798e348b17?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = false,
                    CategoryId = catClean
                },
                new Product
                {
                    Name = "Multi-Surface Lavender Disinfectant Spray",
                    Description = "Kills 99.9% of bacteria and viruses with calming French lavender aroma.",
                    Price = 449m,
                    DiscountPrice = 399m,
                    StockQuantity = 60,
                    Unit = "500 ml",
                    ImageUrl = "https://images.unsplash.com/photo-1584362917165-526a968579e8?w=600&auto=format&fit=crop&q=80",
                    InStock = true,
                    IsFeatured = true,
                    CategoryId = catClean
                }
            };

            var existingProducts = context.Products.ToList();
            foreach (var seedProduct in products)
            {
                var existingProduct = existingProducts.FirstOrDefault(product =>
                    product.Name.Equals(seedProduct.Name, StringComparison.OrdinalIgnoreCase));
                if (existingProduct != null && existingProduct.Price == seedProduct.Price / 100m)
                {
                    existingProduct.Price = seedProduct.Price;
                    existingProduct.DiscountPrice = seedProduct.DiscountPrice;
                }
            }

            var existingProductNames = existingProducts
                .Select(product => product.Name.ToLower())
                .ToHashSet();
            var missingProducts = products
                .Where(product => !existingProductNames.Contains(product.Name.ToLower()))
                .ToList();
            context.Products.AddRange(missingProducts);
            context.SaveChanges();

            var seedProductsByName = products.ToDictionary(product => product.Name, StringComparer.OrdinalIgnoreCase);
            var legacyOrderItems = context.OrderItems
                .Include(item => item.Product)
                .Where(item => item.Product != null && products.Select(product => product.Name).Contains(item.Product.Name))
                .ToList();
            var migratedOrderIds = new HashSet<int>();
            foreach (var orderItem in legacyOrderItems)
            {
                if (orderItem.Product == null || !seedProductsByName.TryGetValue(orderItem.Product.Name, out var seedProduct))
                {
                    continue;
                }

                if (orderItem.UnitPrice == seedProduct.Price / 100m ||
                    (seedProduct.DiscountPrice.HasValue && orderItem.UnitPrice == seedProduct.DiscountPrice.Value / 100m))
                {
                    orderItem.UnitPrice *= 100m;
                    orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
                    migratedOrderIds.Add(orderItem.OrderId);
                }
            }

            if (migratedOrderIds.Count > 0)
            {
                var legacyOrders = context.Orders
                    .Include(order => order.OrderItems)
                    .Where(order => migratedOrderIds.Contains(order.Id))
                    .ToList();
                foreach (var legacyOrder in legacyOrders)
                {
                    var subtotal = legacyOrder.OrderItems.Sum(item => item.TotalPrice);
                    legacyOrder.TotalAmount = subtotal + (subtotal > 0 && subtotal < 2000m ? 199m : 0m);
                }
                context.SaveChanges();
            }

            if (!context.Orders.Any())
            {
                var appleProduct = context.Products.First(product => product.Name == "Organic Royal Gala Apples");
                var milkProduct = context.Products.First(product => product.Name == "Whole Organic Milk");
                var chipsProduct = context.Products.First(product => product.Name == "Classic Sea Salt Kettle Chips");
                var applePrice = appleProduct.DiscountPrice ?? appleProduct.Price;
                var milkPrice = milkProduct.DiscountPrice ?? milkProduct.Price;
                var chipsPrice = chipsProduct.DiscountPrice ?? chipsProduct.Price;
                var appleTotal = applePrice * 2;
                var subtotal = appleTotal + milkPrice + chipsPrice;
                var deliveryFee = subtotal < 2000m ? 199m : 0m;

                var sampleOrder = new Order
                {
                    UserId = customerUser.Id,
                    OrderDate = DateTime.UtcNow.AddHours(-2),
                    Status = "Processing",
                    ShippingAddress = customerUser.Address ?? "42 Willow Street, Apt 3B, Bengaluru, Karnataka",
                    PhoneNumber = customerUser.PhoneNumber ?? "+91 98765 01928",
                    PaymentMethod = "Cash on Delivery",
                    PaymentStatus = "Pending",
                    TotalAmount = subtotal + deliveryFee,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = appleProduct.Id,
                            Quantity = 2,
                            UnitPrice = applePrice,
                            TotalPrice = appleTotal
                        },
                        new OrderItem
                        {
                            ProductId = milkProduct.Id,
                            Quantity = 1,
                            UnitPrice = milkPrice,
                            TotalPrice = milkPrice
                        },
                        new OrderItem
                        {
                            ProductId = chipsProduct.Id,
                            Quantity = 1,
                            UnitPrice = chipsPrice,
                            TotalPrice = chipsPrice
                        }
                    }
                };

                context.Orders.Add(sampleOrder);
                context.SaveChanges();
            }
        }

    }
}
