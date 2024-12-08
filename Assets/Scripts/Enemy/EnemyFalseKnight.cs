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

    // 착지 감지를 위한 변수
    public Transform groundCheck;
    public Vector2 groundCheckSize;
    public LayerMask groundLayer;
    private bool isGrounded;

    // 참조
    public Transform player;
    public Animator anim;
    public Rigidbody2D rb;
    public EnemyHealth health;

    // 프리팹
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

        // 스턴
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
            // 랜덤값으로 보스 행동 지정
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
                // 점프 공격
                if (LastOnGroundTime > 0 && !isJumping && !isJumpAttacking)
                {
                    isJumping = true;
                    isJumpAttacking = true;
                    Jump();
                    startedJumpAttack = true;
                }
            }
        }

        #region 점프 체크
        if (!isJumping)
        {
            // 지면 체크
            if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer))
            {
                // 착지
                if (LastOnGroundTime < -0.1f)
                {
                    if (isJumpAttacking)
                    {
                        jumpAttackLanded = true;

                        Vector3 dir = isFacingRight ? Vector3.right : Vector3.left;

                        // 충격파 소환
                        GameObject obj = Instantiate(ShockWave,
                            transform.position + dir * 3, Quaternion.identity);

                        // 적의 방향 정보를 ShockWave에 전달
                        ShockWave shockwaveScript = obj.GetComponent<ShockWave>();
                        if (shockwaveScript != null)
                        {
                            // isFacingRight가 true면 오른쪽(Vector2.right), 아니면 왼쪽(Vector2.left)
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
            // 플레이어가 점프 중이지만 낙하하는 경우, 점프 상태를 종료
            isJumping = false;
            isJumpFalling = true;
        }

        if (LastOnGroundTime > 0 && !isJumping)
        {
            // 지면에 있을 경우 점프 관련 변수를 초기화
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

            // 충격파 소환
            GameObject obj = Instantiate(ShockWave,
                transform.position + dir * 3, Quaternion.identity);

            // 적의 방향 정보를 ShockWave에 전달
            ShockWave shockwaveScript = obj.GetComponent<ShockWave>();
            if (shockwaveScript != null)
            {
                // isFacingRight가 true면 오른쪽(Vector2.right), 아니면 왼쪽(Vector2.left)
                shockwaveScript.direction = isFacingRight ? Vector2.right : Vector2.left;
            }

            // 공격 관련 초기화
            isAttackAnticipate = false;
            isAttacking = false;
            randomState = 0;
            ActionTime = 2f;
        }
    }


    #region Check Method

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 충돌한 객체의 태그가 "Player"인지 확인
        if (collision.gameObject.CompareTag("Player"))
        {
            // Player가 피격당하는 로직 호출
            PlayerHurt player = collision.gameObject.GetComponent<PlayerHurt>();

            if (player != null)
            {
                player.TakeDamage(2); // 예: 10만큼의 데미지를 입음
            }
        }
    }

    private void Flip()
    {
        if (player != null)
        {
            // 플레이어의 위치가 적의 위치보다 오른쪽에 있는지 확인
            bool shouldFaceRight = player.position.x > transform.position.x;

            // 바라보는 방향
            moveDirection = shouldFaceRight ? 1 : -1;

            // 바라보는 방향이 현재 방향과 다르면 방향을 전환
            if (shouldFaceRight != isFacingRight)
            {
                isFacingRight = shouldFaceRight;

                // localScale의 x축 값을 반전시켜 적을 플립
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


        // 이동 애니메이션
        anim.SetFloat("velocityX", Mathf.Abs(rb.velocity.x));

        // 점프 애니메이션
        anim.SetFloat("velocityY", rb.velocity.y);

        // 공격 애니메이션
        anim.SetBool("attacking", isAttacking);

        // 스턴 애니메이션
        anim.SetBool("stuning", isStuning);
    }

    #endregion

    #region 에디터 메서드
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
