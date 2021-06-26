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
using System.Collections.ObjectModel;
using System.Drawing;

namespace Croquis_.NET_Framework_
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        string CurrentImage = "";
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        int Interval_time = 30;
        bool pause_flag = false;
        bool reset_flag = false;
        bool grayscale_flag = false;

        private ObservableCollection<string> FileList;
        ImageSource imgsourse = new RenderTargetBitmap(525,       // Imageの幅
                                       350,      // Imageの高さ
                                       96.0d,                 // 横解像度
                                       96.0d,                 // 縦解像度
                           PixelFormats.Pbgra32);
        public MainWindow()
        {
            InitializeComponent();
            FileList = new ObservableCollection<string>();
            image_.Source = imgsourse;
            text_pop(true, "Drag & Drop");
            BuildView();
        }


        private void Image_PreviewDragEnter(object sender, DragEventArgs e)
        {

        }

        private void Image_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private async void Image_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && sw.IsRunning == false)
            {
                //ファイル名を取得
                var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);

                //最初の1つ目のドロップフォルダなら
                if (System.IO.Directory.Exists(fileNames[0]))
                {
                    //フォルダ内のファイル名を取得
                    fileNames = System.IO.Directory.GetFiles(@fileNames[0], "*", System.IO.SearchOption.TopDirectoryOnly);
                }

                foreach (var name in fileNames)
                {
                    text_pop(false, "");
                    FileList.Add(name);
                }

                reset_flag = false;

                try
                {
                    foreach (string image_name in FileList)
                    {
                        CurrentImage = image_name;
                        await Slideshow(image_name, Interval_time);
                        if (reset_flag == true)
                            throw new Exception();
                    }
                    image_.Source = imgsourse;
                    text_pop(true, "終了！");
                }
                catch (Exception)
                {
                    Reset();
                }
                finally
                {
                    FileList.Clear();
                }
            }

        }

        private async Task Slideshow(string image_name, double time)
        {
            time = time * 1000;
            try
            {
                BitmapImage imageSource = new BitmapImage(new Uri(image_name));
                if (grayscale_flag)
                    image_.Source = ToGrayScale(imageSource);
                else
                    image_.Source = imageSource;
            }
            catch (Exception ex)
            {
                //text_pop(true,"非対応ファイルが含まれています");
            }
            sw.Reset();
            sw.Start();
            while (sw.ElapsedMilliseconds <= time && reset_flag == false)
            {
                pause();

                await Task.Delay(10);
                progress.Value = sw.ElapsedMilliseconds / time * 100;
                mainwindow.Title = ((time - sw.ElapsedMilliseconds) / 1000).ToString("f1");
            }
            sw.Stop();
        }

        private void text_pop(bool on_off, string content)
        {
            if (on_off)
            {
                text.Visibility = Visibility.Visible;
                text.Content = content;
            }
            else
            {
                text.Visibility = Visibility.Hidden;
            }
        }

        private void mainwindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter == true)
            {
                if (WindowState == WindowState.Normal)
                {
                    //適用する順番でタスクバーが隠れるか隠れないかが決まる
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                }
            }

            if (e.Key == Key.Space == true)
                pause_flag = !pause_flag;
        }

        private void Reset()
        {
            sw.Reset();
            text_pop(true, "Drag & Drop");
            image_.Source = imgsourse;
            mainwindow.Title = "croquis";
            progress.Value = 0;
            grayscale_flag = false;
            pause_flag = false;
        }

        private void pause()
        {
            if (pause_flag == true)
                sw.Stop();
            else if (sw.IsRunning == false && pause_flag == false)
                sw.Start();
        }

        private void item10_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 10;
            mainwindow.Title = "set:" + Interval_time;
        }

        private void item30_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 30;
            mainwindow.Title = "set:" + Interval_time;
        }

        private void item60_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 60;
            mainwindow.Title = "set:" + Interval_time;
        }

        private void item90_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 90;
            mainwindow.Title = "set:" + Interval_time;
        }

        private void item180_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 180;
            mainwindow.Title = "set:" + Interval_time;
        }

        private void item300_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 300;
            mainwindow.Title = "set:" + Interval_time;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            pause_flag = !pause_flag;
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            reset_flag = true;
            Reset();
        }

        private void Black(object sender, RoutedEventArgs e)
        {
            mainwindow.Background = Brushes.Black;
            text.Foreground = Brushes.White;
        }

        private void White(object sender, RoutedEventArgs e)
        {
            mainwindow.Background = Brushes.White;
            text.Foreground = Brushes.Black;
        }


        //https://zawapro.com/?p=988
        private const int GRID_SIZE = 100;
        private ScaleTransform scaleTransform = new ScaleTransform();
        private void BuildView()
        {
            canvas.Children.Clear();

            // 縦線
            for (int i = 0; i < canvas.ActualWidth; i += GRID_SIZE)
            {
                Path path = new Path()
                {
                    Data = new LineGeometry(new Point(i, 0), new Point(i, canvas.ActualHeight)),
                    Stroke = new SolidColorBrush(Color.FromArgb(128,128,128,128)),
                    StrokeThickness = 1
                };

                path.Data.Transform = scaleTransform;

                canvas.Children.Add(path);
            }

            // 横線
            for (int i = 0; i < canvas.ActualHeight; i += GRID_SIZE)
            {
                Path path = new Path()
                {
                    Data = new LineGeometry(new Point(0, i), new Point(canvas.ActualWidth, i)),
                    Stroke = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128)),
                    StrokeThickness = 1
                };

                path.Data.Transform = scaleTransform;

                canvas.Children.Add(path);
            }
        }

        private void canvas_Loaded(object sender, RoutedEventArgs e)
        {
            BuildView();
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BuildView();
        }

        private void Grid(object sender, RoutedEventArgs e)
        {
            if (canvas.Visibility == Visibility.Hidden)
                canvas.Visibility = Visibility.Visible;
            else
                canvas.Visibility = Visibility.Hidden;
        }

        private BitmapSource ToGrayScale( BitmapSource bitmap)
        {
            FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
            newFormatedBitmapSource.BeginInit();
            newFormatedBitmapSource.Source = bitmap;

            newFormatedBitmapSource.DestinationFormat = PixelFormats.Gray32Float;
            newFormatedBitmapSource.EndInit();

            return newFormatedBitmapSource;
        }

        private void GrayScale(object sender, RoutedEventArgs e)
        {
            try
            {
                grayscale_flag = !grayscale_flag;
                if (grayscale_flag == true)
                    image_.Source = ToGrayScale(new BitmapImage(new Uri(CurrentImage)));
                else
                    image_.Source = new BitmapImage(new Uri(CurrentImage));
            }
            catch { }
        }
    }
}
