using DialogueProgrammer.Views.SubViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DialogueProgrammer.Views
{
    public class DialogueNodeSeedVM : DialogueNodeVM
    {
        public DialogueNodeSeedVM(Action optionCollectionChanged, Action<DialogueNodeVM, Point> attemptToLinkToNode, Action<DialogueNodeVM, DialogueOptionVM, Point> initializeOptionLink, Action<DialogueNodeVM> clearTerminalLinks, Action<DialogueNodeVM, DialogueOptionVM> clearOptionLink, Action<DialogueNodeVM, DialogueOptionVM, Point> layoutUpdatedOption, Action<DialogueNodeVM, Point> layoutUpdatedTerminal) : base(optionCollectionChanged, attemptToLinkToNode, initializeOptionLink, clearTerminalLinks, clearOptionLink, layoutUpdatedOption, layoutUpdatedTerminal)
        {
        }
    }
}
