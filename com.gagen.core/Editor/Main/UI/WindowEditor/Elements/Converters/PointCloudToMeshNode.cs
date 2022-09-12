using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GAGen.Graph.Elements
{
    public class PointCloudToMeshNode : ConverterNode
    {
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.PointCloudToMesh;
            base.Initialise(position);
            NodeName = "Point-Cloud to Mesh";
            _inputPortType = GAPortType.PointCloud;
            _outputPortType = GAPortType.Mesh;
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}