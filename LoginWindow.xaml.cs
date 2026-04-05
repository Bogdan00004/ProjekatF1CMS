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
using System.Windows.Shapes;
using ProjekatF1CMS.Helpers;
using ProjekatF1CMS.Model;

namespace ProjekatF1CMS
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private DataIO serializer = new DataIO();
        private List<User> users;
        private string _usernamePlaceholder = "Enter your username";
        public LoginWindow()
        {
            InitializeComponent();
            users = serializer.DeSerializeObject<List<User>>("Data/Users.xml");
            if (users == null)
                users = new List<User>();

            UsernameTextBox.Text = _usernamePlaceholder;
            UsernameTextBox.Foreground = (SolidColorBrush)Application.Current.Resources["PlaceholderTextColor"];
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFormData())
                return;

            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            User user = users.FirstOrDefault(u =>u.Username == username && u.Password == password);

            if (user != null)
            {
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.LoggedInUser = user;
                mainWindow.NavigateToPage(new Pages.TeamsTablePage());
                mainWindow.Show();
                this.Close();
            }
            else
            {
                LoginErrorLabel.Content = "Invalid username or password!";
            }
        }

        private bool ValidateFormData()
        {
            bool isValid = true;

            if (UsernameTextBox.Text.Trim().Equals(string.Empty) ||
                UsernameTextBox.Text.Trim().Equals(_usernamePlaceholder))
            {
                isValid = false;
                UsernameErrorLabel.Content = "Username cannot be empty!";
                UsernameTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["ValidationErrorColor"];
                UsernameTextBox.BorderThickness = new Thickness(1);
            }
            else
            {
                UsernameErrorLabel.Content = string.Empty;
                UsernameTextBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderColor"];
            }

            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                isValid = false;
                PasswordErrorLabel.Content = "Password cannot be empty!";
                PasswordBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["ValidationErrorColor"];
                PasswordBox.BorderThickness = new Thickness(1);
            }
            else
            {
                PasswordErrorLabel.Content = string.Empty;
                PasswordBox.BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderColor"];
            }

            return isValid;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text.Trim().Equals(_usernamePlaceholder))
            {
                UsernameTextBox.Text = string.Empty;
                UsernameTextBox.Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryTextColor"];
            }
        }

        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text.Trim().Equals(string.Empty))
            {
                UsernameTextBox.Text = _usernamePlaceholder;
                UsernameTextBox.Foreground = (SolidColorBrush)Application.Current.Resources["PlaceholderTextColor"];
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordBox.Password))
                PasswordPlaceholder.Visibility = Visibility.Visible;
        }
    }
}
