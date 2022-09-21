using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GAGen.Graph;
using System.Linq;
using System.IO;

namespace GAGen.Data.Utils
{
    public static class GAGenDataUtils
    {
        public static Dictionary<GANodeType, string> DisplayNameLookup = new Dictionary<GANodeType, string>
        {
            { GANodeType.ImageInput,            "Image Input" },
            { GANodeType.TextInput,             "Text Input" },
            { GANodeType.TexturedMeshInput,     "Textured Mesh Input" },
            { GANodeType.MeshInput,             "Mesh Input" },
            { GANodeType.NumberInput,           "Number Input" },
            { GANodeType.Grammar,               "Grammar" },
            { GANodeType.MeshFromPhoto,         "Mesh from Photo" },
            { GANodeType.MeshFromSketch,        "Mesh from Sketch" },
            { GANodeType.Interpolator3D,        "Interpolator" },
            { GANodeType.Labeller,              "Labeller" },
            { GANodeType.VoxelToMesh,           "Voxel to Mesh" },
            { GANodeType.MeshGenerator,         "Mesh Generator" },
            { GANodeType.Parametriser,          "Parametriser" },
            { GANodeType.PointCloudToMesh,      "Point-cloud to Mesh" },
            { GANodeType.Slicer,                "Slicer" },
            { GANodeType.StyleTransfer2D,       "2D Style Transfer" },
            { GANodeType.StyleTransfer3D,       "3D Style Transfer" },
            { GANodeType.ImageFromText,         "Image from Text" },
            { GANodeType.TexturedMeshSplitter,  "Textured Mesh Splitter" },
            { GANodeType.TexturedMeshCombiner,  "Textured Mesh Combiner" },
            { GANodeType.BitmapToVector,        "Bitmap to Vector" },
            { GANodeType.VectorToBitmap,        "Vector to Bitmap" },
            { GANodeType.VoxelGenerator,        "Voxel Generator" },
            { GANodeType.MeshToVoxel,           "Mesh to Voxel" },
            { GANodeType.Output,                "Output" },
        };
        public static GAGenNodeData GraphNodeToNodeData(GraphViewNode node)
        {
            return new GAGenNodeData()
            {
                ID = node.ID,
                TrainConnections = node.GetIngoingConnectionIDs(true),
                GenConnections = node.GetIngoingConnectionIDs(false),
                Position = node.GetPosition().position,
                NodeType = node.NodeType,
                AdditionalSettings = node.GetSettings()
            };
        }

        public static string BasePath
        {
            get
            {
                //"Packages/com.gagen.core/"
                return "Assets/com.gagen.core/";
            }
        }

        public static GANodeType GetNodeTypeFromName(string name)
        {
            if (Enum.GetNames(typeof(GANodeType)).Contains(name))
            {
                return (GANodeType)Enum.Parse(typeof(GANodeType), name);
            }
            throw new System.Exception($"GANodeType of {name} does not exist!");
        }

        public static string[] GetFileNames(string folderPath)
        {
            string[] paths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            string[] names = new string[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                //Remove rest of path to get name
                names[i] = paths[i].Replace(folderPath, "").Replace("Node", "").Split('.')[0];
            }
            names = names.Distinct().ToArray();
            return names;
        }

        public static string[] GetFolderPaths(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
        }
        public static string CleanFileName(string fileName)
        {
            string cleanName;
            bool containsException = false;

            if (fileName.Contains("2D") || fileName.Contains("3D"))
                containsException = true;

            //Replace 2 with To
            cleanName = string.Concat(fileName.Select(x => x == '2' ? "To" : x.ToString()));
            //Add spaces before caps
            cleanName = string.Concat(cleanName.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

            if (containsException)
            {
                if (cleanName.Contains("To D"))
                    cleanName = cleanName.Replace("To D", "2D");
                if (cleanName.Contains("3 D"))
                    cleanName = cleanName.Replace("3 D", " 3D");
            }

            return cleanName;
        }
    }
}