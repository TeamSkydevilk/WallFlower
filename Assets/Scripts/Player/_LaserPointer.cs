using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _LaserPointer : MonoBehaviour
{
    public GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    public GameObject Controller;
    public UnityEngine.UI.Button button;
    public bool isButton;
    int m_layerMask = 1 << 5;           // UI layer
    private void Start()
    {
        laserTransform = laser.transform;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(Controller.transform.position, transform.forward, out hit, 100, m_layerMask))
        {
            hitPoint = hit.point;
            ShowLaser(hit);
            if(hit.collider.name == "Button")
            {
                button.image.color = button.colors.highlightedColor;
                isButton = true;
            }
            else
            {
                isButton = false;
                button.image.color = button.colors.normalColor;
            }
        }
    }
    private void ShowLaser(RaycastHit hit)
    {
        laserTransform.position = Vector3.Lerp(Controller.transform.position, hitPoint, .5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laser.transform.localScale.x, laserTransform.localScale.y, hit.distance);
    }
}
