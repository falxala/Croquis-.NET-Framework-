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

        public int ProgressBarHeight
        {
            get { return _progressbarHeight; }
            set { _progressbarHeight = value; }
        }
        public string ProgressBarColor
        {
            get { return _progressbarColor; }
            set { _progressbarColor = value; }
        }
        public int ThumbnailSize
        {
            get { return _thumbnailSize; }
            set { _thumbnailSize = value; }
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
            _customInterval = 1;
            _preparationTime = 3;
        }
    }

}
