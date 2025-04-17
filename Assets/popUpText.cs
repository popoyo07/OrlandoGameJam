using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class popUpText : MonoBehaviour
{
    public GameObject popUp;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        popUp.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        popUp.SetActive(false);
    }
}
