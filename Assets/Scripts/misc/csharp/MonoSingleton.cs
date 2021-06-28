using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    public static T GetInstance()
    {
        if (!_instance)
        {
            _instance = new GameObject(string.Format("{0} Singleton", typeof(T).Name), typeof(T)).GetComponent<T>();
            DontDestroyOnLoad(_instance.gameObject);
        }

        return _instance;
    }
}