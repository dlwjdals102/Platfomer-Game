using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerHurt : MonoBehaviour
{
    public PlayerData Data;

    // �������� ���� �� ���� �ð� ���� ���� �� �̻� �������� ���� �� ���� �ϴ� �ִ� �ð�
    // ������ ���� ������ ���� �� �������� �ִ� ���� �����ϱ� �����Դϴ�.
    [SerializeField]
    private float invulnerabilityTime = .2f;

    public bool hit = false;
    public bool isKnockback = false;

    // ����
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

            // �ݴ� �������� ���ư��� ���� �߰�
            Knockback();

            StartCoroutine(TurnOffHit());
        }
    }

    private void Knockback()
    {
        // ���� �ٶ󺸴� ���� ��� (localScale.x�� ���� ����)
        float knockbackDirection = mov.IsFacingRight ? -1 : 1; // �������� �ٶ󺸸� -1 (�������� ���ư�)

        // �ݴ� �������� ���� ����
        Vector2 knockbackForce = new Vector2(knockbackDirection * 10f, 10f); // X, Y�� �� ����
        rb.velocity = Vector2.zero; // ���� �ӵ� �ʱ�ȭ (�ʿ��)
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
