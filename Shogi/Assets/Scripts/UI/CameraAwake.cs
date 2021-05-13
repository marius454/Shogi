using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAwake : MonoBehaviour
{
    private void Awake(){
        this.gameObject.GetComponent<Camera>().enabled = true;
    }
}
