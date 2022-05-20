using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
            TxtCustomizeInterval.Text = setting.CustomInterval.ToString();
            TBStatusTxtSize.Text = setting.InfoTextSize.ToString();
            VerText.Text = "Version=" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
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

            ComboInterpolationMethod.Items.Add(new combIMitem { ScalingModeStr = Properties.Resources.CBItemFant, ScalingMode = BitmapScalingMode.Fant });
            ComboInterpolationMethod.Items.Add(new combIMitem { ScalingModeStr = Properties.Resources.CBItemLinear, ScalingMode = BitmapScalingMode.Linear });
            ComboInterpolationMethod.Items.Add(new combIMitem { ScalingModeStr = Properties.Resources.CBItemNearestNeighbor, ScalingMode = BitmapScalingMode.NearestNeighbor });
            if (setting.BitmapScalingMode == BitmapScalingMode.NearestNeighbor)
                ComboInterpolationMethod.SelectedIndex = 2;
            else if (setting.BitmapScalingMode == BitmapScalingMode.Linear)
                ComboInterpolationMethod.SelectedIndex = 1;
            else
                ComboInterpolationMethod.SelectedIndex = 0;

            ComboAlignment.Items.Add(new combAlitem { Str = Properties.Resources.UpperLeft, horizontal = HorizontalAlignment.Left, vertical = VerticalAlignment.Top });
            ComboAlignment.Items.Add(new combAlitem { Str = Properties.Resources.LowerLeft, horizontal = HorizontalAlignment.Left, vertical = VerticalAlignment.Bottom });
            ComboAlignment.Items.Add(new combAlitem { Str = Properties.Resources.UpperRight, horizontal = HorizontalAlignment.Right, vertical = VerticalAlignment.Top });
            ComboAlignment.Items.Add(new combAlitem { Str = Properties.Resources.LowerRight, horizontal = HorizontalAlignment.Right, vertical = VerticalAlignment.Bottom });

            if (setting.Horizontal == HorizontalAlignment.Left) {
                if (setting.Vertical == VerticalAlignment.Top)
                {
                    ComboAlignment.SelectedIndex = 0;
                }
                else
                {
                    ComboAlignment.SelectedIndex = 1;
                }
            }
            else
            {
                if (setting.Vertical == VerticalAlignment.Top)
                {
                    ComboAlignment.SelectedIndex = 2;
                }
                else
                {
                    ComboAlignment.SelectedIndex = 3;
                }
            }

            ComboAlignment1.Items.Add(new combAlitem { Str = Properties.Resources.Not_record });
            ComboAlignment1.Items.Add(new combAlitem { Str = Properties.Resources.Record });
            if (setting.Memory == true)
                ComboAlignment1.SelectedIndex = 1;
            else
                ComboAlignment1.SelectedIndex = 0;

        }

        class combIMitem
        {
            public string ScalingModeStr { get; set; }
            public BitmapScalingMode ScalingMode{ get; set; }
            public override string ToString()
            {
                return ScalingModeStr;
            }
        }

        class combAlitem
        {
            public string Str { get; set; }
            public HorizontalAlignment  horizontal { get; set; }
            public VerticalAlignment vertical { get; set; }
            public override string ToString()
            {
                return Str;
            }
        }

        private void ComboQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboQuality.SelectedIndex == 0)
            {
                setting.UnificationOfDisplayQuality = false;
                ComboResolution.IsEnabled = false;
                ComboInterpolationMethod.IsEnabled = true;
            }
            else
            {
                setting.UnificationOfDisplayQuality = true;
                ComboResolution.IsEnabled = true;
                ComboInterpolationMethod.IsEnabled = false;
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

        private void ComboInterpolationMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setting.BitmapScalingMode = (ComboInterpolationMethod.SelectedItem as combIMitem).ScalingMode;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }

        private void TextBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        private void TextBoxPrice_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼り付けを許可しない
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (int.Parse(TxtCustomizeInterval.Text) > 0)
                    setting.CustomInterval = int.Parse(TxtCustomizeInterval.Text);
            }
            catch { }
        }

        private void ComboAlignment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (ComboAlignment.SelectedIndex)
            {

                default:
                    setting.Horizontal = HorizontalAlignment.Left;
                    setting.Vertical = VerticalAlignment.Top;
                    break;
                case 1:
                    setting.Horizontal = HorizontalAlignment.Left;
                    setting.Vertical = VerticalAlignment.Bottom;
                    break;
                case 2:
                    setting.Horizontal = HorizontalAlignment.Right;
                    setting.Vertical = VerticalAlignment.Top;
                    break;
                case 3:
                    setting.Horizontal = HorizontalAlignment.Right;
                    setting.Vertical = VerticalAlignment.Bottom;
                    break;
            }
        }

        private void TBStatusTxtSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (int.Parse(TBStatusTxtSize.Text) > 0 && int.Parse(TBStatusTxtSize.Text) <= 1000)
                    setting.InfoTextSize = int.Parse(TBStatusTxtSize.Text);
            }
            catch { }
        }

        private void ComboAlignment1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboAlignment1.SelectedIndex == 0)
                setting.Memory = false;
            else
                setting.Memory = true;
        }
    }
}
