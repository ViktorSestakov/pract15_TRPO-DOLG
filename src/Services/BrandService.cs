using System.Collections.ObjectModel;
using ElectronicsShop.Data;
using ElectronicsShop.Infrastructure;
using ElectronicsShop.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsShop.Services
{
    public class BrandService
    {
        private readonly ShopDbContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Brand> Brands { get; set; } = new();

        public void GetAll()
        {
            var list = _db.Brands.AsNoTracking().OrderBy(x => x.Name).ToList();
            Brands.Clear();

            foreach (var item in list)
                Brands.Add(item);
        }

        public void Add(Brand item)
        {
            AuthService.CheckManager();
            InputRules.CheckName(item.Name);
            string name = item.Name.Trim();

            try
            {
                if (_db.Brands.Any(x => x.Name.ToLower() == name.ToLower()))
                    throw new Exception("Такое название уже существует");

                var newItem = new Brand();
                newItem.Name = name;
                _db.Brands.Add(newItem);
                Commit();
                item.Id = newItem.Id;
            }
            finally
            {
                _db.ChangeTracker.Clear();
            }
        }

        public void Update(Brand item)
        {
            AuthService.CheckManager();
            InputRules.CheckName(item.Name);
            string name = item.Name.Trim();

            try
            {
                if (_db.Brands.Any(x => x.Name.ToLower() == name.ToLower() && x.Id != item.Id))
                    throw new Exception("Такое название уже существует");

                var existingItem = _db.Brands.FirstOrDefault(x => x.Id == item.Id);
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

        public void Remove(Brand item)
        {
            AuthService.CheckManager();

            try
            {
                if (_db.Products.Any(p => p.BrandId == item.Id))
                    throw new Exception("Нельзя удалить бренд, пока есть связанные товары");

                var existingItem = _db.Brands.FirstOrDefault(x => x.Id == item.Id);
                if (existingItem == null)
                    throw new Exception("Запись уже удалена");

                _db.Brands.Remove(existingItem);
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
