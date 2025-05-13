
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
            //Debug.Log("Get in HighlightPatch");
            Path = path;
            while (allHighlights.Count > 0)
            {
                DestroyHighlight(0);
            }

            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }
            if (path != null) {
                highlightCoroutine = StartCoroutine(HighlightCoroutine(path));
            }
        }

        private IEnumerator HighlightCoroutine(BaseUnitPath path)
        {
            //Debug.Log("IENumerator");
            int counter = 0;
            while (true)
            {
                
                foreach (var cell in path.GetPath())
                {

                    //Debug.Log("Coroutine");
                    CreateHighlight(cell);
                    counter++;

                    if (counter >= maxHighlights)
                    {
                        DestroyHighlight(0);
                    }

                    yield return new WaitForSeconds(0.2f);
                }
            }
            
        }

        private void CreateHighlight(Vector2Int atCell)
        {
            //Debug.Log("get in createHighlite");
            var pos = Gameplay3dView.ToWorldPosition(atCell, 1f);
            var highlight = Instantiate(cellHighlightPrefab, pos, Quaternion.identity);
            highlight.transform.SetParent(transform);
            allHighlights.Add(highlight);
        }

        private void DestroyHighlight(int index)
        {
            //Debug.Log("get in DestroyHL");
            Destroy(allHighlights[index]);
            allHighlights.RemoveAt(index);
        }
    }
}
