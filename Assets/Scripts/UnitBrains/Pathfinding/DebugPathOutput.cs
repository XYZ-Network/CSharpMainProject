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
            //Debug.Log("HighlightPath called!", this);  // Проверка на то вызывается ли хайлайт
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
            //yield break;

            // Очистка старых хайлайтов
            while (allHighlights.Count > 0)
            {
                Destroy(allHighlights[0]);
                allHighlights.RemoveAt(0);
            }

            // Проверка пути 
            var pathCells = path?.GetPath();
            if (pathCells == null || cellHighlightPrefab == null)
            {
                yield break;
            }
                

            // Идём по всем клеткам
            foreach (var cell in pathCells)
            {
                // Подсвечиваем тек. клетку
                CreateHighlight(cell);

                // Ограничиваем количество подсветок
                if (allHighlights.Count > maxHighlights)
                {
                    Destroy(allHighlights[0]);
                    allHighlights.RemoveAt(0);
                }

                // Задержка
                yield return new WaitForSeconds(0.05f);
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