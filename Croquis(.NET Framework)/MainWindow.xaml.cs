using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using shapesPath = System.Windows.Shapes.Path;
using System.Windows.Interop;

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

        private int num = 0;
        public int Num
        {
            get
            {
                return this.num;
            }
            set
            {
                if (value != this.num)
                {
                    this.num = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string progressColor = "";
        public string ProgressColor
        {
            get
            {
                return this.progressColor;
            }
            set
            {
                if (value != this.progressColor)
                {
                    this.progressColor = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int infoTextSize = 0;
        public int InfoTextSize
        {
            get
            {
                return this.infoTextSize;
            }
            set
            {
                if (value != this.infoTextSize)
                {
                    this.infoTextSize = value;
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
        public int ThumbnailGridSize { get; set; }
    }

    public class LogicalStringComparer :
    System.Collections.IComparer,
    System.Collections.Generic.IComparer<string>
    {
        [System.Runtime.InteropServices.DllImport("shlwapi.dll",
            CharSet = System.Runtime.InteropServices.CharSet.Unicode,
            ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string x, string y);

        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }

        public int Compare(object x, object y)
        {
            return this.Compare(x.ToString(), y.ToString());
        }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        int maxWidth = 1000;
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
        ViewModel vm = new ViewModel();
        private ObservableCollection<ListImage> listImages = new ObservableCollection<ListImage>();
        private ImageSource imgsourse = new RenderTargetBitmap(1,1,96.0d,96.0d,PixelFormats.Pbgra32);
        private readonly BitmapSource dummy = new RenderTargetBitmap(1, 1, 96.0d, 96.0d, PixelFormats.Pbgra32);
        public Settings appSettings = new Settings();
        readonly string settingsFileName = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\')+ @"\settings.config";

        public MainWindow()
        {
            InitializeComponent();
            image_.Source = imgsourse;
            BuildView();
            progressbar.Visibility = Visibility.Hidden;

            //バインディング
            listview1.DataContext = listImages;
            text.DataContext = vm;
            BindingOperations.EnableCollectionSynchronization(listImages, new object());

            vm.Message = "Drop files here";
            text_pop(true);

            info_label.DataContext = vm;
            vm.info_Message = "--";
            RowDefinition.DataContext = vm;
            vm.Num = appSettings.ProgressBarHeight;
            info_label.DataContext = vm;
            vm.InfoTextSize = appSettings.InfoTextSize;
            progressbar.DataContext = vm;
            vm.ProgressColor = appSettings.ProgressBarColor;

            RenderOptions.SetBitmapScalingMode(image_, BitmapScalingMode.Fant);
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
            if (SystemParameters.PrimaryScreenWidth > SystemParameters.PrimaryScreenHeight)
                maxWidth = (int)SystemParameters.PrimaryScreenWidth;
            else
                maxWidth = (int)SystemParameters.PrimaryScreenHeight;

            appSettings = Load(appSettings, settingsFileName);
            vm.Num = appSettings.ProgressBarHeight;
            vm.InfoTextSize = appSettings.InfoTextSize;
            vm.ProgressColor = appSettings.ProgressBarColor;

            itemCustomInterval.DataContext = "Custom : " + new TimeSpan(0, 0, appSettings.CustomInterval).ToString(@"mm\:ss");
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
            //ファイル名を取得
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);


            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (System.IO.Directory.Exists(fileNames[0]))
                {
                    //フォルダ内のファイル名を取得
                    fileNames = System.IO.Directory.GetFiles(@fileNames[0], "*", System.IO.SearchOption.TopDirectoryOnly);
                    Array.Sort(fileNames, new LogicalStringComparer());
                }

                await Set_List(fileNames);
                StartSlideshow();
            }
        }

        private async Task Set_List(string[] fileNames)
        {
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 8;
            object lockobj = new object();
            
            int offset = 0;
            if (listImages.Count != 0)
                offset = listImages.Count;

            List<int> removeIndex = new List<int>();

            vm.Message = "Loading";
            await Task.Run(() =>
            {

                foreach (var item in fileNames)
                {
                    listImages.Add(new ListImage { Name = null, Image = ResizeImage(null, 0, true), Guid = Guid.NewGuid(), Num = 0});
                }

                Parallel.For(0, fileNames.Length, options, i =>
                {
                    int j = i + offset;
                    ListImage image = new ListImage { 
                        Name = fileNames[i], Image = ResizeImage(fileNames[i], appSettings.ThumbnailSize, true), 
                        Guid = Guid.NewGuid(), Num = j , ThumbnailGridSize = appSettings.ThumbnailSize };
                    if (image.Image == dummy)
                        removeIndex.Add(j);
                    else
                        lock (lockobj) listImages[j] = image;
                });
            });
            removeIndex.Sort();//Parallelでバラバラなので並び替え
            for (int index = removeIndex.Count -1; index >= 0; index--)
            {
                listImages.RemoveAt(removeIndex[index]);
            }
        }

        private BitmapSource ResizeImage(string uri,int pixel,bool thumbnail_flag)
        {
            if (uri == null)
                return dummy;
            try
            {
                using (FileStream stream = File.OpenRead(uri))
                {
                    BitmapImage thumbnail = new BitmapImage();
                    thumbnail.BeginInit();
                    thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                    thumbnail.CreateOptions = BitmapCreateOptions.None;
                    if (thumbnail_flag)
                        thumbnail.DecodePixelWidth = pixel;
                    
                    var metaData = BitmapFrame.Create(stream).Metadata as BitmapMetadata;
                    stream.Position = 0;
                    string query = "/app1/ifd/exif:{uint=274}";
                    if (metaData.ContainsQuery(query))
                    {

                        switch (Convert.ToUInt32(metaData.GetQuery(query)))
                        {
                            case 3:
                                thumbnail.Rotation = Rotation.Rotate180;
                                break;
                            case 6:
                                thumbnail.Rotation = Rotation.Rotate90;
                                break;
                            case 8:
                                thumbnail.Rotation = Rotation.Rotate270;
                                break;
                            default:
                                thumbnail.Rotation = Rotation.Rotate0;
                                break;
                        }
                    }
                    thumbnail.StreamSource = stream;
                    thumbnail.EndInit();
                    thumbnail.Freeze();
                    stream.Close();
                    return thumbnail;
                }
            }
            catch
            {
                try
                {
                    using (FileStream stream = File.OpenRead(uri))
                    {
                        BitmapImage thumbnail = new BitmapImage();
                        thumbnail.BeginInit();
                        thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                        thumbnail.CreateOptions = BitmapCreateOptions.None;
                        if (thumbnail_flag)
                            thumbnail.DecodePixelWidth = pixel;
                        thumbnail.StreamSource = stream;
                        thumbnail.EndInit();
                        thumbnail.Freeze();
                        stream.Close();
                        return thumbnail;
                    }
                }
                catch
                {
                    return dummy;
                }
            }
            
        }

        bool IsRun = false;
        private async void StartSlideshow()
        {
            progressbar.Visibility = Visibility.Visible;

            IsRun = true;
            for (int t = 3; t > 0; t--)
            {
                text_pop(true);
                vm.Message = t.ToString();

                await Task.Delay(1000);
            }
            pause_flag = false;
            reset_flag = false;

            await Slideshow();
        }

        private async Task Slideshow()
        {
            text_pop(false);

            IsRun = true;

            try
            {
                do
                {
                    if (listview1.SelectedIndex <= -1)
                        listview1.SelectedIndex++;

                    foreach (ListImage selected_item in listview1.SelectedItems)
                    {
                        CurrentImage = selected_item.Name;
                    }
                    if(CurrentImage == null)
                    {
                        text_pop(true);
                        vm.Message = "Still loading";
                        await Task.Delay(200);
                        text_pop(false);
                        continue;
                    }

                    await showImage(CurrentImage);//イメージを表示

                    if (reset_flag == true)
                    {
                        throw new Exception();
                    }

                    //Repeatが有効かつ最後のイメージなら最初に戻る
                    if (listview1.SelectedIndex + 1 >= listImages.Count && MenuRepeat.IsChecked)
                        listview1.SelectedIndex = -1;
                    else if (listview1.SelectedIndex + 1 >= listImages.Count)
                        break;

                    listview1.SelectedIndex++;

                    int Pretime = appSettings.PreparationTime * 5;
                    string oldCurrent = CurrentImage;
                    text_pop(true);
                    image_.Opacity = 0.5;

                    while (MenuPreparationTime.IsChecked &&
                        Pretime > 0 && reset_flag == false && 
                        CurrentImage == oldCurrent)
                    {
                        Pretime--;
                        vm.Message = ((Pretime / 5) + 1).ToString();
                        await Task.Delay(200);
                    }
                    text_pop(false);
                    image_.Opacity = 1;


                } while (listview1.SelectedIndex < listImages.Count);
                image_.Opacity = 0.5;
                vm.Message = "Finish!";

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
                    listview1.SelectedIndex = -1;
                    await Slideshow();
                }
                else
                {
                    if (!reset_flag)
                    {
                        text_pop(true);
                        sw.Reset();
                    }
                }
                IsRun = false;
            }
        }

        WriteableBitmap m_processedBitmap = null;
        private bool SetImage(string image_name)
        {
            try
            {
                image_.Opacity = 1;
                CurrentImage = image_name;

                m_processedBitmap = new WriteableBitmap(ResizeImage(CurrentImage, 1, false));

                if (m_processedBitmap == null)
                    throw new Exception();

                if (grayscale_flag)
                {
                    image_.Source = ToGrayscale(m_processedBitmap);
                }
                else
                {
                    image_.Source = m_processedBitmap;
                }
                m_processedBitmap.Freeze();
                m_processedBitmap = null;
                return true;

            }
            catch (OutOfMemoryException ex)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                return SetImage(image_name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    ; current_info = "[ " + (listview1.SelectedIndex + 1) + " out of " + listImages.Count + " ] " + span.ToString(@"mm\:ss");
                    if (current_info != old_info)
                    {
                        vm.info_Message = current_info;
                    }
                    old_info = current_info;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
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

                case Key.OemCloseBrackets:
                    if (grid_size < 480)
                        grid_size += 4;
                    BuildView();
                    break;

                case Key.OemOpenBrackets:
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
                    if (!listview1.IsEnabled && listview1.SelectedIndex > 0 && !reset_flag)
                    {
                        listview1.SelectedIndex--;
                    }
                    break;

                case Key.Right:
                    if (!listview1.IsEnabled && listview1.Items.Count >= listview1.SelectedIndex && !reset_flag)
                    {
                        listview1.SelectedIndex++;
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
            image_.Source = null;
            image_.Source = imgsourse;
            progressbar.Value = 0;
            progressbar.Visibility = Visibility.Hidden;
            pause_flag = false;
            listImages.Clear();
            reset_flag = true;
            listview1.SelectedIndex = -1;
            CurrentImage = "";
            IsRun = false;
            appSettings = Load(appSettings, settingsFileName);
            vm.Num = appSettings.ProgressBarHeight;
            vm.InfoTextSize = appSettings.InfoTextSize;
            vm.info_Message = "--";
            vm.Message = "Drop files here";
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void pause()
        {
            if (pause_flag == true)
            {
                sw.Stop();
            }
            else if (sw.IsRunning == false && pause_flag == false)
            {
                sw.Start();
            }
        }

        private void itemCustomInterval_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = appSettings.CustomInterval;
            Update_interval = 15;
            vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item15_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 15;
            Update_interval = 15;
            vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item30_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 30;
            Update_interval = 15;
            vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item60_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 60;
            Update_interval = 20;
            vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item90_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 90;
            Update_interval = 20;
            vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item180_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 180;
            Update_interval = 50;
            vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item300_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 300;
            Update_interval = 50;
            vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item600_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 600;
            Update_interval = 100;
            vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }

        private void item900_Click(object sender, RoutedEventArgs e)
        {
            Interval_time = 900;
            Update_interval = 100;
            vm.info_Message = "set:" + new TimeSpan(0, 0, Interval_time).ToString(@"mm\:ss");
        }


        private async void Pause(object sender, RoutedEventArgs e)
        {
            if (!IsRun)
                await Slideshow();
            pause_flag = !pause_flag;
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


        
        //参考　http://www.pronowa.com/room/wpf_imaging003.html
        private WriteableBitmap ToGrayscale(WriteableBitmap m_processedBitmap)
        {
            if (m_processedBitmap == null)
            {
                return null;
            }

            int channel = 4;

            int width = m_processedBitmap.PixelWidth;
            int height = m_processedBitmap.PixelHeight;
            int stride = width * channel;
            int res = stride - channel * width;
            m_processedBitmap.Lock();
            unsafe
            {
                byte* pBackBuffer =
                    (byte*)(void*)m_processedBitmap.BackBuffer;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte b = pBackBuffer[0];
                        byte g = pBackBuffer[1];
                        byte r = pBackBuffer[2];

                        int gray =
                            (int)(0.29811 * r + 0.58661 * g + 0.11448 * b);
                        if (gray > 255)
                        {
                            gray = 255;
                        }
                        if (gray < 0)
                        {
                            gray = 0;
                        }

                        pBackBuffer[0] = (byte)gray;
                        pBackBuffer[1] = (byte)gray;
                        pBackBuffer[2] = (byte)gray;

                        pBackBuffer += channel;
                    }
                    pBackBuffer += res;
                }
            }
            m_processedBitmap.AddDirtyRect(
                new Int32Rect(0, 0, width, height));
            m_processedBitmap.Unlock();
            m_processedBitmap.Freeze();
            return m_processedBitmap;
        }

        private void GrayScale(object sender, RoutedEventArgs e)
        {
            try
            {
                if (reset_flag == true)
                    throw new Exception();

                grayscale_flag = !grayscale_flag;
                SetImage(CurrentImage);
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

        private async void mainwindow_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
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
                await Set_List(dialog.FileNames);
                StartSlideshow();
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
                    RowDefinition.Height = new GridLength(appSettings.ProgressBarHeight);
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
                progressbar.Value = (double)sw.ElapsedMilliseconds / (Interval_time * 1000) * 100;
                foreach (ListImage selected_item in listview1.SelectedItems)
                {
                    if (!reset_flag)
                    {
                        text_pop(false);
                        SetImage(selected_item.Name);
                    }
                }
                if (listview1.SelectedIndex >= 0)
                {
                    ListViewItem item = (ListViewItem)listview1.ItemContainerGenerator.ContainerFromItem(listview1.SelectedItem);
                    item.Focus();
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
                listview1.IsEnabled = false;
            }
            else
            {
                thumbnailColumn.MinWidth = 160;
                thumbnailColumn.Width = old_thumbnailColumnWidth;
                GridSplitterColumn.MinWidth = 10;
                GridSplitterColumn.Width = new GridLength(7, GridUnitType.Auto);
                listview1.IsEnabled = true;

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
            StartSlideshow();
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

        private void mainwindow_Closing(object sender, CancelEventArgs e)
        {
            Save(appSettings,settingsFileName);
        }

        public void Save(Settings appSettings, string fileName)
        {
            try
            {
                //書き込むオブジェクトの型を指定する
                System.Xml.Serialization.XmlSerializer serializer1 =
                    new System.Xml.Serialization.XmlSerializer(typeof(Settings));
                //ファイルを開く（UTF-8 BOM無し）
                System.IO.StreamWriter sw = new System.IO.StreamWriter(
                    fileName, false, new System.Text.UTF8Encoding(false));
                //シリアル化し、XMLファイルに保存する
                serializer1.Serialize(sw, appSettings);
                //閉じる
                sw.Close();
            }
            catch { }
        }

        public Settings Load(Settings loadSettings, string fileName)
        {
            try
            {
                //＜XMLファイルから読み込む＞
                //XmlSerializerオブジェクトの作成
                System.Xml.Serialization.XmlSerializer serializer2 =
                    new System.Xml.Serialization.XmlSerializer(typeof(Settings));
                //ファイルを開く
                System.IO.StreamReader sr = new System.IO.StreamReader(
                    fileName, new System.Text.UTF8Encoding(false));
                //XMLファイルから読み込み、逆シリアル化する
                loadSettings = (Settings)serializer2.Deserialize(sr);
                //閉じる
                sr.Close();
            }
            catch { }
            return loadSettings;
        }

        private void MenuPreparationTime_Click(object sender, RoutedEventArgs e)
        {
            MenuPreparationTime.IsChecked = !MenuPreparationTime.IsChecked;
        }
    }
    public static class Extention
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }
    } 
}
