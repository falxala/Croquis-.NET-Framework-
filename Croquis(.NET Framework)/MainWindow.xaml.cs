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
using System.IO;
using Path = System.Windows.Shapes.Path;

namespace Croquis_.NET_Framework_
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        string CurrentImage = "";
        int Count = 0;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        int Interval_time = 30;
        bool pause_flag = false;
        bool reset_flag = false;
        bool grayscale_flag = false;
        Brush background_Color = Brushes.White;

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
            for (int t = 3; t > 0; t--)
            {
                text_pop(true, t.ToString());
                await Task.Delay(1000);
            }

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

                await Slideshow(fileNames);
            }

        }

        private async Task Slideshow(string[] fileNames)
        {

            foreach (var name in fileNames)
            {
                text_pop(false, "");
                FileList.Add(name);
            }

            reset_flag = false;

            try
            {
                Count = 0;
                while (Count < FileList.Count)
                {
                    string image_name = FileList[Count];
                    CurrentImage = image_name;
                    Count++;
                    await showImage(image_name, Interval_time);
                    if (reset_flag == true)
                    {
                        throw new Exception();
                    }
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

        private void setImage(string image_name)
        {
            CurrentImage = image_name;
            try
            {
                BitmapImage bmpImage = new BitmapImage();
                using (FileStream stream = File.OpenRead(image_name))
                {
                    bmpImage.BeginInit();
                    bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                    bmpImage.StreamSource = stream;
                    bmpImage.EndInit();
                    stream.Close();
                }
                if (grayscale_flag)
                    image_.Source = ToGrayScale(bmpImage);
                else
                    image_.Source = bmpImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                //text_pop(true,"非対応ファイルが含まれています");
            }
        }

        private async Task showImage(string image_name, double time)
        {
            time = time * 1000;

            setImage(image_name);
            sw.Reset();
            sw.Start();
            while (sw.ElapsedMilliseconds <= time && reset_flag == false)
            {
                pause();

                await Task.Delay(10);
                progress.Value = sw.ElapsedMilliseconds / time * 100;
                mainwindow.Title = "[ " + Count + " out of " + FileList.Count + " ]  remaining : " + ((time - sw.ElapsedMilliseconds) / 1000).ToString("f1");
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
            {
                Pause(null,null);
            }

            if (e.Key == Key.G == true)
            {
                Grid(null,null);
            }

            if (e.Key == Key.B == true)
            {
                if (background_Color == Brushes.White)
                    Black(null, null);
                else
                    White(null, null);
            }

            if (e.Key == Key.S == true || e.Key == Key.C == true)
            {
                GrayScale(null,null);
            }

            if (e.Key == Key.Left == true && Count > 1)
            {
                Count--;
                setImage(FileList[Count-1]);
                sw.Reset();
            }
            if (e.Key == Key.Right == true && Count < FileList.Count)
            {
                Count++;
                setImage(FileList[Count-1]);
                sw.Reset();
            }
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

        private void Pause(object sender, RoutedEventArgs e)
        {
            pause_flag = !pause_flag;
            if (pause_flag == true)
            {
                mainwindow.Background = Brushes.Gray;
            }
            else
                mainwindow.Background = background_Color;
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            reset_flag = true;
            Reset();
        }

        private void Black(object sender, RoutedEventArgs e)
        {
            background_Color = Brushes.Black;
            mainwindow.Background = background_Color;
            text.Foreground = Brushes.White;
        }

        private void White(object sender, RoutedEventArgs e)
        {
            background_Color = Brushes.White;
            mainwindow.Background = background_Color;
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
                if (reset_flag == true)
                    throw new Exception();

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
