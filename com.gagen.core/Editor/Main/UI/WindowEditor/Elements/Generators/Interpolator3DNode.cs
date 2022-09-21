using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using GAGen.Data;

namespace GAGen.Graph.Elements
{
    public class Interpolator3DNode : GraphViewNode
    {
        protected GraphicalAssetPort _outputPort, _inputPortA;
        protected GAPortType _outputPortType;
        protected List<string> _models = new List<string>(){
            "Tree",
            "Face",
            "Furniture"
        };
        protected int _chosenModelIndex;

        protected float _sliderValue = .5f;

        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.Interpolator3D;
            base.Initialise(position);
            NodeName = "Interpolator 3D";
            NodeDescription = "This node interpolates between to given meshes.";
            _inputPortType = GAPortType.TexturedMesh;
            _outputPortType = GAPortType.TexturedMesh;
        }

        public override void Draw()
        {
            base.Draw();

            _ingoingPorts = new List<GraphicalAssetPort>();
            _outgoingPorts = new List<GraphicalAssetPort>();

            VisualElement modelSelector = new VisualElement();
            Label modelSelectorLabel = new Label("Model");
            PopupField<string> modelSelectorDropdown = new PopupField<string>(_models, _chosenModelIndex);
            modelSelectorDropdown.RegisterValueChangedCallback(x => { _chosenModelIndex = _models.IndexOf(x.newValue); CallSettingsEditEvent(); });
            modelSelector.Add(modelSelectorLabel);
            modelSelector.Add(modelSelectorDropdown);
            extensionContainer.Add(modelSelector);

            VisualElement sliderGroup = new VisualElement();
            sliderGroup.AddToClassList("slider-group");
            Slider interpSlider = new Slider
            {
                lowValue = 0,
                highValue = 1,
                value = _sliderValue
            };
            interpSlider.RegisterCallback<ClickEvent>(x => OnSliderChanged(interpSlider.value));
            interpSlider.tooltip = "How similar the result is to input A or input B. In the middle by default.";
            Label sliderLabelA = new Label("Input A");
            Label sliderLabelB = new Label("Input B");
            sliderGroup.Add(sliderLabelA);
            sliderGroup.Add(interpSlider);
            sliderGroup.Add(sliderLabelB);
            extensionContainer.Add(sliderGroup);



            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            _outgoingPorts.Add(_outputPort);
            outputContainer.Add(_outputPort);

            _inputPort = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, "Input A");
            _ingoingPorts.Add(_inputPort);
            inputContainer.Add(_inputPort);

            _inputPortA = new GraphicalAssetPort(this, _inputPortType, Orientation.Horizontal, Direction.Input, Port.Capacity.Single, "Input B");
            _ingoingPorts.Add(_inputPortA);
            inputContainer.Add(_inputPortA);

            RefreshExpandedState();
        }

        void OnSliderChanged(float value)
        {
            _sliderValue = value;
            CallSettingsEditEvent();
        }

        public override void LoadSettings(NodeSetting setting)
        {
            _sliderValue = setting.slider1;
            _chosenModelIndex = setting.i2m_dropdownOpt2;
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting settings = base.GetSettings();
            settings.slider1 = _sliderValue;
            settings.i2m_dropdownOpt2 = _chosenModelIndex;
            return settings;
        }


        public void UpdateUI()
        {

        }

    }
}