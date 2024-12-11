using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;  // 기본 6종 몬스터의 데이터
    public Material monsterMaterial;
    public int level;
    public float timer;

    private readonly Color[] difficultyColors = new Color[]
    {
        new Color(1f, 1f, 1f),      // 흰색 (기본)
        new Color(0f, 1f, 0f),      // 초록색 (약함)
        new Color(1f, 1f, 0f),      // 노랑색 (보통)
        new Color(1f, 0.5f, 0f),    // 주황색 (강함)
        new Color(1f, 0f, 0f)       // 빨강색 (매우 강함)
    };

    private void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;
        timer += Time.deltaTime;

        // 레벨 제한 제거 (30레벨까지)
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / 10f), 29);

        SpawnData currentData = GetSpawnData(level);
        if (timer > currentData.spawnTime)
        {
            timer = 0;
            spawn();
        }
    }

    SpawnData GetSpawnData(int currentLevel)
    {
        // 6종류의 몬스터를 순환하되, 배열 범위를 벗어나지 않도록 보장
        int baseMonsterType = currentLevel % spawnData.Length;
        return spawnData[baseMonsterType];
    }

    void spawn()
    {
        GameObject enemy = GameManager.instance.pool.Get(0);
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;

        // 현재 몬스터 타입과 난이도 계산
        int baseMonsterType = level % spawnData.Length;  // spawnData 배열의 범위 내로 제한
        int difficultyTier = Mathf.Min(level / spawnData.Length, 4);  // 색상 단계 (0~4)

        // 기본 데이터 가져오기
        SpawnData baseData = spawnData[baseMonsterType];

        // SpawnData 복사 및 능력치 스케일링
        SpawnData scaledData = new SpawnData
        {
            spawnTime = baseData.spawnTime,
            spriteType = baseData.spriteType,
            health = baseData.health * (difficultyTier + 1),
            speed = baseData.speed * (1 + difficultyTier * 0.2f)
        };

        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        enemyComponent.Init(scaledData);

        // AllIn1SpriteShader 효과 적용
        SpriteRenderer spriter = enemy.GetComponent<SpriteRenderer>();
        if (monsterMaterial != null)
        {
            spriter.material = monsterMaterial;

            Material matInstance = spriter.material;
            matInstance.SetColor("_GlowColor", difficultyColors[difficultyTier]);

            // 기본 아웃라인 설정
            matInstance.SetFloat("_OutlineEnabled", 1f);
            matInstance.SetFloat("_OutlineAlpha", 1f);
            matInstance.SetFloat("_OutlineBaseWidth", 0.004f); // 정확한 수치 적용

            switch (difficultyTier)
            {
                case 0: // 기본
                    matInstance.SetFloat("_GlowIntensity", 0f);
                    matInstance.SetColor("_OutlineColor", Color.clear);
                    break;
                case 1: // 2배 난이도 - 초록
                    matInstance.SetFloat("_GlowIntensity", 2f);
                    matInstance.SetColor("_OutlineColor", new Color(0f, 0.5f, 0f));
                    break;
                case 2: // 3배 난이도 - 노랑
                    matInstance.SetFloat("_GlowIntensity", 3f);
                    matInstance.SetColor("_OutlineColor", new Color(0.5f, 0.5f, 0f));
                    break;
                case 3: // 4배 난이도 - 주황
                    matInstance.SetFloat("_GlowIntensity", 4f);
                    matInstance.SetColor("_OutlineColor", new Color(0.5f, 0.25f, 0f));
                    break;
                case 4: // 5배 난이도 - 빨강
                    matInstance.SetFloat("_GlowIntensity", 5f);
                    matInstance.SetColor("_OutlineColor", new Color(0.5f, 0f, 0f));
                    matInstance.SetFloat("_ShineIntensity", 1f);
                    break;
            }
        }
    }
}

[System.Serializable]
public class SpawnData
{
    public float spawnTime;
    public int spriteType;
    public int health;
    public float speed;
}