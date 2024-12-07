using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public Texture2D[] textures;  // 0-3: Run, 4: Hit, 5: Death
    public Rigidbody2D target;
    public bool isLive;

    Rigidbody2D rigid;
    Collider2D coll;
    SpriteRenderer spriter;
    WaitForFixedUpdate wait;

    private float frameTimer = 0f;
    private int currentRunFrame = 0;
    private const float FRAME_RATE = 0.1f;  // 애니메이션 프레임 속도
    private bool isHit = false;
    private bool isDead = false;
    private float fadeTimer = 1f;
    private bool hasSetDeathSprite = false;  // 사망 스프라이트 설정 여부 체크

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
    }

    private void Update()
    {
        if (!isLive)
        {
            if (isDead)
            {
                // 사망 스프라이트 설정
                if (!hasSetDeathSprite)
                {
                    spriter.sprite = Sprite.Create(textures[5], new Rect(0, 0, textures[5].width, textures[5].height), Vector2.one * 0.5f);
                    hasSetDeathSprite = true;
                }

                // 페이드 아웃 처리
                fadeTimer -= Time.deltaTime;
                Color color = spriter.color;
                color.a = fadeTimer;
                spriter.color = color;

                if (fadeTimer <= 0)
                {
                    Dead();
                }
            }
            return;
        }

        UpdateSprite();
    }

    void UpdateSprite()
    {
        frameTimer += Time.deltaTime;

        if (isHit)
        {
            spriter.sprite = Sprite.Create(textures[4], new Rect(0, 0, textures[4].width, textures[4].height), Vector2.one * 0.5f);
            return;
        }

        // Run animation (0-3)
        if (frameTimer >= FRAME_RATE)
        {
            frameTimer = 0f;
            currentRunFrame = (currentRunFrame + 1) % 4;  // 0부터 3까지 순환
            Texture2D currentTexture = textures[currentRunFrame];
            spriter.sprite = Sprite.Create(currentTexture, new Rect(0, 0, currentTexture.width, currentTexture.height), Vector2.one * 0.5f);
        }
    }

    private void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        if (!isLive || isHit)
            return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.velocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        if (!isLive)
            return;
        spriter.flipX = target.position.x < rigid.position.x;
    }

    private void OnEnable()
    {
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        isDead = false;
        isHit = false;
        hasSetDeathSprite = false;
        health = maxHealth;
        fadeTimer = 1f;

        // 스프라이트 알파값 초기화
        Color color = spriter.color;
        color.a = 1f;
        spriter.color = color;
    }

    public void Init(SpawnData data)
    {
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet"))
            return;

        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack());
        StartCoroutine(HitEffect());

        if (health > 0)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            isLive = false;
            isDead = true;
            coll.enabled = false;
            rigid.simulated = false;
            spriter.sortingOrder = 1;
            GameManager.instance.kill++;
            GameManager.instance.GetExp();
            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    IEnumerator HitEffect()
    {
        isHit = true;
        yield return new WaitForSeconds(0.2f);  // 히트 스프라이트 지속 시간
        isHit = false;
    }

    IEnumerator KnockBack()
    {
        yield return wait;
        Vector3 playerPos = GameManager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }
}