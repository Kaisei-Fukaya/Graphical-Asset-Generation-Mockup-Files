using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GAGen.Graph.Elements
{
    public class BitmapToVectorNode : ConverterNode
    {
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.BitmapToVector;
            base.Initialise(position);
            _inputPortType = GAPortType.Bitmap;
            _outputPortType = GAPortType.VectorGraphic;
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}