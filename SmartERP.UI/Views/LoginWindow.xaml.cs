using System.Windows;
using System.Windows.Input;
using SmartERP.Core.Services;

namespace SmartERP.UI.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthenticationService _authService;

        public LoginWindow(IAuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;
            
            // Set focus to username textbox
            Loaded += (s, e) => UsernameTextBox.Focus();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageText.Visibility = Visibility.Collapsed;

            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Please enter username and password");
                return;
            }

            try
            {
                var user = await _authService.LoginAsync(username, password);

                if (user != null)
                {
                    // Login successful - open main window
                    var mainWindow = new MainWindow(_authService);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ShowError("Invalid username or password");
                    PasswordBox.Clear();
                    PasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Login error: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageText.Visibility = Visibility.Visible;
        }
    }
}
