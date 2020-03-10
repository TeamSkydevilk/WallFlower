using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class souldondes : MonoBehaviour
{
    public int soulNPCIndex;
    public static souldondes instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = gameObject.GetComponent<souldondes>();
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
}
