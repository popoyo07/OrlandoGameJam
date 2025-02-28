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
    private bool isSearching = false;
    public GameObject levelCanvas;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            player.GetComponent<PlayerMovement>().gotCaught = true;
            transform.LookAt(player.transform.position);
            Debug.Log("player is caught");
            levelCanvas.GetComponent<MenuBehavior>().GameOver();
            agent.isStopped = true;
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
            StartCoroutine(SearchArea());
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
            /* float radius = 3f;
          float angle = Random.Range(0f, 360f);

          float SurroundingX = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
          float SurroundingZ = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;


        Vector3 HotDudeSurroundingPlayer = new Vector3(player.transform.position.x + SurroundingX, 
                                                         player.transform.position.y, 
                                                         player.transform.position.z + SurroundingZ);
          agent.SetDestination(HotDudeSurroundingPlayer);*/

            Vector3 closetoplayer = new Vector3(player.transform.position.x - 1f, player.transform.position.y, player.transform.position.z - 1f);
            agent.SetDestination(closetoplayer);
        }
    }

    public void goToPlayer()
    {
        agent.SetDestination(player.transform.position);
    }

    IEnumerator SearchArea()
    {
        isSearching = true;

        for (int i = 0; i < 2; i++)
        {
            Vector3 randomSearchPos = transform.position + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)
            );

            agent.SetDestination(randomSearchPos);
            Debug.Log("Give up but keep searching: " + i);

            yield return new WaitForSeconds(4);
        }

        Debug.Log("Give up");

        isSearching = false;
        ReturnToPatrol();
    }

    void ReturnToPatrol()
    {
        wCount = 0;
        agent.SetDestination(waypoints[wCount].position);
    }

}

