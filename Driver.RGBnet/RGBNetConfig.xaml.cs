using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace Driver.RGBnet
{
    /// <summary>
    /// Interaction logic for RGBNetConfig.xaml
    /// </summary>
    public partial class RGBNetConfig : UserControl
    {
        public RGBNetConfig()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string settingsPath = Directory.GetCurrentDirectory() + "\\PluginManager.Settings.json";
                string templatePath = Directory.GetCurrentDirectory() +
                                      "\\SLSProvider\\09fc46d2-6880-487f-9a8e-16b907b20eb1\\PluginManager.Settings.json";

                File.Copy(templatePath, settingsPath);

                string pmPath = Directory.GetCurrentDirectory() +
                                    "\\SLSProvider\\09fc46d2-6880-487f-9a8e-16b907b20eb1\\PluginManager.exe";
                Process.Start(pmPath);
            }
            catch
            {
                MessageBox.Show("Unable to locate plugin manager executable.","Error!",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
