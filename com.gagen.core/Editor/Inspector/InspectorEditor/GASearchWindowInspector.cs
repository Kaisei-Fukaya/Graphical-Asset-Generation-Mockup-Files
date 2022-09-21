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
    public class GASearchWindowInspector : GASearchWindow
    {
        GAGenDataInspector _inspector;
        bool _isOutput;
        string[] _foldersToUse;
        public void Initialise(GAGenDataInspector inspector, string[] foldersToUse, bool isOutput = false)
        {
            //_graphView = graphView;
            _inspector = inspector;
            _foldersToUse = foldersToUse;
            _isOutput = isOutput;
        }
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element"))
            };

            if (!_isOutput)
            {
                string sourcePath = $"{GAGenDataUtils.BasePath}Editor/Main/UI/WindowEditor/Elements/";
                string[] allFolders = GAGenDataUtils.GetFolderPaths(sourcePath);
                foreach (string folder in allFolders)
                {
                    string[] fileNames = GAGenDataUtils.GetFileNames(folder + "/");
                    string folderName = folder.Replace(sourcePath, "");
                    if (_foldersToUse.Contains(folderName))
                    {
                        searchTreeEntries.Add(new SearchTreeGroupEntry(new GUIContent(folderName), 1));
                        foreach (string fileName in fileNames)
                        {
                            if (GAGenDataUtils.GetNodeTypeFromName(fileName) == GANodeType.Labeller)
                                continue;
                            searchTreeEntries.Add(new SearchTreeEntry(new GUIContent(GAGenDataUtils.CleanFileName(fileName)))
                            {
                                level = 2,
                                userData = GAGenDataUtils.GetNodeTypeFromName(fileName)
                            });
                        }
                    }
                }
                return searchTreeEntries;
            }

            //For output listings
            return searchTreeEntries;
        }

        public override bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if (SearchTreeEntry.userData is GANodeType)
            {
                //Bad but quick solution
                if (_foldersToUse.Contains("Inputs"))
                {
                    _inspector.AddNewNode((GANodeType)SearchTreeEntry.userData, 0);
                }
                if (_foldersToUse.Contains("Generators"))
                {
                    _inspector.AddNewNode((GANodeType)SearchTreeEntry.userData, 1);
                }
                if (_foldersToUse.Contains("Converters"))
                {
                    _inspector.AddNewNode((GANodeType)SearchTreeEntry.userData, 2);
                }
                if (_isOutput)
                {
                    //_inspector.AddNewNode((GANodeType)SearchTreeEntry.userData, 3);
                }
            }
            return true;
        }
    }
}