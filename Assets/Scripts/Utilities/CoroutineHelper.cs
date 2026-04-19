using System.Collections;
using UnityEngine;

namespace Utilities
{
    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;

        public static CoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineHelper");
                    _instance = go.AddComponent<CoroutineHelper>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public void StartDelayedAction(float delay, System.Action action)
        {
            StartCoroutine(DelayedActionCoroutine(delay, action));
        }

        private IEnumerator DelayedActionCoroutine(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}
