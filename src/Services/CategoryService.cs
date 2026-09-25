using System.Collections.ObjectModel;
using ElectronicsShop.Data;
using ElectronicsShop.Infrastructure;
using ElectronicsShop.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsShop.Services
{
    public class CategoryService
    {
        private readonly ShopDbContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Category> Categories { get; set; } = new();

        public void GetAll()
        {
            var list = _db.Categories.AsNoTracking().OrderBy(x => x.Name).ToList();
            Categories.Clear();

            foreach (var item in list)
                Categories.Add(item);
        }

        public void Add(Category item)
        {
            AuthService.CheckManager();
            InputRules.CheckName(item.Name);
            string name = item.Name.Trim();

            try
            {
                if (_db.Categories.Any(x => x.Name.ToLower() == name.ToLower()))
                    throw new Exception("Такое название уже существует");

                var newItem = new Category();
                newItem.Name = name;
                _db.Categories.Add(newItem);
                Commit();
                item.Id = newItem.Id;
            }
            finally
            {
                _db.ChangeTracker.Clear();
            }
        }

        public void Update(Category item)
        {
            AuthService.CheckManager();
            InputRules.CheckName(item.Name);
            string name = item.Name.Trim();

            try
            {
                if (_db.Categories.Any(x => x.Name.ToLower() == name.ToLower() && x.Id != item.Id))
                    throw new Exception("Такое название уже существует");

                var existingItem = _db.Categories.FirstOrDefault(x => x.Id == item.Id);
                if (existingItem == null)
                    throw new Exception("Запись уже удалена");

                existingItem.Name = name;
                Commit();
            }
            finally
            {
                _db.ChangeTracker.Clear();
            }
        }

        public void Remove(Category item)
        {
            AuthService.CheckManager();

            try
            {
                if (_db.Products.Any(p => p.CategoryId == item.Id))
                    throw new Exception("Нельзя удалить категорию, пока есть связанные товары");

                var existingItem = _db.Categories.FirstOrDefault(x => x.Id == item.Id);
                if (existingItem == null)
                    throw new Exception("Запись уже удалена");

                _db.Categories.Remove(existingItem);
                Commit();
            }
            finally
            {
                _db.ChangeTracker.Clear();
            }
        }

        public int Commit()
        {
            AuthService.CheckManager();
            return _db.SaveChanges();
        }
    }
}
