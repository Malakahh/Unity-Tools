using UnityEngine;
using System.Collections;

public class CoroutineHandler
{
    static MonoBehaviour _instance;

    /// <summary>
    /// Gets instance of CoroutineHandler, creates one if it doesn't exist.
    /// For use in scripts that aren't monobehaviour, but still need to run coroutines.
    /// </summary>
    static MonoBehaviour Instance
    {
        get 
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("CoroutineHandler", new System.Type[] { typeof(MonoBehaviour) });
                _instance = go.GetComponent<MonoBehaviour>();
            }
            return _instance; 
        }
    }

    public static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        return Instance.StartCoroutine(coroutine);
    }

    public static void StopCoroutine(Coroutine coroutine)
    {
        Instance.StopCoroutine(coroutine);
    }
}

