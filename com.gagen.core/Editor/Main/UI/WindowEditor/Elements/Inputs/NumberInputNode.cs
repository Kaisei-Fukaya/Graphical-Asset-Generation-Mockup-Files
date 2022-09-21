using GAGen.Data;
using GAGen.Data.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAGen.Graph.Elements
{
    public class NumberInputNode : GraphViewNode
    {
        IntegerField _intField;
        VisualElement _preview;
        GAPortType _outputPortType;
        bool _isBatchInput;
        protected List<string> _inputTypes = new List<string>(){
        "Provide Value",
        "Randomised"
        };
        int _chosenInputTypeIndex = 0;
        int _intValue = 0;
        int _minValue = 0;
        int _maxValue = 0;
        protected GraphicalAssetPort _outputPort;

        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.NumberInput;
            base.Initialise(position);
            NodeName = "Number Input";
            _outputPortType = GAPortType.Integer;
            //styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetInputNodeConnectorStyle.uss", typeof(StyleSheet)));
        }

        public override void Draw()
        {
            base.Draw();

            _outgoingPorts = new List<GraphicalAssetPort>();

            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            _outgoingPorts.Add(_outputPort);
            //_outputPort.portName = _outputPortType.ToString();
            outputContainer.Add(_outputPort);

            SetInputType(_inputTypes[_chosenInputTypeIndex]);

            RefreshExpandedState();
        }

        public void SetInputType(string value)
        {
            extensionContainer.Clear();

            _chosenInputTypeIndex = _inputTypes.IndexOf(value);

            PopupField<string> inputTypeDropdown = new PopupField<string>(_inputTypes, _chosenInputTypeIndex);
            inputTypeDropdown.RegisterValueChangedCallback(x => { SetInputType(x.newValue); CallSettingsEditEvent(); });
            extensionContainer.Add(inputTypeDropdown);

            if (value == "Provide Value")
            {
                _intField = new IntegerField();
                _intField.value = _intValue;
                _intField.RegisterValueChangedCallback(x => { _intValue = x.newValue; });
                _intField.RegisterCallback<BlurEvent>(x => CallSettingsEditEvent());
                extensionContainer.Add(_intField);
                return;
            }
            VisualElement minGroup = new VisualElement();
            VisualElement maxGroup = new VisualElement();
            Label minLabel = new Label("Minimum Value");
            Label maxLabel = new Label("Maximum Value");
            IntegerField minField = new IntegerField();
            IntegerField maxField = new IntegerField();

            minField.value = _minValue;
            maxField.value = _maxValue;
            minField.RegisterValueChangedCallback(x => { _minValue = x.newValue; });
            maxField.RegisterValueChangedCallback(x => { _maxValue = x.newValue; });
            minField.RegisterCallback<BlurEvent>(x => CallSettingsEditEvent());
            maxField.RegisterCallback<BlurEvent>(x => CallSettingsEditEvent());
            minGroup.Add(minLabel);
            minGroup.Add(minField);
            maxGroup.Add(maxLabel);
            maxGroup.Add(maxField);

            extensionContainer.Add(minGroup);
            extensionContainer.Add(maxGroup);
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting setting = base.GetSettings();
            setting.n_min = _minValue;
            setting.n_max = _maxValue;
            setting.n_val = _intValue;
            setting.i_chosenInputMode = _chosenInputTypeIndex;
            return setting;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            base.LoadSettings(setting);
            _minValue = setting.n_min;
            _maxValue = setting.n_max;
            _intValue = setting.n_val;
            _chosenInputTypeIndex = setting.i_chosenInputMode;
        }
    }
}