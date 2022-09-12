using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAGen.Graph.Elements
{
    public class TextInputNode : InputNode
    {
        TextField _textField;
        VisualElement _preview;
        bool _isBatchInput;
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.TextInput;
            base.Initialise(position);
            NodeName = "Text Input";
            _outputPortType = GAPortType.Text;
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

            _textField = new TextField();
            _inputPaths.Add(_textField);
            outputContainer.Insert(1, _textField);

            Button openPathPickerButton = new Button();
            openPathPickerButton.text = "...";
            openPathPickerButton.clicked += OpenPathPicker;
            outputContainer.Insert(2, openPathPickerButton);

            PopupField<string> popupField = new PopupField<string>(new List<string>() { "Single", "Batch" }, "Single");
            popupField.RegisterValueChangedCallback(x => SetInputType(popupField.value));
            outputContainer.Insert(3, popupField);

            _preview = new VisualElement();
            extensionContainer.Add(_preview);

            RefreshExpandedState();
        }

        public void SetInputType(string value)
        {
            _textField.value = "";
            if (value == "Single")
            {
                _isBatchInput = false;
                return;
            }
            _isBatchInput = true;
        }

        public void OpenPathPicker()
        {
            if (_textField == null)
                return;

            if (_isBatchInput)
                _textField.value = EditorUtility.OpenFolderPanel("Data Source", "Assets", "");
            else
                _textField.value = EditorUtility.OpenFilePanel("Data Source", "Assets", "");

            //if (preview != null)
            //{
            //    preview.style.backgroundImage = new StyleBackground(AssetPreview.GetAssetPreview(AssetDatabase.LoadMainAssetAtPath(textField.value)));
            //}
        }
    }
}