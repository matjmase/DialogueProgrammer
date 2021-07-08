using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DialogueProgrammer.Models
{
    public class DimensionScalerVM : INotifyPropertyChanged
    {
        private double _baseWidth;
        private double _baseHeight;
        private double _scalerWidth;
        private double _scalerHeight;

        public double BaseWidth
        {
            get { return _baseWidth; }
            set
            {
                if (value != _baseWidth)
                {
                    _baseWidth = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("OutputWidth");
                }
            }
        }

        public double BaseHeight
        {
            get { return _baseHeight; }
            set
            {
                if (value != _baseHeight)
                {
                    _baseHeight = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("OutputHeight");
                }
            }
        }

        public double ScalerWidth
        {
            get { return _scalerWidth; }
            set
            {
                if (value != _scalerWidth)
                {
                    _scalerWidth = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("OutputWidth");
                }
            }
        }

        public double ScalerHeight
        {
            get { return _scalerHeight; }
            set
            {
                if (value != _scalerHeight)
                {
                    _scalerHeight = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("OutputHeight");
                }
            }
        }

        public double OutputWidth
        {
            get { return ScalerWidth * BaseWidth; }
        }

        public double OutputHeight
        {
            get
            {
                return ScalerHeight * BaseHeight;
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
