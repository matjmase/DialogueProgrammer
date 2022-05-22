using DialogueProgrammer.Common;
using DialogueProgrammer.Serialization.Export;
using DialogueProgrammer.Views.SubViews;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DialogueProgrammer.Views
{
    public class DialogueNodeVM : INotifyPropertyChanged
    {
        private DialogueTerminalVM _terminalVM;
        private Action _optionCollectionChanged;
        private Action<DialogueNodeVM, Point> _attemptToLinkToNode;
        private Action<DialogueNodeVM, DialogueOptionVM, Point> _initializeOptionLink;
        private Action<DialogueNodeVM, DialogueOptionVM, Point> _layoutUpdatedOption;
        private Action<DialogueNodeVM> _clearTerminalLinks;
        private Action<DialogueNodeVM, DialogueOptionVM> _clearOptionLink;
        private Action<DialogueNodeVM, Point> _layoutUpdatedTerminal;

        public DialogueTerminalVM TerminalVM
        {
            get { return _terminalVM; }
            set
            {
                if (value != _terminalVM)
                {
                    _terminalVM = value;
                    NotifyPropertyChanged();
                }
            }
        }


        public ObservableCollection<DialogueOptionVM> OptionDialogue { get; set; } = new ObservableCollection<DialogueOptionVM>();

        public DelegateCommand AddNewOptionCommand => new DelegateCommand((obj) => AddNewOption());

        public DialogueNodeVM(Action optionCollectionChanged, Action<DialogueNodeVM, Point> attemptToLinkToNode, Action<DialogueNodeVM, DialogueOptionVM, Point> initializeOptionLink, Action<DialogueNodeVM> clearTerminalLinks, Action<DialogueNodeVM, DialogueOptionVM> clearOptionLink, Action<DialogueNodeVM, DialogueOptionVM, Point> layoutUpdatedOption, Action<DialogueNodeVM, Point> layoutUpdatedTerminal)
        {
            TerminalVM = new DialogueTerminalVM(AttemptToLinkToTerminal, ClearTerminalLinks, LayoutUpdatedTerminal);
            OptionDialogue = new ObservableCollection<DialogueOptionVM>();
            _optionCollectionChanged = optionCollectionChanged;
            _attemptToLinkToNode = attemptToLinkToNode;
            _initializeOptionLink = initializeOptionLink;
            _clearTerminalLinks = clearTerminalLinks;
            _clearOptionLink = clearOptionLink;
            _layoutUpdatedOption = layoutUpdatedOption;
            _layoutUpdatedTerminal = layoutUpdatedTerminal;
            AddNewOption();

        }

        public void AttemptToLinkToTerminal(Point linkPoint)
        {
            _attemptToLinkToNode(this, linkPoint);
        }
        public void ClearTerminalLinks()
        {
            _clearTerminalLinks(this);
        }

        private void LayoutUpdatedTerminal(Point point)
        {
            _layoutUpdatedTerminal(this, point);
        }

        public void AddNewOption()
        {
            OptionDialogue.Add(new DialogueOptionVM(RemoveOption, InitializeOptionLink, ClearOptionLink, OptionLayoutUpdated) { OptionText = "Option Text" });
            _optionCollectionChanged();
        }
        public void RemoveOption(DialogueOptionVM vm)
        {
            _clearOptionLink(this, vm);
            OptionDialogue.Remove(vm);
            _optionCollectionChanged();
        }
        public void InitializeOptionLink(DialogueOptionVM vm, Point initPoint)
        {
            _initializeOptionLink(this, vm, initPoint);
        }
        public void ClearOptionLink(DialogueOptionVM option)
        {
            _clearOptionLink(this, option);
        }
        private void OptionLayoutUpdated(DialogueOptionVM optionVM, Point loc)
        {
            _layoutUpdatedOption(this, optionVM, loc);
        }

        public ExportSerializedNode ToExportSerialized(int Id)
        {
            var output = new ExportSerializedNode() { NodeId = Id, DialogueText = TerminalVM.TerminalText };
            output.Options = OptionDialogue.Select(e => new ExportSerializedOption() { ResponseId = (int)e.OptionId, OptionText = e.OptionText }).ToArray();
            return output;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
