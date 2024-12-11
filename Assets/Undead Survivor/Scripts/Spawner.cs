using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;  // �⺻ 6�� ������ ������
    public Material monsterMaterial;
    public int level;
    public float timer;

    private readonly Color[] difficultyColors = new Color[]
    {
        new Color(1f, 1f, 1f),      // ��� (�⺻)
        new Color(0f, 1f, 0f),      // �ʷϻ� (����)
        new Color(1f, 1f, 0f),      // ����� (����)
        new Color(1f, 0.5f, 0f),    // ��Ȳ�� (����)
        new Color(1f, 0f, 0f)       // ������ (�ſ� ����)
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

        // ���� ���� ���� (30��������)
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
        // 6������ ���͸� ��ȯ�ϵ�, �迭 ������ ����� �ʵ��� ����
        int baseMonsterType = currentLevel % spawnData.Length;
        return spawnData[baseMonsterType];
    }

    void spawn()
    {
        GameObject enemy = GameManager.instance.pool.Get(0);
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;

        // ���� ���� Ÿ�԰� ���̵� ���
        int baseMonsterType = level % spawnData.Length;  // spawnData �迭�� ���� ���� ����
        int difficultyTier = Mathf.Min(level / spawnData.Length, 4);  // ���� �ܰ� (0~4)

        // �⺻ ������ ��������
        SpawnData baseData = spawnData[baseMonsterType];

        // SpawnData ���� �� �ɷ�ġ �����ϸ�
        SpawnData scaledData = new SpawnData
        {
            spawnTime = baseData.spawnTime,
            spriteType = baseData.spriteType,
            health = baseData.health * (difficultyTier + 1),
            speed = baseData.speed * (1 + difficultyTier * 0.2f)
        };

        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        enemyComponent.Init(scaledData);

        // AllIn1SpriteShader ȿ�� ����
        SpriteRenderer spriter = enemy.GetComponent<SpriteRenderer>();
        if (monsterMaterial != null)
        {
            spriter.material = monsterMaterial;

            Material matInstance = spriter.material;
            matInstance.SetColor("_GlowColor", difficultyColors[difficultyTier]);

            // �⺻ �ƿ����� ����
            matInstance.SetFloat("_OutlineEnabled", 1f);
            matInstance.SetFloat("_OutlineAlpha", 1f);
            matInstance.SetFloat("_OutlineBaseWidth", 0.004f); // ��Ȯ�� ��ġ ����

            switch (difficultyTier)
            {
                case 0: // �⺻
                    matInstance.SetFloat("_GlowIntensity", 0f);
                    matInstance.SetColor("_OutlineColor", Color.clear);
                    break;
                case 1: // 2�� ���̵� - �ʷ�
                    matInstance.SetFloat("_GlowIntensity", 2f);
                    matInstance.SetColor("_OutlineColor", new Color(0f, 0.5f, 0f));
                    break;
                case 2: // 3�� ���̵� - ���
                    matInstance.SetFloat("_GlowIntensity", 3f);
                    matInstance.SetColor("_OutlineColor", new Color(0.5f, 0.5f, 0f));
                    break;
                case 3: // 4�� ���̵� - ��Ȳ
                    matInstance.SetFloat("_GlowIntensity", 4f);
                    matInstance.SetColor("_OutlineColor", new Color(0.5f, 0.25f, 0f));
                    break;
                case 4: // 5�� ���̵� - ����
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