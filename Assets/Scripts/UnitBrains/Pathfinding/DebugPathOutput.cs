using Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private IReadOnlyRuntimeModel _runtimeModel;

        private void Start()
        {
            _runtimeModel = ServiceLocator.Get<IReadOnlyRuntimeModel>();

            // Принудительно запускаем демонстрацию пути от базы до базы
            StartCoroutine(DemonstratePathFromBaseToBase());
        }

        public void HighlightPath(BaseUnitPath path)
        {
            Path = path;
            // Не запускаем корутину здесь, используем демонстрацию из Start
        }

        private IEnumerator DemonstratePathFromBaseToBase()
        {
            // Ждём, пока модель загрузится
            yield return new WaitForSeconds(0.5f);

            if (_runtimeModel == null)
            {
                Debug.LogError("RuntimeModel is null!");
                yield break;
            }

            // Очищаем старые подсветки
            ClearAllHighlights();

            // Получаем базу игрока и базу противника
            var playerBase = _runtimeModel.RoMap.Bases[RuntimeModel.PlayerId];
            var enemyBase = _runtimeModel.RoMap.Bases[RuntimeModel.BotPlayerId];

            Debug.Log($"Player base: {playerBase}, Enemy base: {enemyBase}");

            // Строим прямой путь от базы до базы
            var pathCells = new List<Vector2Int>();
            var current = playerBase;
            pathCells.Add(current);

            while (current.x != enemyBase.x)
            {
                if (current.x < enemyBase.x) current.x++;
                else if (current.x > enemyBase.x) current.x--;
                pathCells.Add(new Vector2Int(current.x, current.y));
            }

            while (current.y != enemyBase.y)
            {
                if (current.y < enemyBase.y) current.y++;
                else if (current.y > enemyBase.y) current.y--;
                pathCells.Add(new Vector2Int(current.x, current.y));
            }

            Debug.Log($"Demonstration path has {pathCells.Count} cells from {playerBase} to {enemyBase}");

            // Показываем путь с задержкой
            var delay = new WaitForSeconds(0.15f);

            for (int i = 0; i < pathCells.Count; i++)
            {
                CreateHighlight(pathCells[i]);

                while (allHighlights.Count > maxHighlights)
                {
                    RemoveOldestHighlight();
                }

                Debug.Log($"Highlighted cell {i + 1}/{pathCells.Count}: {pathCells[i]}");
                yield return delay;
            }

            while (allHighlights.Count > maxHighlights)
            {
                RemoveOldestHighlight();
            }

            Debug.Log($"Demonstration completed. {allHighlights.Count} highlights remain.");

            // Повторяем демонстрацию каждые 5 секунд
            yield return new WaitForSeconds(5f);
            StartCoroutine(DemonstratePathFromBaseToBase());
        }

        private void RemoveOldestHighlight()
        {
            if (allHighlights.Count == 0) return;
            var oldest = allHighlights[0];
            allHighlights.RemoveAt(0);
            if (oldest != null)
                Destroy(oldest);
        }

        private void ClearAllHighlights()
        {
            for (int i = allHighlights.Count - 1; i >= 0; i--)
            {
                if (allHighlights[i] != null)
                    Destroy(allHighlights[i]);
            }
            allHighlights.Clear();
        }

        private void CreateHighlight(Vector2Int atCell)
        {
            if (cellHighlightPrefab == null)
            {
                Debug.LogError("cellHighlightPrefab is not assigned! Please assign YellowHighlight prefab.");
                return;
            }

            var pos = Gameplay3dView.ToWorldPosition(atCell, 1f);
            var highlight = Instantiate(cellHighlightPrefab, pos, Quaternion.identity);
            highlight.transform.SetParent(transform);
            allHighlights.Add(highlight);
        }
    }
}