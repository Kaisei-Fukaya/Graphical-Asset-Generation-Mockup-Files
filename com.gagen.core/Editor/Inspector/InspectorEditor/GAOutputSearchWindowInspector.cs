using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using GAGen.Graph;
using GAGen.Data.Utils;

namespace GAGen.Inspector
{
    public class GAOutputSearchWindowInspector : GASearchWindow
    {
        GraphicalAssetGeneratorInspector _inspector;
        bool _isOutput;
        string[] _foldersToUse;
        public void Initialise(GraphicalAssetGeneratorInspector inspector)
        {
            //_graphView = graphView;
            _inspector = inspector;
        }
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Add New Output"))
            };

            foreach (GAPortType type in Enum.GetValues(typeof(GAPortType)))
            {
                searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(type.ToString()))
                {
                    level = 1,
                    userData = type
                });
            }

            return searchTreeEntries;
        }

        public override bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if (SearchTreeEntry.userData is GAPortType)
            {
                _inspector.AddNewOutput((GAPortType)SearchTreeEntry.userData);
            }
            return true;
        }
    }
}