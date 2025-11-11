using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


public class EnemyNavigation : MonoBehaviour
{
    // --- Core Components ---
    private NavMeshAgent agent;
    public Transform[] waypoints;
    private int wCount;
    private GameObject player;
    private PlayerAudio playerAudio;
    AudioSource monsterAudio;
    private Animator SusEnemy;

    // --- State Flags ---
    private bool playerNoise;
    private bool isChasing = false;
    public bool isSearching = false;

    // --- Miscellaneous ---
    public GameObject levelCanvas;
    public Transform enemyHead;
    [SerializeField] AudioClip catchingSound;
    private Vector3 storePlayerLastPosition;

    // Adjustable field of view and detection range for visualization
    [Header("Detection Settings")]
    public float viewDistance = 10f;
    public float viewAngle = 45f;


    // Start is called before the first frame update
    void Start()
    {
        // Initialize references
        wCount = 0;
        agent = GetComponent<NavMeshAgent>();

        player = GameObject.Find("Player");
        playerAudio = player.GetComponent<PlayerAudio>();
        SusEnemy = this.GetComponent<Animator>();
        monsterAudio = GetComponent<AudioSource>();

        // Start patrolling to the first waypoint
        agent.SetDestination(waypoints[wCount].position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Check if player is currently making noise
        if (playerAudio != null)
        {
            playerNoise = playerAudio.haveSound;
        }
        NavMeshHit hit;

        // --- MAIN BEHAVIOR SELECTION ---
        if (playerNoise && NavMesh.SamplePosition(player.transform.position, out hit, 1f, NavMesh.AllAreas))
        {
            // Player made noise within navigable area → start chasing
            isChasing = true;
            closeToPlayer();

        }

        else if (isChasing )
        {
            // Player stopped making noise → move to last known location and search
            isChasing = false;
            StartCoroutine(GotoLastHeard());

        }

        else
        {
            // No player sound → continue patrolling
            Patrol();
        }

        // Update walk/idle animations based on speed
        UpdateAnimation();

        // Draw visible rays in Scene view for debugging
        DrawDebugVision();
    }
    private void OnTriggerEnter(Collider other)
    {
        // Capture player on contact
        if (other.transform.tag == "Player")
        {
            CapturePlayer();
        }
        // Reset state when reaching specific trigger
        else if (other.transform.tag == "EndGame")
        {
            agent.isStopped = true;
            Patrol();
            agent.isStopped = false;
        }
    }

    // Handles the sequence when the enemy catches the player
    private void CapturePlayer()
    {

        SusEnemy.StartPlayback();
        agent.isStopped = true;

        // Play sound effect for catching the player
        monsterAudio.clip = catchingSound;
        monsterAudio.loop = false;
        monsterAudio.Play();

        // Make both characters face each other
        transform.LookAt(player.transform.position);
        player.transform.LookAt(transform.position);

        // Disable player control and trigger game over
        player.GetComponent<PlayerMovement>().gotCaught = true;
        player.GetComponent<MouseLook>().enabled = false;

        // Smoothly rotate the player's camera to face the enemy
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera != null && enemyHead != null)
        {
            StartCoroutine(SmoothLookAt(playerCamera, enemyHead.position, 0.2f));
        }

        Debug.Log("player is caught");
        StartCoroutine(GameOverWithDelay(2.5f));
    }

    // Smoothly rotates the player's camera to face the enemy.
    private IEnumerator SmoothLookAt(Camera camera, Vector3 targetPosition, float duration)
    {
        // Initialize a timer to track how long the rotation has been running
        float Timer = 0f;

        // Save the camera's current rotation as the starting point
        Quaternion startRotation = camera.transform.rotation;

        // Calculate the final rotation needed to look directly at the target position
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - camera.transform.position);

        // Gradually interpolate the camera rotation over time
        while (Timer < duration)
        {
            camera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, Timer / duration);
            Timer += Time.deltaTime;
            yield return null;
        }

        // Ensure the camera ends exactly facing the target (in case of minor timing offsets)
        camera.transform.rotation = targetRotation;
        Debug.Log("camera rotated");
    }

    // Delays game over sequence for dramatic effect.
    private IEnumerator GameOverWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        levelCanvas.GetComponent<MenuBehavior>().GameOver();
        Debug.Log("GameOver");

    }


    private void Patrol()
    {
        if (agent.remainingDistance < .1f && wCount < waypoints.Length)
        {
            Debug.Log("Current waypoint " + wCount);
            if (wCount < waypoints.Length - 1) 
            {
                wCount++;
            } else
            {
                wCount = 0;
            }

            agent.SetDestination(waypoints[wCount].position);
        }

    }

    // Moves the enemy toward the player's position when a noise is detected
    public void closeToPlayer()
    {
        if (playerNoise)
        {
            // Slightly offset target position so the enemy doesn't collide directly with the player
            Vector3 closetoplayer = new Vector3(player.transform.position.x - 1f, player.transform.position.y, player.transform.position.z - 1f);

            // Send the NavMeshAgent toward the calculated position near the player
            agent.SetDestination(closetoplayer);

            // Record the player’s last known position in case the sound stops mid-chase
            storePlayerLastPosition = player.transform.position;

        }
    }

    public void StartSearchArea()
    {
        StartCoroutine(SearchArea());
    }

    // Go to the last heard player position and begin search pattern.
    IEnumerator GotoLastHeard()
    {
        isSearching = true;
        agent.SetDestination(storePlayerLastPosition);
        transform.LookAt(storePlayerLastPosition);
        Debug.Log("Moving to last heard position");

        // Wait until reaching destination
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

    // Updates animation states based on movement velocity.
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

    // Draws debug rays to visualize the enemy’s field of view and target direction.
    private void DrawDebugVision()
    {
        if (enemyHead == null || player == null) return;

        Vector3 directionToPlayer = (player.transform.position - enemyHead.position).normalized;
        float angleToPlayer = Vector3.Angle(enemyHead.forward, directionToPlayer);

        // Draw yellow rays to show left and right boundaries of FOV
        Quaternion leftRayRotation = Quaternion.Euler(0, -viewAngle / 2, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, viewAngle / 2, 0);
        Vector3 leftRayDir = leftRayRotation * enemyHead.forward;
        Vector3 rightRayDir = rightRayRotation * enemyHead.forward;

        Debug.DrawRay(enemyHead.position, leftRayDir * viewDistance, Color.yellow);
        Debug.DrawRay(enemyHead.position, rightRayDir * viewDistance, Color.yellow);

        // Red line = direct line toward player
        Debug.DrawLine(enemyHead.position, player.transform.position, Color.red);

        // Green ray = active detection when within FOV and range
        if (angleToPlayer < viewAngle / 2 && Vector3.Distance(enemyHead.position, player.transform.position) < viewDistance)
        {
            Debug.DrawRay(enemyHead.position, directionToPlayer * viewDistance, Color.green);
        }
    }

}