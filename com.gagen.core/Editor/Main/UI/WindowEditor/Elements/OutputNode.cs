using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GAGen.Data;
using System.Linq;

namespace GAGen.Graph.Elements
{
    public class OutputNode : GraphViewNode
    {
        List<Connector> _connectors;
        public override void Initialise(Vector2 position)
        {
            _connectors = new List<Connector>();
            base.Initialise(position);
            NodeName = "Output";
            NodeType = GANodeType.Output;
            capabilities &= ~Capabilities.Deletable;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            if (setting == null)
                return;
            for (int i = 0; i < setting.o_portTypes.Count; i++)
            {
                CreateConnector(setting.o_portTypes[i], setting.o_outputPaths[i]);
            }
            DrawConnectors();
        }

        public override NodeSetting GetSettings()
        {
            List<GAPortType> inPortTypes = new List<GAPortType>();

            foreach (GraphicalAssetPort port in _ingoingPorts)
            {
                inPortTypes.Add(port.PortType);
            }

            List<GAPortType> portTypes = new List<GAPortType>();
            List<string> outputPaths = new List<string>();

            foreach (Connector connector in _connectors)
            {
                portTypes.Add(connector.PortType);
                outputPaths.Add(connector.GetPathText());
            }
            OutputNodeSetting setting = new OutputNodeSetting(portTypes, outputPaths);
            setting.i_portTypes = inPortTypes;
            return setting;
        }

        public override void Draw()
        {
            base.Draw();

            DrawConnectors();

            Button addButton = new Button()
            {
                text = "Add New Output"
            };

            addButton.clicked += CreateConnector;

            extensionContainer.Add(addButton);

            RefreshExpandedState();
        }

        void CreateConnector()
        {
            _connectors.Add(new Connector(this, GraphView));
            DrawConnectors();
        }

        void CreateConnector(GAPortType portType, string path = "")
        {
            _connectors.Add(new Connector(this, GraphView, path) { PortType = portType });
        }

        void RemoveConnector(Connector c)
        {
            _connectors.Remove(c);
            GraphView.DeleteElements(c.port.Connections(true));
            GraphView.DeleteElements(c.port.Connections(false));
            c.port.DisconnectAll();
            DrawConnectors();
        }

        public void UpdateUI()
        {

        }

        void DrawConnectors()
        {
            //Store connections
            Dictionary<int, List<Port>> dictT = new Dictionary<int, List<Port>>();
            Dictionary<int, List<Port>> dictG = new Dictionary<int, List<Port>>();
            for (int i = 0; i < _connectors.Count; i++)
            {
                if (_connectors[i].port == null)
                    continue;
                List<Port> connectionsT = new List<Port>();
                List<Port> connectionsG = new List<Port>();
                foreach (Edge edge in _connectors[i].port.Connections(true))
                {
                    connectionsT.Add(edge.output);
                }
                foreach (Edge edge in _connectors[i].port.Connections(false))
                {
                    connectionsG.Add(edge.output);
                }
                dictT.Add(i, connectionsT);
                dictG.Add(i, connectionsG);
                GraphView.DeleteElements(_connectors[i].port.Connections(true));
                GraphView.DeleteElements(_connectors[i].port.Connections(false));
                _connectors[i].port.DisconnectAll();
            }
            inputContainer.Clear();

            //Redraw
            _ingoingPorts = new List<GraphicalAssetPort>();
            foreach (Connector c in _connectors)
            {
                c.Draw();
                inputContainer.Add(c);
                _ingoingPorts.Add(c.port);
            }

            //Reconnect
            for (int i = 0; i < _connectors.Count; i++)
            {
                if (dictT.ContainsKey(i))
                {
                    foreach (Port port in dictT[i])
                    {
                        GraphView.AddElement(_connectors[i].port.ConnectTo(port, true));
                    }
                }
                if (dictG.ContainsKey(i))
                {
                    foreach (Port port in dictG[i])
                    {
                        GraphView.AddElement(_connectors[i].port.ConnectTo(port, false));
                    }
                }
            }

            RefreshExpandedState();
        }

        public class Connector : VisualElement
        {
            public GraphicalAssetPort port;
            private GAPortType _portType;
            private OutputNode _parentNode;
            private GraphicalAssetGraphView _graphView;
            private TextField _textField;
            private string path;

            public GAPortType PortType
            {
                get
                {
                    return _portType;
                }
                set
                {
                    _portType = value;
                    Draw();
                }
            }
            public Connector(OutputNode outputNode, GraphicalAssetGraphView graphView, string outputPathText = "")
            {
                _parentNode = outputNode;
                _graphView = graphView;
                path = outputPathText;
                styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath("Packages/com.gagen.core/Editor/Assets/UIStyles/GraphicalAssetOutputNodeConnectorStyle.uss", typeof(StyleSheet)));
            }

            public void Draw()
            {
                if (_textField != null)
                    path = _textField.value;

                contentContainer.Clear();
                //port = _parentNode.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
                //port.portName = _portType.ToString();
                //port.portColor = Color.red;
                port = new GraphicalAssetPort(_parentNode, _portType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);

                List<GAPortType> popupOptions = new List<GAPortType>();
                var ptValues = Enum.GetValues(typeof(GAPortType));
                for (int i = 0; i < ptValues.Length; i++)
                {
                    GAPortType x = (GAPortType)ptValues.GetValue(i);
                    popupOptions.Add(x);
                }
                PopupField<GAPortType> popupField = new PopupField<GAPortType>(popupOptions, _portType);
                popupField.RegisterValueChangedCallback(x => UpdatePort(popupField.value));
                Button deleteButton = new Button()
                {
                    text = "Delete"
                };
                deleteButton.clicked += Delete;

                Label textFieldLabel = new Label()
                {
                    text = "Path:"
                };

                _textField = new TextField();
                _textField.SetEnabled(false);
                _textField.SetValueWithoutNotify(path);

                Button openPathPickerButton = new Button();
                openPathPickerButton.text = "...";
                openPathPickerButton.clicked += OpenPathPicker;

                VisualElement topSection = new VisualElement() { name = "top" };
                VisualElement bottomSection = new VisualElement() { name = "bottom" };


                topSection.Add(port);
                topSection.Add(popupField);
                topSection.Add(deleteButton);
                bottomSection.Add(textFieldLabel);
                bottomSection.Add(_textField);
                bottomSection.Add(openPathPickerButton);
                contentContainer.Add(topSection);
                contentContainer.Add(bottomSection);
            }

            public void Delete()
            {
                _parentNode.RemoveConnector(this);
            }

            void UpdatePort(GAPortType value)
            {
                _graphView.DeleteElements(port.Connections(true));
                _graphView.DeleteElements(port.Connections(false));
                port.DisconnectAll();
                PortType = value;
            }

            public void OpenPathPicker()
            {
                if (_textField == null)
                    return;

                _textField.value = EditorUtility.OpenFolderPanel("Output Folder", "Assets", "");
            }

            public string GetPathText()
            {
                return _textField.value;
            }
        }
    }
}