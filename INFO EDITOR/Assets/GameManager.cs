using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<InteractiveObjectController> interactiveObjects = new List<InteractiveObjectController>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddInteractiveObject(InteractiveObjectController obj)
    {
        interactiveObjects.Add(obj);
    }
}
