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
using System.Windows.Controls;
using System.Windows.Input;

namespace DialogueProgrammer.Views.SubViews
{
    public class DialogueOptionVM : INotifyPropertyChanged
    {
        private int? _optionId;
        private string _optionText;
        private DialogueNodeVM _linkedNode;
        private Action<DialogueOptionVM> _removeSelf;
        private Action<DialogueOptionVM, Point> _initializingLink;
        private Action<DialogueOptionVM> _clearOptionLink;
        private Action<DialogueOptionVM, Point> _updatedLayoutPosition;

        public int? OptionId
        {
            get { return _optionId; }
            set
            {
                if(_optionId != value)
                {
                    _optionId = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string OptionText
        {
            get { return _optionText; }
            set
            {
                if (_optionText != value)
                {
                    _optionText = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public DialogueNodeVM LinkedNode
        {
            get { return _linkedNode; }
            set
            {
                if (_linkedNode != value)
                {
                    _linkedNode = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public DelegateCommand RemoveSelf => new DelegateCommand((obj) => _removeSelf(this));
        public DelegateCommand ClearOptionLink => new DelegateCommand((obj) => { LinkedNode = null; _clearOptionLink(this); });
        public DelegateCommand InitializeLink => new DelegateCommand((obj) => InitializeLinkPosition((MouseButtonEventArgs)obj));

        public DelegateCommand UpdatedLayout => new DelegateCommand((obj) => UpdatedLayoutPosition((FrameworkElement)obj));

        public DialogueOptionVM(Action<DialogueOptionVM> removeSelf, Action<DialogueOptionVM, Point> initializingLink, Action<DialogueOptionVM> clearOptionLink, Action<DialogueOptionVM, Point> updatedLayoutPosition)
        {
            _updatedLayoutPosition = updatedLayoutPosition;
            _clearOptionLink = clearOptionLink;
            _removeSelf = removeSelf;
            _initializingLink = initializingLink;
        }


        private void InitializeLinkPosition(MouseButtonEventArgs e)
        {
            _initializingLink(this, ((FrameworkElement)e.OriginalSource).GetLocalCanvasPoint());
        }

        private void UpdatedLayoutPosition(FrameworkElement e)
        {
            var point = e.GetLocalCanvasPoint();
            _updatedLayoutPosition(this, point);
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
