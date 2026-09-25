using System.Globalization;
using System.Windows;
using ElectronicsShop.Services;

namespace ElectronicsShop
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var culture = CultureInfo.GetCultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            BaseDbService.Close();
            base.OnExit(e);
        }
    }
}
