using GAGen.Data;
using GAGen.Data.Utils;
using GAGen.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAGen.Inspector
{
    [CustomEditor(typeof(GAGenData))]
    public class GAGenDataInspector : Editor
    {
        GASearchWindowInspector _searchWindowInput, _searchWindowGen, _searchWindowConvert;
        GAOutputSearchWindowInspector _searchWindowOutput;
        GAGInspectorSubsection _inputContainer, _generatorContainer, _conversionContainer, _outputContainer;
        public Dictionary<GAGenNodeData, GraphViewNode> proxyNodes = new Dictionary<GAGenNodeData, GraphViewNode>();
        public Dictionary<GraphicalAssetPort, List<GAGenNodeData>> connectedOutputPorts = new Dictionary<GraphicalAssetPort, List<GAGenNodeData>>();

        public GAGenData Data
        {
            get
            {
                var t = target as GAGenData;
                return t;
            }
        }

        Dictionary<string, GAGenNodeData> _nodeLookup;
        public Dictionary<string, GAGenNodeData> GetNodeLookup()
        {
            _nodeLookup = new Dictionary<string, GAGenNodeData>();
            foreach (GAGenNodeData data in Data.Nodes)
            {
                _nodeLookup.Add(data.ID, data);
            }
            return _nodeLookup;
        }
        Dictionary<string, int> _nameNumberLookup = new Dictionary<string, int>();
        public int GetNameNumberLookup(string iD)
        {
            if (!_nameNumberLookup.ContainsKey(iD))
            {
                if (_nodeLookup != null && _nodeLookup.ContainsKey(iD)) 
                {
                    GANodeType targetNodeType = _nodeLookup[iD].NodeType;
                    int count = 1;
                    foreach (var kvp in _nameNumberLookup)
                    {
                        if (_nodeLookup.ContainsKey(kvp.Key))
                        {
                            if (_nodeLookup[kvp.Key].NodeType == targetNodeType) 
                            {
                                count++;
                            }
                        }
                    }
                    _nameNumberLookup.Add(iD, count);
                }
                else
                {
                    return -1;
                }
            }

            return _nameNumberLookup[iD];
        }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement myInspector = new VisualElement();

            Button loadGraphButton = new Button()
            {
                text = "Open in Graph Editor"
            };
            loadGraphButton.clicked += () =>
            {
                GraphicalAssetGeneratorWindow graphWindow = (GraphicalAssetGeneratorWindow)EditorWindow.GetWindow(typeof(GraphicalAssetGeneratorWindow), false);
                graphWindow.Show();
                graphWindow.Load((GAGenData)target);
            };
            loadGraphButton.AddToClassList("load-graph-button");

            _inputContainer = BuildInputSection();
            _generatorContainer = BuildGeneratorSection();
            _conversionContainer = BuildConversionSection();
            _outputContainer = BuildOutputSection();

            //Add the generate button
            VisualElement endGroup = new VisualElement();
            Button genButton = new Button();
            genButton.text = "Generate";
            genButton.name = "gen-button";
            genButton.tooltip = "Click to run the generator (NOT FUNCTIONAL).";
            endGroup.AddToClassList("end-group");
            endGroup.Add(genButton);
            endGroup.Add(loadGraphButton);

            myInspector.Add(_inputContainer);
            myInspector.Add(_generatorContainer);
            myInspector.Add(_conversionContainer);
            myInspector.Add(_outputContainer);
            //myInspector.Add(addButton);
            myInspector.Add(endGroup);

            //Add styles
            myInspector.styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetGeneratorVariablesGenerate.uss", typeof(StyleSheet)));
            myInspector.styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetInspectorStyle.uss", typeof(StyleSheet)));

            // Return the finished inspector UI
            UpdateUI();
            return myInspector;
        }

        List<GAGenNodeData> GetAllDataForSection(string[] folders = null, bool isOutput = false)
        {
            List<GAGenNodeData> output = new List<GAGenNodeData>();
            if (Data == null || Data.Nodes == null)
                return output;

            if (isOutput)
            {
                for (int i = 0; i < Data.Nodes.Count; i++)
                {
                    if (Data.Nodes[i].NodeType == GANodeType.Output)
                    {
                        output.Add(Data.Nodes[i]);
                        return output;
                    }
                }
                return output;
            }

            List<GANodeType> relevantNodeTypes = new List<GANodeType>();


            //Find relevant node types based on folder filter
            string sourcePath = $"{GAGenDataUtils.BasePath}Editor/Main/UI/WindowEditor/Elements/";
            string[] allFolders = GAGenDataUtils.GetFolderPaths(sourcePath);
            if (folders != null)
            {
                foreach (string f in allFolders)
                {
                    string[] fileNames = GAGenDataUtils.GetFileNames(f + "/");
                    string folderName = f.Replace(sourcePath, "");
                    if (folders.Contains(folderName))
                    {
                        foreach (string fileName in fileNames)
                        {
                            GANodeType nType = GAGenDataUtils.GetNodeTypeFromName(fileName);
                            if (nType == GANodeType.Labeller)
                                continue;
                            if (!relevantNodeTypes.Contains(nType))
                                relevantNodeTypes.Add(nType);
                        }
                    }
                }
            }

            //Find all nodes of the relevant types
            foreach (GAGenNodeData node in Data.Nodes)
            {
                if (relevantNodeTypes.Contains(node.NodeType))
                {
                    output.Add(node);
                }
            }
            return output;
        }

        public List<string> GetAllConnectableNodes(GAGenNodeData nodeToConnectTo, GAPortType desiredPortType, int index)
        {
            List<string> candidateConnections = new List<string>();

            if (Data == null || Data.Nodes == null)
                return candidateConnections;

            //Default no choice
            candidateConnections.Add("EMPTY");

            //for (int i = 0; i < Data.Nodes.Count; i++)
            //{
            //    GAGenNodeData other = Data.Nodes[i];
            //    foreach (ConnectionData con in other.GenConnections)
            //    {
            //        foreach (string canCon in candidateConnections) 
            //        {
            //            if (con.iD == canCon)

            //        }
            //    }
            //}

            for (int i = 0; i < Data.Nodes.Count; i++)
            {
                GAGenNodeData other = Data.Nodes[i];
                if (other == nodeToConnectTo)
                    continue;
                if (other.AdditionalSettings == null || other.AdditionalSettings.o_portTypes == null)
                    continue;
                for (int j = 0; j < other.AdditionalSettings.o_portTypes.Count; j++)
                {
                    if (
                        other.AdditionalSettings.o_portTypes[j] == desiredPortType &&
                        other.NodeType != GANodeType.Output &&
                        other.NodeType != GANodeType.Labeller
                    )
                    {
                        candidateConnections.Add(other.ID);
                    }
                }
            }

            return candidateConnections;
        }

        GAGInspectorSubsection BuildInputSection()
        {

            string[] appropriateFolders = new string[] { "Inputs" };
            //Add the add button
            Button addButton = CreateSearchButton(appropriateFolders, _searchWindowInput);
            addButton.tooltip = "Choose an input node to add to the generator.";
            GAGInspectorInputSubsection inputSection = new GAGInspectorInputSubsection(this, GetAllDataForSection, addButton, appropriateFolders, title: "Inputs", name: "input-section-container");

            return inputSection;
        }
        GAGInspectorSubsection BuildGeneratorSection()
        {
            string[] appropriateFolders = new string[] { "Generators", "Utilities" };
            //Add the add button
            Button addButton = CreateSearchButton(appropriateFolders, _searchWindowGen);
            addButton.tooltip = "Choose a generator node to add to the generator.";
            GAGInspectorSubsection generatorSection = new GAGInspectorSubsection(this, GetAllDataForSection, addButton, appropriateFolders, title: "Generators", name: "generate-section-container");

            return generatorSection;
        }
        GAGInspectorSubsection BuildConversionSection()
        {
            string[] appropriateFolders = new string[] { "Converters" };
            //Add the add button
            Button addButton = CreateSearchButton(appropriateFolders, _searchWindowConvert);
            addButton.tooltip = "Choose a conversion node to add to the generator.";
            GAGInspectorSubsection conversionSection = new GAGInspectorSubsection(this, GetAllDataForSection, addButton, appropriateFolders, title: "Converters", name: "convert-section-container");

            return conversionSection;
        }
        GAGInspectorSubsection BuildOutputSection()
        {
            //Add the add button
            Button addButton = CreateOutputSearchButton(_searchWindowOutput);
            addButton.tooltip = "Choose an output to add to the generator.";
            GAGInspectorSubsection outputSection = new GAGInspectorOutputSubsection(this, GetAllDataForSection, addButton, isOutput: true, title: "Outputs", name: "output-section-container");

            return outputSection;
        }

        Button CreateOutputSearchButton(GAOutputSearchWindowInspector searchWindow)
        {
            Button addButton = new Button();
            searchWindow = CreateInstance<GAOutputSearchWindowInspector>();
            searchWindow.Initialise(this);
            Vector2 searchWindowPosition = GUIUtility.GUIToScreenPoint(addButton.worldBound.center);
            addButton.clicked += () => SearchWindow.Open(new SearchWindowContext(GetSearchWindowPosition(addButton)), searchWindow);
            addButton.text = "Add New Node";
            return addButton;
        }

        Button CreateSearchButton(string[] folders, GASearchWindowInspector searchWindow)
        {
            Button addButton = new Button();
            searchWindow = CreateInstance<GASearchWindowInspector>();
            searchWindow.Initialise(this, folders);
            Vector2 searchWindowPosition = GUIUtility.GUIToScreenPoint(addButton.worldBound.center);
            addButton.clicked += () => SearchWindow.Open(new SearchWindowContext(GetSearchWindowPosition(addButton)), searchWindow);
            addButton.text = "Add New Node";
            return addButton;
        }


        public void UpdateUI()
        {
            serializedObject.ApplyModifiedProperties();
            SetAllPortStatuses();
            _inputContainer.Draw();
            _generatorContainer.Draw();
            _conversionContainer.Draw();
            _outputContainer.Draw();
        }

        void SetAllPortStatuses()
        {
            List<GAGenNodeData> nodes = Data.Nodes;
            connectedOutputPorts = new Dictionary<GraphicalAssetPort, List<GAGenNodeData>>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].GenConnections == null)
                    continue;
                List<ConnectionData> connections = nodes[i].GenConnections;
                for (int j = 0; j < connections.Count; j++)
                {
                    var nL = GetNodeLookup();
                    if (nL.ContainsKey(connections[j].iD) && proxyNodes.ContainsKey(nL[connections[j].iD]))
                    {
                        List<GraphicalAssetPort> ports = proxyNodes[nL[connections[j].iD]].OutgoingPorts;
                        if (ports != null && ports.Count > connections[j].indexInOther)
                        {
                            if(!connectedOutputPorts.ContainsKey(proxyNodes[nL[connections[j].iD]].OutgoingPorts[connections[j].indexInOther]))
                                connectedOutputPorts.Add(proxyNodes[nL[connections[j].iD]].OutgoingPorts[connections[j].indexInOther], new List<GAGenNodeData> {});
                            connectedOutputPorts[proxyNodes[nL[connections[j].iD]].OutgoingPorts[connections[j].indexInOther]].Add(nodes[i]);
                        }
                    }
                }
            }
        }

        public void ConsolidateChangesAndUpdateUI()
        {
            for (int i = 0; i < Data.Nodes.Count; i++)
            {
                if (proxyNodes.ContainsKey(Data.Nodes[i]))
                    Data.Nodes[i] = GAGenDataUtils.GraphNodeToNodeData(proxyNodes[Data.Nodes[i]]);
            }
            UpdateUI();
        }

        public void AddNewOutput(GAPortType portType)
        {
            if (Data != null && Data.Nodes != null)
            {
                for (int i = 0; i < Data.Nodes.Count; i++)
                {
                    if (Data.Nodes[i].NodeType == GANodeType.Output)
                    {
                        //Initialisation
                        if (Data.Nodes[i].AdditionalSettings == null)
                        {
                            Data.Nodes[i].AdditionalSettings = new NodeSetting();
                        }
                        if (Data.Nodes[i].AdditionalSettings.o_portTypes == null)
                        {
                            Data.Nodes[i].AdditionalSettings.o_portTypes = new List<GAPortType>();
                            Data.Nodes[i].AdditionalSettings.o_outputPaths = new List<string>();
                            Data.Nodes[i].AdditionalSettings.i_portTypes = new List<GAPortType>();
                        }
                        if (Data.Nodes[i].GenConnections == null)
                        {
                            Data.Nodes[i].GenConnections = new List<ConnectionData>();
                            Data.Nodes[i].TrainConnections = new List<ConnectionData>();
                        }

                        //Assignment
                        Data.Nodes[i].AdditionalSettings.o_portTypes.Add(portType);
                        Data.Nodes[i].AdditionalSettings.i_portTypes.Add(portType);
                        Data.Nodes[i].GenConnections.Add(new ConnectionData("EMPTY", 0, Data.Nodes[i].AdditionalSettings.i_portTypes.Count - 1, Data.Nodes[i].ID, portType));
                        Data.Nodes[i].TrainConnections.Add(new ConnectionData("EMPTY", 0, Data.Nodes[i].AdditionalSettings.i_portTypes.Count - 1, Data.Nodes[i].ID, portType));
                        Data.Nodes[i].AdditionalSettings.o_outputPaths.Add("");
                        UpdateUI();
                        return;
                    }
                }
            }
        }

        public void AddNewNode(GANodeType nodeType, int sectionIndex)
        {
            CreateNewNode(nodeType);
            UpdateUI();
        }

        public GraphViewNode CreateProxyNode(GANodeType nodeType)
        {
            Type proxyNodeType = Type.GetType($"GAGen.Graph.Elements.{nodeType}Node");
            GraphViewNode proxyNode = (GraphViewNode)Activator.CreateInstance(proxyNodeType);
            proxyNode.Initialise(Vector2.zero);
            proxyNode.Draw();
            return proxyNode;
        }

        void CreateNewNode(GANodeType nodeType)
        {
            if (Data == null)
                return;

            GraphViewNode proxyNode = CreateProxyNode(nodeType);
            proxyNode.onSettingEdit += ConsolidateChangesAndUpdateUI;

            GAGenNodeData newNodeData = GAGenDataUtils.GraphNodeToNodeData(proxyNode);

            proxyNodes.Add(newNodeData, proxyNode);

            if (Data.Nodes == null)
            {
                Data.Nodes = new List<GAGenNodeData>();
            }
            Data.Nodes.Add(newNodeData);
            serializedObject.ApplyModifiedProperties();
        }

        public void RemoveNode(GAGenNodeData node)
        {
            if (Data == null || Data.Nodes == null)
                return;

            if (Data.Nodes.Contains(node))
                Data.Nodes.Remove(node);

            serializedObject.ApplyModifiedProperties();
            UpdateUI();
        }

        Vector2 GetSearchWindowPosition(VisualElement element)
        {
            Vector2 value = GUIUtility.GUIToScreenPoint(element.worldBound.center);
            value.y += (element.worldBound.height * 1.2f);
            return value;
        }

    }
}