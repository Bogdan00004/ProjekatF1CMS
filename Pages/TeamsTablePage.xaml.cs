using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Notification.Wpf;
using ProjekatF1CMS.Helpers;
using ProjekatF1CMS.Model;
using System.Collections.ObjectModel;
using System.IO;

namespace ProjekatF1CMS.Pages
{
    /// <summary>
    /// Interaction logic for TeamsTablePage.xaml
    /// </summary>
    public partial class TeamsTablePage : Page
    {
        public ObservableCollection<F1Team> Teams { get; set; }
        private MainWindow mainWindow;
        public TeamsTablePage()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;

            Teams = mainWindow.Teams;

            DataContext = this;

            AdjustUIForUserRole();
        }
        private void AdjustUIForUserRole()
        {
            if (mainWindow.LoggedInUser.Role == UserRole.Visitor)
            {
                AddTeamButton.Visibility = Visibility.Collapsed;
                DeleteTeamButton.Visibility = Visibility.Collapsed;
                SelectAllCheckBox.Visibility = Visibility.Collapsed;
                TeamsDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            }
        }
        private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var team in Teams)
            {
                team.IsSelected = true;
            }
            TeamsDataGrid.Items.Refresh();
        }

        private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var team in Teams)
            {
                team.IsSelected = false;
            }
            TeamsDataGrid.Items.Refresh();
        }

        private void AddTeamButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.NavigateToPage(new AddEditTeamPage());
        }

        private void DeleteTeamButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTeams = Teams.Where(t => t.IsSelected).ToList();

            if (selectedTeams.Count == 0)
            {
                mainWindow.ShowToastNotification(new ToastNotification("Warning", "No teams selected for deletion!", NotificationType.Warning));
                return;
            }

            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete {selectedTeams.Count} team(s)?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var team in selectedTeams)
                {
                    if (!string.IsNullOrEmpty(team.DescriptionFilePath))
                    {
                        string fullRtfPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,team.DescriptionFilePath);

                        if (File.Exists(fullRtfPath))
                        {
                            File.Delete(fullRtfPath);
                        }
                    }

                    Teams.Remove(team);
                }

                SelectAllCheckBox.IsChecked = false;

                mainWindow.ShowToastNotification(new ToastNotification("Success", "Selected teams deleted successfully!", NotificationType.Success));
            }
        }

        private void TeamHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = (Hyperlink)sender;
            F1Team selectedTeam = (F1Team)hyperlink.DataContext;

            if (mainWindow.LoggedInUser.Role == UserRole.Admin)
            {
                mainWindow.NavigateToPage(new AddEditTeamPage(selectedTeam));
            }
            else
            {
                mainWindow.NavigateToPage(new TeamDetailsPage(selectedTeam));
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.LoggedInUser = null;
            this.NavigationService.Navigate(new System.Uri("Pages/LoginPage.xaml",System.UriKind.RelativeOrAbsolute));
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

    }
}
