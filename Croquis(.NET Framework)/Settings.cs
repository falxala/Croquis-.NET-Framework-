using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Croquis_.NET_Framework_
{
    public class Settings
    {
        private int _progressbarHeight;
        private string _progressbarColor;
        private int _thumbnailSize;
        private int _infoTextSize;
        private int _customInterval;
        private int _oldInterval;
        private int _preparationTime;
        private bool _preparationTimeIsEnabled;
        private bool _unificationOfDisplayQuality;
        private int _unifiedResolution;
        private BitmapScalingMode _bitmapScalingMode;
        private HorizontalAlignment _Horizontal;
        private VerticalAlignment _Vertical;
        private bool _memory;
        private int _number;
        private string[] _fileNames;

        public bool UnificationOfDisplayQuality
        {
            get { return _unificationOfDisplayQuality; }
            set { _unificationOfDisplayQuality = value; }
        }
        public int UnifiedResolution
        {
            get { return _unifiedResolution; }
            set { _unifiedResolution = value; }
        }
        public int ProgressBarHeight
        {
            get { return _progressbarHeight; }
            set
            {
                if (value <= 2560)
                    _progressbarHeight = value;
            }
        }
        public string ProgressBarColor
        {
            get { return _progressbarColor; }
            set { _progressbarColor = value; }
        }
        public int ThumbnailSize
        {
            get { return _thumbnailSize; }
            set
            {
                if (value <= 512)
                    _thumbnailSize = value;
            }
        }
        public int InfoTextSize
        {
            get { return _infoTextSize; }
            set { _infoTextSize = value; }
        }
        public int CustomInterval
        {
            get { return _customInterval; }
            set { _customInterval = value; }
        }
        public int OldInterval
        {
            get { return _oldInterval; }
            set { _oldInterval = value; }
        }
        public bool PreparationTimeIsEnabled
        {
            get { return _preparationTimeIsEnabled; }
            set { _preparationTimeIsEnabled = value; }
        }
        public int PreparationTime
        {
            get { return _preparationTime; }
            set { _preparationTime = value; }
        }
        public BitmapScalingMode BitmapScalingMode
        {
            get { return _bitmapScalingMode; }
            set { _bitmapScalingMode = value; }
        }
        public HorizontalAlignment Horizontal
        {
            get { return _Horizontal; }
            set { _Horizontal = value; }
        }
        public VerticalAlignment Vertical
        {
            get { return _Vertical; }
            set { _Vertical = value; }
        }

        public bool Memory
        {
            get { return _memory; }
            set { _memory = value; }
        }

        public int Number
        {
            get { return _number; }
            set { _number = value; }
        }

        public string[] FileNames
        {
            get { return _fileNames; }
            set { _fileNames = value; }
        }

        public Settings()
        {
            _progressbarHeight = 7;
            _thumbnailSize = 127;
            _infoTextSize = 20;
            _progressbarColor = "#FFFF3030";
            _customInterval = 5;
            _oldInterval = 30;
            _preparationTime = 3;
            _unificationOfDisplayQuality = true;
            _unifiedResolution = 1920;
            _preparationTimeIsEnabled = true;
            _bitmapScalingMode = BitmapScalingMode.Fant;
            _Horizontal = HorizontalAlignment.Right;
            _Vertical = VerticalAlignment.Bottom;
            _memory = false;
            _number = 0;
            _fileNames = null;
        }
    }

}
