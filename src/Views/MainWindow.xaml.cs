using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ElectronicsShop.Infrastructure;
using ElectronicsShop.Models;
using ElectronicsShop.Services;

namespace ElectronicsShop.Views
{
    public partial class MainWindow : Window
    {
        public ProductService service { get; set; } = new();
        public Product? selected { get; set; }
        public ObservableCollection<Category> Categories { get; set; } = new();
        public ObservableCollection<Brand> Brands { get; set; } = new();

        private CategoryService categoryService = new();
        private BrandService brandService = new();
        private ICollectionView productsView;
        private bool isLoading = true;
        private bool validPrice = true;
        private string search = "";
        private int categoryId;
        private int brandId;
        private decimal? minPrice;
        private decimal? maxPrice;

        public MainWindow()
        {
            productsView = CollectionViewSource.GetDefaultView(service.Products);
            productsView.Filter = FilterProducts;

            InitializeComponent();
            DataContext = this;

            if (AuthService.IsManager)
            {
                RoleText.Text = "Роль: Менеджер";
            }
            else
            {
                RoleText.Text = "Роль: Посетитель";
                ManagerPanel.Visibility = Visibility.Collapsed;
                EditButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadList(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void LoadProducts()
        {
            int oldCategory = categoryId;
            int oldBrand = brandId;
            int oldProduct = 0;
            if (selected != null)
                oldProduct = selected.Id;

            isLoading = true;
            try
            {
                categoryService.GetAll();
                brandService.GetAll();
                service.GetAll();

                Categories.Clear();
                Categories.Add(new Category { Id = 0, Name = "Все категории" });
                foreach (var category in categoryService.Categories)
                    Categories.Add(category);

                Brands.Clear();
                Brands.Add(new Brand { Id = 0, Name = "Все бренды" });
                foreach (var brand in brandService.Brands)
                    Brands.Add(brand);

                if (!Categories.Any(c => c.Id == oldCategory))
                    oldCategory = 0;
                if (!Brands.Any(b => b.Id == oldBrand))
                    oldBrand = 0;

                CategoryBox.SelectedValue = oldCategory;
                BrandBox.SelectedValue = oldBrand;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseError.GetMessage(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isLoading = false;
            }

            RefreshFilters();
            ApplySort();
            ProductsList.SelectedItem = service.Products.FirstOrDefault(p => p.Id == oldProduct);
        }

        private bool FilterProducts(object obj)
        {
            Product? product = obj as Product;
            if (product == null || !validPrice)
                return false;

            return InputRules.Matches(product, search, categoryId, brandId, minPrice, maxPrice);
        }

        private void RefreshFilters()
        {
            if (isLoading)
                return;

            search = SearchBox.Text;
            categoryId = 0;
            brandId = 0;

            Category? category = CategoryBox.SelectedItem as Category;
            Brand? brand = BrandBox.SelectedItem as Brand;
            if (category != null)
                categoryId = category.Id;
            if (brand != null)
                brandId = brand.Id;

            string error;
            validPrice = InputRules.TryPriceRange(PriceFromBox.Text, PriceToBox.Text,
                out minPrice, out maxPrice, out error);
            FilterError.Text = error;
            productsView.Refresh();

            if (ProductsList.Items.Count == 0)
                EmptyText.Visibility = Visibility.Visible;
            else
                EmptyText.Visibility = Visibility.Collapsed;
        }

        private void SearchChanged(object sender, TextChangedEventArgs e)
        {
            RefreshFilters();
        }

        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshFilters();
        }

        private void SortChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isLoading)
                ApplySort();
        }

        private void ApplySort()
        {
            ComboBoxItem? item = SortBox.SelectedItem as ComboBoxItem;
            if (item == null)
                return;

            productsView.SortDescriptions.Clear();
            switch (item.Tag.ToString())
            {
                case "NameAsc":
                    productsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                    break;
                case "NameDesc":
                    productsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
                    break;
                case "PriceAsc":
                    productsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Ascending));
                    break;
                case "PriceDesc":
                    productsView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Descending));
                    break;
                case "StockAsc":
                    productsView.SortDescriptions.Add(new SortDescription("Stock", ListSortDirection.Ascending));
                    break;
                case "StockDesc":
                    productsView.SortDescriptions.Add(new SortDescription("Stock", ListSortDirection.Descending));
                    break;
            }
            productsView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending));
        }

        private void Reset(object sender, RoutedEventArgs e)
        {
            isLoading = true;
            SearchBox.Clear();
            PriceFromBox.Clear();
            PriceToBox.Clear();
            CategoryBox.SelectedValue = 0;
            BrandBox.SelectedValue = 0;
            SortBox.SelectedIndex = 0;
            isLoading = false;
            RefreshFilters();
            ApplySort();
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            if (!AuthService.IsManager)
                return;

            var window = new ProductEditorWindow();
            window.Owner = this;
            if (window.ShowDialog() == true)
                LoadProducts();
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            if (!AuthService.IsManager)
                return;
            if (selected == null)
            {
                MessageBox.Show(this, "Выберите товар", "Редактирование");
                return;
            }

            var window = new ProductEditorWindow(selected);
            window.Owner = this;
            if (window.ShowDialog() == true)
                LoadProducts();
        }

        private void View(object sender, RoutedEventArgs e)
        {
            ViewProduct();
        }

        private void ViewDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(ProductsList, e.OriginalSource as DependencyObject);
            if (item is ListViewItem)
                ViewProduct();
        }

        private void ViewProduct()
        {
            if (selected == null)
            {
                MessageBox.Show(this, "Выберите товар", "Просмотр");
                return;
            }

            var window = new ProductEditorWindow(selected, true);
            window.Owner = this;
            window.ShowDialog();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (!AuthService.IsManager)
                return;
            if (selected == null)
            {
                MessageBox.Show(this, "Выберите товар", "Удаление");
                return;
            }

            if (MessageBox.Show(this, $"Удалить товар «{selected.Name}»?\nОтменить удаление нельзя.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            try
            {
                service.Remove(selected);
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, DatabaseError.GetMessage(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoToCategories(object sender, RoutedEventArgs e)
        {
            OpenDictionary("categories");
        }

        private void GoToBrands(object sender, RoutedEventArgs e)
        {
            OpenDictionary("brands");
        }

        private void GoToTags(object sender, RoutedEventArgs e)
        {
            OpenDictionary("tags");
        }

        private void OpenDictionary(string type)
        {
            if (!AuthService.IsManager)
                return;

            var window = new DictionaryWindow(type);
            window.Owner = this;
            window.ShowDialog();
            LoadProducts();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            AuthService.EnterVisitor();
            var window = new LoginWindow();
            Application.Current.MainWindow = window;
            window.Show();
            Close();
        }
    }
}
