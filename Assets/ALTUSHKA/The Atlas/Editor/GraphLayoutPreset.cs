using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ALTUSHKA.TheAtlas
{
    [CreateAssetMenu(fileName = "TheAtlasPreset", menuName = "The Atlas/Layout Preset")]
    public class GraphLayoutPreset : ScriptableObject
    {
        [SerializeField] private List<NodePos> _positions = new List<NodePos>();
        [SerializeField] private List<BoxData> _boxes = new List<BoxData>();
        [SerializeField] private List<GroupData> _groups = new List<GroupData>();

        public DefaultAsset LinkedFolder;

        [Serializable]
        private struct NodePos
        {
            public string Id;
            public Vector2 Pos;
        }

        public void UpdateData(Dictionary<string, Vector2> positions, List<BoxData> boxes, List<GroupData> groups, DefaultAsset folder)
        {
            _positions.Clear();
            foreach (var kvp in positions)
                _positions.Add(new NodePos { Id = kvp.Key, Pos = kvp.Value });

            _boxes = new List<BoxData>(boxes);
            _groups = new List<GroupData>(groups);
            LinkedFolder = folder;
        }

        public Dictionary<string, Vector2> GetPositions()
        {
            var dict = new Dictionary<string, Vector2>();
            foreach (var np in _positions) dict[np.Id] = np.Pos;
            return dict;
        }

        public List<BoxData> Boxes => _boxes;
        public List<GroupData> Groups => _groups;
    }

    [Serializable]
    public struct BoxData
    {
        public string Title;
        public Vector2 Position;
        public List<string> ContainedTypes;
    }

    [Serializable]
    public struct GroupData
    {
        public string Title;
        public Vector2 Position;
        public List<string> ContainedNodeIds;
    }
}