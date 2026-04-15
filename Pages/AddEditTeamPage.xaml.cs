using Microsoft.Win32;
using Notification.Wpf;
using ProjekatF1CMS.Helpers;
using ProjekatF1CMS.Model;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProjekatF1CMS.Pages
{
    /// <summary>
    /// Interaction logic for AddEditTeamPage.xaml
    /// </summary>
    public partial class AddEditTeamPage : Page
    {
        private MainWindow mainWindow;
        private F1Team existingTeam;
        private bool isEditMode = false;

        private string _teamNamePlaceholder = "e.g. Red Bull Racing";
        private string _headquartersPlaceholder = "e.g. Milton Keynes, UK";
        private string _engineSupplierPlaceholder = "e.g. Honda RBPT";
        private string _yearFoundedPlaceholder = "e.g. 1998";
        private string _championshipsPlaceholder = "e.g. 6";
        public AddEditTeamPage()
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            InitializePlaceholders();
            InitializeEditorOptions();
        }
        public AddEditTeamPage(F1Team team)
        {
            InitializeComponent();
            mainWindow = (MainWindow)Application.Current.MainWindow;
            isEditMode = true;
            existingTeam = team;
            InitializeEditorOptions();
            PopulateFormWithTeamData(team);
            PageTitleLabel.Content = "EDIT TEAM";
        }

        private void InitializePlaceholders()
        {
            SetPlaceholder(TeamNameTextBox, _teamNamePlaceholder);
            SetPlaceholder(HeadquartersTextBox, _headquartersPlaceholder);
            SetPlaceholder(EngineSupplierTextBox, _engineSupplierPlaceholder);
            SetPlaceholder(YearFoundedTextBox, _yearFoundedPlaceholder);
            SetPlaceholder(ChampionshipsTextBox, _championshipsPlaceholder);
        }

        private void SetPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.Foreground = (SolidColorBrush)Application.Current.Resources["PlaceholderTextColor"];
        }

        private void InitializeEditorOptions()
        {
            FontFamilyComboBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);

            FontSizeComboBox.ItemsSource = new List<double>
            {
                8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 28, 32, 36, 48, 72
            };
            FontSizeComboBox.SelectedIndex = 4;

            TextColorComboBox.ItemsSource = typeof(Colors).GetProperties().Select(p => new { Name = p.Name, Brush = new SolidColorBrush((Color)p.GetValue(null)) }).ToList();
        }

        private void PopulateFormWithTeamData(F1Team team)
        {
            TeamNameTextBox.Text = team.TeamName;
            TeamNameTextBox.Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryTextColor"];

            HeadquartersTextBox.Text = team.Headquarters;
            HeadquartersTextBox.Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryTextColor"];

            EngineSupplierTextBox.Text = team.EngineSupplier;
            EngineSupplierTextBox.Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryTextColor"];

            YearFoundedTextBox.Text = team.YearFounded.ToString();
            YearFoundedTextBox.Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryTextColor"];

            ChampionshipsTextBox.Text = team.NumberOfChampionships.ToString();
            ChampionshipsTextBox.Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryTextColor"];

            LogoPathTextBox.Text = team.LogoPath;

            if (!string.IsNullOrEmpty(team.LogoPath))
            {
                string fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, team.LogoPath);
                if (File.Exists(fullPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    LogoPreviewImage.Source = bitmap;
                    LogoPlaceholder.Visibility = Visibility.Collapsed;
                }
            }

            if (!string.IsNullOrEmpty(team.DescriptionFilePath))
            {
                string fullRtfPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, team.DescriptionFilePath);
                if (File.Exists(fullRtfPath))
                {
                    using (FileStream fs = new FileStream(fullRtfPath, FileMode.Open))
                    {
                        TextRange range = new TextRange(DescriptionRichTextBox.Document.ContentStart, DescriptionRichTextBox.Document.ContentEnd);
                        range.Load(fs, DataFormats.Rtf);
                    }
                }
            }
        }

        private void BrowseLogoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Team Logo",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string destinationFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images");
                Directory.CreateDirectory(destinationFolder);

                string fileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                string destinationPath = System.IO.Path.Combine(destinationFolder, fileName);

                File.Copy(openFileDialog.FileName, destinationPath, true);

                string relativePath = System.IO.Path.Combine("Assets", "Images", fileName);
                LogoPathTextBox.Text = relativePath;

                LogoPreviewImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(openFileDialog.FileName));
                LogoErrorLabel.Content = string.Empty;
                LogoPlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFormData())
            {
                mainWindow.ShowToastNotification(new ToastNotification("Error", "Please fill in all required fields correctly!", NotificationType.Error));
                return;
            }

            string rtfPath = SaveRtfFile();

            if (isEditMode)
            {
                existingTeam.TeamName = TeamNameTextBox.Text.Trim();
                existingTeam.Headquarters = HeadquartersTextBox.Text.Trim();
                existingTeam.EngineSupplier = EngineSupplierTextBox.Text.Trim();
                existingTeam.YearFounded = int.Parse(YearFoundedTextBox.Text.Trim());
                existingTeam.NumberOfChampionships = int.Parse(ChampionshipsTextBox.Text.Trim());
                existingTeam.LogoPath = LogoPathTextBox.Text;
                existingTeam.DescriptionFilePath = rtfPath;

                mainWindow.ShowToastNotification(new ToastNotification("Success", $"{existingTeam.TeamName} updated successfully!", NotificationType.Success));
            }
            else
            {
                F1Team newTeam = new F1Team
                {
                    TeamName = TeamNameTextBox.Text.Trim(),
                    Headquarters = HeadquartersTextBox.Text.Trim(),
                    EngineSupplier = EngineSupplierTextBox.Text.Trim(),
                    YearFounded = int.Parse(YearFoundedTextBox.Text.Trim()),
                    NumberOfChampionships = int.Parse(ChampionshipsTextBox.Text.Trim()),
                    LogoPath = LogoPathTextBox.Text,
                    DescriptionFilePath = rtfPath,
                    DateAdded = DateTime.Now
                };

                mainWindow.Teams.Add(newTeam);

                mainWindow.ShowToastNotification(new ToastNotification("Success", $"{newTeam.TeamName} added successfully!", NotificationType.Success));
            }

            mainWindow.NavigateToPage(new TeamsTablePage());
        }

        private string SaveRtfFile()
        {
            string rtfFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "RTF");
            Directory.CreateDirectory(rtfFolder);

            string teamName = TeamNameTextBox.Text.Trim().Replace(" ", "_").Replace("/", "_");
            string fileName = $"{teamName}_{DateTime.Now:yyyyMMdd_HHmmss}.rtf";
            string filePath = System.IO.Path.Combine(rtfFolder, fileName);

            if (isEditMode && !string.IsNullOrEmpty(existingTeam.DescriptionFilePath))
            {
                string existingPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, existingTeam.DescriptionFilePath);
                if (File.Exists(existingPath))
                {
                    filePath = existingPath;
                }
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                TextRange range = new TextRange(DescriptionRichTextBox.Document.ContentStart, DescriptionRichTextBox.Document.ContentEnd);
                range.Save(fs, DataFormats.Rtf);
            }

            string relativePath = System.IO.Path.Combine("Assets", "RTF", System.IO.Path.GetFileName(filePath));
            return relativePath;
        }

        private bool ValidateFormData()
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(TeamNameTextBox.Text) ||
                TeamNameTextBox.Text == _teamNamePlaceholder)
            {
                TeamNameErrorLabel.Content = "Team name is required!";
                TeamNameTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["ValidationErrorColor"];
                isValid = false;
            }
            else
            {
                TeamNameErrorLabel.Content = string.Empty;
                TeamNameTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderColor"];
            }

            if (string.IsNullOrWhiteSpace(HeadquartersTextBox.Text) ||
                HeadquartersTextBox.Text == _headquartersPlaceholder)
            {
                HeadquartersErrorLabel.Content = "Headquarters is required!";
                HeadquartersTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["ValidationErrorColor"];
                isValid = false;
            }
            else
            {
                HeadquartersErrorLabel.Content = string.Empty;
                HeadquartersTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderColor"];
            }

            if (string.IsNullOrWhiteSpace(EngineSupplierTextBox.Text) ||
                EngineSupplierTextBox.Text == _engineSupplierPlaceholder)
            {
                EngineSupplierErrorLabel.Content = "Engine supplier is required!";
                EngineSupplierTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["ValidationErrorColor"];
                isValid = false;
            }
            else
            {
                EngineSupplierErrorLabel.Content = string.Empty;
                EngineSupplierTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderColor"];
            }

            if (string.IsNullOrWhiteSpace(YearFoundedTextBox.Text) ||
                YearFoundedTextBox.Text == _yearFoundedPlaceholder ||
                !int.TryParse(YearFoundedTextBox.Text, out int year) ||
                year < 1950 || year > DateTime.Now.Year)
            {
                YearFoundedErrorLabel.Content = "Enter a valid year (1950 - present)!";
                YearFoundedTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["ValidationErrorColor"];
                isValid = false;
            }
            else
            {
                YearFoundedErrorLabel.Content = string.Empty;
                YearFoundedTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderColor"];
            }

            if (string.IsNullOrWhiteSpace(ChampionshipsTextBox.Text) ||
                ChampionshipsTextBox.Text == _championshipsPlaceholder ||
                !int.TryParse(ChampionshipsTextBox.Text, out int champs) ||
                champs < 0)
            {
                ChampionshipsErrorLabel.Content = "Enter a valid number of championships (0 or more)!";
                ChampionshipsTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["ValidationErrorColor"];
                isValid = false;
            }
            else
            {
                ChampionshipsErrorLabel.Content = string.Empty;
                ChampionshipsTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderColor"];
            }

            if (string.IsNullOrEmpty(LogoPathTextBox.Text))
            {
                LogoErrorLabel.Content = "Please select a logo image!";
                isValid = false;
            }
            else
            {
                LogoErrorLabel.Content = string.Empty;
            }

            TextRange textRange = new TextRange(
                DescriptionRichTextBox.Document.ContentStart,
                DescriptionRichTextBox.Document.ContentEnd);

            if (string.IsNullOrWhiteSpace(textRange.Text))
            {
                DescriptionErrorLabel.Content = "Team description is required!";
                DescriptionRichTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["ValidationErrorColor"];
                isValid = false;
            }
            else
            {
                DescriptionErrorLabel.Content = string.Empty;
                DescriptionRichTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderColor"];
            }

            return isValid;
        }

        private void UpdateWordCount()
        {
            TextRange textRange = new TextRange(
                DescriptionRichTextBox.Document.ContentStart,
                DescriptionRichTextBox.Document.ContentEnd);

            string text = textRange.Text.Trim();
            int wordCount = string.IsNullOrWhiteSpace(text) ? 0 : text.Split(new char[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

            WordCountLabel.Content = $"Words: {wordCount}";
        }

        private void DescriptionRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            object fontWeight = DescriptionRichTextBox.Selection.GetPropertyValue(Inline.FontWeightProperty);
            BoldToggleButton.IsChecked = fontWeight != DependencyProperty.UnsetValue && fontWeight.Equals(FontWeights.Bold);

            object fontStyle = DescriptionRichTextBox.Selection.GetPropertyValue(Inline.FontStyleProperty);
            ItalicToggleButton.IsChecked = fontStyle != DependencyProperty.UnsetValue && fontStyle.Equals(FontStyles.Italic);

            object textDecorations = DescriptionRichTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            UnderlineToggleButton.IsChecked = textDecorations != DependencyProperty.UnsetValue && textDecorations.Equals(TextDecorations.Underline);

            object fontFamily = DescriptionRichTextBox.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            FontFamilyComboBox.SelectedItem = fontFamily;

            object fontSize = DescriptionRichTextBox.Selection.GetPropertyValue(Inline.FontSizeProperty);
            if (fontSize != DependencyProperty.UnsetValue && fontSize is double fontSizeValue)
            {
                var matchingSize = FontSizeComboBox.Items.Cast<double>().OrderBy(s => Math.Abs(s - fontSizeValue)).FirstOrDefault();
                if (matchingSize > 0)
                    FontSizeComboBox.SelectedItem = matchingSize;
            }

            object foreground = DescriptionRichTextBox.Selection.GetPropertyValue(Inline.ForegroundProperty);
            if (foreground != DependencyProperty.UnsetValue && foreground is SolidColorBrush brush)
            {
                var colorItem = TextColorComboBox.Items.Cast<dynamic>().FirstOrDefault(c => c.Brush.Color == brush.Color);
                TextColorComboBox.SelectedItem = colorItem;
            }

            UpdateWordCount();
        }

        private void FontFamilyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontFamilyComboBox.SelectedItem != null && !DescriptionRichTextBox.Selection.IsEmpty)
            {
                DescriptionRichTextBox.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, FontFamilyComboBox.SelectedItem);
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem != null && !DescriptionRichTextBox.Selection.IsEmpty)
            {
                DescriptionRichTextBox.Selection.ApplyPropertyValue(Inline.FontSizeProperty, FontSizeComboBox.SelectedItem);
            }
        }

        private void TextColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TextColorComboBox.SelectedItem != null)
            {
                dynamic selectedColor = TextColorComboBox.SelectedItem;
                SelectedColorRect.Fill = selectedColor.Brush;

                if (!DescriptionRichTextBox.Selection.IsEmpty)
                {
                    DescriptionRichTextBox.Selection.ApplyPropertyValue(Inline.ForegroundProperty, selectedColor.Brush);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.NavigateToPage(new TeamsTablePage());
        }

        // Placeholder metode za TextBox-ove
        private void TeamNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ClearPlaceholder(TeamNameTextBox, _teamNamePlaceholder);
        }
        private void TeamNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RestorePlaceholder(TeamNameTextBox, _teamNamePlaceholder);
        }

        private void HeadquartersTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ClearPlaceholder(HeadquartersTextBox, _headquartersPlaceholder);
        }
        private void HeadquartersTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RestorePlaceholder(HeadquartersTextBox, _headquartersPlaceholder);
        }

        private void EngineSupplierTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ClearPlaceholder(EngineSupplierTextBox, _engineSupplierPlaceholder);
        }
        private void EngineSupplierTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RestorePlaceholder(EngineSupplierTextBox, _engineSupplierPlaceholder);
        }

        private void YearFoundedTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ClearPlaceholder(YearFoundedTextBox, _yearFoundedPlaceholder);
        }
        private void YearFoundedTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RestorePlaceholder(YearFoundedTextBox, _yearFoundedPlaceholder);
        }

        private void ChampionshipsTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ClearPlaceholder(ChampionshipsTextBox, _championshipsPlaceholder);
        }
        private void ChampionshipsTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RestorePlaceholder(ChampionshipsTextBox, _championshipsPlaceholder);
        }

        private void ClearPlaceholder(TextBox textBox, string placeholder)
        {
            if (textBox.Text.Trim().Equals(placeholder))
            {
                textBox.Text = string.Empty;
                textBox.Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryTextColor"];
            }
        }

        private void RestorePlaceholder(TextBox textBox, string placeholder)
        {
            if (textBox.Text.Trim().Equals(string.Empty))
            {
                textBox.Text = placeholder;
                textBox.Foreground = (SolidColorBrush)Application.Current.Resources["PlaceholderTextColor"];
            }
        }
    }
}
