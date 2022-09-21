using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using GAGen.Data;
using GAGen.Graph;
using System;
using GAGen.Data.Utils;
using System.Linq;
using GAGen.Runtime;

namespace GAGen.Inspector
{
    [CustomEditor(typeof(GraphicalAssetGenerator))]
    public class GraphicalAssetGeneratorInspector : Editor
    {
        ObjectField _dataObjectField;
        VisualElement _currentDataInspector;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement myInspector = new VisualElement();

            //Add the SO field
            VisualElement SOGroup = BuildProfileSection();
            _currentDataInspector = new VisualElement();

            myInspector.Add(SOGroup);
            myInspector.Add(_currentDataInspector);

            //Add styles
            myInspector.styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetGeneratorVariablesGenerate.uss", typeof(StyleSheet)));
            myInspector.styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetInspectorStyle.uss", typeof(StyleSheet)));

            // Return the finished inspector UI
            UpdateUI();
            return myInspector;
        }

        VisualElement BuildProfileSection()
        {
            VisualElement SOGroup = new VisualElement();
            SOGroup.name = "so-group";
            _dataObjectField = new ObjectField("Data");
            _dataObjectField.objectType = typeof(GAGenData);
            _dataObjectField.allowSceneObjects = false;
            _dataObjectField.BindProperty(serializedObject.FindProperty("data"));
            _dataObjectField.RegisterValueChangedCallback(x => { EditorApplication.delayCall += UpdateUI; });

            Button SONewButton = new Button();
            SONewButton.text = "New";
            SONewButton.clicked += CreateNewProfile;

            SOGroup.Add(_dataObjectField);
            SOGroup.Add(SONewButton);
            SOGroup.tooltip = "Select a profile, or create a new one.";

            return SOGroup;
        }

        void DrawCurrentDataInspector()
        {
            UnityEngine.Object data = serializedObject.FindProperty("data").objectReferenceValue;
            _currentDataInspector.Clear();
            if (data == null)
                return;
            var editor = CreateEditor(data);
            var insp = editor.CreateInspectorGUI();
            _currentDataInspector.Add(insp);
            insp.Bind(editor.serializedObject);
        }

        public void UpdateUI()
        {
            serializedObject.ApplyModifiedProperties();
            DrawCurrentDataInspector();
        }

        public void CreateNewProfile()
        {
            string savePath = EditorUtility.SaveFilePanelInProject("Save As", "New Graphical Asset Generator", "asset", "");
            if (savePath == string.Empty)
                return;
            var newData = CreateInstance<GAGenData>();
            //Add output node
            newData.Nodes = new List<GAGenNodeData>();
            newData.Nodes.Add(new GAGenNodeData() { NodeType = GANodeType.Output, ID = Guid.NewGuid().ToString()});
            AssetDatabase.CreateAsset(newData, savePath);
            _dataObjectField.value = AssetDatabase.LoadAssetAtPath<GAGenData>(savePath);
        }

    }
}