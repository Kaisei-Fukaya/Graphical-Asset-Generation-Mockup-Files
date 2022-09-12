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
        GASearchWindowInspector _searchWindowInput, _searchWindowGen, _searchWindowConvert;
        GAOutputSearchWindowInspector _searchWindowOutput;
        GAGInspectorSubsection _inputContainer, _generatorContainer, _conversionContainer, _outputContainer;
        ObjectField _dataObjectField;
        public Dictionary<GAGenNodeData, GraphViewNode> proxyNodes = new Dictionary<GAGenNodeData, GraphViewNode>();

        public GAGenData Data
        {
            get
            {
                var t = target as GraphicalAssetGenerator;
                return t.data as GAGenData;
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


        public override VisualElement CreateInspectorGUI()
        {
            VisualElement myInspector = new VisualElement();



            //Add the SO field
            VisualElement SOGroup = BuildProfileSection();

            _inputContainer = BuildInputSection();
            _generatorContainer = BuildGeneratorSection();
            _conversionContainer = BuildConversionSection();
            _outputContainer = BuildOutputSection();



            //Add the generate button
            Button genButton = new Button();
            genButton.text = "Generate";
            genButton.name = "gen-button";
            genButton.tooltip = "Click to run the generator (NOT FUNCTIONAL).";

            myInspector.Add(SOGroup);
            myInspector.Add(_inputContainer);
            myInspector.Add(_generatorContainer);
            myInspector.Add(_conversionContainer);
            myInspector.Add(_outputContainer);
            //myInspector.Add(addButton);
            myInspector.Add(genButton);

            //Add styles
            myInspector.styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath("Packages/com.gagen.core/Editor/Assets/UIStyles/GraphicalAssetGeneratorVariablesTrain.uss", typeof(StyleSheet)));
            myInspector.styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath("Packages/com.gagen.core/Editor/Assets/UIStyles/GraphicalAssetInspectorStyle.uss", typeof(StyleSheet)));

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
            string sourcePath = $"Packages/com.gagen.core/Editor/Main/UI/WindowEditor/Elements/";
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
                        (nodeToConnectTo.GenConnections[index].iD == "EMPTY" || nodeToConnectTo.GenConnections[index].iD == other.ID)
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
            GAGInspectorInputSubsection inputSection = new GAGInspectorInputSubsection(this, GetAllDataForSection, appropriateFolders, title:"Input", name: "input-section-container");

            //Add the add button
            Button addButton = new Button();
            addButton = CreateSearchButton(appropriateFolders, _searchWindowInput);
            addButton.tooltip = "Choose an input node to add to the generator.";
            inputSection.contentContainer.Add(addButton);

            return inputSection;
        }
        GAGInspectorSubsection BuildGeneratorSection()
        {
            string[] appropriateFolders = new string[] { "Generators", "Utilities" };
            GAGInspectorSubsection generatorSection = new GAGInspectorSubsection(this, GetAllDataForSection, appropriateFolders, title:"Generator", name: "generate-section-container");

            //Add the add button
            Button addButton = new Button();
            addButton = CreateSearchButton(appropriateFolders, _searchWindowGen);
            addButton.tooltip = "Choose a generator node to add to the generator.";
            generatorSection.contentContainer.Add(addButton);

            return generatorSection;
        }
        GAGInspectorSubsection BuildConversionSection()
        {
            string[] appropriateFolders = new string[] { "Converters" };
            GAGInspectorSubsection conversionSection = new GAGInspectorSubsection(this, GetAllDataForSection, appropriateFolders, title:"Conversion", name: "convert-section-container");

            //Add the add button
            Button addButton = new Button();
            addButton = CreateSearchButton(appropriateFolders, _searchWindowConvert);
            addButton.tooltip = "Choose a conversion node to add to the generator.";
            conversionSection.contentContainer.Add(addButton);

            return conversionSection;
        }
        GAGInspectorSubsection BuildOutputSection()
        {
            GAGInspectorSubsection outputSection = new GAGInspectorOutputSubsection(this, GetAllDataForSection, isOutput: true, title: "Output", name: "output-section-container");

            //Add the add button
            Button addButton = new Button();
            addButton = CreateOutputSearchButton(_searchWindowOutput);
            addButton.tooltip = "Choose an output to add to the generator.";
            outputSection.contentContainer.Add(addButton);

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
            _inputContainer.Draw();
            _generatorContainer.Draw();
            _conversionContainer.Draw();
            _outputContainer.Draw();
        }

        public void ConsolidateChangesAndUpdateUI()
        {
            for (int i = 0; i < Data.Nodes.Count; i++)
            {
                if(proxyNodes.ContainsKey(Data.Nodes[i]))
                    Data.Nodes[i] = GAGenDataUtils.GraphNodeToNodeData(proxyNodes[Data.Nodes[i]]);
            }
            UpdateUI();
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

        public void AddNewOutput(GAPortType portType)
        {
            if(Data != null && Data.Nodes != null)
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
                        if(Data.Nodes[i].AdditionalSettings.o_portTypes == null)
                        {
                            Data.Nodes[i].AdditionalSettings.o_portTypes = new List<GAPortType>();
                            Data.Nodes[i].AdditionalSettings.o_outputPaths = new List<string>();
                            Data.Nodes[i].AdditionalSettings.i_portTypes = new List<GAPortType>();
                        }
                        if(Data.Nodes[i].GenConnections == null)
                        {
                            Data.Nodes[i].GenConnections = new List<ConnectionData>();
                            Data.Nodes[i].TrainConnections = new List<ConnectionData>();
                        }

                        //Assignment
                        Data.Nodes[i].AdditionalSettings.o_portTypes.Add(portType);
                        Data.Nodes[i].AdditionalSettings.i_portTypes.Add(portType);
                        Data.Nodes[i].GenConnections.Add(new ConnectionData("EMPTY", Data.Nodes[i].AdditionalSettings.i_portTypes.Count - 1, Data.Nodes[i].ID, portType));
                        Data.Nodes[i].TrainConnections.Add(new ConnectionData("EMPTY", Data.Nodes[i].AdditionalSettings.i_portTypes.Count - 1, Data.Nodes[i].ID, portType));
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

            if(Data.Nodes == null)
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