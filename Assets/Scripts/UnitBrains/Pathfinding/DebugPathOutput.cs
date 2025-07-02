using System;
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

            foreach (var i in path.GetPath())
            {

                CreateHighlight(i);
                if (allHighlights.Count > maxHighlights)
                {
                    DestroyHighlight(0);
                }

                yield return new WaitForSeconds(0.1f);

            }
            HighlightPath(path);


            //foreach (var item in path.GetPath())
            //{
            //    CreateHighlight(item);
            //    if (allHighlights.Count > maxHighlights)
            //    {
            //        DestroyHighlight(0);
            //    }
            //    yield return new WaitForSeconds(0.1f);

            //}
            //HighlightPath(path);
        }

        private void CreateHighlight(Vector2Int atCell) // создание ячеек
        {
            var pos = Gameplay3dView.ToWorldPosition(atCell, 1f);
            var highlight = Instantiate(cellHighlightPrefab, pos, Quaternion.identity);
            highlight.transform.SetParent(transform);
            allHighlights.Add(highlight);
        }

        private void DestroyHighlight(int index) // удаления ячеек
        {
            Destroy(allHighlights[index]);
            allHighlights.RemoveAt(index);
        }
    }
}