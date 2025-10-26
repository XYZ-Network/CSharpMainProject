using log4net.Util;
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
        private BaseUnitPath _pendingPath; 
        private bool _isRunning = false;

        public void HighlightPath(BaseUnitPath path)
        {
            if (_isRunning)
            {
                _pendingPath = path; 
                return;
            }
            StartHighlight(path); 
        }
        private void StartHighlight(BaseUnitPath path)
        {
            Path = path;
            _pendingPath = null;
            _isRunning = true;

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
            if (path == null)
            {
                Debug.Log("Path is null in HighlightCoroutine");
                _isRunning = false;
                yield break;
            }

            var pathCells = new List<Vector2Int>(path.GetPath());
            if (pathCells.Count == 0)
            {
                _isRunning = false;
                yield break;
            }
            float delayBetweenCells = 0.05f;

            foreach (var cell in pathCells)
            {
                CreateHighlight(cell);

                if (allHighlights.Count > maxHighlights)
                {
                    DestroyHighlight(0);
                }

                yield return new WaitForSeconds(delayBetweenCells);
            }

           
            _isRunning = false;

            if (_pendingPath != null)
            {
                StartHighlight(_pendingPath);
            }
        }


        private void CreateHighlight(Vector2Int atCell)
        {
            if (cellHighlightPrefab == null)
            {
                Debug.LogError("CellHighlightPrefab is not assigned in DebugPathOutput!");
                return;
            }
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