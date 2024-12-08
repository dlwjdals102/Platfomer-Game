using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    // �� GameObject�� �������� ���� �� �ִ��� ���θ� �����մϴ�.
    [SerializeField]
    private bool damageable = true;

    // �� GameObject�� ������ �� �� ü��
    [SerializeField]
    public int healthAmount = 100;

    // �������� ���� �� ���� �ð� ���� ���� �� �̻� �������� ���� �� ���� �ϴ� �ִ� �ð�
    // ������ ���� ������ ���� �� �������� �ִ� ���� �����ϱ� �����Դϴ�.
    [SerializeField]
    private float invulnerabilityTime = .2f;

    // �÷��̾ �� ������ �Ʒ� ���� ������ ������ �� ���� ƨ�� ������ �ϴ� ����
    public bool giveUpwardForce = true;

    // ���� �߰��� �������� ���� �� �ִ��� ���θ� �����ϴ� �Ҹ���
    private bool hit;

    // �������� ���� �� ���� ü��
    public int currentHealth;

    // �׾����� Ȯ���ϴ� bool��
    public bool isDead = false;

    // ����
    public EnemyFalseKnight enemy;
    public Animator anim;
    public Rigidbody2D rb;
    public BoxCollider2D coll;

    private void Start()
    {
        // ���� �ε�� �� ���� ü���� �ִ밪���� �����մϴ�.
        currentHealth = healthAmount;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
    }

    public void Damage(int amount)
    {
        // ���� ���� �������� Ȯ���մϴ�. ���� ���°� �ƴϸ� ���� ������ �����մϴ�.
        if (damageable && !hit && currentHealth > 0)
        {
            // ���� `hit`�� true�� �����մϴ�.
            hit = true;

            enemy.hitCount++;

            // ������ �縸ŭ `currentHealth`�� ���ҽ�ŵ�ϴ�.
            currentHealth -= amount;

            // `currentHealth`�� 0 ���϶�� ���� ���·� �����ϰ�, 
            // ��� ���¸� ó���ϱ� ���� ������ �����մϴ�.
            if (currentHealth <= 0)
            {
                // �ڵ� �������� ���� `currentHealth`�� 0���� �����մϴ�.
                currentHealth = 0;
                if (!isDead)
                {
                    isDead = true;
                    anim.SetTrigger("dead");
                }

                rb.velocity = Vector3.zero;
                rb.gravityScale = 0;
                coll.enabled = false;

                // GameObject�� ������ �����մϴ�.
                // �����δ� ��� �ִϸ��̼��� ����ϰ� 
                // ��� ó���� �����ϴ� �޼��忡�� ó���ؾ� ������, 
                // ���⼭�� �׽�Ʈ�� ���� GameObject�� ��Ȱ��ȭ�մϴ�.
                //gameObject.SetActive(false);
            }
            else
            {
                // ���� �ٽ� �������� ���� �� �ֵ��� �ϱ� ���� �ڷ�ƾ ����
                StartCoroutine(TurnOffHit());
            }
        }
    }

    // ���� �ٽ� �������� ���� �� �ֵ��� �ϱ� ���� �ڷ�ƾ
    private IEnumerator TurnOffHit()
    {
        // �⺻���� 0.2�ʷ� ������ `invulnerabilityTime`��ŭ ����մϴ�.
        yield return new WaitForSeconds(invulnerabilityTime);

        // `hit` �Ҹ����� false�� �����Ͽ� ���� �ٽ� �������� ���� �� �ֵ��� �մϴ�.
        hit = false;
    }
}
