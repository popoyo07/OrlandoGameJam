using System.Collections;
using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    private MenuBehavior menuBehavior;
    public TextMeshProUGUI textmesh;
    public GameObject stamina;
    private bool isPaused;
    private bool dead;
    private GameObject enemy;
    public float duration = 3f;
    public GameObject player;
    public bool isFading = false;
    private AudioSource enemyAudioSource;
    private float audioRange;
    private bool isInRange = false; // New variable to track range status

    void Start()
    {
        menuBehavior = GetComponent<MenuBehavior>();
        enemy = GameObject.Find("Enemy/SK_Mannequin");

        if (enemy != null)
        {
            enemyAudioSource = enemy.GetComponent<AudioSource>();
            if (enemyAudioSource != null)
            {
                audioRange = enemyAudioSource.maxDistance - 2f;
            }
            else
            {
                Debug.LogWarning("No AudioSource found on the enemy!");
            }
        }
        else
        {
            Debug.LogError("Enemy object not found!");
        }

        textmesh.gameObject.SetActive(false);
    }

    void Update()
    {
        isPaused = menuBehavior.isPaused;
        dead = menuBehavior.isGameOver;

        if (isPaused || dead)
        {
            textmesh.gameObject.SetActive(false);
            stamina.SetActive(false);
            return;
        }

        if (enemy == null || enemyAudioSource == null)
        {
            return;
        }

        float distance = Vector3.Distance(player.transform.position, enemy.transform.position);

        // Only trigger FadeIn when entering range
        if (distance <= audioRange && !isInRange)
        {
            isInRange = true;
            StartCoroutine(FadeIn());
            Debug.Log($"Player entered audio range: {audioRange}");
        }
        // Only trigger FadeOut when leaving range
        else if (distance > audioRange && isInRange)
        {
            isInRange = false;
            StartCoroutine(FadeOut());
            Debug.Log($"Player exited audio range: {audioRange}");
        }
    }

    IEnumerator FadeIn()
    {
        isFading = true;
        textmesh.gameObject.SetActive(true);
        Color color = textmesh.color;
        color.a = 0;
        textmesh.color = color;

        float StartTimer = 0;
        while (StartTimer < duration)
        {
            StartTimer += Time.deltaTime;
            color.a = Mathf.Clamp01(StartTimer / duration);
            textmesh.color = color;
            yield return null;
        }
        isFading = false;
    }

    IEnumerator FadeOut()
    {
        isFading = true;
        Color color = textmesh.color;
        float StartTimer = 0;

        while (StartTimer < duration)
        {
            StartTimer += Time.deltaTime;
            color.a = Mathf.Clamp01(1 - (StartTimer / duration));
            textmesh.color = color;
            yield return null;
        }

        textmesh.gameObject.SetActive(false);
        isFading = false;
    }
}
