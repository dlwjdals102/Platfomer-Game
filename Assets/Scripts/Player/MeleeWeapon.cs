using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.UI.ScrollRect;

public class MeleeWeapon : MonoBehaviour
{
    // 근접 공격이 입히는 데미지 양
    [SerializeField]
    private int damageAmount = 20;

    // EnemyHealth 스크립트를 가진 GameObject와 근접 공격이 충돌할 때 플레이어가 얼마나 오랫동안 이동해야 하는지 설정
    public float movementTime = .1f;

    // 근접 무기가 무언가와 접촉한 후 플레이어가 이동해야 할 방향에 대한 참조
    public Vector2 Direction { get; private set; }

    // 근접 무기가 충돌한 후 플레이어가 움직여야 하는지 여부를 관리하는 불리언
    public bool Collided { get; private set; }

    // 중력을 상쇄하기 위해 추가 힘을 가하기 위한 하향 공격인지 여부를 결정
    public bool DownwardStrike { get; private set; }

    // 참조
    public PlayerMovement mov;
    public PlayerAttack attack;
    public Collider2D col;

    private void Start()
    {
        col = GetComponent<BoxCollider2D>();
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 근접 무기가 충돌한 GameObject에 EnemyHealth 스크립트가 있는지 확인
        if (collision.GetComponent<EnemyHealth>())
        {
            // 근접 공격 시 플레이어에 적용할 수 있는 힘을 확인하는 메서드 호출
            HandleCollision(collision.GetComponent<EnemyHealth>());
            DisableCollider();
        }
    }

    private void HandleCollision(EnemyHealth objHealth)
    {
        // GameObject가 상향 힘을 허용하고 공격이 하향 공격인지 및 땅에 닿지 않았는지 확인
        if (objHealth.giveUpwardForce && Input.GetAxis("Vertical") < 0 && !mov.IsGrounded)
        {
            // direction 변수를 위쪽으로 설정
            Direction = Vector2.up;

            // downwardStrike를 true로 설정
            DownwardStrike = true;

            // collided를 true로 설정
            Collided = true;
        }
        if (Input.GetAxis("Vertical") > 0 && !mov.IsGrounded)
        {
            // direction 변수를 아래쪽으로 설정
            Direction = Vector2.down;

            // collided를 true로 설정
            Collided = true;
        }
        // 근접 공격이 표준 근접 공격인지 확인
        if ((Input.GetAxis("Vertical") <= 0 && mov.IsGrounded) || Input.GetAxis("Vertical") == 0)
        {
            // 플레이어가 왼쪽을 보고 있는지 확인
            if (!mov.IsFacingRight) //(transform.parent.localScale.x < 0)
            {
                // direction 변수를 오른쪽으로 설정
                Direction = Vector2.right;
            }
            else
            {
                // direction 변수를 왼쪽으로 설정
                Direction = Vector2.left;
            }

            // collided를 true로 설정
            Collided = true;
        }
        // damageAmount만큼 데미지를 입힘
        objHealth.Damage(damageAmount);

        // 근접 공격 충돌 및 방향 관련 불리언을 비활성화하는 코루틴 호출
        StartCoroutine(NoLongerColliding());
    }

    // HandleMovement 메서드의 이동 관련 불리언을 비활성화하는 코루틴
    public IEnumerator NoLongerColliding()
    {
        // MeleeAttackManager 스크립트에서 설정된 시간(기본값: 0.1초) 동안 대기
        yield return new WaitForSeconds(movementTime);

        // collided 불리언 비활성화
        Collided = false;

        // downwardStrike 불리언 비활성화
        DownwardStrike = false;
    }

    public void EnableCollider()
    {
        // Enable collider and set IsTrigger
        col.enabled = true;
        col.isTrigger = true;
    }

    public void DisableCollider()
    {
        // Disable collider to prevent unintended interactions
        col.enabled = false;
    }
}
