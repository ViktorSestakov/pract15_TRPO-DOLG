using System.Collections.ObjectModel;
using ElectronicsShop.Data;
using ElectronicsShop.Infrastructure;
using ElectronicsShop.Models;
using Microsoft.EntityFrameworkCore;

namespace ElectronicsShop.Services
{
    public class TagService
    {
        private readonly ShopDbContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Tag> Tags { get; set; } = new();

        public void GetAll()
        {
            var list = _db.Tags.AsNoTracking().OrderBy(x => x.Name).ToList();
            Tags.Clear();

            foreach (var item in list)
                Tags.Add(item);
        }

        public void Add(Tag item)
        {
            AuthService.CheckManager();
            InputRules.CheckName(item.Name);
            string name = item.Name.Trim();

            try
            {
                if (_db.Tags.Any(x => x.Name.ToLower() == name.ToLower()))
                    throw new Exception("Такое название уже существует");

                var newItem = new Tag();
                newItem.Name = name;
                _db.Tags.Add(newItem);
                Commit();
                item.Id = newItem.Id;
            }
            finally
            {
                _db.ChangeTracker.Clear();
            }
        }

        public void Update(Tag item)
        {
            AuthService.CheckManager();
            InputRules.CheckName(item.Name);
            string name = item.Name.Trim();

            try
            {
                if (_db.Tags.Any(x => x.Name.ToLower() == name.ToLower() && x.Id != item.Id))
                    throw new Exception("Такое название уже существует");

                var existingItem = _db.Tags.FirstOrDefault(x => x.Id == item.Id);
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

        public void Remove(Tag item)
        {
            AuthService.CheckManager();

            try
            {
                var existingItem = _db.Tags.FirstOrDefault(x => x.Id == item.Id);
                if (existingItem == null)
                    throw new Exception("Запись уже удалена");

                _db.Tags.Remove(existingItem);
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
