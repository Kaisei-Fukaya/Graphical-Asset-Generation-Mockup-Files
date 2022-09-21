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
    public class ObjectDistributorNode : GraphViewNode
    {
        protected GraphicalAssetPort _outputPort;
        protected GraphicalAssetPort _inputPortA;
        protected GAPortType _outputPortType;
        protected GAPortType _inputPortAType;
        protected List<string> _models = new List<string>(){
            "Interiors",
            "Exteriors"
        };
        protected int _chosenModelIndex;
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.ObjectDistributor;
            base.Initialise(position);
            NodeName = "Object Distributor";
            NodeDescription = "This node distributes provided objects over a provided environment mesh";
            _inputPortType = GAPortType.TexturedMesh;
            _inputPortAType = GAPortType.TexturedMeshSet;
            _outputPortType = GAPortType.TexturedMesh;
        }

        public override void Draw()
        {
            base.Draw();

            _ingoingPorts = new List<GraphicalAssetPort>();
            _outgoingPorts = new List<GraphicalAssetPort>();

            VisualElement modelSelector = new VisualElement();
            Label modelSelectorLabel = new Label("Type of Distribution");
            PopupField<string> modelSelectorPopup = new PopupField<string>(_models, _chosenModelIndex);
            modelSelectorPopup.RegisterValueChangedCallback(x => { _chosenModelIndex = _models.IndexOf(x.newValue); CallSettingsEditEvent(); });
            modelSelector.Add(modelSelectorLabel);
            modelSelector.Add(modelSelectorPopup);
            extensionContainer.Insert(0, modelSelector);


            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, "Prefab");
            _outgoingPorts.Add(_outputPort);
            //_outputPort.portName = _outputPortType.ToString();
            outputContainer.Add(_outputPort);

            _inputPort = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, "Environment");
            _ingoingPorts.Add(_inputPort);
            //_inputPort.portName = _inputPortType.ToString();
            inputContainer.Add(_inputPort);

            _inputPortA = new GraphicalAssetPort(this, _inputPortAType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, "Items");
            _ingoingPorts.Add(_inputPortA);
            //_inputPort.portName = _inputPortType.ToString();
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