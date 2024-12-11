using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;
    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    Material materialInstance;
    bool isHit;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);

        // 머티리얼 인스턴스 생성
        materialInstance = spriter.material;
    }

    private void OnEnable()
    {
        speed *= Character.Speed;
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
    }

    private void FixedUpdate()
    {
        if (!GameManager.instance.isLive)
            return;
        Vector2 nextVec = inputVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }

    private void LateUpdate()
    {
        anim.SetFloat("Speed", inputVec.magnitude);
        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!GameManager.instance.isLive)
            return;

        if (!isHit)
        {
            isHit = true;
            StartCoroutine(HitEffect());
        }

        GameManager.instance.health -= Time.deltaTime * 10;

        if (GameManager.instance.health < 0)
        {
            for (int index = 2; index < transform.childCount; index++)
            {
                transform.GetChild(index).gameObject.SetActive(false);
            }
            anim.SetTrigger("Dead");
            GameManager.instance.GameOver();
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isHit = false;
    }

    IEnumerator HitEffect()
    {
        // 히트 효과 설정
        materialInstance.SetFloat("_HitEffectBlend", 1f);
        materialInstance.SetColor("_HitEffectColor", Color.red);
        materialInstance.SetFloat("_HitEffectGlow", 5f);

        // 0.2초 대기
        yield return new WaitForSeconds(0.2f);

        // 효과 제거
        materialInstance.SetFloat("_HitEffectBlend", 0f);
        materialInstance.SetFloat("_HitEffectGlow", 0f);
    }
}