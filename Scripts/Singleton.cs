using UnityEngine;
using Sirenix.OdinInspector;

namespace OneHit
{
     public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
     {
          public static T Instance { get; private set; }

          [Title("Singleton")] public bool dontDestroyOnLoad = true;

          protected virtual void Awake()
          {
               //! Singleton dont destroy on load
               if (dontDestroyOnLoad)
               {
                    if (Instance == null)
                    {
                         Instance = this as T;
                         DontDestroyOnLoad(gameObject);
                    }
                    else
                    {
                         Debug.LogWarning($"{nameof(T)} already exists! Destroying game object!");
                         Destroy(gameObject);
                    }
               }
               else
               {
                    //! Singleton destroy on load
                    Instance = this as T;
               }
          }
     }
}