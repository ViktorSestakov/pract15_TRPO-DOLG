using System.ComponentModel;
using System.Windows;
using ElectronicsShop.Infrastructure;
using ElectronicsShop.Models;
using ElectronicsShop.Services;

namespace ElectronicsShop.Views
{
    public partial class NameEditorWindow : Window
    {
        private CategoryService categoryService = new();
        private BrandService brandService = new();
        private TagService tagService = new();
        private DictionaryItem _item;
        private string type;
        private string originalName = "";
        private bool isEdit;
        private bool isSaved;

        public NameEditorWindow(string dictionaryType, DictionaryItem? item = null)
        {
            AuthService.CheckManager();
            InitializeComponent();
            type = dictionaryType;
            _item = new DictionaryItem();

            if (item != null)
            {
                _item.Id = item.Id;
                _item.Name = item.Name;
                originalName = item.Name;
                isEdit = true;
            }

            string name;
            switch (type)
            {
                case "categories":
                    name = "категории";
                    break;
                case "brands":
                    name = "бренда";
                    break;
                case "tags":
                    name = "тега";
                    break;
                default:
                    throw new ArgumentException("Неизвестный справочник");
            }

            if (isEdit)
                Title = "Редактирование " + name;
            else
                Title = "Добавление " + name;

            Heading.Text = Title;
            DataContext = _item;
        }

        private bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(_item.Name))
            {
                MessageBox.Show(this, "Введите название", "Ошибка");
                return false;
            }
            return true;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (!IsValid())
                return;

            if (isEdit && MessageBox.Show(this, "Сохранить новое название? Оно изменится во всех связанных товарах.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            try
            {
                switch (type)
                {
                    case "categories":
                        var category = new Category { Id = _item.Id, Name = _item.Name };
                        if (isEdit)
                            categoryService.Update(category);
                        else
                            categoryService.Add(category);
                        break;
                    case "brands":
                        var brand = new Brand { Id = _item.Id, Name = _item.Name };
                        if (isEdit)
                            brandService.Update(brand);
                        else
                            brandService.Add(brand);
                        break;
                    case "tags":
                        var tag = new Tag { Id = _item.Id, Name = _item.Name };
                        if (isEdit)
                            tagService.Update(tag);
                        else
                            tagService.Add(tag);
                        break;
                }

                isSaved = true;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseError.GetMessage(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowClosing(object? sender, CancelEventArgs e)
        {
            if (!isSaved && NameBox.Text != originalName)
            {
                if (MessageBox.Show(this, "Закрыть форму без сохранения изменений?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                    e.Cancel = true;
            }
        }
    }
}
