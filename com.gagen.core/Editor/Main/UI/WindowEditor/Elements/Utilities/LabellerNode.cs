using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using GAGen.Data.Utils;
using GAGen.Data;

namespace GAGen.Graph.Elements
{
    public class LabellerNode : ConverterNode
    {
        TextField _textField;
        PopupField<GAPortType> _popupField;
        string _textValue;

        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.Labeller;
            base.Initialise(position);
            _inputPortType = GAPortType.Bitmap;
            _outputPortType = GAPortType.Bitmap;
            styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetLabellerStyle.uss", typeof(StyleSheet)));
        }

        public override void Draw()
        {
            base.Draw();

            DrawInputPort();

            Label pathTitle = new Label() { text = "Label data" };
            extensionContainer.Add(pathTitle);

            _textField = new TextField();
            _textField.value = _textValue;
            _textField.RegisterValueChangedCallback(x => { _textValue = x.newValue; CallSettingsEditEvent(); });
            _textField.SetEnabled(false);
            extensionContainer.Add(_textField);

            Button openPathPickerButton = new Button();
            openPathPickerButton.text = "...";
            openPathPickerButton.clicked += OpenPathPicker;
            extensionContainer.Add(openPathPickerButton);

            RefreshExpandedState();

        }

        void DrawInputPort()
        {
            if (_popupField != null && inputContainer.Contains(_popupField))
                inputContainer.Remove(_popupField);

            //_inputPort.portName = _inputPortType.ToString();
            //_outputPort.portName = _outputPortType.ToString();

            List<GAPortType> popupOptions = new List<GAPortType>();
            var ptValues = Enum.GetValues(typeof(GAPortType));
            for (int i = 0; i < ptValues.Length; i++)
            {
                GAPortType x = (GAPortType)ptValues.GetValue(i);
                popupOptions.Add(x);
            }
            _popupField = new PopupField<GAPortType>(popupOptions, _inputPortType);
            _popupField.RegisterValueChangedCallback(x => { UpdatePort(_popupField.value); CallSettingsEditEvent(); });
            inputContainer.Insert(1, _popupField);
        }

        void UpdatePort(GAPortType value)
        {
            GraphView.DeleteElements(_inputPort.Connections(true));
            GraphView.DeleteElements(_inputPort.Connections(false));
            _inputPort.DisconnectAll();
            _inputPortType = value;
            GraphView.DeleteElements(_outputPort.Connections(true));
            GraphView.DeleteElements(_outputPort.Connections(false));
            _outputPort.DisconnectAll();
            _outputPortType = value;
            Draw();
        }

        public void OpenPathPicker()
        {
            if (_textField == null)
                return;

            _textField.value = EditorUtility.OpenFilePanel("Label Data Source", "Assets", "");

            CallSettingsEditEvent();
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting setting = base.GetSettings();
            setting.l_selectedPortType = _inputPortType;
            setting.i_plainText = _textValue;
            return setting;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            base.LoadSettings(setting);
            _inputPortType = setting.l_selectedPortType;
            _textValue = setting.i_plainText;
        }
    }
}