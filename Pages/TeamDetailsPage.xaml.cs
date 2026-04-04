using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
using ProjekatF1CMS.Model;

namespace ProjekatF1CMS.Pages
{
    /// <summary>
    /// Interaction logic for TeamDetailsPage.xaml
    /// </summary>
    public partial class TeamDetailsPage : Page
    {
        private MainWindow mainWindow;
        public TeamDetailsPage(F1Team team)
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            PopulateDetails(team);
        }
        private void PopulateDetails(F1Team team)
        {
            TeamNameValue.Content = team.TeamName;
            HeadquartersValue.Content = team.Headquarters;
            EngineSupplierValue.Content = team.EngineSupplier;
            YearFoundedValue.Content = team.YearFounded.ToString();
            ChampionshipsValue.Content = team.NumberOfChampionships.ToString();
            DateAddedValue.Content = team.DateAdded.ToString("dd.MM.yyyy HH:mm");

            if (!string.IsNullOrEmpty(team.LogoPath))
            {
                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, team.LogoPath);
                if (File.Exists(fullPath))
                {
                    TeamLogoImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(fullPath, UriKind.Absolute));
                }
            }

            if (!string.IsNullOrEmpty(team.DescriptionFilePath))
            {
                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, team.DescriptionFilePath);
                if (File.Exists(fullPath))
                {
                    using (FileStream fs = new FileStream(fullPath, FileMode.Open))
                    {
                        TextRange range = new TextRange(DescriptionRichTextBox.Document.ContentStart, DescriptionRichTextBox.Document.ContentEnd);
                        range.Load(fs, DataFormats.Rtf);
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.NavigateToPage(new TeamsTablePage());
        }

    }
}
