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
    public float duration = 2f;
    public GameObject player;
    public float warningDistance = 7f;

    private bool isFading = false;

    void Start()
    {
        menuBehavior = GetComponent<MenuBehavior>();
        enemy = GameObject.Find("Enemy/SK_Mannequin");
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

        float distance = Vector3.Distance(player.transform.position, enemy.transform.position);
        if (distance <= warningDistance && !isFading)
        {
            StartCoroutine(FadeIn()); 
        }
        else if (distance > warningDistance && textmesh.gameObject.activeSelf)
        {
            StartCoroutine(FadeOut());
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
