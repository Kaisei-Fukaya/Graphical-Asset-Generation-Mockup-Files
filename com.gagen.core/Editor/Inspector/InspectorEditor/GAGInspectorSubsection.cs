using GAGen.Data;
using GAGen.Data.Utils;
using GAGen.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAGen.Inspector
{
    public class GAGInspectorSubsection : VisualElement
    {
        public string TitleText { get; set; }
        public VisualElement mainContainer;
        public delegate List<GAGenNodeData> DataRetriever(string[] folders, bool isOutput);
        protected List<GAGenNodeData> _data;
        protected DataRetriever _dRetriever;
        protected GraphicalAssetGeneratorInspector _mainInspector;
        protected string[] _folders;
        protected bool _isOutput;
        public GAGInspectorSubsection(GraphicalAssetGeneratorInspector mainInspector, DataRetriever dataRetriever, string[] folders = null, bool isOutput = false, string title = "", string name = "")
        {
            TitleText = title;
            _dRetriever = dataRetriever;
            _mainInspector = mainInspector;
            _folders = folders;
            _isOutput = isOutput;
            AddToClassList("subsection");
            Label titleLabel = new Label()
            {
                text = TitleText
            };
            titleLabel.AddToClassList("subsection-title");
            mainContainer = new VisualElement();
            mainContainer.AddToClassList("subsection-main");
            if (name != string.Empty)
                contentContainer.name = name;
            contentContainer.Add(titleLabel);
            contentContainer.Add(mainContainer);
            Draw();
        }

        public virtual void Draw()
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
                nodeDataContainer.Add(CreateConnectionsGroup(node));
                nodeDataContainer.Add(CreateAdditionalDataGroup(node));
                mainContainer.Add(nodeDataContainer);
            }
        }

        protected virtual VisualElement CreateLabelGroup(GAGenNodeData node)
        {
            VisualElement group = new VisualElement();
            group.AddToClassList("label-content");
            Label titleLabel = new Label(GAGenDataUtils.DisplayNameLookup[node.NodeType]);
            Button deleteButton = new Button() { text= "X"};
            deleteButton.clicked += () => _mainInspector.RemoveNode(node);
            deleteButton.tooltip = "Delete this node";
            group.Add(titleLabel);
            group.Add(deleteButton);
            return group;
        }

        protected virtual VisualElement CreateConnectionsGroup(GAGenNodeData node)
        {
            VisualElement group = new VisualElement();
            group.AddToClassList("connections-content");
            VisualElement subGroup = new VisualElement();
            subGroup.AddToClassList("connections-content-sub");
            Label connectionsTitle = new Label("Input Connections");
            group.Add(connectionsTitle);
            if (node.AdditionalSettings != null && node.AdditionalSettings.i_portTypes != null)
            {
                for (int i = 0; i < node.AdditionalSettings.i_portTypes.Count; i++)
                {
                    int iteration = i;
                    VisualElement portElement = new VisualElement();
                    Label popupLabel = new Label($"{node.AdditionalSettings.i_portTypes[iteration]}");

                    PopupField<string> portConnectionDropdown = new PopupField<string>();
                    portConnectionDropdown.choices = _mainInspector.GetAllConnectableNodes(node, node.AdditionalSettings.i_portTypes[iteration], iteration);
                    portConnectionDropdown.value = node.GenConnections[iteration].iD;
                    portConnectionDropdown.tooltip = "Select a node to receive input from.";
                    Func<string, string> formatMethod = (string cD) =>
                    {
                        if (cD == "EMPTY")
                            return "Not Connected";
                        return _mainInspector.GetNodeLookup()[cD].NodeType.ToString();
                    };
                    portConnectionDropdown.formatListItemCallback += formatMethod;
                    portConnectionDropdown.formatSelectedValueCallback += formatMethod;
                    portConnectionDropdown.RegisterValueChangedCallback(x => {
                        var nodeLookup = _mainInspector.GetNodeLookup();
                        var newData = nodeLookup[node.ID];
                        newData.GenConnections[iteration] = new ConnectionData(x.newValue, iteration, node.ID, node.AdditionalSettings.i_portTypes[iteration]);
                        _mainInspector.Data.Nodes[_mainInspector.Data.Nodes.IndexOf(nodeLookup[node.ID])] = newData;
                        _mainInspector.UpdateUI();
                    });

                    portElement.Add(popupLabel);
                    portElement.Add(portConnectionDropdown);
                    subGroup.Add(portElement);
                }
            }
            group.Add(subGroup);
            return group;
        }

        protected virtual VisualElement CreateAdditionalDataGroup(GAGenNodeData node)
        {
            VisualElement group = new VisualElement();
            group.AddToClassList("additional-data-content");
            if (_mainInspector.proxyNodes.ContainsKey(node)) 
            {
                group.contentContainer.Add(_mainInspector.proxyNodes[node].DrawAdditionalSettings());
            }
            return group;
        }

        public virtual void UpdateData()
        {
            _data = _dRetriever.Invoke(_folders, _isOutput);
        }
    }
}