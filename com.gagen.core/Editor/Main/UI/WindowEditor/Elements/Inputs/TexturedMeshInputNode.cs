using GAGen.Data;
using GAGen.Data.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAGen.Graph.Elements
{
    public class TexturedMeshInputNode : InputNode
    {
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.TexturedMeshInput;
            base.Initialise(position);
            NodeName = "Textured Mesh Input";
            _outputPortType = GAPortType.TexturedMesh;
            styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetInputNodeConnectorStyle.uss", typeof(StyleSheet)));
        }

        public override void Draw()
        {
            base.Draw();
        }

    }
}