using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bar : MonoBehaviour
{
    public UnityEngine.UI.Image tooltip;
    public Transform[] transforms;
    private void Awake()
    {
        transforms = new Transform[2];
        transforms[0] = transform.GetChild(0).GetComponent<Transform>();
        transforms[1] = transform.GetChild(1).GetComponent<Transform>();
        tooltip.gameObject.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tooltip.gameObject.SetActive(true);
            tooltip.transform.LookAt(Camera.main.transform);
            for (int i = 0; i < transforms.Length; ++i)
            {
                Vector3 v = other.transform.position - transforms[i].position;
                v.x = v.z = 0;
                transforms[i].LookAt(other.transform.position - v);
            }
            other.gameObject.GetComponent<_PlayerController>().EnterBar();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tooltip.gameObject.SetActive(false);
            other.gameObject.GetComponent<_PlayerController>().ExitBar();
        }
    }
}
