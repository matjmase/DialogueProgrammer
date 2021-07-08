using DialogueProgrammer.Common;
using DialogueProgrammer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DialogueProgrammer.Views.SubViews
{
    public class DialogueTerminalVM : INotifyPropertyChanged
    {

        private string _terminalText;
        private Action _clearLinks;
        private Action<Point> _attemptToLink;
        private Action<Point> _updatedLayoutPosition;

        public string TerminalText
        {
            get { return _terminalText; }
            set
            {
                if (value != _terminalText)
                {
                    _terminalText = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public DelegateCommand AttemptToLink => new DelegateCommand((obj) => AttempToLinkPosition((MouseButtonEventArgs)obj));
        public DelegateCommand LayoutUpdated => new DelegateCommand((obj) => LayoutUpdatePosition((FrameworkElement)obj));

        public DelegateCommand ClearTerminalLinks => new DelegateCommand((obj) => _clearLinks());

        public DialogueTerminalVM(Action<Point> attemptToLink, Action clearLinks, Action<Point> updatedLayoutPosition, string terminalText = "Dialogue Text")
        {
            _clearLinks = clearLinks;
            _updatedLayoutPosition = updatedLayoutPosition;
            _attemptToLink = attemptToLink;
            TerminalText = terminalText;
        }

        private void AttempToLinkPosition(MouseButtonEventArgs e)
        {
            _attemptToLink(((FrameworkElement)e.OriginalSource).GetLocalCanvasPoint());
        }

        private void LayoutUpdatePosition(FrameworkElement e)
        {
            _updatedLayoutPosition(e.GetLocalCanvasPoint());
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
