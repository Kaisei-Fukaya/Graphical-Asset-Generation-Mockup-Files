using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAGen.Graph.Elements
{
    public class ConverterNode : GraphViewNode
    {
        protected GraphicalAssetPort _outputPort;
        protected GAPortType _outputPortType;
        public override void Initialise(Vector2 position)
        {
            base.Initialise(position);
        }

        public override void Draw()
        {
            base.Draw();

            _outgoingPorts = new List<GraphicalAssetPort>();
            _ingoingPorts = new List<GraphicalAssetPort>();

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
    }
}