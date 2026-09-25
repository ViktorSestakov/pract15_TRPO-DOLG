using ElectronicsShop.Data;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsShop.Services
{
    public class BaseDbService
    {
        private static BaseDbService? instance;
        private ShopDbContext context;

        private BaseDbService()
        {
            string? connection = "Server=localhost;Database=ElectronicsShopDb;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=10";

            if (string.IsNullOrWhiteSpace(connection))
                throw new Exception("Укажите строку подключения!");

            var options = new DbContextOptionsBuilder<ShopDbContext>();
            options.UseSqlServer(connection, sql => sql.CommandTimeout(20));
            context = new ShopDbContext(options.Options);
        }

        public static BaseDbService Instance
        {
            get
            {
                if (instance == null)
                    instance = new BaseDbService();

                return instance;
            }
        }

        public ShopDbContext Context => context;

        public static void Close()
        {
            if (instance != null)
            {
                instance.context.Dispose();
                instance = null;
            }
        }
    }
}
