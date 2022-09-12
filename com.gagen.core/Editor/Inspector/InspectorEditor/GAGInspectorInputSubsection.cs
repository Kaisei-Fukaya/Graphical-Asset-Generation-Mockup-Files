using GAGen.Data;
using GAGen.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAGen.Inspector
{
    public class GAGInspectorInputSubsection : GAGInspectorSubsection
    {
        public GAGInspectorInputSubsection(GraphicalAssetGeneratorInspector mainInspector, DataRetriever dataRetriever, string[] folders = null, bool isOutput = false, string title = "", string name = "") : base(mainInspector, dataRetriever, folders, isOutput, title, name)
        {

        }

        public override void Draw()
        {
            mainContainer.Clear();
            UpdateData();

            foreach (GAGenNodeData node in _data)
            {
                VisualElement nodeDataContainer = new VisualElement();
                if (!_mainInspector.proxyNodes.ContainsKey(node))
                {
                    GraphViewNode pNode = _mainInspector.CreateProxyNode(node.NodeType);
                    _mainInspector.proxyNodes.Add(node, pNode);
                }
                nodeDataContainer.Add(CreateLabelGroup(node));
                nodeDataContainer.Add(CreatePathGroup(node));
                nodeDataContainer.Add(CreateAdditionalDataGroup(node));
                mainContainer.Add(nodeDataContainer);
            }
        }

        VisualElement CreatePathGroup(GAGenNodeData node)
        {
            VisualElement group = new VisualElement();
            if (node.AdditionalSettings == null || node.AdditionalSettings.i_inputPaths == null)
                return group;

            for (int i = 0; i < node.AdditionalSettings.i_inputPaths.Count; i++)
            {
                int iteration = i;
                VisualElement pathGroup = new VisualElement();
                pathGroup.AddToClassList("output-path-container");
                Label pathLabel = new Label();
                pathLabel.text = node.AdditionalSettings.i_inputPaths[i];
                pathLabel.tooltip = pathLabel.text;

                Button openPathPickerButton = new Button();
                openPathPickerButton.text = "...";
                openPathPickerButton.clicked += () => OpenPathPicker(node.AdditionalSettings.i_inputPaths, iteration, pathLabel);
                openPathPickerButton.tooltip = "Select a folder to pull input data from.";

                pathGroup.Add(pathLabel);
                pathGroup.Add(openPathPickerButton);
                group.Add(pathGroup);
            }
            return group;
        }

        public void OpenPathPicker(List<string> paths, int index, Label pathLabel)
        {
            if (paths == null)
                return;
            Debug.Log($"count: {paths.Count}, index: {index}");
            paths[index] = EditorUtility.OpenFolderPanel("Output Folder", "Assets", "");
            pathLabel.text = paths[index];
            pathLabel.tooltip = pathLabel.text;
        }
    }
}