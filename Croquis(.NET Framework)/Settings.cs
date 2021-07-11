using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private int _preparationTime;
        private bool _preparationTimeIsEnabled;
        private bool _unificationOfDisplayQuality;
        private int _unifiedResolution;

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

        public Settings()
        {
            _progressbarHeight = 7;
            _thumbnailSize = 127;
            _infoTextSize = 14;
            _progressbarColor = "#FFFF3030";
            _customInterval = 5;
            _preparationTime = 3;
            _unificationOfDisplayQuality = true;
            _unifiedResolution = 1920;
            _preparationTimeIsEnabled = true;
        }
    }

}
