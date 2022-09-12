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
    public class ImageInputNode : InputNode
    {
        TextField textField;
        VisualElement preview;
        bool _isBatchInput;
        int _chosenInputTypeIndex = 0;
        List<string> _inputTypeChoices = new List<string>() { "Single", "Batch", "Plain-Text" };
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.ImageInput;
            base.Initialise(position);
            NodeName = "Image Input";
            _outputPortType = GAPortType.Bitmap;
            styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath("Packages/com.gagen.core/Editor/Assets/UIStyles/GraphicalAssetInputNodeConnectorStyle.uss", typeof(StyleSheet)));
        }

        public override void Draw()
        {
            base.Draw();

            Label textFieldLabel = new Label()
            {
                text = "Path:"
            };
            outputContainer.Insert(0, textFieldLabel);

            textField = new TextField();
            textField.SetEnabled(false);
            _inputPaths.Add(textField);
            outputContainer.Insert(1, textField);

            Button openPathPickerButton = new Button();
            openPathPickerButton.text = "...";
            openPathPickerButton.clicked += OpenPathPicker;
            outputContainer.Insert(2, openPathPickerButton);

            PopupField<string> popupField = new PopupField<string>(_inputTypeChoices, "Single");
            popupField.RegisterValueChangedCallback(x => SetInputType(popupField.value));
            outputContainer.Insert(3, popupField);

            preview = new VisualElement();
            extensionContainer.Add(preview);

            RefreshExpandedState();
        }

        public void SetInputType(string value)
        {
            textField.value = "";
            _chosenInputTypeIndex = _inputTypeChoices.IndexOf(value);
            if (value == "Single")
            {
                _isBatchInput = false;
                return;
            }
            if(value == "Plain-Text")
            {

                return;
            }
            _isBatchInput = true;
            CallSettingsEditEvent();
        }

        public void OpenPathPicker()
        {
            if (textField == null)
                return;

            if (_isBatchInput)
                textField.value = EditorUtility.OpenFolderPanel("Data Source", "Assets", "");
            else
                textField.value = EditorUtility.OpenFilePanel("Data Source", "Assets", "");

            textField.tooltip = textField.value;
            //if (preview != null)
            //{
            //    preview.style.backgroundImage = new StyleBackground(AssetPreview.GetAssetPreview(AssetDatabase.LoadMainAssetAtPath(textField.value)));
            //}
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting settings = base.GetSettings();
            settings.i_pathOrPlainText = textField.value;
            settings.i_chosenInputMode = _chosenInputTypeIndex;
            return settings;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            base.LoadSettings(setting);
            _chosenInputTypeIndex = setting.i_chosenInputMode;
            textField.value = setting.i_pathOrPlainText;
            textField.tooltip = setting.i_pathOrPlainText;
        }


    }
}