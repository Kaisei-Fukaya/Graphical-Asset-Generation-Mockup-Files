using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;
using GAGen.Data;

namespace GAGen.Graph.Elements
{
    public class SketchToMeshNode : PhotoToMeshNode
    {
        public override void Initialise(Vector2 position)
        {
            NodeType = GANodeType.SketchToMesh;
            base.Initialise(position);
            NodeName = "Sketch to Mesh";
            _inputPortType = GAPortType.Bitmap;
            _outputPortType = GAPortType.Mesh;
            _models = new List<string>(){
                "Buildings",
                "Furniture",
                "Vehicles",
                "Other object (Requires training data to fine-tune)"
            };
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void OnModelSelected(string model)
        {
            if (model == _models[0])
            {

            }
            if (model == _models[1])
            {

            }
            if (model == _models[2])
            {

            }
            if (model == _models[3])
            {

            }
            CallSettingsEditEvent();
        }

        public override NodeSetting GetSettings()
        {
            NodeSetting settings = base.GetSettings();
            settings.i2m_dropdownOpt1 = _chosenInputTypeIndex;
            return settings;
        }

        public override void LoadSettings(NodeSetting setting)
        {
            _chosenInputTypeIndex = setting.i2m_dropdownOpt1;
        }
    }
}