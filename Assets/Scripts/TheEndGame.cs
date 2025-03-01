using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TheEndGame : MonoBehaviour
{
    private GameObject player;
    private void OnTriggerEnter(Collider other)
    { 
        if(other.transform.tag == "Player")
        {
            Cursor.lockState = CursorLockMode.None;
            SceneManager.LoadScene("Win Screen");

        }
    }
}
