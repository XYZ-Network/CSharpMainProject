using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using View;

namespace UnitBrains.Pathfinding
{
    public class DebugPathOutput : MonoBehaviour
    {
        [SerializeField] private GameObject cellHighlightPrefab;
        [SerializeField] private int maxHighlights = 5;

        public BaseUnitPath Path { get; private set; }
        private readonly List<GameObject> allHighlights = new();
        private Coroutine highlightCoroutine;

        public void HighlightPath(BaseUnitPath path)
        {
            Path = path;
            DestroyHighlight();

            highlightCoroutine = StartCoroutine(HighlightCoroutine(path));
        }

        private void OnDisable()
        {
            DestroyHighlight();
        }

        private IEnumerator HighlightCoroutine(BaseUnitPath path)
        {
            Vector2Int[] arrayPath = path.GetPath().ToArray();
            
            while (true)
            {
                for (int i = 0; i < arrayPath.Length; i++)
                {
                    CreateHighlight(arrayPath[i]);
                    yield return new WaitForSeconds(0.3f);
                    if (allHighlights.Count > this.maxHighlights)
                        DestroyHighlight(0);
                }
            }
        }

        private void CreateHighlight(Vector2Int atCell)
        {
            var pos = Gameplay3dView.ToWorldPosition(atCell, 1f);
            var highlight = Instantiate(cellHighlightPrefab, pos, Quaternion.identity);
            allHighlights.Add(highlight);
        }

        private void DestroyHighlight(int index)
        {
            Destroy(allHighlights[index]);
            allHighlights.RemoveAt(index);
        }

        public void DestroyHighlight()
        {
            while (allHighlights.Count > 0)
            {
                DestroyHighlight(0);
            }
        }
    }
}