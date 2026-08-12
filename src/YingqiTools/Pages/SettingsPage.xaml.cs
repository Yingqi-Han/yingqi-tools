using System.Windows.Controls;
using YingqiTools.ViewModels;

namespace YingqiTools.Pages;

public partial class SettingsPage : Page
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
