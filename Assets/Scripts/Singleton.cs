using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;

    public static T Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = (T)FindObjectOfType(typeof(T));

            if (instance == null)
            {
                throw new Exception(typeof(T).Name + " is a singleton and has no instance in the current scene!");
            }

            DontDestroyOnLoad(instance);
            return instance;
        }
    }
}