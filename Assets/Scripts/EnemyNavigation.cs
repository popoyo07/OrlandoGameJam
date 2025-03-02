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
    public bool isSearching = false;
    public GameObject levelCanvas;
    private Vector3 storePlayerLastPosition;
    public Transform enemyHead;

    private Animator SusEnemy;
    AudioSource monsterAudio;
    [SerializeField] AudioClip catchingSound;


    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            CapturePlayer();
        }
    }

    private void CapturePlayer()
    {
        monsterAudio.clip = catchingSound;
        monsterAudio.loop = false;
        monsterAudio.Play();
        player.GetComponent<PlayerMovement>().gotCaught = true;
        transform.LookAt(player.transform.position);

        Camera playerCamera = player.GetComponentInChildren<Camera>();
        playerCamera.transform.LookAt(enemyHead);

        Debug.Log("player is caught");
        levelCanvas.GetComponent<MenuBehavior>().GameOver();
        agent.isStopped = true;
        player.GetComponent<MouseLook>().enabled = false;
    }


    // Start is called before the first frame update
    void Start()
    {
        wCount = 0;
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(waypoints[wCount].position);
        player = GameObject.Find("Player");
        playerAudio = player.GetComponent<PlayerAudio>();
        SusEnemy = this.GetComponent<Animator>();
        monsterAudio = GetComponent<AudioSource>();

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
            StartCoroutine(GotoLastHeard());

        }

        else
        {
            Patrol();
        }

        UpdateAnimation();
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
            storePlayerLastPosition = player.transform.position;

        }
    }

    public void StartSearchArea()
    {
        StartCoroutine(SearchArea());
    }

    IEnumerator GotoLastHeard()
    {
        isSearching = true;
        agent.SetDestination(storePlayerLastPosition);
        transform.LookAt(storePlayerLastPosition);
        Debug.Log("Moving to last heard position");

        while (agent.remainingDistance > 0.1f)
        {
            yield return null;
        }

        Debug.Log("Reached last heard position. Searching...");
        StartSearchArea();
    }

    public IEnumerator SearchArea()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 randomSearchPos = storePlayerLastPosition + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
            agent.SetDestination(randomSearchPos);
            transform.LookAt(randomSearchPos);

            Debug.Log("Searching area attempt: " + i);

            yield return new WaitForSeconds(2f);
        }

        Debug.Log("Search finished, returning to patrol.");
        isSearching = false;
        ReturnToPatrol();
    }



    void ReturnToPatrol()
    {
        wCount = 0;
        agent.SetDestination(waypoints[wCount].position);
        transform.LookAt(waypoints[wCount].position);

    }

    void UpdateAnimation()
    {
        float speed = agent.velocity.magnitude;

        if (speed > 0.01f)
        {
            SusEnemy.SetBool("isWalking", true);
            SusEnemy.SetBool("isIdle", false);
        }
        else
        {
            SusEnemy.SetBool("isWalking", false);
            SusEnemy.SetBool("isIdle", true);
        }
    }


}