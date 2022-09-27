using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using GAGen.Data;

namespace GAGen.Graph.Elements
{
    public class MeshFromPhotoNode : GraphViewNode
    {
        protected GraphicalAssetPort _outputPort, _inputNumPort;
        protected GAPortType _outputPortType;
        protected List<string> _inputTypes = new List<string>(){
        "Single-view",
        "Multi-view"
        };
        protected List<string> _models = new List<string>(){
        "Human Character",
        "Human Faces",
        "Caricature Human Faces",
        "Human Hair",
        "Buildings",
        "Furniture",
        "Vehicles",
        "Other object (Requires training data to fine-tune)"
        };

        protected int _chosenInputTypeIndex = 0;
        protected int _chosenModelIndex = 0;

        protected string _inputTypeName = "Photograph";
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.MeshFromPhoto;
            base.Initialise(position);
            NodeName = "Mesh from Photo";
            _inputPortType = GAPortType.Bitmap;
            _outputPortType = GAPortType.TexturedMesh;
        }

        public override void Draw()
        {
            base.Draw();

            _outgoingPorts = new List<GraphicalAssetPort>();

            VisualElement modelSelector = new VisualElement();
            Label modelSelectorLabel = new Label("Model");
            PopupField<string> modelSelectorPopup = new PopupField<string>(_models, _chosenModelIndex);
            modelSelectorPopup.RegisterValueChangedCallback(x => OnModelSelected(modelSelectorPopup.value));
            modelSelector.Add(modelSelectorLabel);
            modelSelector.Add(modelSelectorPopup);
            modelSelector.tooltip = "Select a model to use in generate mode, or fine-tune in train mode";
            extensionContainer.Insert(0, modelSelector);

            VisualElement inputTypeSelector = new VisualElement();
            Label inputTypeSelectorLabel = new Label("Input Type");

            PopupField<string> inputTypeSelectorPopup = new PopupField<string>(_inputTypes, _chosenInputTypeIndex);
            inputTypeSelectorPopup.RegisterValueChangedCallback(x => OnModeSelected(inputTypeSelectorPopup.value));
            inputTypeSelector.Add(inputTypeSelectorLabel);
            inputTypeSelector.Add(inputTypeSelectorPopup);
            inputTypeSelector.tooltip = "Single-view mode generates one asset per input, Multi-view requires four images at different angles of a subject";
            extensionContainer.Insert(0, inputTypeSelector);


            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, "Generated");
            //_outputPort.portName = _outputPortType.ToString();
            outputContainer.Add(_outputPort);
            _outgoingPorts.Add(_outputPort);

            OnModeSelected(_inputTypes[_chosenInputTypeIndex]);

            RefreshExpandedState();
        }

        public void OnModeSelected(string mode)
        {
            _ingoingPorts = new List<GraphicalAssetPort>();
            _chosenInputTypeIndex = _inputTypes.IndexOf(mode);

            foreach (GraphicalAssetPort port in inputContainer.Children().OfType<GraphicalAssetPort>())
            {
                IEnumerable<Edge> trainingEdges = port.Connections(true);
                IEnumerable<Edge> genEdges = port.Connections(false);
                if(trainingEdges != null)
                    GraphView.DeleteElements(trainingEdges);
                if(genEdges != null)
                    GraphView.DeleteElements(genEdges);
                port.DisconnectAll();
            }

            inputContainer.Clear();

            if (mode == _inputTypes[0])
            {
                _inputPort = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, _inputTypeName);
                _ingoingPorts.Add(_inputPort);
                inputContainer.Add(_inputPort);
                _inputNumPort = new GraphicalAssetPort(this, GAPortType.Integer, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, "Amount to generate");
                _ingoingPorts.Add(_inputNumPort);
                inputContainer.Add(_inputNumPort);
                CallSettingsEditEvent();
                return;
            }

            GraphicalAssetPort port1, port2, port3, port4;
            port1 = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, $"Front-view {_inputTypeName}");
            port2 = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, $"Left-side-view {_inputTypeName}");
            port3 = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, $"Right-side-view {_inputTypeName}");
            port4 = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, $"Back-view {_inputTypeName}");
            //port1.portName = _inputPortType.ToString();
            //port2.portName = _inputPortType.ToString();
            //port3.portName = _inputPortType.ToString();
            //port4.portName = _inputPortType.ToString();
            inputContainer.Add(port1);
            inputContainer.Add(port2);
            inputContainer.Add(port3);
            inputContainer.Add(port4);

            _ingoingPorts.Add(port1);
            _ingoingPorts.Add(port2);
            _ingoingPorts.Add(port3);
            _ingoingPorts.Add(port4);

            _inputNumPort = new GraphicalAssetPort(this, GAPortType.Integer, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, "Amount to generate");
            _ingoingPorts.Add(_inputNumPort);
            inputContainer.Add(_inputNumPort);

            CallSettingsEditEvent();
        }

        public virtual void OnModelSelected(string model)
        {
            for (int i = 0; i < _models.Count; i++)
            {
                if (_models[i] == model)
                {
                    _chosenModelIndex = i;
                    CallSettingsEditEvent();
                    return;
                }
            }
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting settings = base.GetSettings();
            settings.i2m_dropdownOpt1 = _chosenInputTypeIndex;
            settings.i2m_dropdownOpt2 = _chosenModelIndex;
            return settings;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            _chosenInputTypeIndex = setting.i2m_dropdownOpt1;
            _chosenModelIndex = setting.i2m_dropdownOpt2;
        }

        public void UpdateUI()
        {

        }
    }
}