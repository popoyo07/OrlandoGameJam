using System.Collections;
using TMPro;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    private MenuBehavior menuBehavior;
    public TextMeshProUGUI[] textmesh;
    public TextMeshProUGUI textmesh_orignal;
    public GameObject stamina;
    private bool isPaused;
    private bool dead;
    private GameObject enemy;
    public float duration = 2f;
    public GameObject player;
    public bool isFading = false;
    private AudioSource enemyAudioSource;
    private float audioRange;
    private bool isInRange = false;
    public GameObject startText;

    void Start()
    {
        startText = GameObject.Find("StartText");
        menuBehavior = GetComponent<MenuBehavior>();
        enemy = GameObject.FindWithTag("enemy");

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

        foreach (var text in textmesh)
        {
            text.gameObject.SetActive(false);
        }
        textmesh_orignal.gameObject.SetActive(false);
        StartCoroutine(waitFor(5f));
    }

    void Update()
    {
        isPaused = menuBehavior.isPaused;
        dead = menuBehavior.isGameOver;

        if (isPaused || dead)
        {
            foreach (var text in textmesh)
            {
                text.gameObject.SetActive(false);
            }
            textmesh_orignal.gameObject.SetActive(false);
            stamina.SetActive(false);
            return;
        }

        if (!isPaused)
        {
            stamina.SetActive(true);
        }

        if (enemy == null || enemyAudioSource == null)
        {
            return;
        }

        float distance = Vector3.Distance(player.transform.position, enemy.transform.position);

        if (distance <= audioRange && !isInRange && !isFading)
        {
            isInRange = true;
            StartCoroutine(FadeIn());
            Debug.Log($"Player entered audio range: {audioRange}");
        }
        else if (distance > audioRange && isInRange && !isFading)
        {
            isInRange = false;
            StartCoroutine(FadeOut());
            Debug.Log($"Player exited audio range: {audioRange}");
        }
    }
    IEnumerator waitFor(float waitTime)
    {
        yield return new WaitForSeconds(waitTime) ;

        startTextDis();
        //yield return null;
    }

    public void startTextDis()
    {
        startText.SetActive(false);
    }
    IEnumerator FadeIn()
    {
        isFading = true;
        float StartTimer = 0;

        foreach (var text in textmesh)
        {
            text.gameObject.SetActive(true);
        }
        textmesh_orignal.gameObject.SetActive(true);

        while (StartTimer < duration)
        {
            StartTimer += Time.deltaTime;
            byte alphaText = (byte)(Mathf.Clamp01(StartTimer / duration) * 255);
            float alphaOrignal = Mathf.Clamp01(StartTimer / duration);

            foreach (var text in textmesh)
            {
                Color32 color = text.faceColor;
                color.a = alphaText;
                text.faceColor = color;
            }
            Color colorOrignal = textmesh_orignal.color;
            colorOrignal.a = alphaOrignal;
            textmesh_orignal.color = colorOrignal;
            yield return null;
        }
        isFading = false;
    }

    IEnumerator FadeOut()
    {
        isFading = true;
        float StartTimer = 0;

        while (StartTimer < duration)
        {
            StartTimer += Time.deltaTime;
            byte alphaText = (byte)(Mathf.Clamp01(1 - (StartTimer / duration)) * 255);
            float alphaOrignal = Mathf.Clamp01(1 - (StartTimer / duration));

            foreach (var text in textmesh)
            {
                Color32 color = text.faceColor;
                color.a = alphaText;
                text.faceColor = color;
            }
            Color colorOrignal = textmesh_orignal.color;
            colorOrignal.a = alphaOrignal;
            textmesh_orignal.color = colorOrignal;
            yield return null;
        }

        foreach (var text in textmesh)
        {
            text.gameObject.SetActive(false);
        }
        textmesh_orignal.gameObject.SetActive(false);
        isFading = false;
    }
}