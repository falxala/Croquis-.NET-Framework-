using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.IO;
using Path = System.Windows.Shapes.Path;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Croquis_.NET_Framework_
{

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string message = "";
        public string Message
        {
            get
            {
                return this.message;
            }
            set
            {
                if (value != this.message)
                {
                    this.message = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string info_message = "";
        public string info_Message
        {
            get
            {
                return this.info_message;
            }
            set
            {
                if (value != this.info_message)
                {
                    this.info_message = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }

/// <summary>
/// MainWindow.xaml の相互作用ロジック
/// </summary>
public partial class MainWindow : Window
    {
        string CurrentImage = "";
        int Count = 0;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        double Interval_time = 30;
        int Update_interval = 15;
        bool pause_flag = false;
        bool reset_flag = true;
        bool grayscale_flag = false;
        Brush background_Color = Brushes.White;
        ViewModel navigate_vm = new ViewModel();

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
            BuildView();
            progressbar.Visibility = Visibility.Hidden;

            text.DataContext = navigate_vm;
            navigate_vm.Message = "Drop files here";
            text_pop(true);

            info_label.DataContext = navigate_vm;
            navigate_vm.info_Message = "--";

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

        private  void Image_PreviewDrop(object sender, DragEventArgs e)
        {
            //ファイル名を取得
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && sw.IsRunning == false)
            {
                StartSlideshow(fileNames);
            }
        }

        private async void StartSlideshow(string[] fileNames)
        {
            progressbar.Visibility = Visibility.Visible;
            reset_flag = false;

            for (int t = 3; t > 0; t--)
            {
                text_pop(true);
                navigate_vm.Message = t.ToString();

                await Task.Delay(1000);
            }

            //最初の1つ目のドロップフォルダなら
            if (System.IO.Directory.Exists(fileNames[0]))
            {
                //フォルダ内のファイル名を取得
                fileNames = System.IO.Directory.GetFiles(@fileNames[0], "*", System.IO.SearchOption.TopDirectoryOnly);
            }

            await Slideshow(fileNames);
        }


        private async Task Slideshow(string[] fileNames)
        {

            foreach (var name in fileNames)
            {
                text_pop(false);
                FileList.Add(name);
            }

            try
            {
                Count = 0;
                while (Count < FileList.Count)
                {
                    string image_name = FileList[Count];
                    CurrentImage = image_name;
                    Count++;
                    await showImage(image_name);
                    if (reset_flag == true)
                    {
                        throw new Exception();
                    }
                }
                image_.Source = imgsourse;
                text_pop(true);
                navigate_vm.Message = "Finish!";
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

        private void SetImage(string image_name)
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

        private async Task showImage(string image_name)
        {
            SetImage(image_name);
            sw.Reset();
            sw.Start();
            string old_info = "--";
            string current_info;
            while (sw.ElapsedMilliseconds <= Interval_time * 1000 && reset_flag == false)
            {
                pause();

                await Task.Delay(Update_interval);//delayの精度が15ms
                progressbar.Value = sw.ElapsedMilliseconds / (Interval_time * 1000) * 100;
                current_info = "[ " + Count + " out of " + FileList.Count + " ]  remaining : " + ((Interval_time * 1000 - sw.ElapsedMilliseconds) / 1000).ToString("f0");
                if(current_info != old_info)
                {
                    navigate_vm.info_Message = current_info;
                }
                old_info = current_info;
            }
            sw.Stop();
        }

        private void text_pop(bool on_off)
        {
            if (on_off)
            {
                text.Visibility = Visibility.Visible;
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
                FullScreen_Click(null,null);
            }

            if (e.Key == Key.Escape == true)
            {
                if (WindowState == WindowState.Maximized)
                    FullScreen_Click(null, null);
            }

            if (e.Key == Key.Space == true)
            {
                Pause(null,null);
            }

            if (e.Key == Key.G == true)
            {
                Grid(null,null);
            }

            if (e.Key == Key.Oem4)
            {
                if (grid_size < 480)
                grid_size += 4;
                BuildView();
            }

            if (e.Key == Key.Oem6)
            {
                if (grid_size > 10)
                    grid_size -= 4;
                BuildView();
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

            if (e.Key == Key.P == true)
            {
                menuProgress_Click(null, null);
            }

            if (e.Key == Key.R == true)
            {
                if (sw.IsRunning)
                    sw.Restart();
            }

            if (e.Key == Key.I == true)
            {
                info_Click(null,null);
            }

            if (e.Key == Key.Left == true && Count > 1 && FileList.Count != 0)
            {
                Count--;
                SetImage(FileList[Count - 1]);
                sw.Reset();
            }
            if (e.Key == Key.Right == true && Count < FileList.Count && FileList.Count != 0)
            {
                Count++;
                SetImage(FileList[Count - 1]);
                sw.Reset();
            }


        }

        private void Reset()
        {
            sw.Reset();
            text_pop(true);
            navigate_vm.Message = "Drop files here";
            image_.Source = imgsourse;
            navigate_vm.info_Message = "--";
            progressbar.Value = 0;
            progressbar.Visibility = Visibility.Hidden;
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
            Update_interval = 15;
            navigate_vm.info_Message = "set:" + Interval_time;
        }

        private void item30_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 30;
            Update_interval = 15;
            navigate_vm.info_Message = "set:" + Interval_time;
        }

        private void item60_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 60;
            Update_interval = 20;
            navigate_vm.info_Message = "set:" + Interval_time;
        }

        private void item90_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 90;
            Update_interval = 20;
            navigate_vm.info_Message = "set:" + Interval_time;
        }

        private void item180_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 180;
            Update_interval = 50;
            navigate_vm.info_Message = "set:" + Interval_time;
        }

        private void item300_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 300;
            Update_interval = 50;
            navigate_vm.info_Message = "set:" + Interval_time;
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
        private  int grid_size = 96;
        private ScaleTransform scaleTransform = new ScaleTransform();
        private void BuildView()
        {
            canvas.Children.Clear();

            // 縦線
            for (int i = 0; i < canvas.ActualWidth; i += grid_size)
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
            for (int i = 0; i < canvas.ActualHeight; i += grid_size)
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

        private void info_Click(object sender, RoutedEventArgs e)
        {
            if (info_label.Visibility == Visibility.Visible)
                info_label.Visibility = Visibility.Hidden;
            else
                info_label.Visibility = Visibility.Visible;
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

        private void mainwindow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.GetPosition(this).Y > SystemParameters.CaptionHeight)
                    DragMove();
            }
            catch { }
        }

        private void mainwindow_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!reset_flag)
                return;
            var dialog = new OpenFileDialog();

            dialog.Multiselect = true;
            // ファイルの種類を設定
            dialog.Filter = "JPEG |*.jpeg;*.jpg;|PNG |*.png;|WEBP |*.webp;|全てのファイル (*.*)|*.*";
            dialog.FilterIndex = 4;

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                StartSlideshow(dialog.FileNames);
            }
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
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

        private void menuProgress_Click(object sender, RoutedEventArgs e)
        {
            if (reset_flag == false)
            {
                if (progressbar.Visibility == Visibility.Visible)
                {
                    progressbar.Visibility = Visibility.Hidden;
                    RowDefinition.Height = new GridLength(0);
                }
                else
                {
                    progressbar.Visibility = Visibility.Visible;
                    RowDefinition.Height = new GridLength(7);
                }
            }
        }
    }
}
