using System.Windows;

namespace ProjekatF1CMS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }

}
