using GAGen.Data;
using GAGen.Data.Utils;
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
    public class GAGInspectorOutputSubsection : GAGInspectorSubsection
    {
        public GAGInspectorOutputSubsection(GAGenDataInspector mainInspector, DataRetriever dataRetriever, Button addButton, string[] folders = null, bool isOutput = true, string title = "", string name = "") : base(mainInspector, dataRetriever, addButton, folders, isOutput, title, name)
        {

        }

        protected override VisualElement CreateLabelGroup(GAGenNodeData node)
        {
            VisualElement group = new VisualElement();
            group.AddToClassList("label-content");
            Label titleLabel = new Label(node.NodeType.ToString());
            group.Add(titleLabel);
            return group;
        }

        public override void Draw()
        {
            mainContainer.Clear();
            UpdateData();

            foreach (GAGenNodeData node in _data)
            {
                //Debug.Log("node in data");
                if (node.NodeType == Graph.GANodeType.Output)
                {
                    GraphViewNode pNode;
                    if (!_mainInspector.proxyNodes.ContainsKey(node))
                    {
                        pNode = _mainInspector.CreateProxyNode(node.NodeType);
                        _mainInspector.proxyNodes.Add(node, pNode);
                    }
                    else
                    {
                        pNode = _mainInspector.proxyNodes[node];
                    }

                    NodeSetting setting = node.AdditionalSettings;
                    //Debug.Log($"Is outputnodesetting {node.AdditionalSettings.ToString()}");

                    if (setting.o_outputPaths == null)
                        break;

                    for (int i = 0; i < setting.o_outputPaths.Count; i++)
                    {
                        int iteration = i;
                        //Debug.Log("Adding path");
                        VisualElement newOutputPath = new VisualElement();
                        newOutputPath.AddToClassList("output-path-container");
                        Label pathLabel = new Label();
                        pathLabel.text = setting.o_outputPaths[i];
                        pathLabel.tooltip = pathLabel.text;

                        Button openPathPickerButton = new Button();
                        openPathPickerButton.text = "...";
                        openPathPickerButton.clicked += () => OpenPathPicker(setting.o_outputPaths, iteration, pathLabel);
                        openPathPickerButton.tooltip = "Select a folder for assets to be saved in.";

                        List<GAPortType> choices = new List<GAPortType>();
                        var ptValues = Enum.GetValues(typeof(GAPortType));
                        for (int j = 0; j < ptValues.Length; j++)
                        {
                            GAPortType x = (GAPortType)ptValues.GetValue(j);
                            choices.Add(x);
                        }

                        PopupField<GAPortType> portTypeDropdown = new PopupField<GAPortType>();
                        portTypeDropdown.value = setting.o_portTypes[i];
                        portTypeDropdown.choices = choices;
                        portTypeDropdown.RegisterValueChangedCallback(x => {
                            setting.o_portTypes[iteration] = x.newValue;
                            node.GenConnections[iteration] = new ConnectionData("EMPTY", 0, iteration, node.ID, setting.o_portTypes[iteration]);
                            _mainInspector.UpdateUI(); 
                        });
                        portTypeDropdown.tooltip = "Select the type of data you would like to save.";

                        PopupField<string> portConnectionDropdown = new PopupField<string>();
                        portConnectionDropdown.choices = _mainInspector.GetAllConnectableNodes(node, setting.o_portTypes[iteration], iteration);

                        //Check if node still exists, if not, remove
                        var nl = _mainInspector.GetNodeLookup();
                        if (!nl.ContainsKey(node.GenConnections[iteration].iD))
                        {
                            node.GenConnections[iteration] = new ConnectionData("EMPTY", 0, iteration, node.ID, setting.o_portTypes[iteration]);
                        }

                        portConnectionDropdown.value = node.GenConnections[iteration].iD;
                        Func<string, string> formatMethod = (string cD) =>
                        {
                            var nl = _mainInspector.GetNodeLookup();
                            if (cD == "EMPTY")
                                return "Not Connected";
                            return $"{GAGenDataUtils.DisplayNameLookup[nl[cD].NodeType]} {_mainInspector.GetNameNumberLookup(cD)}";
                        };
                        portConnectionDropdown.formatListItemCallback += formatMethod;
                        portConnectionDropdown.formatSelectedValueCallback += formatMethod;
                        portConnectionDropdown.RegisterValueChangedCallback(x => {
                            var nodeLookup = _mainInspector.GetNodeLookup();
                            var newData = nodeLookup[node.ID];
                            int indexInOther = GetIndexOfOtherPort(x.newValue, setting.o_portTypes[iteration]);
                            newData.GenConnections[iteration] = new ConnectionData(x.newValue, indexInOther, iteration, node.ID, setting.o_portTypes[iteration]);
                            _mainInspector.Data.Nodes[_mainInspector.Data.Nodes.IndexOf(nodeLookup[node.ID])] = newData;
                            _mainInspector.UpdateUI();
                        });
                        portConnectionDropdown.tooltip = "Select a node to receive input from.";

                        Button deletePathButton = new Button();
                        deletePathButton.text = "X";
                        deletePathButton.clicked += () => DeletePath(node, iteration);
                        deletePathButton.tooltip = "Delete this output.";

                        newOutputPath.Add(pathLabel);
                        newOutputPath.Add(openPathPickerButton);
                        newOutputPath.Add(portTypeDropdown);
                        newOutputPath.Add(portConnectionDropdown);
                        newOutputPath.Add(deletePathButton);
                        mainContainer.Add(newOutputPath);
                    }
                }
            }
            mainContainer.Add(_addButton);
        }

        void DeletePath(GAGenNodeData node, int index)
        {
            if(node.NodeType == GANodeType.Output)
            {
                node.AdditionalSettings.o_outputPaths.RemoveAt(index);
                node.AdditionalSettings.o_portTypes.RemoveAt(index);
                node.GenConnections.RemoveAt(index);
            }
            _mainInspector.UpdateUI();
        }

        public void OpenPathPicker(List<string> paths, int index, Label pathLabel)
        {
            if (paths == null)
                return;
            //Debug.Log($"count: {paths.Count}, index: {index}");
            paths[index] = EditorUtility.OpenFolderPanel("Output Folder", "Assets", "");
            pathLabel.text = paths[index];
            pathLabel.tooltip = pathLabel.text;
        }
    }
}