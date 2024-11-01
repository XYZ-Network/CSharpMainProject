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
                DestroyHighlight(0);
            }
            
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }

            highlightCoroutine = StartCoroutine(HighlightCoroutine(path));
        }

        private IEnumerator HighlightCoroutine(BaseUnitPath path)
        {
            var vectorPath = path.GetPath();
            bool drawn = false;
            while(true) {
                foreach (var cell in vectorPath)
                {
                    if (!drawn)
                    {
                        CreateHighlight(cell);
                    }
                    else
                    {
                        CreateHighlight(cell);
                        DestroyHighlight(0);
                        yield return new WaitForSeconds(0.15f);
                    }
                    if (allHighlights.Count >= maxHighlights)
                    {
                        drawn = true;
                    }

                }
                drawn = false;
                while (allHighlights.Count > 0)
                {
                    DestroyHighlight(0);
                    yield return new WaitForSeconds(0.15f);
                }
            }
        }

        private void OnDestroy()
        {
            while (allHighlights.Count > 0)
            {
                DestroyHighlight(0);
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
    }
}