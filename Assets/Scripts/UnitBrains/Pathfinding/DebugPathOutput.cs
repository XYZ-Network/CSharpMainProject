using System;
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
            // TODO Implement me
            int counter = 0;
            while (true)
            {
                foreach (var cell in path.GetPath())
                {
                    CreateHighlight(cell);
                    counter++;

                    if (counter >= maxHighlights)
                    {
                        DestroyHighlight(0);
                    }

                    yield return new WaitForSeconds(0.1f);
                }
            }

            //Тут тесты Vector2Int
            //Vector2Int test1 = new Vector2Int(5, 3);
            //Vector2Int test2 = new Vector2Int(2, 10);
            //Vector2Int testequals = new Vector2Int(5, 3);
            //PathNode nodik1 = new PathNode(test1);
            //PathNode nodik2 = new PathNode(test2);
            //nodik2.Parent = nodik1;
            //PathNode nodik3 = new PathNode(testequals);
            //Vector2Int testNod = nodik1.Position;

            //Vector2Int plus = test1 + test2;
            //Vector2Int minus = test1 - test2;
            //int x = Math.Abs(minus.x);
            //int y = Math.Abs(minus.y);
            //Vector2Int abs = new Vector2Int(Math.Abs(minus.x), Math.Abs(minus.y));
            //Vector2Int kek = new Vector2Int(7 + 2, 6 + 8);

            //Debug.Log($"NodnotEquals = {nodik1.Position == nodik2.Position}");
            //Debug.Log($"NodEquals = {nodik1.Position == nodik3.Position}");
            //Debug.Log($"5.3 Nod To Vector2Int 5.3 = {testNod}");
            //Debug.Log($"test1 - test2 = {minus}");
            //Debug.Log($"minusXABS = {x}");
            //Debug.Log($"minusYABS = {y}");
            //Debug.Log($"ABSMinus = {abs}");
            //Debug.Log($"Kek = {kek}");

            yield break;
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