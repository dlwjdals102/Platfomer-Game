using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform attackTransform;
    public Vector2 attackRange = new Vector2(1f, 1f);
    public LayerMask attackable;
    public GameObject attackEffect;

    // 공격 키 입력 후 공격 조건이 충족될 때 공격을 자동으로 수행할 수 있는 유예 시간.
    [Range(0.01f, 0.5f)] public float attackInputBufferTime;
    public bool IsAttacking { get; private set; }  
    public float LastPressedAttackTime { get; private set; }
    public float attackBetweenTime = 0.5f;
    private float lastAttackTime;

    // EnemyHealth 스크립트를 가진 GameObject와 근접 공격이 충돌할 때 플레이어가 아래쪽 또는 수평으로 얼마나 이동해야 하는지 설정
    public float defaultForce = 300;
    // EnemyHealth 스크립트를 가진 GameObject와 근접 공격이 충돌할 때 플레이어가 위로 얼마나 이동해야 하는지 설정
    public float upwardsForce = 600;

    // 참조
    public PlayerAnimator AnimHandler { get; private set; }
    public PlayerMovement Movement { get; private set; }
    [field: SerializeField]
    public MeleeWeapon MeleeWeapon { get; private set; }

    private void Awake()
    {
        AnimHandler = GetComponent<PlayerAnimator>();
        Movement = GetComponent<PlayerMovement>();
    }

    // Start is called before the first frame update
    void Start()
    {
        IsAttacking = false;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    // Update is called once per frame
    void Update()
    {
        LastPressedAttackTime -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.X))
        {
            OnAttackInput();
        }

        if (CanAttack() && LastPressedAttackTime > 0)
        {
            Attack();
        }
    }

    private void Attack()
    {
        IsAttacking = true;
        LastPressedAttackTime = 0;
        lastAttackTime = Time.time; // 마지막 공격 시간 갱신
        AnimHandler.startedAttacking = true;
        MeleeWeapon.EnableCollider();

        // 실제 공격 로직 실행
        PerformAttack();

        // 일정 시간이 지나면 공격 상태 해제
        Invoke(nameof(EndAttack), attackBetweenTime);
    }

    private void EndAttack()
    {
        // 공격 상태 해제
        IsAttacking = false;
        MeleeWeapon.DisableCollider();
    }

    private void PerformAttack()
    {
        // 양 옆 사이드
        if (Movement._moveInput.y == 0 || Movement._moveInput.y < 0 && Movement.IsGrounded)
        {
            attackTransform.position = transform.position + (Movement.IsFacingRight ? Vector3.right : Vector3.left);
        }
        // 위
        if (Movement._moveInput.y > 0)
        {
            attackTransform.position = transform.position + Vector3.up * 1.5f;
        }
        // 아래
        if (Movement._moveInput.y < 0 && Movement.IsJumping)
        {
            attackTransform.position = transform.position + Vector3.down * 1.5f;
        }

        MeleeWeapon.transform.position = attackTransform.position;

        StartCoroutine(DelayedAttackEffect(0.1677f));

    }

    IEnumerator DelayedAttackEffect(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 공격 이펙트 생성
        GameObject _obj = Instantiate(attackEffect, attackTransform.position, Quaternion.identity);
        _obj.transform.localScale *= Movement.IsFacingRight ? 1f : -1f;
        Destroy(_obj, 0.1f);
    }

    private bool CanAttack()
    {
        // 공격 가능 조건: 현재 공격 중이 아니며, 마지막 공격 후 쿨타임이 지남
        return !IsAttacking && (Time.time >= lastAttackTime + attackBetweenTime);
    }

    private void OnAttackInput()
    {
        // 공격 입력이 발생한 시점에 버퍼 시간 설정
        LastPressedAttackTime = attackInputBufferTime;
    }

    // HandleMovement 메서드에서 이동을 허용하는지 확인하고 
    // 근접 공격의 힘에 따라 적절한 방향으로 힘을 적용
    private void HandleMovement()
    {
        if (MeleeWeapon)
        {
            // 근접 공격이 충돌했을 때 플레이어가 움직일 수 있는지 확인
            if (MeleeWeapon.Collided)
            {
                // 공격이 하향 공격일 경우
                if (MeleeWeapon.DownwardStrike)
                {
                    // MeleeAttackManager 스크립트의 upwardsForce만큼 플레이어를 위로 이동
                    Movement.RB.AddForce(MeleeWeapon.Direction * upwardsForce);
                }
                else
                {
                    // MeleeAttackManager 스크립트의 defaultForce만큼 플레이어를 뒤로 이동
                    Movement.RB.AddForce(MeleeWeapon.Direction * defaultForce);
                }
            }
        }
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(attackTransform.position, attackRange);
    }
}
