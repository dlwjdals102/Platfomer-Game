using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerHurt : MonoBehaviour
{
    public PlayerData Data;

    // 데미지를 받은 후 일정 시간 동안 적이 더 이상 데미지를 받을 수 없게 하는 최대 시간
    // 동일한 근접 공격이 여러 번 데미지를 주는 것을 방지하기 위함입니다.
    [SerializeField]
    private float invulnerabilityTime = .2f;

    public bool hit = false;
    public bool isKnockback = false;

    // 참조
    public UIHealthController HealthController;
    public Animator anim;
    public Rigidbody2D rb;
    public PlayerMovement mov;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        mov = GetComponent<PlayerMovement>();
    }

    public void TakeDamage(int damage)
    {
        if (Data != null && !hit)
        {
            hit = true;
            isKnockback = true;

            Data.currentHealth = Mathf.Clamp(Data.currentHealth - damage, 0, Data.maxHealth);
            HealthController.DrawHearts();

            anim.SetTrigger("Hurt");

            // 반대 방향으로 날아가는 로직 추가
            Knockback();

            StartCoroutine(TurnOffHit());
        }
    }

    private void Knockback()
    {
        // 현재 바라보는 방향 계산 (localScale.x로 방향 결정)
        float knockbackDirection = mov.IsFacingRight ? -1 : 1; // 오른쪽을 바라보면 -1 (왼쪽으로 날아감)

        // 반대 방향으로 힘을 가함
        Vector2 knockbackForce = new Vector2(knockbackDirection * 10f, 10f); // X, Y축 힘 설정
        rb.velocity = Vector2.zero; // 기존 속도 초기화 (필요시)
        rb.AddForce(knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(TurnOffKnockback());
    }

    IEnumerator TurnOffKnockback()
    {
        yield return new WaitForSeconds(0.2f);

        isKnockback = false;
    }

    IEnumerator TurnOffHit()
    {
        yield return new WaitForSeconds(invulnerabilityTime);

        hit = false;
    }

    public void Restore(int amount)
    {
        if (Data != null)
        {
            Data.currentHealth = Mathf.Clamp(Data.currentHealth + amount, 0, Data.maxHealth);
        }
    }
}
