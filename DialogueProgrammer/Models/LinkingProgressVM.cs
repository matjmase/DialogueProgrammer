using DialogueProgrammer.Views;
using DialogueProgrammer.Views.SubViews;
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
    public class LinkingProgressVM : INotifyPropertyChanged
    {
        private LinkingType? _linkStartType;

        public Point LinkStartPoint { get; private set; }
        public DialogueOptionVM LinkStartOption { get; private set; }
        public DialogueNodeVM LinkStartNode { get; private set; }

        public LinkingType? LinkStartType
        {
            get { return _linkStartType; }
            private set
            {
                if (value != _linkStartType)
                {
                    _linkStartType = value;
                    NotifyPropertyChanged("LinkingInProgress");
                }
            }
        }

        public bool LinkingInProgress
        {
            get { return _linkStartType != null; }
        }

        public void Clear()
        {
            LinkStartType = null;
            LinkStartPoint = default(Point);
            LinkStartOption = null;
            LinkStartNode = null;
        }

        public void StartLinkingProcess(Point location, DialogueOptionVM option)
        {
            LinkStartPoint = location;
            LinkStartOption = option;
            LinkStartType = LinkingType.Option;
        }
        public void StartLinkingProcess(Point location, DialogueNodeVM node)
        {
            LinkStartPoint = location;
            LinkStartNode = node;
            LinkStartType = LinkingType.Terminal;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public enum LinkingType
    {
        Option,
        Terminal
    }
}
