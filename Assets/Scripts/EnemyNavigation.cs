using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ProBuilder.MeshOperations;

public class EnemyNavigation : MonoBehaviour
{
    // Reference to the NavMeshAgent component (handles movement and pathfinding)
    private NavMeshAgent agent;
    // List of patrol waypoints for the enemy to move between
    public Transform[] waypoints;
    // Tracks the current waypoint index
    private int wCount;
    // Reference to the player GameObject
    private GameObject player;
    // Reference to the PlayerAudio script (used to detect noise made by player)
    private PlayerAudio playerAudio;
    // Whether the player is currently making noise
    private bool playerNoise;
    // Whether the enemy is actively chasing the player
    private bool isChasing = false;
    // Whether the enemy is currently searching for the player after losing them
    public bool isSearching = false;
    // Reference to the level canvas (used to trigger Game Over screen)
    public GameObject levelCanvas;
    // Stores the player's last known position when heard
    private Vector3 storePlayerLastPosition;
    // Reference to the enemy’s head (used for camera focus on capture)
    public Transform enemyHead;

    // Animator component controlling enemy animation states
    private Animator SusEnemy;
    // AudioSource used for enemy sound effects
    AudioSource monsterAudio;
    // Audio clip played when catching the player
    [SerializeField] AudioClip catchingSound;

    private void OnTriggerEnter(Collider other)
    {
        // If the enemy collides with the player, capture them
        if (other.transform.tag == "Player")
        {
            CapturePlayer();
        }
        // If enemy collides with an EndGame trigger, resume patrol
        else if (other.transform.tag == "EndGame")
        {
            agent.isStopped = true;
            Patrol();
            agent.isStopped = false;
        }
    }

    /// <summary>
    /// Handles capturing the player when detected by the enemy.
    /// </summary>
    private void CapturePlayer()
    {
        // Freeze current animation playback
        SusEnemy.StartPlayback();
        // Stop all NavMesh movement
        agent.isStopped = true;

        // Play the catching sound once
        monsterAudio.clip = catchingSound;
        monsterAudio.loop = false;
        monsterAudio.Play();

        // Make both the enemy and the player face each other
        transform.LookAt(player.transform.position);
        player.transform.LookAt(transform.position);

        // Mark player as caught
        player.GetComponent<PlayerMovement>().gotCaught = true;

        // Find player camera and smoothly turn it toward the enemy’s head
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera != null && enemyHead != null)
        {
            StartCoroutine(SmoothLookAt(playerCamera, enemyHead.position, 0.2f));
        }

        Debug.Log("player is caught");
        // Disable player’s camera control after capture
        player.GetComponent<MouseLook>().enabled = false;

        // Delay before triggering Game Over
        StartCoroutine(GameOverWithDelay(2.5f));
    }

    /// <summary>
    /// Smoothly rotates the player’s camera to look at the enemy’s head.
    /// </summary>
    private IEnumerator SmoothLookAt(Camera camera, Vector3 targetPosition, float duration)
    {
        float Timer = 0f;
        Quaternion startRotation = camera.transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - camera.transform.position);

        // Gradually rotate camera toward enemy head over time
        while (Timer < duration)
        {
            camera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, Timer / duration);
            Timer += Time.deltaTime;
            yield return null;
        }

        camera.transform.rotation = targetRotation;
        Debug.Log("camera rotated");
    }

    /// <summary>
    /// Waits for a short delay before triggering Game Over.
    /// </summary>
    private IEnumerator GameOverWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        levelCanvas.GetComponent<MenuBehavior>().GameOver();
        Debug.Log("GameOver");
    }

    // Start is called before the first frame update
    void Start()
    {
        // Start at the first waypoint
        wCount = 0;
        // Initialize NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        // Set destination to the first waypoint
        agent.SetDestination(waypoints[wCount].position);
        // Find player in the scene and get PlayerAudio component
        player = GameObject.Find("Player");
        playerAudio = player.GetComponent<PlayerAudio>();
        // Get Animator and AudioSource components from the enemy
        SusEnemy = this.GetComponent<Animator>();
        monsterAudio = GetComponent<AudioSource>();
    }

    // FixedUpdate is used for consistent movement updates
    void FixedUpdate()
    {
        // Check if playerAudio reference exists and read if player is making noise
        if (playerAudio != null)
        {
            playerNoise = playerAudio.haveSound;
        }

        NavMeshHit hit;

        // If player is making noise and their position is valid on NavMesh
        if (playerNoise && NavMesh.SamplePosition(player.transform.position, out hit, 1f, NavMesh.AllAreas))
        {
            isChasing = true;
            closeToPlayer();
        }
        // If player stops making noise while enemy was chasing
        else if (isChasing)
        {
            isChasing = false;
            StartCoroutine(GotoLastHeard());
        }
        // Otherwise, patrol normally
        else
        {
            Patrol();
        }

        // Update animation states based on movement
        UpdateAnimation();
    }

    /// <summary>
    /// Handles patrolling between waypoints when the enemy is not chasing or searching.
    /// </summary>
    private void Patrol()
    {
        // When enemy reaches a waypoint, move to the next one
        if (agent.remainingDistance < .1f && wCount < waypoints.Length)
        {
            Debug.Log("Current waypoint " + wCount);

            if (wCount < waypoints.Length - 1)
            {
                wCount++;
            }
            else
            {
                wCount = 0;
            }

            agent.SetDestination(waypoints[wCount].position);
        }
    }

    /// <summary>
    /// Moves the enemy closer to the player's position when noise is detected.
    /// </summary>
    public void closeToPlayer()
    {
        if (playerNoise)
        {
            // Move slightly behind player’s position
            Vector3 closetoplayer = new Vector3(player.transform.position.x - 1f, player.transform.position.y, player.transform.position.z - 1f);
            agent.SetDestination(closetoplayer);
            // Store last known player position
            storePlayerLastPosition = player.transform.position;
        }
    }

    /// <summary>
    /// Starts the search routine when the player escapes after being heard.
    /// </summary>
    public void StartSearchArea()
    {
        StartCoroutine(SearchArea());
    }

    /// <summary>
    /// Moves to the player’s last known position and begins searching.
    /// </summary>
    IEnumerator GotoLastHeard()
    {
        isSearching = true;
        agent.SetDestination(storePlayerLastPosition);
        transform.LookAt(storePlayerLastPosition);
        Debug.Log("Moving to last heard position");

        // Wait until the enemy reaches that position
        while (agent.remainingDistance > 0.1f)
        {
            yield return null;
        }

        Debug.Log("Reached last heard position. Searching...");
        StartSearchArea();
    }

    /// <summary>
    /// Makes the enemy move around randomly near the last heard location for a short time.
    /// </summary>
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

    /// <summary>
    /// Updates the enemy’s animation states based on movement speed.
    /// </summary>
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
