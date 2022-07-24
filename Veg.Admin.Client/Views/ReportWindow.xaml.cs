using ControlzEx.Theming;
using MahApps.Metro.Controls.Dialogs;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Veg.Admin.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ReportWindow : MahApps.Metro.Controls.MetroWindow
    {
        public ReportWindow()
        {
            ThemeManager.Current.ChangeTheme(this, "Light.Green");

            InitializeComponent();
        }
    }
}
