using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T m_instance;

    public static T Instance
    {
        get 
        { 
            if (m_instance == null)
            {
                //try to find the singleton
                m_instance = GameObject.FindObjectOfType<T>();

                //if still null
                if (m_instance == null)
                {
                    //created new singleton of type
                    GameObject singleton = new GameObject(typeof(T).Name);
                    //add component of type and update variable
                    m_instance = singleton.AddComponent<T>();
                }
            }
            return m_instance; 
        }
    }
    
    public virtual void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this as T;

            //required if grouping managers under a parent game object
            //transform.parent = null;
            //allow singletons to persist between scenes
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
