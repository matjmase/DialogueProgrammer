using DialogueProgrammer.Common;
using DialogueProgrammer.Serialization.Export;
using DialogueProgrammer.Serialization.Project;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DialogueProgrammer.Views
{
    public class HandleWindowVM : INotifyPropertyChanged
    {
        private DialogueNodeVM _node;
        private double _canvasLeft;
        private double _canvasTop;
        private bool _isMoving;
        private Point _holdPoint;
        private Action<HandleWindowVM> _removeWindow;

        public DialogueNodeVM Node
        {
            get { return _node; }
            set
            {
                if (value != _node)
                {
                    _node = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double CanvasLeft
        {
            get { return _canvasLeft; }
            set
            {
                if (value != _canvasLeft)
                {
                    _canvasLeft = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double CanvasTop
        {
            get { return _canvasTop; }
            set
            {
                if (value != _canvasTop)
                {
                    _canvasTop = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DelegateCommand OnMouseMove => new DelegateCommand((obj) => MouseMove((MouseEventArgs)obj));

        public DelegateCommand OnMouseDown => new DelegateCommand((obj) => MouseDown((MouseButtonEventArgs)obj));

        public DelegateCommand OnMouseUp => new DelegateCommand((obj) => MouseUp((MouseButtonEventArgs)obj));

        public DelegateCommand OnMouseLeave => new DelegateCommand((obj) => MouseLeave((MouseEventArgs)obj));

        public DelegateCommand OnWindowClose => new DelegateCommand((obj) => _removeWindow(this));

        public HandleWindowVM(Action<HandleWindowVM> removeWindow, DialogueNodeVM node)
        {
            Node = node;
            _removeWindow = removeWindow;
        }

        private void MouseDown(MouseButtonEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Pressed)
            {
                _isMoving = true;
                _holdPoint = args.GetPosition((IInputElement)args.OriginalSource);
            }
        }

        private void MouseUp(MouseButtonEventArgs args)
        {
            if (args.LeftButton == MouseButtonState.Released)
            {
                _isMoving = false;
            }
        }

        private void MouseMove(MouseEventArgs args)
        {
            if (_isMoving && args.LeftButton == MouseButtonState.Pressed)
            {
                var newPoint = args.GetPosition((IInputElement)args.OriginalSource);
                var dif = newPoint - _holdPoint;
                CanvasLeft += dif.X;
                CanvasTop += dif.Y;
            }
        }

        private void MouseLeave(MouseEventArgs args)
        {
            _isMoving = false;
        }

        public ProjectSerializedNode ToProjectSerialized(int Id)
        { 
            var output = new ProjectSerializedNode() { NodeId = Id, DialogueText= _node.TerminalVM.TerminalText, CanvasLeft = CanvasLeft, CanvasTop = CanvasTop };
            output.Options = _node.OptionDialogue.Select(e => new ExportSerializedOption() { ResponseId = e.OptionId??-1, OptionText = e.OptionText }).ToArray();
            return output;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
