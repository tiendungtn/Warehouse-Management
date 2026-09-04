using QuanLyKho.Models;

namespace QuanLyKho.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any()) return; // CSDL đã có dữ liệu

            // Tạo các tài khoản khởi tạo (Mật khẩu mặc định: 123456)
            var users = new User[]
            {
                new User { Username = "admin", Password = BCrypt.Net.BCrypt.HashPassword("123456"), Fullname = "Hệ Thống Quản Trị", Role = "Admin" },
                new User { Username = "manager", Password = BCrypt.Net.BCrypt.HashPassword("123456"), Fullname = "Trưởng Kho Nguyễn Văn B", Role = "Manager" },
                new User { Username = "staff", Password = BCrypt.Net.BCrypt.HashPassword("123456"), Fullname = "Nhân Viên Kho Trần Văn C", Role = "Staff" }
            };
            context.Users.AddRange(users);

            // Tạo danh mục mẫu
            var categories = new Category[]
            {
                new Category { CategoryName = "Thiết bị điện tử", Description = "Điện thoại, Laptop, Linh kiện" },
                new Category { CategoryName = "Gia dụng", Description = "Đồ dùng phòng bếp, gia đình" }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // Tạo hàng hoá mẫu
            var products = new Product[]
            {
                new Product { ProductCode = "SP001", ProductName = "Màn hình Dell 24 inch", CategoryId = categories[0].Id, Unit = "Chiếc", Price = 3500000, StockQuantity = 20 },
                new Product { ProductCode = "SP002", ProductName = "Bàn phím cơ DareU", CategoryId = categories[0].Id, Unit = "Cái", Price = 650000, StockQuantity = 50 }
            };
            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
