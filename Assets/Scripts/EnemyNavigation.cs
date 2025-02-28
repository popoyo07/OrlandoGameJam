using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;

public class EnemyNavigation : MonoBehaviour
{

    private NavMeshAgent agent;
    public Transform[] waypoints;
    private int wCount;
    private GameObject player;
    private PlayerAudio playerAudio;
    private bool playerNoise;
    private bool isChasing = false;

    public GameObject levelCanvas;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            player.GetComponent<PlayerMovement>().gotCaught = true;
            transform.LookAt(player.transform.position);
            Debug.Log("player is caught");
            levelCanvas.GetComponent<MenuBehavior>().GameOver();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        wCount = 0;
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(waypoints[wCount].position);
        player = GameObject.Find("Player");
        playerAudio = player.GetComponent<PlayerAudio>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerAudio != null)
        {
            playerNoise = playerAudio.haveSound; 
        }

        if (playerNoise)
        {
            isChasing = true;
            closeToPlayer();
        }
        else if (isChasing)
        {
            isChasing = false;
            ReturnToPatrol();
        }
        else
        {
            Patrol();
        }
    }

    private void Patrol()
    {
        if (agent.remainingDistance < .1f && wCount < waypoints.Length) 
        {
            agent.SetDestination(waypoints[wCount].position);
            wCount++;
        }
        else if (wCount >= waypoints.Length)
        {
            wCount = 0;
            Debug.Log("Executes " + wCount);
        }
    }

    public void closeToPlayer()
    {
        if (playerNoise)
        {
            Vector3 closetoplayer = new Vector3(player.transform.position.x - 1f, player.transform.position.y, player.transform.position.z - 1f);
            agent.SetDestination(closetoplayer);
        }
    }

    public void goToPlayer()
    {
        agent.SetDestination(player.transform.position);
    }

    private void ReturnToPatrol()
    {
        wCount = 0;
        agent.SetDestination(waypoints[wCount].position);
    }

    

}
