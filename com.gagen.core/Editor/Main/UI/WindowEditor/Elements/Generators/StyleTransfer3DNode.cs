using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GAGen.Graph.Elements
{
    public class StyleTransfer3DNode : GraphViewNode
    {
        protected GraphicalAssetPort _inputPortA, _inputPortB, _outputPort;
        protected GAPortType _inputPortAType, _inputPortBType, _outputPortType;
        protected List<string> _models = new List<string>(){
        "A",
        "B",
        "C"
        };
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.StyleTransfer3D;
            base.Initialise(position);
            NodeName = "Style Transfer 3D";
            _inputPortType = GAPortType.TexturedMesh;
            _inputPortAType = GAPortType.Mesh;
            _inputPortBType = GAPortType.Bitmap;
            _outputPortType = GAPortType.TexturedMesh;
        }

        public override void Draw()
        {
            base.Draw();

            _ingoingPorts = new List<GraphicalAssetPort>();
            _outgoingPorts = new List<GraphicalAssetPort>();

            VisualElement modelSelector = new VisualElement();
            Label modelSelectorLabel = new Label("Model");
            PopupField<string> modelSelectorPopup = new PopupField<string>(_models, 0);
            modelSelector.Add(modelSelectorLabel);
            modelSelector.Add(modelSelectorPopup);
            extensionContainer.Insert(0, modelSelector);

            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            _outgoingPorts.Add(_outputPort);
            //_outputPort.portName = _outputPortType.ToString();
            outputContainer.Add(_outputPort);

            _inputPort = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            _ingoingPorts.Add(_inputPort);
            //_inputPort.portName = _inputPortType.ToString();
            inputContainer.Add(_inputPort);

            _inputPortA = new GraphicalAssetPort(this, _inputPortAType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            _ingoingPorts.Add(_inputPortA);
            //_inputPortA.portName = _inputPortAType.ToString();
            inputContainer.Add(_inputPortA);

            _inputPortB = new GraphicalAssetPort(this, _inputPortBType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            _ingoingPorts.Add(_inputPortB);
            //_inputPortB.portName = _inputPortBType.ToString();
            inputContainer.Add(_inputPortB);


            RefreshExpandedState();
        }
    }
}