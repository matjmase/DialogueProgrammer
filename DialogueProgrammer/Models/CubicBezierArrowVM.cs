using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DialogueProgrammer.Models
{
    public class CubicBezierArrowVM : INotifyPropertyChanged
    {
        private Point _startPoint;
        private Point _reference1;
        private Point _reference2;
        private Point _endPoint;

        private Point _endPointUpperArrow;
        private Point _endPointLowerArrow;

        public Point StartPoint
        {
            get { return _startPoint; }
            set
            {
                _startPoint = value;
                NotifyPropertyChanged();
            }
        }

        public Point Reference1
        {
            get { return _reference1; }
            set
            {
                _reference1 = value;
                NotifyPropertyChanged();
            }
        }

        public Point Reference2
        {
            get { return _reference2; }
            set
            {
                _reference2 = value;
                NotifyPropertyChanged();
            }
        }

        public Point EndPoint
        {
            get { return _endPoint; }
            set
            {
                _endPoint = value;
                NotifyPropertyChanged();
            }
        }

        public Point EndPointUpperArrow
        {
            get { return _endPointUpperArrow; }
            set
            {
                _endPointUpperArrow = value;
                NotifyPropertyChanged();
            }
        }

        public Point EndPointLowerArrow
        {
            get { return _endPointLowerArrow; }
            set
            {
                _endPointLowerArrow = value;
                NotifyPropertyChanged();
            }
        }

        public void UpdateCurve(Point start, double startAngle, Point end, double endAngle, double intensity, double arrowLength, double arrowTheta)
        {
            var startRadians = startAngle / 360 * 2 * Math.PI;
            var endRadians = endAngle / 360 * 2 * Math.PI;

            var thetaRadians = (arrowTheta / 2) / 360 * 2 * Math.PI;

            StartPoint = start;
            Reference1 = new Point(Math.Cos(startRadians) * intensity + start.X, Math.Sin(startRadians) * intensity + start.Y);

            EndPoint = end;
            Reference2 = new Point(Math.Cos(endRadians) * intensity + end.X, Math.Sin(endRadians) * intensity + end.Y);

            var theta1 = endRadians + thetaRadians;
            var theta2 = endRadians - thetaRadians;

            EndPointLowerArrow = new Point(Math.Cos(theta1) * arrowLength + EndPoint.X, Math.Sin(theta1) * arrowLength + EndPoint.Y);
            EndPointUpperArrow = new Point(Math.Cos(theta2) * arrowLength + EndPoint.X, Math.Sin(theta2) * arrowLength + EndPoint.Y);
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

    }
}
