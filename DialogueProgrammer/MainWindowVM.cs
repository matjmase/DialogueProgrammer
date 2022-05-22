using DialogueProgrammer.Common;
using DialogueProgrammer.Common.Extensions;
using DialogueProgrammer.Models;
using DialogueProgrammer.Serialization.Export;
using DialogueProgrammer.Serialization.Project;
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
        public DelegateCommand SaveProjectFileCommand => new DelegateCommand((obj) => SaveProjectFile());
        public DelegateCommand LoadProjectFileCommand => new DelegateCommand((obj) => LoadProjectFile());

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
            AddSeedWindow();
            LabelResponseIds();
        }

        private void AddSeedWindow()
        {
            NodeWindows.Add(new HandleWindowVM(RemoveWindow, new DialogueNodeSeedVM(OptionCollectionChanged, AttemptToLinkToNode, InitOptionLink, ClearTerminalLinks, ClearOptionLink, LayoutUpdateForOption, LayoutUpdateForTerminal)));
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

        private ExportSerializedNode[] ConvertToSerializedExport()
        {
            try
            {
                var outputCollection = new LinkedList<ExportSerializedNode>();
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

                    var newSerialized = focus.ToExportSerialized(nodeIdCounter);

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


        private ProjectSerializedNode[] ConvertToSerializedProject()
        {
            var responseVMs = new Dictionary<DialogueOptionVM, ExportSerializedOption>();
            var nodeVMs = new Dictionary<DialogueNodeVM, int>();

            var output = new ProjectSerializedNode[NodeWindows.Count];

            for (var i = 0; i < NodeWindows.Count; i++)
            {
                output[i] = NodeWindows[i].ToProjectSerialized(i);

                nodeVMs.Add(NodeWindows[i].Node, i);
                for (var j = 0; j < output[i].Options.Length; j++)
                {
                    responseVMs.Add(NodeWindows[i].Node.OptionDialogue[j], output[i].Options[j]);
                }
            }

            foreach (var window in NodeWindows)
            {
                var serialized = output[nodeVMs[window.Node]];
                var options = window.Node.OptionDialogue;

                foreach (var option in options)
                {
                    if (option.LinkedNode != null)
                    {
                        responseVMs[option].PointToNode = nodeVMs[option.LinkedNode];
                    }
                    else
                    {
                        responseVMs[option].PointToNode = -1;
                    }
                }
            }

            return output;
        }

        private void WriteFile(string fileName, object output, bool json = false)
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
                    XmlSerializer ser = new XmlSerializer(typeof(ExportSerializedNode[]));
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
                var output = ConvertToSerializedExport();

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

        private void SaveProjectFile()
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "dialogue file (*.diag)|*.diag";
            saveFile.RestoreDirectory = true;

            if (saveFile.ShowDialog() == true)
            {
                var output = ToSerializedMainWindow();

                WriteFile(saveFile.FileName, output, true);
            }
        }

        #endregion

        #region LoadFile

        private void LoadProjectFile()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "dialogue file (*.diag)|*.diag";
            openFile.RestoreDirectory = true;

            if (openFile.ShowDialog() == true)
            {
                var file = openFile.FileName;
                var mainWindowModel = DeserializeFile(file);

                if (mainWindowModel != null)
                {
                    InitializeLoadedModel(mainWindowModel);
                }
            }
        }

        private ProjectSerializedMainWindow DeserializeFile(string file)
        {
            ProjectSerializedMainWindow output = null;

            try
            {
                using (FileStream fileStream = new FileStream(file, FileMode.Open))
                using (StreamReader sr = new StreamReader(fileStream))
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    JsonSerializer ser = new JsonSerializer();
                    output = ser.Deserialize<ProjectSerializedMainWindow>(reader);
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

            return output;
        }

        private void InitializeLoadedModel(ProjectSerializedMainWindow mainWindowModel)
        {
            CanvasSize = mainWindowModel.Canvas.ToDimensionScaler();

            NodeWindows.Clear();
            ConnectionLines.Clear();
            _optionsToTerminalMap.Clear();
            _terminalToOptionsMap.Clear();

            var serializedToDialogueModel = new Dictionary<ProjectSerializedNode, DialogueNodeVM>();
            var indexToSerialized = new Dictionary<int, ProjectSerializedNode>();
            var serializedToOptionModel = new Dictionary<ExportSerializedOption, DialogueOptionVM>();

            var first = true;
            foreach (var node in mainWindowModel.Nodes)
            {
                if (first)
                {
                    AddSeedWindow();
                }
                else
                {
                    AddWindow();
                }

                var currentWindow = NodeWindows.Last();

                serializedToDialogueModel.Add(node, currentWindow.Node);
                indexToSerialized.Add(node.NodeId, node);

                currentWindow.CanvasLeft = node.CanvasLeft;
                currentWindow.CanvasTop = node.CanvasTop;

                currentWindow.Node.TerminalVM.TerminalText = node.DialogueText;
                currentWindow.Node.OptionDialogue.Clear();

                foreach (var option in node.Options)
                {
                    currentWindow.Node.AddNewOption();

                    var currentOption = currentWindow.Node.OptionDialogue.Last();
                    currentOption.OptionText = option.OptionText;

                    serializedToOptionModel.Add(option, currentOption);
                }

                first = false;
            }

            try
            {
                foreach (var node in mainWindowModel.Nodes)
                { 
                    foreach(var option in node.Options)
                    {
                        if (option.PointToNode == -1)
                            continue;

                        InitOptionLink(serializedToDialogueModel[node], serializedToOptionModel[option], new Point());
                        AttemptToLinkToNode(serializedToDialogueModel[indexToSerialized[option.PointToNode]], new Point());
                    }
                }
            }
            catch (Exception ex)
            {
                NodeWindows.Clear();
                AddSeedWindow();
            }
        }

        #endregion

        #region Conversions

        private ProjectSerializedMainWindow ToSerializedMainWindow()
        {
            return new ProjectSerializedMainWindow()
            {
                Canvas = _canvasSize.ToProjectSerialized(),
                Nodes = ConvertToSerializedProject()
            };
        }

        #endregion

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }


}
