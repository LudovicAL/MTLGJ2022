using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static bool _applicationIsQuitting = false;
    private static readonly object Lock = new object();

    public static T Instance
    {
        get
        {
            _instance = GameObject.FindObjectOfType<T>();

            if (!_instance && !_applicationIsQuitting)
            {
                lock (Lock)
                {
                    GameObject singletonObject = new GameObject();
                    _instance = singletonObject.AddComponent<T>();
                    singletonObject.name = typeof(T).ToString();

                    DontDestroyOnLoad(singletonObject);
                }
            }

            return _instance;
        }
    }

    private void OnApplicationQuit()
    {
        _instance = null;
        Destroy(gameObject);
        _applicationIsQuitting = true;
    }
}
