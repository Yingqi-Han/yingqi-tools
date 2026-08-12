using System.Windows.Controls;
using LidWorkMode;

namespace YingqiTools.Pages;

public partial class LidPage : Page
{
    public LidPage(LidWorkModeControl control)
    {
        InitializeComponent();
        ComponentHost.Content = control;
    }
}
