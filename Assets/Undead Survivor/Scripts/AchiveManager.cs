using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchiveManager : MonoBehaviour
{
    public GameObject uiNotice;
    public Text noticeText;
    private Material noticeMaterial;
    private float shineProgress;
    public bool isDebugMode = false;
    private CanvasGroup canvasGroup;  // 전체 알파값 제어를 위한 CanvasGroup

    enum Achive { FirstKill, Kills100, Kills250, Kills500, Kills750, Kills1000 }
    Achive[] achives;
    WaitForSecondsRealtime wait;
    Dictionary<Achive, bool> currentAchives;

    Dictionary<Achive, string> achiveText = new Dictionary<Achive, string>()
    {
        { Achive.FirstKill, "첫 번째 처치 달성!" },
        { Achive.Kills100, "100마리 처치 달성!" },
        { Achive.Kills250, "250마리 처치 달성!" },
        { Achive.Kills500, "500마리 처치 달성!" },
        { Achive.Kills750, "750마리 처치 달성!" },
        { Achive.Kills1000, "1000마리 처치 달성!" }
    };

    void Awake()
    {
        achives = (Achive[])Enum.GetValues(typeof(Achive));
        wait = new WaitForSecondsRealtime(2f);
        noticeText = uiNotice.GetComponentInChildren<Text>();
        noticeMaterial = uiNotice.GetComponent<Image>().material;
        canvasGroup = uiNotice.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = uiNotice.AddComponent<CanvasGroup>();

        currentAchives = new Dictionary<Achive, bool>();
        foreach (Achive achive in achives)
        {
            currentAchives[achive] = false;
        }

        if (!PlayerPrefs.HasKey("MyData") || isDebugMode)
        {
            Init();
        }
    }

    void Init()
    {
        PlayerPrefs.SetInt("MyData", 1);
        foreach (Achive achive in achives)
        {
            PlayerPrefs.SetInt(achive.ToString(), 0);
            currentAchives[achive] = false;
        }
    }

    void Update()
    {
        if (uiNotice.activeSelf)
        {
            shineProgress += Time.deltaTime / 2f;
            if (noticeMaterial != null)
            {
                noticeMaterial.SetFloat("_ShineLocation", shineProgress);
            }
        }

        if (isDebugMode && Input.GetKeyDown(KeyCode.R))
        {
            foreach (Achive achive in achives)
            {
                currentAchives[achive] = false;
            }
            Debug.Log("현재 게임의 업적이 초기화되었습니다.");
        }
    }

    void LateUpdate()
    {
        foreach (Achive achive in achives)
        {
            CheckAchive(achive);
        }
    }

    void CheckAchive(Achive achive)
    {
        bool isAchive = false;
        switch (achive)
        {
            case Achive.FirstKill:
                isAchive = GameManager.instance.kill >= 1;
                break;
            case Achive.Kills100:
                isAchive = GameManager.instance.kill >= 100;
                break;
            case Achive.Kills250:
                isAchive = GameManager.instance.kill >= 250;
                break;
            case Achive.Kills500:
                isAchive = GameManager.instance.kill >= 500;
                break;
            case Achive.Kills750:
                isAchive = GameManager.instance.kill >= 750;
                break;
            case Achive.Kills1000:
                isAchive = GameManager.instance.kill >= 1000;
                break;
        }

        if (isAchive && !currentAchives[achive])
        {
            currentAchives[achive] = true;
            if (!isDebugMode)
            {
                PlayerPrefs.SetInt(achive.ToString(), 1);
            }
            noticeText.text = achiveText[achive];
            StartCoroutine(NoticeRoutine());
        }
    }

    IEnumerator NoticeRoutine()
    {
        shineProgress = 0f;
        uiNotice.SetActive(true);
        canvasGroup.alpha = 1f;

        if (noticeMaterial != null)
        {
            noticeMaterial.SetFloat("_ShineWidth", 0.1f);
            noticeMaterial.SetFloat("_ShineGlow", 1f);
        }

        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);

        yield return new WaitForSecondsRealtime(1.5f);  // 1.5초 동안 온전히 보여줌

        // 0.5초 동안 페이드 아웃
        float fadeTime = 0.5f;
        float fadeTimer = 0f;

        while (fadeTimer < fadeTime)
        {
            fadeTimer += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeTime);
            yield return null;
        }

        uiNotice.SetActive(false);
    }
}