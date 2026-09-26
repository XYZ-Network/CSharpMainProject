using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using View;

namespace UnitBrains.Pathfinding
{
    public class DebugPathOutput : MonoBehaviour
    {
        [SerializeField] private GameObject cellHighlightPrefab; // Префаб для подсветки клетки
        [SerializeField] private int maxHighlights = 5; // Максимальное количество одновременно подсвеченных клеток
        [SerializeField] private float highlightDelay = 0.1f; // Задержка между подсветкой клеток (в секундах)
        [SerializeField] private float fadeOutDelay = 1f; // Задержка перед удалением всех подсветок (в секундах)
        [SerializeField] private Color startColor = Color.green; // Цвет начальной точки пути
        [SerializeField] private Color endColor = Color.red; // Цвет конечной точки пути
        [SerializeField] private Color pathColor = Color.yellow; // Цвет промежуточных точек пути

        public BaseUnitPath Path { get; private set; } // Текущий отображаемый путь
        private readonly List<GameObject> allHighlights = new List<GameObject>(); // Список всех активных подсветок
        private Coroutine highlightCoroutine; // Ссылка на текущую корутину подсветки
        private bool isShuttingDown = false; // Флаг завершения работы для предотвращения ошибок

        // Метод для запуска подсветки пути
        public void HighlightPath(BaseUnitPath path)
        {
            // Проверяем, что объект активен и не завершает работу
            if (!this.isActiveAndEnabled || isShuttingDown)
                return;

            Path = path; // Сохраняем ссылку на путь

            // Очищаем все предыдущие подсветки перед созданием новых
            StopAllCoroutines(); // Останавливаем все текущие корутины
            ClearAllHighlightsImmediate(); // Мгновенно очищаем все подсветки

            // Запускаем новую корутину для визуализации пути
            if (path != null && cellHighlightPrefab != null)
            {
                highlightCoroutine = StartCoroutine(HighlightCoroutine(path));
            }
        }

        // Корутина для пошаговой подсветки пути
        private IEnumerator HighlightCoroutine(BaseUnitPath path)
        {
            // Проверяем, что объект все еще активен
            if (!this.isActiveAndEnabled || isShuttingDown)
                yield break;

            // Получаем все точки пути в виде списка для удобной работы
            List<Vector2Int> pathPoints = new List<Vector2Int>(path.GetPath());

            // Проверяем, что путь содержит хотя бы одну точку
            if (pathPoints.Count == 0)
            {
                Debug.LogWarning("DebugPathOutput: Path is empty, nothing to highlight");
                yield break; // Выходим из корутины
            }

            int totalPoints = pathPoints.Count; // Общее количество точек в пути
            int highlightedCount = 0; // Счетчик подсвеченных клеток

            // Проходим по каждой точке пути последовательно
            for (int i = 0; i < totalPoints; i++)
            {
                // Проверяем, что объект все еще активен
                if (!this.isActiveAndEnabled || isShuttingDown)
                    yield break;

                Vector2Int currentPoint = pathPoints[i]; // Текущая точка пути

                // Проверяем, не превышен ли лимит одновременно подсвеченных клеток
                if (highlightedCount >= maxHighlights)
                {
                    // Удаляем самую старую подсветку (первую в списке)
                    if (allHighlights.Count > 0 && allHighlights[0] != null)
                    {
                        // Запускаем корутину плавного исчезновения для старой подсветки
                        StartCoroutine(FadeOutHighlight(allHighlights[0], 0.3f));

                        // Ждем небольшую задержку
                        yield return new WaitForSeconds(0.05f);

                        // Удаляем объект подсветки если он еще существует
                        if (allHighlights.Count > 0)
                        {
                            DestroyHighlightSafe(0);
                        }
                    }
                }

                // Проверяем снова перед созданием новой подсветки
                if (!this.isActiveAndEnabled || isShuttingDown)
                    yield break;

                // Создаем новую подсветку для текущей точки
                GameObject highlight = CreateHighlight(currentPoint);

                if (highlight != null)
                {
                    // Устанавливаем цвет в зависимости от позиции в пути
                    SetHighlightColor(highlight, i, totalPoints);

                    // Добавляем эффект появления (плавное увеличение)
                    StartCoroutine(ScaleUpHighlight(highlight, 0.3f));
                }

                highlightedCount++; // Увеличиваем счетчик подсвеченных клеток

                // Ждем указанную задержку перед подсветкой следующей клетки
                yield return new WaitForSeconds(highlightDelay);
            }

            // Проверяем перед финальной задержкой
            if (!this.isActiveAndEnabled || isShuttingDown)
                yield break;

            // После подсветки всего пути, ждем дополнительное время
            yield return new WaitForSeconds(fadeOutDelay);

            // Проверяем перед удалением
            if (!this.isActiveAndEnabled || isShuttingDown)
                yield break;

            // Плавно убираем все оставшиеся подсветки
            yield return StartCoroutine(FadeOutAllHighlights(1f));

            // Очищаем все подсветки после завершения
            ClearAllHighlightsImmediate();

            highlightCoroutine = null; // Очищаем ссылку на корутину
        }

        // Создание подсветки для указанной клетки
        private GameObject CreateHighlight(Vector2Int atCell)
        {
            // Проверяем, что объект активен и префаб существует
            if (!this.isActiveAndEnabled || isShuttingDown || cellHighlightPrefab == null)
                return null;

            try
            {
                // Преобразуем координаты сетки в мировые координаты
                Vector3 pos = Gameplay3dView.ToWorldPosition(atCell, 1f);

                // Создаем объект подсветки из префаба
                GameObject highlight = Instantiate(cellHighlightPrefab, pos, Quaternion.identity);

                if (highlight != null)
                {
                    // Устанавливаем родительский объект для организации иерархии
                    highlight.transform.SetParent(transform);

                    // Добавляем созданную подсветку в список для отслеживания
                    allHighlights.Add(highlight);
                }

                return highlight; // Возвращаем созданный объект
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error creating highlight at {atCell}: {e.Message}");
                return null;
            }
        }

        // Установка цвета подсветки в зависимости от позиции в пути
        private void SetHighlightColor(GameObject highlight, int index, int totalPoints)
        {
            // Проверяем существование объекта
            if (highlight == null)
                return;

            // Получаем компонент Renderer для изменения цвета
            Renderer renderer = highlight.GetComponent<Renderer>();

            if (renderer != null && renderer.material != null)
            {
                Color targetColor; // Цвет для текущей точки

                if (index == 0) // Если это начальная точка
                {
                    targetColor = startColor; // Зеленый цвет
                }
                else if (index == totalPoints - 1) // Если это конечная точка
                {
                    targetColor = endColor; // Красный цвет
                }
                else // Промежуточные точки
                {
                    // Градиент от зеленого к красному
                    float t = (float)index / (totalPoints - 1); // Прогресс от 0 до 1
                    targetColor = Color.Lerp(startColor, endColor, t); // Интерполяция цвета
                }

                // Устанавливаем цвет материала
                renderer.material.color = targetColor;
            }
        }

        // Корутина для плавного появления подсветки (увеличение масштаба)
        private IEnumerator ScaleUpHighlight(GameObject highlight, float duration)
        {
            // Проверяем существование объекта в самом начале
            if (highlight == null)
                yield break;

            // Сохраняем исходный масштаб
            Vector3 originalScale = highlight.transform.localScale;

            // Устанавливаем начальный нулевой размер
            highlight.transform.localScale = Vector3.zero;

            float elapsed = 0f; // Прошедшее время

            // Плавно увеличиваем масштаб от 0 до исходного
            while (elapsed < duration)
            {
                // ИСПРАВЛЕНИЕ: Проверяем объект на null в каждой итерации
                if (highlight == null || !highlight.activeInHierarchy)
                    yield break; // Объект уничтожен или деактивирован

                elapsed += Time.deltaTime; // Увеличиваем прошедшее время
                float t = elapsed / duration; // Прогресс анимации (0-1)

                // Используем easing функцию для более красивого эффекта
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                // Устанавливаем промежуточный масштаб
                highlight.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, smoothT);

                yield return null; // Ждем следующий кадр
            }

            // Финальная проверка и установка точного масштаба
            if (highlight != null && highlight.activeInHierarchy)
            {
                highlight.transform.localScale = originalScale;
            }
        }

        // Корутина для плавного исчезновения одной подсветки
        private IEnumerator FadeOutHighlight(GameObject highlight, float duration)
        {
            // Проверяем существование объекта
            if (highlight == null)
                yield break;

            Renderer renderer = highlight.GetComponent<Renderer>();

            // Проверяем наличие рендерера и материала
            if (renderer == null || renderer.material == null)
                yield break;

            Color originalColor = renderer.material.color; // Сохраняем исходный цвет
            float elapsed = 0f; // Прошедшее время

            // Плавно уменьшаем прозрачность
            while (elapsed < duration)
            {
                // ИСПРАВЛЕНИЕ: Проверяем объект и рендерер в каждой итерации
                if (highlight == null || !highlight.activeInHierarchy || renderer == null || renderer.material == null)
                    yield break; // Объект или компоненты уничтожены

                elapsed += Time.deltaTime; // Увеличиваем время
                float t = elapsed / duration; // Прогресс (0-1)

                // Уменьшаем альфа-канал для эффекта исчезновения
                Color fadedColor = originalColor;
                fadedColor.a = Mathf.Lerp(1f, 0f, t); // Плавное уменьшение прозрачности
                renderer.material.color = fadedColor;

                yield return null; // Ждем следующий кадр
            }
        }

        // Корутина для плавного исчезновения всех подсветок
        private IEnumerator FadeOutAllHighlights(float duration)
        {
            // Проверяем, есть ли подсветки
            if (allHighlights.Count == 0)
                yield break;

            // Создаем копию списка для безопасной итерации
            List<GameObject> highlightsToFade = new List<GameObject>();
            foreach (GameObject highlight in allHighlights)
            {
                if (highlight != null)
                {
                    highlightsToFade.Add(highlight);
                }
            }

            // Запускаем исчезновение для всех подсветок одновременно
            foreach (GameObject highlight in highlightsToFade)
            {
                if (highlight != null && this.isActiveAndEnabled)
                {
                    StartCoroutine(FadeOutHighlight(highlight, duration));
                }
            }

            // Ждем указанную длительность
            yield return new WaitForSeconds(duration);
        }

        // Безопасное удаление подсветки по индексу
        private void DestroyHighlightSafe(int index)
        {
            // Проверяем, что индекс в допустимых пределах
            if (index >= 0 && index < allHighlights.Count)
            {
                GameObject highlight = allHighlights[index]; // Получаем объект

                if (highlight != null)
                {
                    // Проверяем, что объект все еще существует перед уничтожением
                    if (highlight.activeInHierarchy || highlight != null)
                    {
                        Destroy(highlight); // Уничтожаем игровой объект
                    }
                }

                allHighlights.RemoveAt(index); // Удаляем из списка в любом случае
            }
        }

        // Мгновенная очистка всех подсветок без анимации
        private void ClearAllHighlightsImmediate()
        {
            // Удаляем все подсветки с конца списка для безопасности
            for (int i = allHighlights.Count - 1; i >= 0; i--)
            {
                if (allHighlights[i] != null)
                {
                    Destroy(allHighlights[i]); // Уничтожаем объект
                }
            }

            allHighlights.Clear(); // Очищаем список
        }

        // Метод для принудительной очистки всех подсветок
        public void ClearAllHighlights()
        {
            // Останавливаем текущую корутину, если она запущена
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
                highlightCoroutine = null;
            }

            // Останавливаем все остальные корутины
            StopAllCoroutines();

            // Очищаем подсветки
            ClearAllHighlightsImmediate();
        }

        // Вызывается при деактивации объекта
        private void OnDisable()
        {
            isShuttingDown = true; // Устанавливаем флаг завершения
            ClearAllHighlights(); // Очищаем все подсветки
        }

        // Вызывается при уничтожении объекта
        private void OnDestroy()
        {
            isShuttingDown = true; // Устанавливаем флаг завершения
            ClearAllHighlights(); // Очищаем все подсветки
        }

        // Вызывается когда объект становится неактивным
        private void OnApplicationQuit()
        {
            isShuttingDown = true; // Устанавливаем флаг при выходе из приложения
        }
    }
}