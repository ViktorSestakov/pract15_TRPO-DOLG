using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ElectronicsShop.Infrastructure;
using ElectronicsShop.Models;
using ElectronicsShop.Services;

namespace ElectronicsShop.Views
{
    public partial class DictionaryWindow : Window
    {
        public ObservableCollection<DictionaryItem> Items { get; set; } = new();
        public DictionaryItem? selected { get; set; }

        private CategoryService categoryService = new();
        private BrandService brandService = new();
        private TagService tagService = new();
        private string type;

        public DictionaryWindow(string dictionaryType)
        {
            AuthService.CheckManager();
            InitializeComponent();
            type = dictionaryType;
            DataContext = this;

            switch (type)
            {
                case "categories":
                    Title = "Категории товаров";
                    break;
                case "brands":
                    Title = "Бренды товаров";
                    break;
                case "tags":
                    Title = "Теги товаров";
                    break;
                default:
                    throw new ArgumentException("Неизвестный справочник");
            }
            Heading.Text = Title;
        }

        private void LoadList(object sender, RoutedEventArgs e)
        {
            LoadItems();
        }

        private void LoadItems()
        {
            try
            {
                var list = new List<DictionaryItem>();
                switch (type)
                {
                    case "categories":
                        categoryService.GetAll();
                        foreach (var category in categoryService.Categories)
                            list.Add(new DictionaryItem { Id = category.Id, Name = category.Name });
                        break;
                    case "brands":
                        brandService.GetAll();
                        foreach (var brand in brandService.Brands)
                            list.Add(new DictionaryItem { Id = brand.Id, Name = brand.Name });
                        break;
                    case "tags":
                        tagService.GetAll();
                        foreach (var tag in tagService.Tags)
                            list.Add(new DictionaryItem { Id = tag.Id, Name = tag.Name });
                        break;
                }

                Items.Clear();
                foreach (var item in list)
                    Items.Add(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseError.GetMessage(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            var window = new NameEditorWindow(type);
            window.Owner = this;
            if (window.ShowDialog() == true)
                LoadItems();
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            EditItem();
        }

        private void EditDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement(ItemsList, e.OriginalSource as DependencyObject);
            if (row is ListViewItem)
                EditItem();
        }

        private void EditItem()
        {
            if (selected == null)
            {
                MessageBox.Show(this, "Выберите запись", "Редактирование");
                return;
            }

            var window = new NameEditorWindow(type, selected);
            window.Owner = this;
            if (window.ShowDialog() == true)
                LoadItems();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (selected == null)
            {
                MessageBox.Show(this, "Выберите запись", "Удаление");
                return;
            }

            string message = $"Удалить «{selected.Name}»?\nОтменить удаление нельзя.";
            if (type == "tags")
                message += "\nТег будет снят со всех товаров. Сами товары останутся.";

            if (MessageBox.Show(this, message, "Подтверждение", MessageBoxButton.YesNo,
                MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            try
            {
                switch (type)
                {
                    case "categories":
                        categoryService.Remove(new Category { Id = selected.Id });
                        break;
                    case "brands":
                        brandService.Remove(new Brand { Id = selected.Id });
                        break;
                    case "tags":
                        tagService.Remove(new Tag { Id = selected.Id });
                        break;
                }
                LoadItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseError.GetMessage(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            LoadItems();
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
