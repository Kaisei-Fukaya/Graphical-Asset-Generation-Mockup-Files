using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GAGen.Graph.Elements
{
    public class TexturedMeshCombinerNode : GraphViewNode
    {
        protected GraphicalAssetPort _outputPort, _inputPortA;
        protected GAPortType _outputPortType, _inputPortTypeA;
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.TexturedMeshCombiner;
            NodeName = "Textured Mesh Combiner";
            base.Initialise(position);
            _inputPortType = GAPortType.Mesh;
            _inputPortTypeA = GAPortType.Bitmap;
            _outputPortType = GAPortType.TexturedMesh;
        }

        public override void Draw()
        {
            base.Draw();

            _ingoingPorts = new List<GraphicalAssetPort>();
            _outgoingPorts = new List<GraphicalAssetPort>();

            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            //_outputPort.portName = _outputPortType.ToString();
            _outgoingPorts.Add(_outputPort);
            outputContainer.Add(_outputPort);

            _inputPort = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            _ingoingPorts.Add(_inputPort);
            //_inputPort.portName = _inputPortType.ToString();
            inputContainer.Add(_inputPort);

            _inputPortA = new GraphicalAssetPort(this, _inputPortTypeA, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            //_outputPortA.portName = _outputPortAType.ToString();
            _ingoingPorts.Add(_inputPortA);
            inputContainer.Add(_inputPortA);


            RefreshExpandedState();
        }
    }
}