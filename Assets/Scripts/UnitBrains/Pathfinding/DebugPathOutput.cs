using System.Collections;
using System.Collections.Generic;
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
            while (allHighlights.Count > 0)
            {
                DestroyHighlight(allHighlights[0]);
            }

            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }

            highlightCoroutine = StartCoroutine(HighlightCoroutine(path));
        }

        private IEnumerator HighlightCoroutine(BaseUnitPath path)
        {
            foreach (Vector2Int cell in path.GetPath())
            {
                var highlight = CreateHighlight(cell);
                StartCoroutine(DestroyHighlightAfter(highlight, 0.5f));

                yield return new WaitForSecondsRealtime(0.1f);
            }                
        }

        private IEnumerator DestroyHighlightAfter(GameObject highlight, float delaySec)
        {
            yield return new WaitForSecondsRealtime(delaySec);
            DestroyHighlight(highlight);
        }

        private GameObject CreateHighlight(Vector2Int atCell)
        {
            var pos = Gameplay3dView.ToWorldPosition(atCell, 1f);
            var highlight = Instantiate(cellHighlightPrefab, pos, Quaternion.identity);
            highlight.transform.SetParent(transform);
            allHighlights.Add(highlight);
            return highlight;
        }

        private void DestroyHighlight(GameObject highlight)
        {
            Destroy(highlight);
            allHighlights.Remove(highlight);
        }
    }
}