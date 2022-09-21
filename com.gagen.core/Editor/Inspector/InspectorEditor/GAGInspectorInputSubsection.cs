using GAGen.Data;
using GAGen.Graph;
using GAGen.Graph.Elements;
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
        public GAGInspectorInputSubsection(GAGenDataInspector mainInspector, DataRetriever dataRetriever, Button addButton, string[] folders = null, bool isOutput = false, string title = "", string name = "") : base(mainInspector, dataRetriever, addButton, folders, isOutput, title, name)
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
                    if (node.AdditionalSettings != null)
                    {
                        pNode.LoadSettings(node.AdditionalSettings);
                        pNode.Draw();
                    }
                    pNode.onSettingEdit += _mainInspector.ConsolidateChangesAndUpdateUI;
                    _mainInspector.proxyNodes.Add(node, pNode);
                }
                nodeDataContainer.Add(CreateLabelGroup(node));
                nodeDataContainer.Add(CreatePathGroup(node));
                nodeDataContainer.Add(CreateAdditionalDataGroup(node));
                nodeDataContainer.Add(CreateOutputsGroup(node));
                mainContainer.Add(nodeDataContainer);
            }
            mainContainer.Add(_addButton);
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
                pathGroup.tooltip = pathLabel.text;

                Button openPathPickerButton = new Button();
                openPathPickerButton.text = "...";
                openPathPickerButton.clicked += () => OpenPathPicker(node.AdditionalSettings.i_inputPaths, iteration, pathLabel, pathGroup, node);
                openPathPickerButton.tooltip = "Select a folder to pull input data from.";

                InputNode pNode = (InputNode)_mainInspector.proxyNodes[node];
                PopupField<string> popupField = pNode.GetPopupField();

                pathGroup.Add(pathLabel);
                pathGroup.Add(openPathPickerButton);
                if(popupField != null)
                {
                    pathGroup.Add(popupField);
                }
                group.Add(pathGroup);
            }
            return group;
        }

        public void OpenPathPicker(List<string> paths, int index, Label pathLabel, VisualElement pathLabelContainer, GAGenNodeData node)
        {
            if (paths == null)
                return;
            //Debug.Log($"count: {paths.Count}, index: {index}");
            if (node.AdditionalSettings.i_chosenInputMode == 1)
                paths[index] = EditorUtility.OpenFolderPanel("Data Source", "Assets", "");
            else
                paths[index] = EditorUtility.OpenFilePanel("Data Source", "Assets", "");
            pathLabel.text = paths[index];
            pathLabelContainer.tooltip = pathLabel.text;
        }
    }
}