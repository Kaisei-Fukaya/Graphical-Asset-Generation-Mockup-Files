using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GAGen.Graph.Elements
{
    public class MeshToVoxelNode : ConverterNode
    {
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.MeshToVoxel;
            base.Initialise(position);
            _inputPortType = GAPortType.Mesh;
            _outputPortType = GAPortType.Voxel;
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}