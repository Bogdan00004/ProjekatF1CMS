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
        public bool _isShuttingDown = false;
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
        public void SaveDataAsXML()
        {
            serializer.SerializeObject<ObservableCollection<F1Team>>(Teams, "Data/F1Teams.xml");
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_isShuttingDown)
                return;

            Ookii.Dialogs.Wpf.TaskDialog dialog = new Ookii.Dialogs.Wpf.TaskDialog();
            dialog.WindowTitle = "Exit Confirmation";
            dialog.MainInstruction = "Are you sure you want to exit?";
            dialog.Content = "All team data will be saved upon exit.";
            dialog.MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Warning;

            Ookii.Dialogs.Wpf.TaskDialogButton yesButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Yes, Exit");
            Ookii.Dialogs.Wpf.TaskDialogButton noButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Cancel");

            dialog.Buttons.Add(yesButton);
            dialog.Buttons.Add(noButton);

            Ookii.Dialogs.Wpf.TaskDialogButton result = dialog.ShowDialog();

            if (result == yesButton)
            {
                _isShuttingDown = true;
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