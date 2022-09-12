using GAGen.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAGen.Graph.Elements
{
    public class InputNode : GraphViewNode
    {
        protected GraphicalAssetPort _outputPort;
        protected GAPortType _outputPortType;
        protected bool _forTraining;
        protected List<TextField> _inputPaths = new List<TextField>();
        public override void Initialise(Vector2 position)
        {
            base.Initialise(position);
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting setting = base.GetSettings();
            setting.i_inputPaths = new List<string>();

            foreach (TextField field in _inputPaths)
            {
                setting.i_inputPaths.Add(field.value);
            }
            return setting;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            if (setting.i_inputPaths == null || setting.i_inputPaths.Count < _inputPaths.Count)
                return;
            for (int i = 0; i < _inputPaths.Count; i++)
            {
                _inputPaths[i].value = setting.i_inputPaths[0];
            }
        }

        public override void Draw()
        {
            base.Draw();
            _outgoingPorts = new List<GraphicalAssetPort>();

            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
            _outgoingPorts.Add(_outputPort);
            //_outputPort.portName = _outputPortType.ToString();
            outputContainer.Add(_outputPort);

            RefreshExpandedState();
        }

        public bool IsForTraining()
        {
            return _forTraining;
        }
    }
}