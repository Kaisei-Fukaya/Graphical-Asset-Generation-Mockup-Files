using GAGen.Data;
using GAGen.Data.Utils;
using GAGen.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        protected GAGenDataInspector _mainInspector;
        protected string[] _folders;
        protected bool _isOutput;
        protected Button _addButton;
        protected Label _titleLabelIcon;
        public GAGInspectorSubsection(GAGenDataInspector mainInspector, DataRetriever dataRetriever, Button addButton, string[] folders = null, bool isOutput = false, string title = "", string name = "")
        {
            TitleText = title;
            _dRetriever = dataRetriever;
            _mainInspector = mainInspector;
            _folders = folders;
            _isOutput = isOutput;
            _addButton = addButton;
            AddToClassList("subsection");
            Label titleLabel = new Label()
            {
                text = TitleText
            };
            VisualElement titleLabelContainer = new VisualElement();
            titleLabelContainer.Add(titleLabel);
            _titleLabelIcon = new Label('\u25BC'.ToString());
            titleLabelContainer.Add(_titleLabelIcon);
            titleLabelContainer.AddToClassList("subsection-title");
            mainContainer = new VisualElement();
            mainContainer.AddToClassList("subsection-main");
            if (name != string.Empty)
                contentContainer.name = name;
            contentContainer.Add(titleLabelContainer);
            contentContainer.Add(mainContainer);

            titleLabelContainer.RegisterCallback<ClickEvent>(x => FoldoutMainContainer());

            Draw();
        }

        void FoldoutMainContainer()
        {
            if (mainContainer.ClassListContains("hidden"))
            {
                //Debug.Log("unhide");
                mainContainer.RemoveFromClassList("hidden");
                if (_titleLabelIcon != null)
                    _titleLabelIcon.text = '\u25BC'.ToString();
                return;
            }
            //Debug.Log("hide");
            mainContainer.AddToClassList("hidden");
            if (_titleLabelIcon != null)
                _titleLabelIcon.text = '\u25BA'.ToString();
        }

        public virtual void Draw()
        {
            mainContainer.Clear();
            UpdateData();

            foreach (GAGenNodeData node in _data)
            {
                if (node.NodeType == GANodeType.Labeller)
                    continue;
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
                nodeDataContainer.Add(CreateOutputsGroup(node));
                mainContainer.Add(nodeDataContainer);
            }

            mainContainer.Add(_addButton);
        }

        protected virtual VisualElement CreateLabelGroup(GAGenNodeData node)
        {
            VisualElement group = new VisualElement();
            group.AddToClassList("label-content");
            Label titleLabel = new Label($"{GAGenDataUtils.DisplayNameLookup[node.NodeType]} {_mainInspector.GetNameNumberLookup(node.ID)}");
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
            GraphViewNode pNode = _mainInspector.proxyNodes[node];
            if (pNode != null && pNode.IngoingPorts != null)
            {
                List<GraphicalAssetPort> ingoingPorts = pNode.IngoingPorts;
                for (int i = 0; i < ingoingPorts.Count; i++)
                {
                    int iteration = i;
                    VisualElement portElement = new VisualElement();
                    Label popupLabel = new Label($"{ingoingPorts[iteration].PortName}");

                    PopupField<string> portConnectionDropdown = new PopupField<string>();
                    portConnectionDropdown.choices = _mainInspector.GetAllConnectableNodes(node, ingoingPorts[iteration].PortType, iteration);

                    //Check if node still exists, if not, remove
                    var nl = _mainInspector.GetNodeLookup();
                    if (!nl.ContainsKey(node.GenConnections[iteration].iD))
                        node.GenConnections[iteration] = new ConnectionData("EMPTY", 0, iteration, node.ID, ingoingPorts[iteration].PortType);

                    portConnectionDropdown.value = node.GenConnections[iteration].iD;
                    portConnectionDropdown.tooltip = "Select a node to receive input from.";
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
                        int indexInOther = GetIndexOfOtherPort(x.newValue, ingoingPorts[iteration].PortType);
                        newData.GenConnections[iteration] = new ConnectionData(x.newValue, indexInOther, iteration, node.ID, ingoingPorts[iteration].PortType);
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
                VisualElement additionalSettings = _mainInspector.proxyNodes[node].DrawAdditionalSettings();
                if (additionalSettings == null)
                {
                    group.style.display = new StyleEnum<DisplayStyle>(StyleKeyword.None);
                    return group;
                }
                group.Add(new Label("Settings"));
                group.contentContainer.Add(additionalSettings);
            }
            return group;
        }

        protected virtual VisualElement CreateOutputsGroup(GAGenNodeData node)
        {
            VisualElement group = new VisualElement();
            group.AddToClassList("outputs-content");
            group.Add(new Label("Output Connections"));
            VisualElement subGroup = new VisualElement();
            GraphViewNode pNode = _mainInspector.proxyNodes[node];
            if (pNode != null && pNode.OutgoingPorts != null)
            {
                List<GraphicalAssetPort> outgoingPorts = pNode.OutgoingPorts;
                for (int i = 0; i < outgoingPorts.Count; i++)
                {
                    int iteration = i;
                    VisualElement outPortElement = new VisualElement();
                    outPortElement.AddToClassList("outport-element");
                    Label outPortLabel = new Label($"{outgoingPorts[iteration].PortName} -->");
                    outPortElement.Add(outPortLabel);
                    if (_mainInspector.connectedOutputPorts.ContainsKey(outgoingPorts[iteration]))
                    {
                        outPortElement.style.backgroundColor = new StyleColor(new Color(0f, 255f, 0f, .5f));
                        outPortElement.tooltip = $"Connected to: ";
                        var nl = _mainInspector.GetNodeLookup();
                        var lastNodeConnectedTo = _mainInspector.connectedOutputPorts[outgoingPorts[iteration]].Last();
                        foreach (GAGenNodeData nodeConnectedTo in _mainInspector.connectedOutputPorts[outgoingPorts[iteration]])
                        {
                            outPortElement.tooltip += $"\n{GAGenDataUtils.DisplayNameLookup[nl[nodeConnectedTo.ID].NodeType]} {_mainInspector.GetNameNumberLookup(nodeConnectedTo.ID)}";
                            VisualElement outputTag = new VisualElement();
                            outputTag.AddToClassList("output-tag");
                            Label outputTagLabel = new Label($"{GAGenDataUtils.DisplayNameLookup[nl[nodeConnectedTo.ID].NodeType]} {_mainInspector.GetNameNumberLookup(nodeConnectedTo.ID)}");
                            outputTag.Add(outputTagLabel);
                            if (nodeConnectedTo != lastNodeConnectedTo)
                            {
                                VisualElement spacer = new VisualElement();
                                spacer.AddToClassList("output-spacer");
                                outputTag.Add(spacer);
                            }
                            outPortElement.Add(outputTag);
                        }
                    }
                    else
                    {
                        outPortElement.style.backgroundColor = new StyleColor(new Color(255f, 0f, 0f, .5f));
                        outPortElement.tooltip = $"Not connected";
                    }
                    subGroup.Add(outPortElement);
                }
            }
            group.Add(subGroup);
            return group;
        }

        protected int GetIndexOfOtherPort(string guidOfOther, GAPortType portType)
        {
            if (guidOfOther == "EMPTY")
                return 0;
            var otherData = _mainInspector.GetNodeLookup()[guidOfOther];
            var otherProxyNode = _mainInspector.proxyNodes[otherData];
            for (int j = 0; j < otherProxyNode.OutgoingPorts.Count; j++)
            {
                if (otherProxyNode.OutgoingPorts[j].PortType == portType)
                    return j;
            }
            return 0;
        }

        public virtual void UpdateData()
        {
            _data = _dRetriever.Invoke(_folders, _isOutput);
        }
    }
}