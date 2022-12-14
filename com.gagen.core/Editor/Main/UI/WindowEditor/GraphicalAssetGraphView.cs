using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Linq;
using UnityEditor;
using GAGen.Graph.Elements;
using GAGen.Data.Utils;

namespace GAGen.Graph
{
    public class GraphicalAssetGraphView : GraphView
    {
        GASearchWindow _searchWindow;
        public GraphicalAssetGeneratorWindow editorWindow;
        List<GraphViewNode> _nodes = new List<GraphViewNode>();
        public List<GraphViewNode> Nodes 
        { 
            get
            {
                return _nodes;
            } 
        }
        public GraphicalAssetGraphView(GraphicalAssetGeneratorWindow editorWindow)
        {
            this.editorWindow = editorWindow;
            graphViewChanged += OnGraphViewChanged;
            AddGridBackground();
            //AddToolWindow();
            //AddPreviewWindow();
            AddSearchWindow();
            AddStyles();
            AddManipulators();
            AddDefaultNodes();
        }

        private void AddSearchWindow()
        {
            if (_searchWindow == null)
            {
                _searchWindow = ScriptableObject.CreateInstance<GASearchWindow>();
                _searchWindow.Initialise(this);

            }
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        public GraphViewNode CreateNode(GANodeType type, Vector2 position)
        {
            Type nodeType = Type.GetType($"GAGen.Graph.Elements.{type}Node");
            GraphViewNode node = (GraphViewNode)Activator.CreateInstance(nodeType);
            node.GraphView = this;
            node.Initialise(position);
            node.Draw();
            _nodes.Add(node);
            return node;
        }

        void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            //this.AddManipulator(CreateNodeContextualManipulator("Add Node (MarchingCubes)", GANodeType.VoxelToMesh));
        }

        private IManipulator CreateNodeContextualManipulator(string actionTitle, GANodeType type)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(type, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );

            return contextualMenuManipulator;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                    return;
                if (startPort.node == port.node)
                    return;
                if (startPort.direction == port.direction)
                    return;

                GAPortData startPortData = (GAPortData)startPort.userData;
                GAPortData portData = (GAPortData)port.userData;
                if (startPortData.PortModeType != portData.PortModeType)
                    return;

                if (startPortData.PortType == portData.PortType)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        void AddDefaultNodes()
        {
            AddElement(CreateNode(GANodeType.Output, 
                new Vector2(500f, 
                    500f
                    )
                ));
            EditorApplication.delayCall += CentreGraphOnNodes;
        }

        public void CentreGraphOnNodes()
        {
            Vector2 averagePos = Vector2.zero;
            for (int i = 0; i < Nodes.Count; i++)
            {
                Rect pos = Nodes[i].GetPosition();
                averagePos += pos.center;
            }
            averagePos = averagePos / Nodes.Count;
            //Add dif between averagePos and centre to contentviewContainer
            Vector2 center = this.WorldToLocal(this.worldBound.center);
            Vector3 dif = center - averagePos;
            if (float.IsNaN(dif.x) || float.IsNaN(dif.y) || float.IsNaN(dif.z))
            {
                EditorApplication.delayCall += CentreGraphOnNodes;
            }
            else
            {
                contentViewContainer.transform.position = dif;
            }
        }

        void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddToolWindow()
        {
            Vector2 blackBoardPosition = new Vector2(0f, 0f);
            Blackboard blackboardWindow = new Blackboard(this)
            {
                title = "Variables"
            };
            Insert(1, blackboardWindow);
        }

        private void AddPreviewWindow()
        {
            GAPreview previewWindow = new GAPreview();
            previewWindow.Initialise(this);
            previewWindow.name = "previewWindow";
            Insert(1, previewWindow);
        }

        public GraphViewChange OnGraphViewChanged(GraphViewChange gvc)
        {
            if (gvc.edgesToCreate != null)
            {
                foreach (Edge edge in gvc.edgesToCreate)
                {
                    edge.styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetGeneratorEdgeStyle.uss", typeof(StyleSheet)));
                    if (editorWindow.inTrainingMode)
                        edge.AddToClassList("train-edge");
                    else
                        edge.AddToClassList("generate-edge");
                }
            }

            if (gvc.elementsToRemove != null)
            {
                List<GraphViewNode> nodesToRemove = gvc.elementsToRemove.OfType<GraphViewNode>().ToList();
                foreach (GraphViewNode element in nodesToRemove)
                {
                    element.DisconnectAllPorts();
                    RemoveElement(element);
                    Nodes.Remove(element);
                }
            }

            return gvc;
        }

        void AddStyles()
        {
            StyleSheet styleSheet = (StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphViewStyle.uss", typeof(StyleSheet));
            styleSheets.Add(styleSheet);
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldPosition = mousePosition;

            if (isSearchWindow)
            {
                worldPosition -= editorWindow.position.position;
            }

            Vector2 localPosition = contentViewContainer.WorldToLocal(worldPosition);
            return localPosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));
            _nodes = new List<GraphViewNode>();
        }
    }
}