using System.Windows;
using ElectronicsShop.Infrastructure;
using ElectronicsShop.Services;

namespace ElectronicsShop.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            if (!AuthService.Login(PinBox.Password))
            {
                ErrorText.Text = "Неверный ПИН-код";
                PinBox.Clear();
                PinBox.Focus();
                return;
            }

            OpenCatalog();
        }

        private void EnterVisitor(object sender, RoutedEventArgs e)
        {
            AuthService.EnterVisitor();
            OpenCatalog();
        }

        private void OpenCatalog()
        {
            try
            {
                var window = new MainWindow();
                Application.Current.MainWindow = window;
                window.Show();
                Close();
            }
            catch (Exception ex)
            {
                BaseDbService.Close();
                MessageBox.Show(this, DatabaseError.GetMessage(ex), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
