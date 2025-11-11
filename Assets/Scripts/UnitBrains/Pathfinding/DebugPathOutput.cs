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

        public BaseUnitPath Path { get; private set; } = null;
        private readonly List<GameObject> allHighlights = new();
        private Coroutine highlightCoroutine;

        public void HighlightPath(BaseUnitPath path)
        {
            //if (Path is not null)
            //{
            //    return;
            //}
            //Path = path;
            //while (allHighlights.Count > 0)
            //{
            //    DestroyHighlight(0);
            //}

            //if (highlightCoroutine != null)
            //{
            //    StopCoroutine(highlightCoroutine);
            //}
            //var _path = path.GetPath().ToList();
            //if (_path.Count > 0)
            //    highlightCoroutine = StartCoroutine(HighlightCoroutine(path));
        }

        private IEnumerator HighlightCoroutine(BaseUnitPath path)
        {
            List<float> stepTimes = new();
            float currentTime = Time.time;
            var _path = path.GetPath().ToList();
            CreateHighlight(_path[0]);
            stepTimes.Add(currentTime);
            _path.RemoveAt(0);

            while (stepTimes.Count > 0)
            {
                currentTime = Time.time;
                if (_path.Count > 0)
                {
                    var lastTime = stepTimes[stepTimes.Count - 1];
                    if (lastTime < currentTime - 0.3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (i < _path.Count)
                            {
                                CreateHighlight(_path[i]);
                                stepTimes.Add(currentTime);
                                _path.RemoveAt(i);
                            }
                        }
                    }
                }
                else
                {
                    _path = path.GetPath().ToList();
                }

                if (stepTimes.Count > maxHighlights)
                {
                    DestroyHighlight(0);
                    stepTimes.RemoveAt(0);
                }

                var stepTime = stepTimes[0];
                if (currentTime > stepTime + 2)
                {
                    DestroyHighlight(0);
                    stepTimes.RemoveAt(0);
                }
                yield return null;
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