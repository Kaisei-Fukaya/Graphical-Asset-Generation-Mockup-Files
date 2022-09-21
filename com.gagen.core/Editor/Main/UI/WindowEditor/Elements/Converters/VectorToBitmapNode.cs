using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GAGen.Graph.Elements
{
    public class VectorToBitmapNode : ConverterNode
    {
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.VectorToBitmap;
            base.Initialise(position);
            NodeName = "Vector to Bitmap";
            _inputPortType = GAPortType.VectorGraphic;
            _outputPortType = GAPortType.Bitmap;
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}