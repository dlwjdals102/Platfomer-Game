using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyFalseKnight : MonoBehaviour
{
    public int moveDirection;
    public float moveSpeed;
    public bool isFacingRight = true;
    public float jumpForce = 10f;

    private int randomState;
    private bool isJumping = false;
    private bool isJumpFalling = false;
    private bool startedJumping = false;
    private bool justLanded = false;

    private bool isAttackAnticipate = false;
    private bool isAttacking = false;
    private bool startedAttacking = false;

    private bool startedJumpAttack = false;
    private bool isJumpAttacking = false;
    private bool jumpAttackLanded = false;

    private bool isStuning = false;
    private int stunCount = 2;
    public int hitCount = 0;

    public float LastOnGroundTime { get; private set; }
    public float ActionTime { get; private set; }

    // ���� ������ ���� ����
    public Transform groundCheck;
    public Vector2 groundCheckSize;
    public LayerMask groundLayer;
    private bool isGrounded;

    // ����
    public Transform player;
    public Animator anim;
    public Rigidbody2D rb;
    public EnemyHealth health;

    // ������
    [SerializeField] private GameObject ShockWave;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();

        ActionTime = 5f;
        randomState = 0;
    }

    private void Update()
    {
        if (health.isDead)
        {
            return; 
        }

        ActionTime -= Time.deltaTime;
        LastOnGroundTime -= Time.deltaTime;

        // ����
        if (hitCount >= 3 && !isStuning && !isJumping && !isJumpAttacking && !isJumpFalling)
        {
            isStuning = true;
            ActionTime = 8f;
            anim.SetTrigger("stun");
        }

        if (isStuning)
        {
            if (ActionTime < 0)
            {
                hitCount = 0;
                isStuning = false;
                randomState = 0;
                ActionTime = 3f;
            }
        }

        if (!isStuning)
        {
            // ���������� ���� �ൿ ����
            if (ActionTime < 0)
            {
                randomState = Random.Range(0, 5);
                Flip();
            }

            if (randomState == 0) 
            {
                Idle();
            } 
            else if (randomState == 1)
            {
                Run();
            }
            else if (randomState == 2)
            {
                if (LastOnGroundTime > 0 && !isJumping)
                {
                    isJumping = true;
                    Jump();
                    startedJumping = true;
                }
            }
            else if (randomState == 3)
            {
                if (!isAttackAnticipate)
                {
                    isAttackAnticipate = true;
                    startedAttacking = true;

                    if (ActionTime < 0)
                        ActionTime = 100f;
                }
                else
                {
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("AttackAnticipate") &&
                        anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
                    {
                        isAttacking = true;
                    }
                }

                if (isAttacking)
                {
                    Attack();
                }
            }
            else if (randomState == 4)
            {
                // ���� ����
                if (LastOnGroundTime > 0 && !isJumping && !isJumpAttacking)
                {
                    isJumping = true;
                    isJumpAttacking = true;
                    Jump();
                    startedJumpAttack = true;
                }
            }
        }

        #region ���� üũ
        if (!isJumping)
        {
            // ���� üũ
            if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer))
            {
                // ����
                if (LastOnGroundTime < -0.1f)
                {
                    if (isJumpAttacking)
                    {
                        jumpAttackLanded = true;

                        Vector3 dir = isFacingRight ? Vector3.right : Vector3.left;

                        // ����� ��ȯ
                        GameObject obj = Instantiate(ShockWave,
                            transform.position + dir * 3, Quaternion.identity);

                        // ���� ���� ������ ShockWave�� ����
                        ShockWave shockwaveScript = obj.GetComponent<ShockWave>();
                        if (shockwaveScript != null)
                        {
                            // isFacingRight�� true�� ������(Vector2.right), �ƴϸ� ����(Vector2.left)
                            shockwaveScript.direction = isFacingRight ? Vector2.right : Vector2.left;
                        }
                    }
                    else
                    {
                        justLanded = true;
                    }

                    randomState = 0;
                    ActionTime = 1f;
                }

                LastOnGroundTime = 0.1f;
            }
        }

        if (isJumping && rb.velocity.y < 0)
        {
            // �÷��̾ ���� �������� �����ϴ� ���, ���� ���¸� ����
            isJumping = false;
            isJumpFalling = true;
        }

        if (LastOnGroundTime > 0 && !isJumping)
        {
            // ���鿡 ���� ��� ���� ���� ������ �ʱ�ȭ
            isJumping = false;
            isJumpFalling = false;
            isJumpAttacking = false;
        }
        #endregion

        CheckAnimationState();
    }

    private void Idle()
    {
        if (ActionTime < 0)
            ActionTime = 2f;

        rb.velocity = Vector2.zero;
    }

    private void Run()
    {
        if (ActionTime < 0)
            ActionTime = 2f;

        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
    }

    private void Jump()
    {
        if (ActionTime < 0)
            ActionTime = 100f;

        LastOnGroundTime = 0;

        StartCoroutine(SyncAnimAndJump(0.233f));
    }

    IEnumerator SyncAnimAndJump(float dealyTime)
    {
        yield return new WaitForSeconds(dealyTime);

        rb.velocity = new Vector2(moveDirection * moveSpeed, jumpForce);
    }

    private void Attack()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
            anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.33f)
        {
            Vector3 dir = isFacingRight ? Vector3.right : Vector3.left;

            // ����� ��ȯ
            GameObject obj = Instantiate(ShockWave,
                transform.position + dir * 3, Quaternion.identity);

            // ���� ���� ������ ShockWave�� ����
            ShockWave shockwaveScript = obj.GetComponent<ShockWave>();
            if (shockwaveScript != null)
            {
                // isFacingRight�� true�� ������(Vector2.right), �ƴϸ� ����(Vector2.left)
                shockwaveScript.direction = isFacingRight ? Vector2.right : Vector2.left;
            }

            // ���� ���� �ʱ�ȭ
            isAttackAnticipate = false;
            isAttacking = false;
            randomState = 0;
            ActionTime = 2f;
        }
    }


    #region Check Method

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // �浹�� ��ü�� �±װ� "Player"���� Ȯ��
        if (collision.gameObject.CompareTag("Player"))
        {
            // Player�� �ǰݴ��ϴ� ���� ȣ��
            PlayerHurt player = collision.gameObject.GetComponent<PlayerHurt>();

            if (player != null)
            {
                player.TakeDamage(2); // ��: 10��ŭ�� �������� ����
            }
        }
    }

    private void Flip()
    {
        if (player != null)
        {
            // �÷��̾��� ��ġ�� ���� ��ġ���� �����ʿ� �ִ��� Ȯ��
            bool shouldFaceRight = player.position.x > transform.position.x;

            // �ٶ󺸴� ����
            moveDirection = shouldFaceRight ? 1 : -1;

            // �ٶ󺸴� ������ ���� ����� �ٸ��� ������ ��ȯ
            if (shouldFaceRight != isFacingRight)
            {
                isFacingRight = shouldFaceRight;

                // localScale�� x�� ���� �������� ���� �ø�
                Vector3 localScale = transform.localScale;
                localScale.x *= -1;
                transform.localScale = localScale;
            }
        }
    }

    void CheckAnimationState()
    {
        if (startedJumping)
        {
            anim.SetTrigger("jump");
            //GameObject obj = Instantiate(jumpFX, transform.position - (Vector3.up * transform.localScale.y / 2), Quaternion.Euler(-90, 0, 0));
            //Destroy(obj, 1);
            startedJumping = false;
            return;
        }

        if (startedJumpAttack)
        {
            anim.SetTrigger("jumpAttack");
            startedJumpAttack = false;
            return;
        }

        if (justLanded)
        {
            anim.SetTrigger("land");
            //GameObject obj = Instantiate(landFX, transform.position - (Vector3.up * transform.localScale.y / 1.5f), Quaternion.Euler(-90, 0, 0));
            //Destroy(obj, 1);
            justLanded = false;
            return;
        }

        if (jumpAttackLanded)
        {
            anim.SetTrigger("jumpAttackLand");
            jumpAttackLanded = false;
            return;
        }

        if (startedAttacking)
        {
            anim.SetTrigger("attack");
            startedAttacking = false;
            return;
        }


        // �̵� �ִϸ��̼�
        anim.SetFloat("velocityX", Mathf.Abs(rb.velocity.x));

        // ���� �ִϸ��̼�
        anim.SetFloat("velocityY", rb.velocity.y);

        // ���� �ִϸ��̼�
        anim.SetBool("attacking", isAttacking);

        // ���� �ִϸ��̼�
        anim.SetBool("stuning", isStuning);
    }

    #endregion

    #region ������ �޼���
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        Gizmos.color = Color.blue;
        //Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        //Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
    #endregion
}
