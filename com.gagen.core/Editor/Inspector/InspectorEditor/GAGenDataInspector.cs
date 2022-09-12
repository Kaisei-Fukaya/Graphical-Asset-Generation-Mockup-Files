using GAGen.Data;
using GAGen.Graph;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAGen.Inspector
{
    [CustomEditor(typeof(GAGenData))]
    public class GAGenDataInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement myInspector = new VisualElement();
            Button loadGraphButton = new Button()
            {
                text = "Load in Graph Editor"
            };
            loadGraphButton.clicked += () =>
            {
                GraphicalAssetGeneratorWindow graphWindow = (GraphicalAssetGeneratorWindow)EditorWindow.GetWindow(typeof(GraphicalAssetGeneratorWindow), false);
                graphWindow.Show();
                graphWindow.Load((GAGenData)target);
            };
            myInspector.Add(loadGraphButton);
            return myInspector;
        }
    }
}