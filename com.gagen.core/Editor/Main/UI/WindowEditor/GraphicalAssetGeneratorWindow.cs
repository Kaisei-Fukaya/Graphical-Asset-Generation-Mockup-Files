using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using UnityEditor.Experimental.GraphView;
using GAGen.Data;

namespace GAGen.Graph
{
    public class GraphicalAssetGeneratorWindow : EditorWindow
    {
        StyleSheet _variablesStyleSheet;
        StyleSheet _generateStyleVariables, _trainStyleVariables;
        StyleSheet _toolbarToggleStyles;
        GraphicalAssetGraphView _graphView;
        VisualElement _mainView;
        public bool inTrainingMode;

        GAGenData _saveData;

        ToolbarToggle _trainButton;
        ToolbarToggle _generateButton;

        public delegate void OnModeChangedEvent(bool val);
        public event OnModeChangedEvent onModeChange;

        [MenuItem("Window/Graphical Asset Generator")]
        public static void ShowWindow()
        {
            GetWindow<GraphicalAssetGeneratorWindow>();
        }

        private void CreateGUI()
        {
            this.titleContent = new GUIContent("Graphical Asset Generator");
            _generateStyleVariables = (StyleSheet)AssetDatabase.LoadAssetAtPath("Packages/com.gagen.core/Editor/Assets/UIStyles/GraphicalAssetGeneratorVariablesGenerate.uss", typeof(StyleSheet));
            _trainStyleVariables = (StyleSheet)AssetDatabase.LoadAssetAtPath("Packages/com.gagen.core/Editor/Assets/UIStyles/GraphicalAssetGeneratorVariablesTrain.uss", typeof(StyleSheet));
            _toolbarToggleStyles = (StyleSheet)AssetDatabase.LoadAssetAtPath("Packages/com.gagen.core/Editor/Assets/UIStyles/GraphicalAssetToolbarToggleStyle.uss", typeof(StyleSheet));
            AddToolbar();
            _mainView = new VisualElement()
            {
                name = "mainView"
            };
            _mainView.AddToClassList("root");
            //mainView.StretchToParentSize();
            rootVisualElement.Add(_mainView);
            AddGraphView();
            AddStyles();
            //AddPreviewWindow();
            SetupDragAndDrop();
        }

        private void SetupDragAndDrop()
        {
            Color origColour = _mainView.style.backgroundColor.value;
            //Drag enter
            _mainView.RegisterCallback<DragEnterEvent>(e =>
            {
                _mainView.style.backgroundColor = new StyleColor(new Color(0f, 0f, 100f, 0.3f));
            });
            //Drag leave
            _mainView.RegisterCallback<DragLeaveEvent>(e =>
            {
                _mainView.style.backgroundColor = new StyleColor(origColour);
            });
            //Drag updated
            _mainView.RegisterCallback<DragUpdatedEvent>(e =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            });
            //Drag perform
            _mainView.RegisterCallback<DragPerformEvent>(e =>
            {
                var draggedObjects = DragAndDrop.objectReferences;

                for (int i = 0; i < draggedObjects.Length; i++)
                {
                    if (draggedObjects[i] is GAGenData)
                    {
                        Load((GAGenData)draggedObjects[i]);
                        break;
                    }
                }

                _mainView.style.backgroundColor = new StyleColor(origColour);
            });
            //Drag exited
            _mainView.RegisterCallback<DragExitedEvent>(e =>
            {
                _mainView.style.backgroundColor = new StyleColor(origColour);
            });
        }

        void AddGraphView()
        {
            _graphView = new GraphicalAssetGraphView(this);
            _graphView.StretchToParentSize();
            _mainView.Add(_graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            ToolbarButton saveButton = new ToolbarButton()
            {
                text = "Save"
            };
            saveButton.clicked += Save;

            ToolbarButton saveAsButton = new ToolbarButton()
            {
                text = "Save As"
            };
            saveAsButton.clicked += SaveAs;

            ToolbarButton loadButton = new ToolbarButton()
            {
                text = "Load"
            };
            loadButton.clicked += Load;

            Label modeLabel = new Label()
            {
                text = "Mode:"
            };

            _trainButton = new ToolbarToggle()
            {
                text = "Train",
                name = "trainButton",
                tooltip = "When this mode is active, the graph will attempt to train any models that are connected to an input. Outputs will not work in this mode."
            };
            _trainButton.styleSheets.Add(_toolbarToggleStyles);

            _generateButton = new ToolbarToggle()
            {
                text = "Generate",
                value = true,
                name = "generateButton",
                tooltip = "When this mode is active, the graph will generate assets. These assets will be saved in the output paths you define."
            };
            _generateButton.styleSheets.Add(_toolbarToggleStyles);

            ToolbarButton runButton = new ToolbarButton()
            {
                text = "Run",
                name = "runButton",
                tooltip = "Click to run the current selected mode (NOT FUNCTIONAL)."
            };

            _trainButton.RegisterValueChangedCallback(x => TrainButtonClicked(_trainButton.value));
            _generateButton.RegisterValueChangedCallback(x => GenerateButtonClicked(_trainButton.value));

            ToolbarSpacer spacer1 = new ToolbarSpacer();
            ToolbarSpacer spacer2 = new ToolbarSpacer();
            ToolbarSpacer spacer3 = new ToolbarSpacer();
            toolbar.Add(saveButton);
            toolbar.Add(spacer1);
            toolbar.Add(saveAsButton);
            toolbar.Add(loadButton);
            toolbar.Add(spacer2);
            toolbar.Add(runButton);
            toolbar.Add(spacer3);
            toolbar.Add(modeLabel);
            toolbar.Add(_trainButton);
            toolbar.Add(_generateButton);

            rootVisualElement.Add(toolbar);
        }

        //private void AddPreviewWindow()
        //{
        //    GAPreview previewWindow = new GAPreview();
        //    previewWindow.Initialise();
        //    previewWindow.name = "previewWindow";
        //    _mainView.Add(previewWindow);
        //}

        void Save()
        {
            //If save data doesn't exist, call save as
            if(_saveData == null)
            {
                SaveAs();
            }

            //Otherwise overwrite the data
            _saveData.Save(_graphView);
            AssetDatabase.SaveAssetIfDirty(_saveData);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = _saveData;
        }

        void SaveAs()
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Save As", "New Graphical Asset Generator", "asset", "");
            _saveData = CreateInstance<GAGenData>();
            _saveData.Save(_graphView);
            AssetDatabase.CreateAsset(_saveData, savePath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = _saveData;
        }

        public void Load(GAGenData data)
        {
            if (data == null)
            {
                Debug.LogWarning("Selected file was not compatible");
                return;
            }

            _saveData = data;

            ClearGraph();

            Dictionary<string, NodeAndData> iDToNode = new Dictionary<string, NodeAndData>();
            List<string> allNodeIDs = new List<string>();
            foreach (GAGenNodeData nodeData in _saveData.Nodes)
            {
                GraphViewNode newNode = _graphView.CreateNode(nodeData.NodeType, nodeData.Position);
                newNode.ID = nodeData.ID;
                newNode.LoadSettings(nodeData.AdditionalSettings);
                iDToNode.Add(newNode.ID, new NodeAndData(newNode, nodeData));
                allNodeIDs.Add(newNode.ID);
                _graphView.AddElement(newNode);
            }



            //Make sure this is done last so that all ports are drawn
            foreach (string id in allNodeIDs)
            {
                NodeAndData nodeAndData = iDToNode[id];
                GraphViewNode node = nodeAndData.node;
                GAGenNodeData nodeData = nodeAndData.data;
                List<GraphicalAssetPort> ports = node.GetPorts(true);
                if (nodeData.GenConnections == null || nodeData.TrainConnections == null)
                    continue;
                //Gen connections
                if (ports.Count != nodeData.GenConnections.Count)
                    continue;
                for (int i = 0; i < nodeData.GenConnections.Count; i++)
                {
                    if (nodeData.GenConnections[i].iD == "EMPTY")
                        continue;

                    GraphViewNode otherNode = iDToNode[nodeData.GenConnections[i].iD].node;
                    List<GraphicalAssetPort> otherPorts = otherNode.GetPorts(false);
                    Edge edge = otherPorts[nodeData.GenConnections[i].index].ConnectTo(ports[i].GetPort(false), false);
                    if (edge == null)
                        continue;
                    _graphView.AddElement(edge);
                }
                //TrainConnections
                for (int i = 0; i < nodeData.TrainConnections.Count; i++)
                {
                    if (nodeData.TrainConnections[i].iD == "EMPTY")
                        continue;

                    GraphViewNode otherNode = iDToNode[nodeData.TrainConnections[i].iD].node;
                    List<GraphicalAssetPort> otherPorts = otherNode.GetPorts(false);
                    _graphView.AddElement(otherPorts[nodeData.TrainConnections[i].index].ConnectTo(ports[i].GetPort(true), true));
                }
            }

            EditorApplication.delayCall += _graphView.CentreGraphOnNodes;
            titleContent = new GUIContent($"{_saveData.name} (Graphical Asset Generator)");
        }

        void Load()
        {
            string path = EditorUtility.OpenFilePanel("Load", "Assets", "asset");
            path = path.Replace(Application.dataPath, "Assets");
            GAGenData data = AssetDatabase.LoadAssetAtPath<GAGenData>(path);
            Load(data);
        }

        struct NodeAndData
        {
            public GraphViewNode node;
            public GAGenNodeData data;
            public NodeAndData(GraphViewNode node, GAGenNodeData data)
            {
                this.node = node;
                this.data = data;
            }
        }

        void ClearGraph()
        {
            _graphView.ClearGraph();
        }

        void TrainButtonClicked(bool val)
        {
            inTrainingMode = val;
            if (_generateButton == null)
                return;
            _generateButton.SetValueWithoutNotify(!val);
            onModeChange?.Invoke(inTrainingMode);
            SetStyle();
        }

        void GenerateButtonClicked(bool val)
        {
            inTrainingMode = !val;
            if (_trainButton == null)
                return;
            _trainButton.SetValueWithoutNotify(!val);
            onModeChange?.Invoke(inTrainingMode);
            SetStyle();
        }

        void SetStyle()
        {
            rootVisualElement.styleSheets.Remove(_variablesStyleSheet);
            if (inTrainingMode)
                _variablesStyleSheet = _trainStyleVariables;
            else
                _variablesStyleSheet = _generateStyleVariables;
            rootVisualElement.styleSheets.Add(_variablesStyleSheet);
        }

        void AddStyles()
        {
            _variablesStyleSheet = _generateStyleVariables;
            StyleSheet windowStyleSheet = (StyleSheet)AssetDatabase.LoadAssetAtPath("Packages/com.gagen.core/Editor/Assets/UIStyles/GraphicalAssetGeneratorWindowStyle.uss", typeof(StyleSheet));
            rootVisualElement.styleSheets.Add(_variablesStyleSheet);
            rootVisualElement.styleSheets.Add(windowStyleSheet);
        }
    }
}