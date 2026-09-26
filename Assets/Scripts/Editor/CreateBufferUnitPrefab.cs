using System.IO;
using Model.Config;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    public static class CreateBufferUnitPrefab
    {
        private const string PlayerUnitsPath = "Assets/Resources/PlayerUnits";
        private const string BufferName = "Buffer";
        private const int BufferCost = 150;

        [MenuItem("Tools/Units/Create Buffer Unit")]
        public static void Create()
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { PlayerUnitsPath });
            if (guids.Length == 0)
            {
                Debug.LogError($"No unit prefabs found in {PlayerUnitsPath}.");
                return;
            }

            GameObject source = null;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var candidate = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (candidate != null && candidate.name != BufferName && candidate.GetComponent<UnitConfig>() != null)
                {
                    source = candidate;
                    break;
                }
            }

            if (source == null)
            {
                Debug.LogError("Could not find a source player unit prefab.");
                return;
            }

            var destination = $"{PlayerUnitsPath}/{BufferName}.prefab";
            if (File.Exists(destination))
            {
                Debug.LogWarning($"Buffer prefab already exists: {destination}");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(destination);
                return;
            }

            var clone = Object.Instantiate(source);
            clone.name = BufferName;

            var config = clone.GetComponent<UnitConfig>();
            if (config == null)
            {
                Object.DestroyImmediate(clone);
                Debug.LogError("Source prefab has no UnitConfig.");
                return;
            }

            // Keep the duplicated unit's visual setup. Serialized fields that are
            // private are changed through SerializedObject so the asset is ready
            // for the existing Settings.LoadPrefabs() pipeline.
            var serializedConfig = new SerializedObject(config);
            serializedConfig.FindProperty("_name").stringValue = BufferName;
            serializedConfig.FindProperty("_cost").intValue = BufferCost;
            serializedConfig.FindProperty("_attackDelay").floatValue = 999999f;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(clone, destination);
            Object.DestroyImmediate(clone);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(destination);
            Debug.Log($"Created {destination}. The Buffer brain is selected automatically by its name.");
        }
    }
}
