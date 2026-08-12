using System.Windows.Controls;
using YingqiTools.ViewModels;

namespace YingqiTools.Pages;

public partial class DashboardPage : Page
{
    public event EventHandler? LidConfigurationRequested;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        IsVisibleChanged += (_, _) => { if (IsVisible) viewModel.Refresh(); };
    }

    private void ConfigureLid_Click(object sender, System.Windows.RoutedEventArgs e) => LidConfigurationRequested?.Invoke(this, EventArgs.Empty);
}
