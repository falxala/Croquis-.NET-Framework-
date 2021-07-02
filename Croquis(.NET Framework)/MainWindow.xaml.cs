using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using shapesPath = System.Windows.Shapes.Path;

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

    public class ListImage
    {
        public string Name { get; set; }

        public int Num { get; set; }

        public BitmapSource Image { get; set; }
        public Guid Guid { get; set; }
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

    public sealed class NaturalOrderComparer : IComparer
    {
        public int Compare(object a, object b)
        {
            // replace DataItem with the actual class of the items in the ListView
            var lhs = (ListImage)a;
            var rhs = (ListImage)b;
            return SafeNativeMethods.StrCmpLogicalW(lhs.Name, rhs.Name);
        }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        string CurrentImage = "";
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        int Interval_time = 30;
        int Update_interval = 15;
        bool pause_flag = false;
        bool reset_flag = true;
        bool grayscale_flag = false;
        Brush background_Color = Brushes.White;
        ushort gridStrokeThickness = 0;
        ICollectionView cv;
        ViewModel navigate_vm = new ViewModel();
        ObservableCollection<ListImage> listImages = new ObservableCollection<ListImage>();
        ImageSource imgsourse = new RenderTargetBitmap(525,       // Imageの幅
                                       350,      // Imageの高さ
                                       96.0d,                 // 横解像度
                                       96.0d,                 // 縦解像度
                           PixelFormats.Pbgra32);
        public MainWindow()
        {
            InitializeComponent();
            image_.Source = imgsourse;
            BuildView();
            progressbar.Visibility = Visibility.Hidden;

            //バインディング
            Listview.DataContext = listImages;
            text.DataContext = navigate_vm;
            BindingOperations.EnableCollectionSynchronization(listImages, new object());

            navigate_vm.Message = "Drop files here";
            text_pop(true);

            info_label.DataContext = navigate_vm;
            navigate_vm.info_Message = "--";

        }

        private void mainwindow_Loaded(object sender, RoutedEventArgs e)
        {
            //並び替え
            cv = CollectionViewSource.GetDefaultView(listImages);
            cv.SortDescriptions.Clear();
            cv.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            HIdeThumbnail();

            comboBox.Items.Add("First In First Out");
            comboBox.Items.Add("Last In First Out");
            comboBox.Items.Add("Ascending");
            comboBox.Items.Add("Descending");
            comboBox.Items.Add("Random");
            comboBox.SelectedIndex = 0;
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

        private void Image_PreviewDrop(object sender, DragEventArgs e)
        {
            //ファイル名を取得
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Set_List(fileNames);
                StartSlideshow(fileNames);
            }
        }

        private async void Set_List(string[] fileNames)
        {
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 8;
            object lockobj = new object();
            
            int offset = 0;
            if (listImages.Count != 0)
                offset = listImages.Count;

            foreach (var item in fileNames)
            {
                listImages.Add(new ListImage());
            }

            await Task.Run(() =>
            {
                Parallel.For(0, fileNames.Length, options, i =>
                {
                    int j = i + offset;
                    Task.Delay(100);
                    ListImage image = new ListImage { Name = fileNames[i], Image = CreateThumbnail(fileNames[i]), Guid = Guid.NewGuid(), Num = j };
                    if (image.Image != null)
                        lock (lockobj) listImages[j] = image;
                });
            });

        }

        private BitmapSource CreateThumbnail(string uri)
        {
            BitmapImage thumbnail = new BitmapImage();
            try
            {
                using (FileStream stream = File.OpenRead(uri))
                {
                    thumbnail.BeginInit();
                    thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                    thumbnail.StreamSource = stream;
                    thumbnail.DecodePixelWidth = 100;
                    thumbnail.EndInit();
                    thumbnail.Freeze();
                    stream.Close();
                }
            }
            catch
            {
                return null;
            }
            return thumbnail;
        }

        bool IsRun = false;
        private async void StartSlideshow(string[] fileNames)
        {
            progressbar.Visibility = Visibility.Visible;

            if(IsRun == false)
            for (int t = 3; t > 0; t--)
            {
                text_pop(true);
                navigate_vm.Message = t.ToString();

                await Task.Delay(1000);
            }
            reset_flag = false;

            //最初の1つ目のドロップフォルダなら
            if (System.IO.Directory.Exists(fileNames[0]))
            {
                //フォルダ内のファイル名を取得
                fileNames = System.IO.Directory.GetFiles(@fileNames[0], "*", System.IO.SearchOption.TopDirectoryOnly);
            }
            
            if (IsRun == false)
                await Slideshow(fileNames);
        }

        private async Task Slideshow(string[] fileNames)
        {
            text_pop(false);

            IsRun = true;

            try
            {
                do
                {
                    Listview.SelectedIndex++;
                    foreach (ListImage selected_item in Listview.SelectedItems)
                    {
                        CurrentImage = selected_item.Name;
                    }
                    await showImage(CurrentImage);
                    if (reset_flag == true)
                    {
                        throw new Exception();
                    }
                    if (Listview.SelectedIndex + 1 >= listImages.Count)
                        break;
                } while (Listview.SelectedIndex < listImages.Count);
                image_.Source = imgsourse;
                navigate_vm.Message = "Finish!";

            }
            catch (Exception)
            {
                Reset();
            }
            finally
            {
                if (MenuRepeat.IsChecked && reset_flag == false)
                {
                    text_pop(false);
                    Listview.SelectedIndex = -1;
                    await Slideshow(fileNames);
                }
                else
                {
                    if (!reset_flag)
                    {
                        text_pop(true);
                        await Task.Delay(3000);
                        Reset();
                    }
                }
            }
        }

        private bool SetImage(string image_name)
        {
            try
            {
                CurrentImage = image_name;
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
                return true;

            }
            catch
            {
                //Console.WriteLine(ex.Message);
                Listview.SelectedIndex = 0;
                return false;

            }

        }

        private async Task showImage(string image_name)
        {
            try
            {
                if (!SetImage(image_name))
                    throw new Exception();

                var seconds = 0;
                var span = new TimeSpan(0, 0, seconds);

                sw.Reset();
                sw.Start();
                string old_info = "--";
                string current_info;
                while (sw.ElapsedMilliseconds <= Interval_time * 1000 && reset_flag == false)
                {
                    pause();

                    await Task.Delay(Update_interval);//delayの精度が15ms
                    progressbar.Value = (double)sw.ElapsedMilliseconds / (Interval_time * 1000) * 100;
                    seconds = (int)((Interval_time * 1000 - sw.ElapsedMilliseconds) / 1000);
                    span = new TimeSpan(0, 0, seconds);
                    ; current_info = "[ " + (Listview.SelectedIndex + 1) + " out of " + listImages.Count + " ]  remaining : " + span.ToString(@"mm\:ss");
                    if (current_info != old_info)
                    {
                        navigate_vm.info_Message = current_info;
                    }
                    old_info = current_info;
                }
                sw.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                sw.Stop();
                sw.Reset();
            }

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
            switch (e.Key)
            {
                case Key.Enter:
                    FullScreen_Click(null, null);
                    break;

                case Key.Escape:
                    if (WindowState == WindowState.Maximized)
                    {
                        FullScreen_Click(null, null);
                    }
                    break;

                case Key.Space:
                    Pause(null, null);
                    break;

                case Key.G:
                    Grid(null, null);
                    break;

                case Key.OemPeriod:
                    if (gridStrokeThickness <= 10)
                    {
                        gridStrokeThickness++;
                        BuildView();
                    }
                    break;

                case Key.OemComma:
                    if (gridStrokeThickness > 0)
                    {
                        gridStrokeThickness--;
                        BuildView();
                    }
                    break;

                case Key.OemOpenBrackets:
                    if (grid_size < 480)
                        grid_size += 4;
                    BuildView();
                    break;

                case Key.OemCloseBrackets:
                    if (grid_size > 10)
                        grid_size -= 4;
                    BuildView();
                    break;

                case Key.B:
                    if (background_Color == Brushes.White)
                        Black(null, null);
                    else
                        White(null, null);
                    break;

                case Key.S:
                    GrayScale(null, null);
                    break;

                case Key.C:
                    GrayScale(null, null);
                    break;

                case Key.P:
                    menuProgress_Click(null, null);
                    break;

                case Key.R:
                    sw.Reset();
                    break;

                case Key.I:
                    info_Click(null, null);
                    break;

                case Key.Tab:
                    HIdeThumbnail();
                    break;

                case Key.Left:
                    if (!Listview.IsEnabled && Listview.SelectedIndex > 0 && !reset_flag)
                    {
                        Listview.SelectedIndex--;
                    }
                    break;

                case Key.Right:
                    if (!Listview.IsEnabled && Listview.Items.Count >= Listview.SelectedIndex && !reset_flag)
                    {
                        Listview.SelectedIndex++;
                    }
                    break;

            }
        }

        private void Reset()
        {
            IsRun = false;
            sw.Stop();
            sw.Reset();
            text_pop(true);
            navigate_vm.Message = "Drop files here";
            image_.Source = imgsourse;
            navigate_vm.info_Message = "--";
            progressbar.Value = 0;
            progressbar.Visibility = Visibility.Hidden;
            grayscale_flag = false;
            pause_flag = false;
            listImages.Clear();
            reset_flag = true;
            Listview.SelectedIndex = -1;
            CurrentImage = "";
            IsRun = false;
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
            Interval_time = 15;
            Update_interval = 15;
            navigate_vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item30_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 30;
            Update_interval = 15;
            navigate_vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item60_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 60;
            Update_interval = 20;
            navigate_vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item90_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 90;
            Update_interval = 20;
            navigate_vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item180_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 180;
            Update_interval = 50;
            navigate_vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item300_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 300;
            Update_interval = 50;
            navigate_vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item600_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 600;
            Update_interval = 100;
            navigate_vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item900_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 900;
            Update_interval = 100;
            navigate_vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }


        private void Pause(object sender, RoutedEventArgs e)
        {
            pause_flag = !pause_flag;
            if (pause_flag == true)
            {
                mainwindow.Background = Brushes.LightGray;
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
        private int grid_size = 96;
        private ScaleTransform scaleTransform = new ScaleTransform();
        private void BuildView()
        {
            canvas.Children.Clear();

            // 縦線
            for (int i = 0; i < canvas.ActualWidth; i += grid_size)
            {
                shapesPath path = new shapesPath()
                {
                    Data = new LineGeometry(new Point(i, 0), new Point(i, canvas.ActualHeight)),
                    Stroke = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128)),
                    StrokeThickness = gridStrokeThickness + 1
                };

                path.Data.Transform = scaleTransform;

                canvas.Children.Add(path);
            }

            // 横線
            for (int i = 0; i < canvas.ActualHeight; i += grid_size)
            {
                shapesPath path = new shapesPath()
                {
                    Data = new LineGeometry(new Point(0, i), new Point(canvas.ActualWidth, i)),
                    Stroke = new SolidColorBrush(Color.FromArgb(128, 128, 128, 128)),
                    StrokeThickness = gridStrokeThickness + 1
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

        private BitmapSource ToGrayScale(BitmapSource bitmap)
        {
            FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
            newFormatedBitmapSource.BeginInit();
            newFormatedBitmapSource.Source = bitmap;
            newFormatedBitmapSource.AlphaThreshold = 0.001;
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

        private void DragMove_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var a = VisualTreeHelper.HitTest(this, e.GetPosition(this)).VisualHit;
                if (VisualTreeHelper.HitTest(this,e.GetPosition(this)).VisualHit != Panel)
                    DragMove();
            }
            catch { }
        }

        private void mainwindow_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (!reset_flag || wait_time) return;

            var dialog = new OpenFileDialog();

            dialog.Multiselect = true;
            // ファイルの種類を設定
            dialog.Filter = "JPEG |*.jpeg;*.jpg;|PNG |*.png;|WEBP |*.webp;|全てのファイル (*.*)|*.*";
            dialog.FilterIndex = 4;

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                Set_List(dialog.FileNames);
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


        private void MenuRepeat_Click(object sender, RoutedEventArgs e)
        {
            MenuRepeat.IsChecked = !MenuRepeat.IsChecked;
        }

        private void quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Listview_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                sw.Reset();
                foreach (ListImage selected_item in Listview.SelectedItems)
                {
                    if (!reset_flag)
                    {
                        SetImage(selected_item.Name);
                    }
                }
            }
            catch { }
        }

        Point oldpoint;
        GridLength old_thumbnailColumnWidth;
        private void GridSplitter_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.GetPosition(this) == oldpoint)
            {
                HIdeThumbnail();
            }
        }

        private void GridSplitter_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            oldpoint = e.GetPosition(this);
        }

        private void HIdeThumbnail()
        {
            if (thumbnailColumn.Width.Value != 0)
            {
                old_thumbnailColumnWidth = thumbnailColumn.Width;
                thumbnailColumn.MinWidth = 0;
                thumbnailColumn.Width = new GridLength(0, GridUnitType.Star);
                GridSplitterColumn.MinWidth = 0;
                GridSplitterColumn.Width = new GridLength(0, GridUnitType.Auto);
                Listview.IsEnabled = false;
            }
            else
            {
                thumbnailColumn.MinWidth = 160;
                thumbnailColumn.Width = old_thumbnailColumnWidth;
                GridSplitterColumn.MinWidth = 10;
                GridSplitterColumn.Width = new GridLength(7, GridUnitType.Auto);
                Listview.IsEnabled = true;

            }
        }

        private void Panel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HIdeThumbnail();
        }

        private void menuListview_Click(object sender, RoutedEventArgs e)
        {
            HIdeThumbnail();
        }

        private void MenuStart_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();
            foreach (ListImage name in listImages)
            {
                list.Add(name.Name);
            }
            StartSlideshow(list.ToArray());
        }

        private void comboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    cv.SortDescriptions.Clear();
                    cv.SortDescriptions.Add(new SortDescription("Num", ListSortDirection.Ascending));
                    break;
                case 1:
                    cv.SortDescriptions.Clear();
                    cv.SortDescriptions.Add(new SortDescription("Num", ListSortDirection.Descending));
                    break;
                case 2:
                    cv.SortDescriptions.Clear();
                    cv.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                    break;
                case 3:
                    cv.SortDescriptions.Clear();
                    cv.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending));
                    break;
                case 4:
                    cv.SortDescriptions.Clear();
                    cv.SortDescriptions.Add(new SortDescription("Guid", ListSortDirection.Ascending));
                    break;
            }
        }

    }
}
