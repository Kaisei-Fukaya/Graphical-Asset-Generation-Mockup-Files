using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GAGen.Graph.Elements
{
    public class TexturedMeshSplitterNode : GraphViewNode
    {
        protected GraphicalAssetPort _outputPort, _outputPortA;
        protected GAPortType _outputPortType, _outputPortAType;
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.TexturedMeshSplitter;
            base.Initialise(position);
            NodeName = "Textured Mesh Splitter";
            _inputPortType = GAPortType.TexturedMesh;
            _outputPortType = GAPortType.Mesh;
            _outputPortAType = GAPortType.Bitmap;
        }

        public override void Draw()
        {
            base.Draw();

            _ingoingPorts = new List<GraphicalAssetPort>();
            _outgoingPorts = new List<GraphicalAssetPort>();

            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            //_outputPort.portName = _outputPortType.ToString();
            _outgoingPorts.Add(_outputPort);
            outputContainer.Add(_outputPort);

            _outputPortA = new GraphicalAssetPort(this, _outputPortAType, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            //_outputPortA.portName = _outputPortAType.ToString();
            _outgoingPorts.Add(_outputPortA);
            outputContainer.Add(_outputPortA);

            _inputPort = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single);
            _ingoingPorts.Add(_inputPort);
            //_inputPort.portName = _inputPortType.ToString();
            inputContainer.Add(_inputPort);

            RefreshExpandedState();
        }
    }
}