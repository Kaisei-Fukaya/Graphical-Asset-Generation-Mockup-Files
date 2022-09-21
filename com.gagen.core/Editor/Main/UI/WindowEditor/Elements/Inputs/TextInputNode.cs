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
    public class TextInputNode : InputNode
    {
        string _plainText = "";
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.TextInput;
            base.Initialise(position);
            NodeName = "Text Input";
            _outputPortType = GAPortType.Text;
            _inputTypeChoices = new List<string>() { "Single", "Batch", "Plain-Text" };
            styleSheets.Add((StyleSheet)AssetDatabase.LoadAssetAtPath($"{GAGenDataUtils.BasePath}Editor/Assets/UIStyles/GraphicalAssetInputNodeConnectorStyle.uss", typeof(StyleSheet)));
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void SetInputType(string value, bool suppressEditEvent = false)
        {
            base.SetInputType(value, true);
            if (!suppressEditEvent)
            {
                _inputPath = "";
                _textField.value = "";
            }

            extensionContainer.Clear();
            if (value == "Plain-Text")
            {
                _isBatchInput = false;
                TextField plainTextField = new TextField();
                plainTextField.AddToClassList("text-field");
                plainTextField.value = _plainText;
                plainTextField.multiline = true;
                plainTextField.RegisterValueChangedCallback(x => { _plainText = x.newValue; });
                plainTextField.RegisterCallback<BlurEvent>(x => CallSettingsEditEvent());
                extensionContainer.Add(plainTextField);
            }
            RefreshExpandedState();
            if(!suppressEditEvent)
                CallSettingsEditEvent();
        }

        public override void LoadSettings(NodeSetting setting)
        {
            base.LoadSettings(setting);
            _plainText = setting.i_plainText;
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting settings = base.GetSettings();
            settings.i_plainText = _plainText;
            return settings;
        }
    }
}