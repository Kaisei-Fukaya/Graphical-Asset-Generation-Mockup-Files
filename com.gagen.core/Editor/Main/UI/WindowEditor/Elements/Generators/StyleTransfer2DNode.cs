using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GAGen.Data;

namespace GAGen.Graph.Elements
{
    public class StyleTransfer2DNode : GraphViewNode
    {
        protected GraphicalAssetPort _inputPortA, _outputPort;
        protected GAPortType _inputPortAType, _outputPortType;
        protected List<string> _models = new List<string>(){
        "Model A",
        "Model B",
        "Model C"
        };
        protected int _chosenModelIndex;
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.StyleTransfer2D;
            base.Initialise(position);
            NodeName = "Style Transfer 2D";
            _inputPortType = GAPortType.Bitmap;
            _inputPortAType = GAPortType.Bitmap;
            _outputPortType = GAPortType.Bitmap;
        }

        public override void Draw()
        {
            base.Draw();

            _ingoingPorts = new List<GraphicalAssetPort>();
            _outgoingPorts = new List<GraphicalAssetPort>();

            VisualElement modelSelector = new VisualElement();
            Label modelSelectorLabel = new Label("Model");
            PopupField<string> modelSelectorPopup = new PopupField<string>(_models, _chosenModelIndex);
            modelSelectorPopup.RegisterValueChangedCallback(x => { _chosenModelIndex = _models.IndexOf(x.newValue); CallSettingsEditEvent(); });
            modelSelector.Add(modelSelectorLabel);
            modelSelector.Add(modelSelectorPopup);
            extensionContainer.Insert(0, modelSelector);

            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            _outgoingPorts.Add(_outputPort);
            //_outputPort.portName = _outputPortType.ToString();
            outputContainer.Add(_outputPort);

            _inputPort = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, "Style");
            _ingoingPorts.Add(_inputPort);
            //_inputPort.portName = _inputPortType.ToString();
            inputContainer.Add(_inputPort);

            _inputPortA = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, "Content");
            _ingoingPorts.Add(_inputPortA);
            //_inputPortA.portName = _inputPortAType.ToString();
            inputContainer.Add(_inputPortA);

            RefreshExpandedState();
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting setting = base.GetSettings();
            setting.i2m_dropdownOpt2 = _chosenModelIndex;
            return setting;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            base.LoadSettings(setting);
            _chosenModelIndex = setting.i2m_dropdownOpt2;
        }
    }
}