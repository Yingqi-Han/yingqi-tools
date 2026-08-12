using System.Windows.Controls;
using KeyboardCoolDownLock;

namespace YingqiTools.Pages;

public partial class KeyboardPage : Page
{
    public KeyboardPage(KeyboardLockControl control)
    {
        InitializeComponent();
        ComponentHost.Content = control;
    }
}
