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
    public class TextToImageNode : GraphViewNode
    {
        protected GraphicalAssetPort _outputPort;
        protected GAPortType _outputPortType;
        protected List<string> _models = new List<string>(){
        "A",
        "B",
        "C"
    };
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.TextToImage;
            base.Initialise(position);
            NodeName = "Text to Image";
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

            RefreshExpandedState();
        }


        public void UpdateUI()
        {

        }

    }
}