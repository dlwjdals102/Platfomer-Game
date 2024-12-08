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

    // ���� Ű �Է� �� ���� ������ ������ �� ������ �ڵ����� ������ �� �ִ� ���� �ð�.
    [Range(0.01f, 0.5f)] public float attackInputBufferTime;
    public bool IsAttacking { get; private set; }  
    public float LastPressedAttackTime { get; private set; }
    public float attackBetweenTime = 0.5f;
    private float lastAttackTime;

    // EnemyHealth ��ũ��Ʈ�� ���� GameObject�� ���� ������ �浹�� �� �÷��̾ �Ʒ��� �Ǵ� �������� �󸶳� �̵��ؾ� �ϴ��� ����
    public float defaultForce = 300;
    // EnemyHealth ��ũ��Ʈ�� ���� GameObject�� ���� ������ �浹�� �� �÷��̾ ���� �󸶳� �̵��ؾ� �ϴ��� ����
    public float upwardsForce = 600;

    // ����
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
        lastAttackTime = Time.time; // ������ ���� �ð� ����
        AnimHandler.startedAttacking = true;
        MeleeWeapon.EnableCollider();

        // ���� ���� ���� ����
        PerformAttack();

        // ���� �ð��� ������ ���� ���� ����
        Invoke(nameof(EndAttack), attackBetweenTime);
    }

    private void EndAttack()
    {
        // ���� ���� ����
        IsAttacking = false;
        MeleeWeapon.DisableCollider();
    }

    private void PerformAttack()
    {
        // �� �� ���̵�
        if (Movement._moveInput.y == 0 || Movement._moveInput.y < 0 && Movement.IsGrounded)
        {
            attackTransform.position = transform.position + (Movement.IsFacingRight ? Vector3.right : Vector3.left);
        }
        // ��
        if (Movement._moveInput.y > 0)
        {
            attackTransform.position = transform.position + Vector3.up * 1.5f;
        }
        // �Ʒ�
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
        // ���� ����Ʈ ����
        GameObject _obj = Instantiate(attackEffect, attackTransform.position, Quaternion.identity);
        _obj.transform.localScale *= Movement.IsFacingRight ? 1f : -1f;
        Destroy(_obj, 0.1f);
    }

    private bool CanAttack()
    {
        // ���� ���� ����: ���� ���� ���� �ƴϸ�, ������ ���� �� ��Ÿ���� ����
        return !IsAttacking && (Time.time >= lastAttackTime + attackBetweenTime);
    }

    private void OnAttackInput()
    {
        // ���� �Է��� �߻��� ������ ���� �ð� ����
        LastPressedAttackTime = attackInputBufferTime;
    }

    // HandleMovement �޼��忡�� �̵��� ����ϴ��� Ȯ���ϰ� 
    // ���� ������ ���� ���� ������ �������� ���� ����
    private void HandleMovement()
    {
        if (MeleeWeapon)
        {
            // ���� ������ �浹���� �� �÷��̾ ������ �� �ִ��� Ȯ��
            if (MeleeWeapon.Collided)
            {
                // ������ ���� ������ ���
                if (MeleeWeapon.DownwardStrike)
                {
                    // MeleeAttackManager ��ũ��Ʈ�� upwardsForce��ŭ �÷��̾ ���� �̵�
                    Movement.RB.AddForce(MeleeWeapon.Direction * upwardsForce);
                }
                else
                {
                    // MeleeAttackManager ��ũ��Ʈ�� defaultForce��ŭ �÷��̾ �ڷ� �̵�
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
