using Notification.Wpf;
using ProjekatF1CMS.Helpers;
using ProjekatF1CMS.Model;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

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
            UpdateNoTeamsMessage();
            Teams.CollectionChanged += (s, e) =>
            {
                UpdateNoTeamsMessage();
                foreach (F1Team team in Teams)
                    team.PropertyChanged += (ts, te) => { if (te.PropertyName == "IsSelected") UpdateSelectAllCheckBox(); };
            };
            foreach (F1Team team in Teams)
                team.PropertyChanged += (ts, te) => { if (te.PropertyName == "IsSelected") UpdateSelectAllCheckBox(); };
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
        private void UpdateSelectAllCheckBox()
        {
            int selectedCount = Teams.Count(t => t.IsSelected);
            if (selectedCount == 0)
                SelectAllCheckBox.IsChecked = false;
            else if (selectedCount == Teams.Count)
                SelectAllCheckBox.IsChecked = true;
            else
                SelectAllCheckBox.IsChecked = null; // Ako je 1 odselektovan, a 2 selektovana bice unchecked
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
                mainWindow.ShowToastNotification(new ToastNotification("Warning","No teams selected for deletion!", NotificationType.Warning));
                return;
            }

            Ookii.Dialogs.Wpf.TaskDialog dialog = new Ookii.Dialogs.Wpf.TaskDialog();
            dialog.WindowTitle = "Confirm Deletion";
            dialog.MainInstruction = $"Delete {selectedTeams.Count} team(s)?";
            dialog.Content = "This action cannot be undone. Selected teams and their files will be permanently deleted.";
            dialog.MainIcon = Ookii.Dialogs.Wpf.TaskDialogIcon.Warning;

            Ookii.Dialogs.Wpf.TaskDialogButton yesButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Yes, Delete");
            Ookii.Dialogs.Wpf.TaskDialogButton noButton = new Ookii.Dialogs.Wpf.TaskDialogButton("Cancel");

            dialog.Buttons.Add(yesButton);
            dialog.Buttons.Add(noButton);

            Ookii.Dialogs.Wpf.TaskDialogButton result = dialog.ShowDialog();

            if (result == yesButton)
            {
                foreach (var team in selectedTeams)
                {
                    if (!string.IsNullOrEmpty(team.DescriptionFilePath))
                    {
                        string fullRtfPath = System.IO.Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            team.DescriptionFilePath);
                        if (File.Exists(fullRtfPath))
                                File.Delete(fullRtfPath);
                    }
                    Teams.Remove(team);
                }

                SelectAllCheckBox.IsChecked = false;
                UpdateNoTeamsMessage();

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
            mainWindow.Hide();

            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
        private void UpdateNoTeamsMessage()
        {
            NoTeamsLabel.Visibility = Teams.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        private void TeamNameText_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            Hyperlink hl = (Hyperlink)tb.Inlines.FirstInline;
            hl.Foreground = Brushes.White;
            hl.TextDecorations = TextDecorations.Underline;
        }

        private void TeamNameText_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            Hyperlink hl = (Hyperlink)tb.Inlines.FirstInline;
            hl.Foreground = (SolidColorBrush)Application.Current.Resources["AccentColor"];
            hl.TextDecorations = null;
        }

    }
}
