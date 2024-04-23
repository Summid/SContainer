using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineHandler : MonoBehaviour
{
    public static CoroutineHandler Instance;
    
    private void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }

    public static void RunCoroutine(IEnumerator coroutine)
    {
        Instance.StartCoroutine(coroutine);
    }
}
