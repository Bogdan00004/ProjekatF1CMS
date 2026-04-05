using Notification.Wpf;
using ProjekatF1CMS.Helpers;
using ProjekatF1CMS.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ProjekatF1CMS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotificationManager notificationManager;
        private DataIO serializer = new DataIO();

        public ObservableCollection<F1Team> Teams { get; set; }
        public User LoggedInUser { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            notificationManager = new NotificationManager();
            Teams = serializer.DeSerializeObject<ObservableCollection<F1Team>>("Data/F1Teams.xml");
            if (Teams == null)
            {
                Teams = new ObservableCollection<F1Team>();
            }
           
        }
        public void ShowToastNotification(ToastNotification toastNotification)
        {
            notificationManager.Show(toastNotification.Title, toastNotification.Message, toastNotification.Type, "WindowNotificationArea");
        }
        public void NavigateToPage(System.Windows.Controls.Page page)
        {
            MainFrame.Navigate(page);
        }
        private void SaveDataAsXML()
        {
            serializer.SerializeObject<ObservableCollection<F1Team>>(Teams, "Data/F1Teams.xml");
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to exit?", "Exit Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SaveDataAsXML();
                Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}