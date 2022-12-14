using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;
using GAGen.Data;
using GAGen.Graph.Elements;
using GAGen.Data.Utils;

namespace GAGen.Graph
{
    public abstract class GraphViewNode : Node
    {
        public string ID { get; set; }
        public string NodeName { get; set; }
        public string NodeDescription { get; set; }
        public string Text { get; set; }
        public GANodeType NodeType { get; set; }


        public GraphicalAssetGraphView GraphView { get; set; }

        protected GAPortType _inputPortType;
        protected GraphicalAssetPort _inputPort;
        protected List<GraphicalAssetPort> _outgoingPorts;
        protected List<GraphicalAssetPort> _ingoingPorts;

        public List<GraphicalAssetPort> OutgoingPorts
        {
            get
            {
                return _outgoingPorts;
            }
        }

        public List<GraphicalAssetPort> IngoingPorts
        {
            get
            {
                return _ingoingPorts;
            }
        }


        public delegate void EventTrigger();
        public event EventTrigger onSettingEdit;

        public virtual void Initialise(Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            NodeName = NodeType.ToString();
            Text = "Hello world!";
            SetPosition(new Rect(position, Vector2.zero));
            styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetDefaultNodeStyle.uss", typeof(StyleSheet)));
        }

        protected void CallSettingsEditEvent()
        {
            onSettingEdit?.Invoke();
        }

        public virtual void Draw()
        {
            extensionContainer.Clear();
            inputContainer.Clear();
            outputContainer.Clear();
            //Title
            //Label nameText = new Label()
            //{
            //    text = NodeName
            //};
            //titleContainer.Insert(0, nameText);

            title = NodeName;
            topContainer.tooltip = NodeDescription;

            //VisualElement customDataContainer = new VisualElement();

            //Foldout textFoldout = new Foldout()
            //{
            //    text = "Foldout"
            //};
            //TextField textTextField = new TextField()
            //{
            //    value = Text
            //};

            //textFoldout.Add(textTextField);
            //customDataContainer.Add(textFoldout);

            //extensionContainer.Add(customDataContainer);

        }

        public virtual VisualElement DrawAdditionalSettings()
        {
            if (extensionContainer.childCount < 1)
                return null;
            return extensionContainer;
        }

        public virtual void LoadSettings(NodeSetting setting)
        {

        }

        public virtual NodeSetting GetSettings()
        {

            List<GAPortType> inPortTypes = new List<GAPortType>();
            List<GAPortType> outPortTypes = new List<GAPortType>();

            if (_ingoingPorts != null)
            {
                foreach (GraphicalAssetPort port in _ingoingPorts)
                {
                    inPortTypes.Add(port.PortType);
                }
            }
            if (_outgoingPorts != null)
            {
                foreach (GraphicalAssetPort port in _outgoingPorts)
                {
                    outPortTypes.Add(port.PortType);
                }
            }

            NodeSetting setting = new NodeSetting()
            {
                i_portTypes = inPortTypes,
                o_portTypes = outPortTypes
            };
            return setting;
        }

        public void DisconnectAllPorts()
        {
            DisconnectPorts(inputContainer);
            DisconnectPorts(outputContainer);
        }
        void DisconnectPorts(VisualElement container)
        {
            foreach (VisualElement element in container.Children())
            {
                if (element is GraphicalAssetPort)
                {
                    GraphicalAssetPort port = element as GraphicalAssetPort;
                    GraphView.DeleteElements(port.Connections(true));
                    GraphView.DeleteElements(port.Connections(false));
                    port.DisconnectAll();
                }
            }
        }

        public List<GraphicalAssetPort> GetPorts(bool isInput)
        {
            List<GraphicalAssetPort> GAPorts = new List<GraphicalAssetPort>();
            IEnumerable<VisualElement> children = isInput ? inputContainer.Children() : outputContainer.Children();
            foreach (VisualElement ve in children)
            {
                GraphicalAssetPort port;
                if (ve is GraphicalAssetPort)
                {
                    port = ve as GraphicalAssetPort;
                }
                else
                {
                    port = null;
                }

                if (port != null)
                {
                    GAPorts.Add(port);
                    continue;
                }

                OutputNode.Connector connector;
                if (ve is OutputNode.Connector)
                {
                    connector = ve as OutputNode.Connector;
                }
                else
                {
                    connector = null;
                }
                if (connector != null)
                {
                    GAPorts.Add(connector.port);
                }
            }

            return GAPorts;
        }

        public List<GAGen.Data.ConnectionData> GetOutgoingConnectionIDs(bool isTrainMode)
        {
            if (_outgoingPorts == null)
                return null;

            List<Data.ConnectionData> outgoingConnections = new List<Data.ConnectionData>();
            foreach (GraphicalAssetPort port in _outgoingPorts)
            {
                Edge connectedEdge = port.GetConnectedEdge(isTrainMode);
                if(connectedEdge == null)
                {
                    outgoingConnections.Add(new Data.ConnectionData("EMPTY", 0, 0, ID, port.PortType));
                    continue;
                }
                GraphViewNode otherNode = connectedEdge.input.node as GraphViewNode;
                int indexOfOtherPort = otherNode.GetPorts(true).IndexOf(otherNode.GetPorts(true).Where(x => x.GetPort(isTrainMode) == connectedEdge.input).FirstOrDefault());
                outgoingConnections.Add(new Data.ConnectionData(otherNode.ID, indexOfOtherPort, _outgoingPorts.IndexOf(port), ID, port.PortType));
            }
            return outgoingConnections;
        }

        public virtual List<GAGen.Data.ConnectionData> GetIngoingConnectionIDs(bool isTrainMode)
        {
            if (_ingoingPorts == null)
                return null;

            List<Data.ConnectionData> ingoingConnections = new List<Data.ConnectionData>();
            foreach (GraphicalAssetPort port in _ingoingPorts)
            {
                Edge connectedEdge = port.GetConnectedEdge(isTrainMode);
                if (connectedEdge == null)
                {
                    ingoingConnections.Add(new Data.ConnectionData("EMPTY", 0, 0, ID, port.PortType));
                    continue;
                }
                GraphViewNode otherNode = connectedEdge.output.node as GraphViewNode;
                int indexOfOtherPort = otherNode.GetPorts(false).IndexOf(otherNode.GetPorts(false).Where(x => x.GetPort(isTrainMode) == connectedEdge.output).FirstOrDefault());
                ingoingConnections.Add(new Data.ConnectionData(otherNode.ID, indexOfOtherPort, _ingoingPorts.IndexOf(port), ID, port.PortType));
            }
            return ingoingConnections;
        }
    }
}

namespace GAGen.Graph
{
    public enum GANodeType
    {
        VoxelToMesh,
        PointCloudToMesh,
        MeshToVoxel,
        BitmapToVector,
        VectorToBitmap,
        Interpolator2D,
        Interpolator3D,
        Slicer,
        Parametriser,
        Grammar,
        StyleTransfer2D,
        StyleTransfer3D,
        MeshFromPhoto,
        MeshFromSketch,
        ImageFromText,
        TexturedMeshSplitter,
        TexturedMeshCombiner,
        Output,
        Labeller,
        ImageInput,
        TextInput,
        TexturedMeshInput,
        MeshInput,
        PointCloudInput,
        MeshGenerator,
        VoxelGenerator,
        NumberInput,
        ObjectDistributor
    }

    public enum GAPortType
    {
        Bitmap,
        VectorGraphic,
        Mesh,
        TexturedMesh,
        TexturedMeshSet,
        PointCloud,
        Voxel,
        Graph,
        Float,
        Integer,
        Boolean,
        Text
    }
}