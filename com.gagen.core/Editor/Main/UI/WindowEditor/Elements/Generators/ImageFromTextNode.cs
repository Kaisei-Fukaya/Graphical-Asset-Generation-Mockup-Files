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
    public class ImageFromTextNode : GraphViewNode
    {
        protected GraphicalAssetPort _outputPort, _sizeInputPortX, _sizeInputPortY;
        protected GAPortType _outputPortType;
        protected List<string> _models = new List<string>(){
        "Icon",
        "Character",
        "General Image"
        };
        protected int _chosenModelIndex;
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.ImageFromText;
            base.Initialise(position);
            NodeName = "Image from Text";
            _inputPortType = GAPortType.Text;
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
            modelSelectorPopup.RegisterValueChangedCallback(x => OnModelSelected(x.newValue));
            modelSelector.Add(modelSelectorLabel);
            modelSelector.Add(modelSelectorPopup);
            extensionContainer.Insert(0, modelSelector);


            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            _outgoingPorts.Add(_outputPort);
            //_outputPort.portName = _outputPortType.ToString();
            outputContainer.Add(_outputPort);

            _inputPort = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            _ingoingPorts.Add(_inputPort);
            inputContainer.Add(_inputPort);

            _sizeInputPortX = new GraphicalAssetPort(this, GAPortType.Integer, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, "Resolution Width");
            _ingoingPorts.Add(_sizeInputPortX);
            inputContainer.Add(_sizeInputPortX);

            _sizeInputPortY = new GraphicalAssetPort(this, GAPortType.Integer, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, "Resolution Height");
            _ingoingPorts.Add(_sizeInputPortY);
            inputContainer.Add(_sizeInputPortY);

            RefreshExpandedState();
        }

        public void OnModelSelected(string model)
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
            NodeSetting setting = base.GetSettings();
            setting.i2m_dropdownOpt2 = _chosenModelIndex;
            return setting;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            base.LoadSettings(setting);
            _chosenModelIndex = setting.i2m_dropdownOpt2;
        }


        public void UpdateUI()
        {

        }

    }
}