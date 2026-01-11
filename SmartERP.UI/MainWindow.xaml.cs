using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SmartERP.Core.Services;

namespace SmartERP.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IAuthenticationService _authService;

    public MainWindow(IAuthenticationService authService)
    {
        InitializeComponent();
        _authService = authService;
        
        // Display user info in title
        if (_authService.CurrentUser != null)
        {
            Title = $"SmartERP - Welcome {_authService.CurrentUser.FullName} ({_authService.CurrentUser.Role})";
        }
    }
}