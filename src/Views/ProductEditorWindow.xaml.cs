using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ElectronicsShop.Infrastructure;
using ElectronicsShop.Models;
using ElectronicsShop.Services;

namespace ElectronicsShop.Views
{
    public partial class ProductEditorWindow : Window
    {
        private ProductService _service = new();
        private CategoryService categoryService = new();
        private BrandService brandService = new();
        private TagService tagService = new();
        private Product _product;
        private bool isEdit;
        private bool isReadOnly;
        private bool isLoading = true;
        private bool isChanged;
        private bool isSaved;

        public ProductEditorWindow(Product? product = null, bool readOnly = false)
        {
            if (!readOnly)
                AuthService.CheckManager();

            InitializeComponent();
            isReadOnly = readOnly;
            _product = new Product();

            if (product != null)
            {
                isEdit = true;
                _product.Id = product.Id;
                _product.Name = product.Name;
                _product.Description = product.Description;
                _product.Price = product.Price;
                _product.Stock = product.Stock;
                _product.Rating = product.Rating;
                _product.CreatedAt = product.CreatedAt;
                _product.CategoryId = product.CategoryId;
                _product.BrandId = product.BrandId;

                foreach (var tag in product.Tags)
                    _product.Tags.Add(tag);

                Title = "Редактирование товара";
            }
            else
            {
                _product.Name = "";
                _product.Description = "";
                _product.CreatedAt = DateOnly.FromDateTime(DateTime.Today);
                Title = "Добавление товара";
            }

            if (readOnly)
            {
                Title = "Просмотр товара";
                SaveButton.Visibility = Visibility.Collapsed;
                BackButton.Content = "Закрыть";
            }

            Heading.Text = Title;
            DataContext = _product;
        }

        private void LoadData(object sender, RoutedEventArgs e)
        {
            try
            {
                categoryService.GetAll();
                brandService.GetAll();
                tagService.GetAll();
                CategoryBox.ItemsSource = categoryService.Categories;
                BrandBox.ItemsSource = brandService.Brands;
                TagsList.ItemsSource = tagService.Tags;

                CategoryBox.SelectedValue = _product.CategoryId;
                BrandBox.SelectedValue = _product.BrandId;
                CreatedPicker.SelectedDate = _product.CreatedAt.ToDateTime(TimeOnly.MinValue);

                if (isEdit)
                {
                    PriceBox.Text = _product.Price.ToString("F2");
                    StockBox.Text = _product.Stock.ToString();
                    RatingBox.Text = _product.Rating.ToString("F1");
                }

                foreach (var tag in tagService.Tags)
                {
                    if (_product.Tags.Any(t => t.Id == tag.Id))
                        TagsList.SelectedItems.Add(tag);
                }

                FormGrid.IsEnabled = true;
                SaveButton.IsEnabled = true;

                if (isReadOnly)
                {
                    NameBox.IsReadOnly = true;
                    DescriptionBox.IsReadOnly = true;
                    PriceBox.IsReadOnly = true;
                    StockBox.IsReadOnly = true;
                    RatingBox.IsReadOnly = true;
                    CategoryBox.IsEnabled = false;
                    BrandBox.IsEnabled = false;
                    CreatedPicker.IsEnabled = false;
                    TagsList.IsEnabled = false;
                }

                isLoading = false;
                isChanged = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseError.GetMessage(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(_product.Name))
            {
                MessageBox.Show(this, "Введите название товара", "Ошибка");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_product.Description))
            {
                MessageBox.Show(this, "Введите описание товара", "Ошибка");
                return false;
            }

            decimal price;
            if (!InputRules.TryDecimal(PriceBox.Text, 2, out price) || price < 0 || price > InputRules.MaxPrice)
            {
                MessageBox.Show(this, "Введите цену от 0 до 9 999 999 999,99, до 2 знаков после запятой", "Ошибка");
                return false;
            }

            int stock;
            if (!int.TryParse(StockBox.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out stock))
            {
                MessageBox.Show(this, "Остаток должен быть целым числом от 0 до 2 147 483 647", "Ошибка");
                return false;
            }

            decimal rating;
            if (!InputRules.TryDecimal(RatingBox.Text, 1, out rating) || rating < 0 || rating > 5)
            {
                MessageBox.Show(this, "Введите рейтинг от 0 до 5, до 1 знака после запятой", "Ошибка");
                return false;
            }

            DateTime date;
            if (CreatedPicker.SelectedDate == null || !DateTime.TryParse(CreatedPicker.Text, out date))
            {
                MessageBox.Show(this, "Выберите правильную дату", "Ошибка");
                return false;
            }

            Category? category = CategoryBox.SelectedItem as Category;
            Brand? brand = BrandBox.SelectedItem as Brand;
            if (category == null || brand == null)
            {
                MessageBox.Show(this, "Выберите категорию и бренд", "Ошибка");
                return false;
            }

            _product.Price = price;
            _product.Stock = stock;
            _product.Rating = rating;
            _product.CreatedAt = DateOnly.FromDateTime(date);
            _product.CategoryId = category.Id;
            _product.BrandId = brand.Id;
            _product.Tags.Clear();
            foreach (Tag tag in TagsList.SelectedItems)
                _product.Tags.Add(tag);

            return true;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            if (isReadOnly || isLoading || !IsValid())
                return;

            if (isEdit && MessageBox.Show(this, "Сохранить изменения товара? Предыдущие значения будут заменены.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            try
            {
                if (isEdit)
                    _service.Update(_product);
                else
                    _service.Add(_product);

                isSaved = true;
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseError.GetMessage(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isLoading)
                isChanged = true;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoading)
                isChanged = true;
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowClosing(object? sender, CancelEventArgs e)
        {
            if (!isReadOnly && !isSaved && isChanged)
            {
                var result = MessageBox.Show(this, "Закрыть форму без сохранения изменений?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (result != MessageBoxResult.Yes)
                    e.Cancel = true;
            }
        }
    }
}
