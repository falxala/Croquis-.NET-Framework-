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

namespace Croquis_.NET_Framework_
{
    /// <summary>
    /// AppSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class AppSetting : Window
    {
        Settings setting;
        public AppSetting(Settings appSettings)
        {
            InitializeComponent();
            setting = appSettings;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            setting.CustomInterval = 10;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (setting.UnificationOfDisplayQuality == false)
                ComboQuality.SelectedIndex = 0;
            else
                ComboQuality.SelectedIndex = 1;
            ComboResolution.Items.Add(setting.UnifiedResolution);
            ComboResolution.Items.Add(3840);
            ComboResolution.Items.Add(2560);
            ComboResolution.Items.Add(1920);
            ComboResolution.Items.Add(1280);
            ComboResolution.Items.Add(640);
            ComboResolution.SelectedIndex = 0;

            ComboPreparationTime.Items.Add(setting.PreparationTime);
            for (int i = 0; i <= 10; i++)
                ComboPreparationTime.Items.Add(i);
            ComboPreparationTime.SelectedIndex = 0;
        }

        private void ComboQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboQuality.SelectedIndex == 0)
            {
                setting.UnificationOfDisplayQuality = false;
                ComboResolution.IsEnabled = false;
            }
            else
            {
                setting.UnificationOfDisplayQuality = true;
                ComboResolution.IsEnabled = true;
            }
        }

        private void ComboResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setting.UnifiedResolution = (int)ComboResolution.SelectedValue;
        }

        private void ComboPreparationTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setting.PreparationTime = (int)ComboPreparationTime.SelectedValue;
        }
    }
}
