using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
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
            // TODO Implement me
            IEnumerable<Vector2Int> targetForHiglight = path.GetPath();
            foreach (var target in targetForHiglight)
            {
                CreateHighlight(target);
                if (allHighlights.Count > maxHighlights)
                {
                    DestroyHighlight(0);
                }
                yield return new WaitForSeconds(0.08f);
            }
            while (allHighlights.Count > 0)
            {
                DestroyHighlight(0);
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void CreateHighlight(Vector2Int atCell)
        {
            var pos = Gameplay3dView.ToWorldPosition(atCell, 0.5f);
            var highlight = Instantiate(cellHighlightPrefab, pos, Quaternion.identity);
            //highlight.transform.SetParent(transform);
            allHighlights.Add(highlight);
        }

        private void DestroyHighlight(int index)
        {
            Destroy(allHighlights[index]);
            allHighlights.RemoveAt(index);
        }

        private void OnDestroy()
        {
            while (allHighlights.Count > 0)
            {
                DestroyHighlight(0);
            }
        }
    }
}