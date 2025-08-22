using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using View;

namespace UnitBrains.Pathfinding
{
    public class DebugPathOutput : MonoBehaviour
    {
        [SerializeField] private GameObject cellHighlightPrefab;
        [SerializeField] private int maxHighlights = 15;

        public BaseUnitPath CurrentPath { get; private set; }

        private readonly List<GameObject> allHighlights = new();
        private Coroutine highlightCoroutine;
        private const float HighlightDelay = 0.1f;

        public void SetPath(BaseUnitPath path)
        {
            CurrentPath = path;

            while (allHighlights.Count > 0)
                DestroyHighlight(0);

            if (highlightCoroutine != null)
                StopCoroutine(highlightCoroutine);

            highlightCoroutine = StartCoroutine(HighlightCoroutine(path));
        }

        private IEnumerator HighlightCoroutine(BaseUnitPath path)
        {
            int counter = 0;
            Vector2Int currentCell = path.StartPoint;

            while (true)
            {
                Vector2Int nextCell = path.GetNextStepFrom(currentCell);

                currentCell = (Mathf.Abs(nextCell.x - path.EndPoint.x) <= 1 &&
                               Mathf.Abs(nextCell.y - path.EndPoint.y) <= 1 ||
                               counter == 0)
                    ? path.StartPoint
                    : nextCell;

                CreateHighlight(currentCell);
                counter++;

                if (counter > maxHighlights)
                    DestroyHighlight(0);

                yield return new WaitForSeconds(HighlightDelay);
            }
        }

        private void CreateHighlight(Vector2Int atCell)
        {
            var pos = Gameplay3dView.ToWorldPosition(atCell, 1f);
            var highlight = Instantiate(cellHighlightPrefab, pos, Quaternion.identity);
            highlight.transform.SetParent(transform);
            allHighlights.Add(highlight);
        }

        private void DestroyHighlight(int index)
        {
            Destroy(allHighlights[index]);
            allHighlights.RemoveAt(index);
        }
    }
}
