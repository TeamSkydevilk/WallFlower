using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempCS : MonoBehaviour {

   public _PlayerController playerController;
    public void EndTalkAnimation()
    {
        playerController.tooltipImage.gameObject.SetActive(false);
    }
}
