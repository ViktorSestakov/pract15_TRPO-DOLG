using System.Collections.ObjectModel;
using ElectronicsShop.Data;
using ElectronicsShop.Infrastructure;
using ElectronicsShop.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsShop.Services
{
    public class ProductService
    {
        private readonly ShopDbContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Product> Products { get; set; } = new();

        public void GetAll()
        {
            var list = _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Tags)
                .OrderBy(p => p.Id)
                .ToList();

            Products.Clear();
            foreach (var product in list)
                Products.Add(product);
        }

        public void Add(Product product)
        {
            AuthService.CheckManager();
            InputRules.CheckProduct(product);

            try
            {
                CheckReferences(product);
                var tags = GetTags(product);
                var newProduct = new Product();

                newProduct.Name = product.Name.Trim();
                newProduct.Description = product.Description.Trim();
                newProduct.Price = product.Price;
                newProduct.Stock = product.Stock;
                newProduct.Rating = product.Rating;
                newProduct.CreatedAt = product.CreatedAt;
                newProduct.CategoryId = product.CategoryId;
                newProduct.BrandId = product.BrandId;

                foreach (var tag in tags)
                    newProduct.Tags.Add(tag);

                _db.Products.Add(newProduct);
                Commit();
                product.Id = newProduct.Id;
            }
            finally
            {
                _db.ChangeTracker.Clear();
            }
        }

        public void Update(Product product)
        {
            AuthService.CheckManager();
            InputRules.CheckProduct(product);

            try
            {
                CheckReferences(product);
                var existingProduct = _db.Products
                    .Include(p => p.Tags)
                    .FirstOrDefault(p => p.Id == product.Id);

                if (existingProduct == null)
                    throw new Exception("Товар уже удалён. Обновите список");

                var tags = GetTags(product);

                existingProduct.Name = product.Name.Trim();
                existingProduct.Description = product.Description.Trim();
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.Rating = product.Rating;
                existingProduct.CreatedAt = product.CreatedAt;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.BrandId = product.BrandId;

                existingProduct.Tags.Clear();
                foreach (var tag in tags)
                    existingProduct.Tags.Add(tag);

                Commit();
            }
            finally
            {
                _db.ChangeTracker.Clear();
            }
        }

        public void Remove(Product product)
        {
            AuthService.CheckManager();

            try
            {
                var existingProduct = _db.Products.FirstOrDefault(p => p.Id == product.Id);
                if (existingProduct == null)
                    throw new Exception("Товар уже удалён");

                _db.Products.Remove(existingProduct);
                Commit();
            }
            finally
            {
                _db.ChangeTracker.Clear();
            }
        }

        private void CheckReferences(Product product)
        {
            if (!_db.Categories.Any(c => c.Id == product.CategoryId))
                throw new Exception("Категория уже удалена. Откройте форму заново");

            if (!_db.Brands.Any(b => b.Id == product.BrandId))
                throw new Exception("Бренд уже удалён. Откройте форму заново");
        }

        private List<Tag> GetTags(Product product)
        {
            var ids = product.Tags.Select(t => t.Id).Distinct().ToList();
            var tags = _db.Tags.Where(t => ids.Contains(t.Id)).ToList();

            if (tags.Count != ids.Count)
                throw new Exception("Один из тегов уже удалён. Откройте форму заново");

            return tags;
        }

        public int Commit()
        {
            AuthService.CheckManager();
            return _db.SaveChanges();
        }
    }
}
