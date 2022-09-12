using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GAGen.Graph.Elements
{
    public class VoxelToMeshNode : ConverterNode
    {
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.VoxelToMesh;
            base.Initialise(position);
            NodeName = "Voxel to Mesh";
            _inputPortType = GAPortType.Voxel;
            _outputPortType = GAPortType.Mesh;
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}