using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    // 이 GameObject가 데미지를 받을 수 있는지 여부를 결정합니다.
    [SerializeField]
    private bool damageable = true;

    // 이 GameObject가 가져야 할 총 체력
    [SerializeField]
    public int healthAmount = 100;

    // 데미지를 받은 후 일정 시간 동안 적이 더 이상 데미지를 받을 수 없게 하는 최대 시간
    // 동일한 근접 공격이 여러 번 데미지를 주는 것을 방지하기 위함입니다.
    [SerializeField]
    private float invulnerabilityTime = .2f;

    // 플레이어가 적 위에서 아래 방향 공격을 수행할 때 위로 튕겨 오르게 하는 설정
    public bool giveUpwardForce = true;

    // 적이 추가로 데미지를 받을 수 있는지 여부를 관리하는 불리언
    private bool hit;

    // 데미지를 받은 후 현재 체력
    public int currentHealth;

    // 죽었는지 확인하는 bool값
    public bool isDead = false;

    // 참조
    public EnemyFalseKnight enemy;
    public Animator anim;
    public Rigidbody2D rb;
    public BoxCollider2D coll;

    private void Start()
    {
        // 씬이 로드될 때 적의 체력을 최대값으로 설정합니다.
        currentHealth = healthAmount;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
    }

    public void Damage(int amount)
    {
        // 현재 무적 상태인지 확인합니다. 무적 상태가 아니면 다음 로직을 실행합니다.
        if (damageable && !hit && currentHealth > 0)
        {
            // 먼저 `hit`을 true로 설정합니다.
            hit = true;

            enemy.hitCount++;

            // 데미지 양만큼 `currentHealth`를 감소시킵니다.
            currentHealth -= amount;

            // `currentHealth`가 0 이하라면 죽은 상태로 간주하고, 
            // 사망 상태를 처리하기 위한 로직을 실행합니다.
            if (currentHealth <= 0)
            {
                // 코드 가독성을 위해 `currentHealth`를 0으로 고정합니다.
                currentHealth = 0;
                if (!isDead)
                {
                    isDead = true;
                    anim.SetTrigger("dead");
                }

                rb.velocity = Vector3.zero;
                rb.gravityScale = 0;
                coll.enabled = false;

                // GameObject를 씬에서 제거합니다.
                // 실제로는 사망 애니메이션을 재생하고 
                // 사망 처리를 관리하는 메서드에서 처리해야 하지만, 
                // 여기서는 테스트를 위해 GameObject를 비활성화합니다.
                //gameObject.SetActive(false);
            }
            else
            {
                // 적이 다시 데미지를 받을 수 있도록 하기 위한 코루틴 실행
                StartCoroutine(TurnOffHit());
            }
        }
    }

    // 적이 다시 데미지를 받을 수 있도록 하기 위한 코루틴
    private IEnumerator TurnOffHit()
    {
        // 기본값은 0.2초로 설정된 `invulnerabilityTime`만큼 대기합니다.
        yield return new WaitForSeconds(invulnerabilityTime);

        // `hit` 불리언을 false로 설정하여 적이 다시 데미지를 받을 수 있도록 합니다.
        hit = false;
    }
}
