using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GAGen.Graph;
using GAGen.Data.Utils;
using System.Linq;
using UnityEditor;

namespace GAGen.Data
{
    public class GAGenData : ScriptableObject
    {
        [field: SerializeField] public List<GAGenNodeData> Nodes { get; set; }

        public void Save(GraphicalAssetGraphView graphView)
        {
            List<GraphViewNode> nodes = graphView.Nodes;
            Nodes = new List<GAGenNodeData>();
            foreach (GraphViewNode node in nodes)
            {
                Nodes.Add(GAGenDataUtils.GraphNodeToNodeData(node));
            }
            EditorUtility.SetDirty(this);
        }
    }
}