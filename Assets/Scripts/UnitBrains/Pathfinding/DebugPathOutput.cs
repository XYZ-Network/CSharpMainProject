using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
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

            DestroyAll();

            highlightCoroutine = StartCoroutine(HighlightCoroutine(path));
        }

        private IEnumerator HighlightCoroutine(BaseUnitPath path)
        {
            var steps = Path.GetPath().ToArray();
            int currentMaxHighlights = Math.Min(maxHighlights, Math.Max(1, steps.Length - 1));
            int counter = 0;

            while (true)
            {
                if (allHighlights.Count >= currentMaxHighlights)
                    DestroyHighlight(0);

                CreateHighlight(steps[counter]);

                counter++;
                if (counter >= steps.Length)
                    counter = 0;

                yield return new WaitForSeconds(0.15f);
            }
        }

        private void CreateHighlight(Vector2Int atCell)
        {
            var pos = Gameplay3dView.ToWorldPosition(atCell, 1f);
            var highlight = Instantiate(cellHighlightPrefab, pos, Quaternion.identity);
            //highlight.transform.SetParent(transform);
            allHighlights.Add(highlight);
        }

        private void DestroyHighlight(int index)
        {
            Destroy(allHighlights[index]);
            allHighlights.RemoveAt(index);
        }

        private void DestroyAll()
        {
            while (allHighlights.Count > 0)
            {
                DestroyHighlight(0);
            }

            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }
        }

        private void OnDisable()
        {
            DestroyAll();
        }

        private void OnDestroy()
        {
            DestroyAll();
        }
    }
}