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
        }else if (other.transform.tag == "EndGame")
        {
            agent.isStopped = true;
            Patrol();
            agent.isStopped = false;
        }
    }

    private void CapturePlayer()
    {

        SusEnemy.StartPlayback();
        agent.isStopped = true;

        monsterAudio.clip = catchingSound;
        monsterAudio.loop = false;
        monsterAudio.Play();
        transform.LookAt(player.transform.position); // bro, didnt want to falling in love
        player.transform.LookAt(transform.position);// falling in love 2
        player.GetComponent<PlayerMovement>().gotCaught = true;
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera != null && enemyHead != null)
        {
            StartCoroutine(SmoothLookAt(playerCamera, enemyHead.position, 0.2f));
        }

        Debug.Log("player is caught");
        player.GetComponent<MouseLook>().enabled = false;

        StartCoroutine(GameOverWithDelay(2.5f));
    }

    private IEnumerator SmoothLookAt(Camera camera, Vector3 targetPosition, float duration)
    {
        float Timer = 0f;
        Quaternion startRotation = camera.transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - camera.transform.position);

        while (Timer < duration)
        {
            camera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, Timer / duration);
            Timer += Time.deltaTime;
            yield return null;
        }

        camera.transform.rotation = targetRotation;
        Debug.Log("camera rotated");

    }

    private IEnumerator GameOverWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        levelCanvas.GetComponent<MenuBehavior>().GameOver();
        Debug.Log("GameOver");

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
        NavMeshHit hit;
      
        if (playerNoise && NavMesh.SamplePosition(player.transform.position, out hit, 1f, NavMesh.AllAreas))
        {
            isChasing = true;
            closeToPlayer();

        }

        else if (isChasing )
        {
            isChasing = false;
            StartCoroutine(GotoLastHeard());

        }

        else
        {
            //isChasing = false ;
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
        Patrol();
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