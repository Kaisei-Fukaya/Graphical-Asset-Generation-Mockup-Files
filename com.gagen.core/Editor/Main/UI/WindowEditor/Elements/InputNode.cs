using GAGen.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
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
        protected TextField _textField;
        protected bool _isBatchInput;
        protected List<string> _inputTypeChoices = new List<string>() { "Single", "Batch" };
        protected int _chosenInputTypeIndex = 0;
        protected PopupField<string> _popupField;
        protected VisualElement _textFieldContainer;
        protected string _inputPath;
        public PopupField<string> GetPopupField()
        {
            return _popupField;
        }
        public override void Initialise(Vector2 position)
        {
            base.Initialise(position);
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting setting = base.GetSettings();
            setting.i_inputPaths = new List<string>();

            setting.i_inputPaths.Add(_inputPath);

            setting.i_chosenInputMode = _chosenInputTypeIndex;
            return setting;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            if (setting.i_inputPaths == null || setting.i_inputPaths.Count < _inputPaths.Count)
                return;

            for (int i = 0; i < _inputPaths.Count; i++)
            {
                _inputPath = setting.i_inputPaths[i];
                _textFieldContainer.tooltip = setting.i_inputPaths[i];
            }
            _chosenInputTypeIndex = setting.i_chosenInputMode;
        }

        public override void Draw()
        {
            base.Draw();
            _outgoingPorts = new List<GraphicalAssetPort>();
            _inputPaths = new List<TextField>();

            _outputPort = new GraphicalAssetPort(this, _outputPortType, Orientation.Horizontal, Direction.Output, Port.Capacity.Multi);
            _outgoingPorts.Add(_outputPort);
            //_outputPort.portName = _outputPortType.ToString();
            outputContainer.Add(_outputPort);

            Label textFieldLabel = new Label()
            {
                text = "Path:"
            };
            outputContainer.Insert(0, textFieldLabel);

            _textFieldContainer = new VisualElement();
            _textFieldContainer.tooltip = _inputPath;
            _textField = new TextField();
            _textField.value = _inputPath;
            _textField.SetEnabled(false);
            _inputPaths.Add(_textField);
            _textFieldContainer.Add(_textField);
            outputContainer.Insert(1, _textFieldContainer);

            Button openPathPickerButton = new Button();
            openPathPickerButton.text = "...";
            openPathPickerButton.clicked += OpenPathPicker;
            openPathPickerButton.tooltip = "Select a folder to pull input data from.";
            outputContainer.Insert(2, openPathPickerButton);

            _popupField = new PopupField<string>(_inputTypeChoices, _chosenInputTypeIndex);
            _popupField.RegisterValueChangedCallback(x => SetInputType(_popupField.value));
            _popupField.tooltip = "Choose the type of input.";
            if (_chosenInputTypeIndex == 1)
                _isBatchInput = true;
            outputContainer.Insert(3, _popupField);

            SetInputType(_inputTypeChoices[_chosenInputTypeIndex], true);

            RefreshExpandedState();
        }

        public bool IsForTraining()
        {
            return _forTraining;
        }
        public virtual void SetInputType(string value, bool suppressEditEvent = false)
        {
            if (!suppressEditEvent)
            {
                _inputPath = "";
                _textField.value = "";
            }
            _chosenInputTypeIndex = _inputTypeChoices.IndexOf(value);
            if (value == "Single")
            {
                _isBatchInput = false;
            }
            if(value == "Batch")
            {
                _isBatchInput = true;
            }
            if(!suppressEditEvent)
                CallSettingsEditEvent();
        }

        public void OpenPathPicker()
        {
            if (_textField == null)
                return;

            if (_isBatchInput)
                _textField.value = EditorUtility.OpenFolderPanel("Data Source", "Assets", "");
            else
                _textField.value = EditorUtility.OpenFilePanel("Data Source", "Assets", "");

            _textFieldContainer.tooltip = _textField.value;
            //if (preview != null)
            //{
            //    preview.style.backgroundImage = new StyleBackground(AssetPreview.GetAssetPreview(AssetDatabase.LoadMainAssetAtPath(textField.value)));
            //}
        }

    }
}