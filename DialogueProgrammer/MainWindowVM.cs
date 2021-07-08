using DialogueProgrammer.Common;
using DialogueProgrammer.Common.Extensions;
using DialogueProgrammer.Models;
using DialogueProgrammer.Serialization;
using DialogueProgrammer.Views;
using DialogueProgrammer.Views.SubViews;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace DialogueProgrammer
{
    public class MainWindowVM : INotifyPropertyChanged
    {
        private const double _curveLinkeIntensity = 200;
        private const double _curveArrowLength = 20;
        private const double _curveArrowTheta = 90;

        private Dictionary<DialogueNodeVM, Dictionary<DialogueOptionVM, CubicBezierArrowVM>> _terminalToOptionsMap = new Dictionary<DialogueNodeVM, Dictionary<DialogueOptionVM, CubicBezierArrowVM>>();
        private Dictionary<DialogueOptionVM, Tuple<CubicBezierArrowVM, DialogueNodeVM>> _optionsToTerminalMap = new Dictionary<DialogueOptionVM, Tuple<CubicBezierArrowVM, DialogueNodeVM>>();

        private DimensionScalerVM _canvasSize = new DimensionScalerVM() { BaseHeight = 2000, BaseWidth = 2000, ScalerHeight = 1.0, ScalerWidth = 1.0 };

        public DelegateCommand WindowMouseMove => new DelegateCommand((obj) => UpdateFromMouseMove((MouseEventArgs)obj));
        public DelegateCommand WindowMouseUp => new DelegateCommand((obj) => UpdateFromMouseUp((MouseButtonEventArgs)obj));
        public DelegateCommand AddNodeCommand => new DelegateCommand((obj) => AddWindow());
        public DelegateCommand CanvasSizeIncWidthCommand => new DelegateCommand((obj) => CanvasSize.BaseWidth *= 1.1);
        public DelegateCommand CanvasSizeDecWidthCommand => new DelegateCommand((obj) => CanvasSize.BaseWidth *= 0.9);
        public DelegateCommand CanvasSizeIncHeightCommand => new DelegateCommand((obj) => CanvasSize.BaseHeight *= 1.1);
        public DelegateCommand CanvasSizeDecHeightCommand => new DelegateCommand((obj) => CanvasSize.BaseHeight *= 0.9);
        public DelegateCommand ZoomIncCommand => new DelegateCommand((obj) => { CanvasSize.ScalerHeight *= 1.1; CanvasSize.ScalerWidth *= 1.1; });
        public DelegateCommand ZoomDecCommand => new DelegateCommand((obj) => { CanvasSize.ScalerHeight *= 0.9; CanvasSize.ScalerWidth *= 0.9; });
        public DelegateCommand ExportFileCommand => new DelegateCommand((obj) => ExportFile());

        private LinkingProgressVM _linkingProgress = new LinkingProgressVM();
        private CubicBezierArrowVM _previewLine = new CubicBezierArrowVM();

        public DimensionScalerVM CanvasSize
        {
            get { return _canvasSize; }
            set
            {
                if (value != _canvasSize)
                {
                    _canvasSize = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public CubicBezierArrowVM PreviewLine
        {
            get { return _previewLine; }
            set
            {
                if (_previewLine != value)
                {
                    _previewLine = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public LinkingProgressVM LinkingProgress
        {
            get { return _linkingProgress; }
            set
            {
                if (value != _linkingProgress)
                {
                    _linkingProgress = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<HandleWindowVM> NodeWindows { get; set; } = new ObservableCollection<HandleWindowVM>();
        public ObservableCollection<CubicBezierArrowVM> ConnectionLines { get; set; } = new ObservableCollection<CubicBezierArrowVM>();

        public MainWindowVM()
        {
            NodeWindows.Add(new HandleWindowVM(RemoveWindow, new DialogueNodeSeedVM(OptionCollectionChanged, AttemptToLinkToNode, InitOptionLink, ClearTerminalLinks, ClearOptionLink, LayoutUpdateForOption, LayoutUpdateForTerminal)));
            LabelResponseIds();
        }

        private void AddWindow()
        {
            NodeWindows.Add(new HandleWindowVM(RemoveWindow, new DialogueNodeVM(OptionCollectionChanged, AttemptToLinkToNode, InitOptionLink, ClearTerminalLinks, ClearOptionLink, LayoutUpdateForOption, LayoutUpdateForTerminal)));
        }

        private void RemoveWindow(HandleWindowVM handleWin)
        {
            foreach (var option in handleWin.Node.OptionDialogue)
            {
                ClearOptionLink(handleWin.Node, option);
            }

            ClearTerminalLinks(handleWin.Node);

            NodeWindows.Remove(handleWin);

            LabelResponseIds();
        }

        private void LabelResponseIds()
        {
            foreach (var nodeWindow in NodeWindows)
            {
                foreach (var option in nodeWindow.Node.OptionDialogue)
                {
                    option.OptionId = null;
                }
            }

            var nodes = new Queue<DialogueNodeVM>();
            var redundant = new HashSet<DialogueNodeVM>();
            var count = 0;

            var seed = NodeWindows.FirstOrDefault()?.Node;

            if (seed == null)
                return;

            nodes.Enqueue(seed);
            redundant.Add(seed);

            while (nodes.Count != 0)
            {
                var focus = nodes.Dequeue();

                foreach (var option in focus.OptionDialogue)
                {
                    if (option.LinkedNode != null && !redundant.Contains(option.LinkedNode))
                    {
                        nodes.Enqueue(option.LinkedNode);
                        redundant.Add(option.LinkedNode);
                    }

                    option.OptionId = count;
                    count++;
                }
            }
        }

        #region TerminalMethods

        private void AttemptToLinkToNode(DialogueNodeVM node, Point point)
        {
            if (LinkingProgress.LinkStartType == LinkingType.Option)
            {
                if (!_optionsToTerminalMap.ContainsKey(LinkingProgress.LinkStartOption))
                {
                    var line = new CubicBezierArrowVM();
                    line.UpdateCurve(LinkingProgress.LinkStartPoint, 0, point, 180, _curveLinkeIntensity, _curveArrowLength, _curveArrowTheta);

                    ConnectionLines.Add(line);
                    _optionsToTerminalMap.Add(LinkingProgress.LinkStartOption, new Tuple<CubicBezierArrowVM, DialogueNodeVM>(line, node));
                    if (_terminalToOptionsMap.ContainsKey(node))
                    {
                        _terminalToOptionsMap[node].Add(LinkingProgress.LinkStartOption, line);
                    }
                    else
                    {
                        _terminalToOptionsMap.Add(node, new Dictionary<DialogueOptionVM, CubicBezierArrowVM>() { { LinkingProgress.LinkStartOption, line } });
                    }
                    LinkingProgress.LinkStartOption.LinkedNode = node;

                    LabelResponseIds();
                }

                LinkingProgress.Clear();
            }
            else
            {
                _previewLine.EndPoint = point;
                LinkingProgress.StartLinkingProcess(point, node);
            }
        }

        private void ClearTerminalLinks(DialogueNodeVM node)
        {
            if (_terminalToOptionsMap.ContainsKey(node))
            {
                var lineAndOptionDict = _terminalToOptionsMap[node];
                _terminalToOptionsMap.Remove(node);
                foreach (var optionAndLine in lineAndOptionDict)
                {
                    if (_optionsToTerminalMap.ContainsKey(optionAndLine.Key))
                    {
                        _optionsToTerminalMap.Remove(optionAndLine.Key);
                    }
                    ConnectionLines.Remove(optionAndLine.Value);
                    optionAndLine.Key.LinkedNode = null;
                }

                LabelResponseIds();
            }
        }

        private void LayoutUpdateForTerminal(DialogueNodeVM node, Point point)
        {
            if (_terminalToOptionsMap.ContainsKey(node))
            {
                foreach (var optionAndLine in _terminalToOptionsMap[node])
                {
                    optionAndLine.Value.UpdateCurve(optionAndLine.Value.StartPoint, 0, point, 180, _curveLinkeIntensity, _curveArrowLength, _curveArrowTheta);
                }
            }
        }

        #endregion

        #region OptionMethods

        private void OptionCollectionChanged()
        {
            LabelResponseIds();
        }

        private void InitOptionLink(DialogueNodeVM node, DialogueOptionVM option, Point point)
        {
            if (LinkingProgress.LinkStartType == LinkingType.Terminal)
            {

                if (!_terminalToOptionsMap.ContainsKey(LinkingProgress.LinkStartNode) || (_terminalToOptionsMap.ContainsKey(LinkingProgress.LinkStartNode) && !_terminalToOptionsMap[LinkingProgress.LinkStartNode].ContainsKey(option)))
                {
                    var line = new CubicBezierArrowVM();
                    line.UpdateCurve(point, 0, LinkingProgress.LinkStartPoint, 180, _curveLinkeIntensity, _curveArrowLength, _curveArrowTheta);

                    ConnectionLines.Add(line);
                    _optionsToTerminalMap.Add(option, new Tuple<CubicBezierArrowVM, DialogueNodeVM>(line, LinkingProgress.LinkStartNode));
                    if (_terminalToOptionsMap.ContainsKey(LinkingProgress.LinkStartNode))
                    {
                        _terminalToOptionsMap[LinkingProgress.LinkStartNode].Add(option, line);
                    }
                    else
                    {
                        _terminalToOptionsMap.Add(LinkingProgress.LinkStartNode, new Dictionary<DialogueOptionVM, CubicBezierArrowVM>() { { option, line } });
                    }
                    option.LinkedNode = LinkingProgress.LinkStartNode;

                    LabelResponseIds();
                }

                LinkingProgress.Clear();
            }
            else if (option.LinkedNode == null)
            {
                _previewLine.StartPoint = point;
                LinkingProgress.StartLinkingProcess(point, option);
            }
        }
        private void ClearOptionLink(DialogueNodeVM node, DialogueOptionVM option)
        {
            if (_optionsToTerminalMap.ContainsKey(option))
            {
                var lineAndTerminal = _optionsToTerminalMap[option];
                _optionsToTerminalMap.Remove(option);
                if (_terminalToOptionsMap.ContainsKey(lineAndTerminal.Item2))
                {
                    _terminalToOptionsMap[lineAndTerminal.Item2].Remove(option);
                }
                ConnectionLines.Remove(lineAndTerminal.Item1);
                option.LinkedNode = null;

                LabelResponseIds();
            }
        }

        private void LayoutUpdateForOption(DialogueNodeVM arg1, DialogueOptionVM option, Point point)
        {
            if (_optionsToTerminalMap.ContainsKey(option))
            {
                _optionsToTerminalMap[option].Item1.UpdateCurve(point, 0, _optionsToTerminalMap[option].Item1.EndPoint, 180, _curveLinkeIntensity, _curveArrowLength, _curveArrowTheta);
            }
        }

        #endregion

        #region PreviewLineUpdate

        private void UpdateFromMouseMove(MouseEventArgs e)
        {
            if (LinkingProgress.LinkingInProgress)
            {
                var mystery = ((FrameworkElement)e.Source);
                var window = mystery.GetParentOfType<Window>();
                var canvas = window.BredthGetFirstChildOfType<Canvas>();

                var canvasPoint = e.GetPosition((FrameworkElement)canvas);

                if (LinkingProgress.LinkStartType == LinkingType.Option)
                {
                    _previewLine.UpdateCurve(_previewLine.StartPoint, 0, canvasPoint, 180, _curveLinkeIntensity, _curveArrowLength, _curveArrowTheta);
                }
                else
                {
                    _previewLine.UpdateCurve(canvasPoint, 0, _previewLine.EndPoint, 180, _curveLinkeIntensity, _curveArrowLength, _curveArrowTheta);
                }
            }
        }

        private void UpdateFromMouseUp(MouseButtonEventArgs e)
        {
            var mystery = e.OriginalSource;
            var terminal = ((FrameworkElement)mystery).GetParentOfType<DialogueTerminal>();
            var option = ((FrameworkElement)mystery).GetParentOfType<DialogueOption>();

            if (!((mystery is Rectangle) && (terminal != null)) && !((mystery is Rectangle) && (option != null)))
            {
                LinkingProgress.Clear();
            }
        }

        #endregion

        #region SaveFile

        private SerializedNode[] ConvertToSerialized()
        {
            try
            {
                var outputCollection = new LinkedList<SerializedNode>();
                var responseVMs = new Dictionary<int, DialogueOptionVM>();
                var nodeVMs = new Dictionary<DialogueNodeVM, int>();

                var redundantNodes = new HashSet<DialogueNodeVM>();

                var seed = NodeWindows.FirstOrDefault();

                if (seed == null)
                    return null;

                var nodes = new Queue<DialogueNodeVM>();
                var nodeIdCounter = 0;

                nodes.Enqueue(seed.Node);
                redundantNodes.Add(seed.Node);

                while (nodes.Count != 0)
                {
                    var focus = nodes.Dequeue();

                    var newSerialized = new SerializedNode() { NodeId = nodeIdCounter, DialogueText = focus.TerminalVM.TerminalText };
                    newSerialized.Options = focus.OptionDialogue.Select(e => new SerializedOption() { ResponseId = (int)e.OptionId, OptionText = e.OptionText }).ToArray();

                    outputCollection.AddLast(newSerialized);
                    foreach (var option in focus.OptionDialogue)
                    {
                        responseVMs.Add((int)option.OptionId, option);
                        if (option.LinkedNode != null && !redundantNodes.Contains(option.LinkedNode))
                        {
                            nodes.Enqueue(option.LinkedNode);
                            redundantNodes.Add(option.LinkedNode);
                        }
                    }
                    nodeVMs.Add(focus, nodeIdCounter);

                    nodeIdCounter++;
                }

                foreach (var serialized in outputCollection)
                {
                    foreach (var option in serialized.Options)
                    {
                        var responseVM = responseVMs[option.ResponseId];

                        if (responseVM.LinkedNode != null)
                        {
                            var nodeId = nodeVMs[responseVM.LinkedNode];

                            option.PointToNode = nodeId;
                        }
                        else
                        {
                            option.PointToNode = -1;
                        }
                    }
                }

                return outputCollection.ToArray();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }

        private void WriteFile(string fileName, SerializedNode[] output, bool json = false)
        {

            if (json)
            {
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fileStream))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    JsonSerializer ser = new JsonSerializer();
                    ser.Serialize(writer, output);
                }
            }
            else
            {
                using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(SerializedNode[]));
                    ser.Serialize(fileStream, output);
                }
            }
        }

        private void ExportFile()
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "xml file (*.xml)|*.xml|JSON file (*.json)|*.json";
            saveFile.RestoreDirectory = true;

            if (saveFile.ShowDialog() == true)
            {
                var output = ConvertToSerialized();

                switch (saveFile.FilterIndex)
                {
                    case 1:
                        WriteFile(saveFile.FileName, output, false);
                        break;
                    case 2:
                        WriteFile(saveFile.FileName, output, true);
                        break;
                    default:
                        throw new NotImplementedException("untracked filter type");
                }
            }
        }

        #endregion

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }


}
