using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using static UnityEngine.UI.ScrollRect;

public class MeleeWeapon : MonoBehaviour
{
    // ���� ������ ������ ������ ��
    [SerializeField]
    private int damageAmount = 20;

    // EnemyHealth ��ũ��Ʈ�� ���� GameObject�� ���� ������ �浹�� �� �÷��̾ �󸶳� �������� �̵��ؾ� �ϴ��� ����
    public float movementTime = .1f;

    // ���� ���Ⱑ ���𰡿� ������ �� �÷��̾ �̵��ؾ� �� ���⿡ ���� ����
    public Vector2 Direction { get; private set; }

    // ���� ���Ⱑ �浹�� �� �÷��̾ �������� �ϴ��� ���θ� �����ϴ� �Ҹ���
    public bool Collided { get; private set; }

    // �߷��� ����ϱ� ���� �߰� ���� ���ϱ� ���� ���� �������� ���θ� ����
    public bool DownwardStrike { get; private set; }

    // ����
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
        // ���� ���Ⱑ �浹�� GameObject�� EnemyHealth ��ũ��Ʈ�� �ִ��� Ȯ��
        if (collision.GetComponent<EnemyHealth>())
        {
            // ���� ���� �� �÷��̾ ������ �� �ִ� ���� Ȯ���ϴ� �޼��� ȣ��
            HandleCollision(collision.GetComponent<EnemyHealth>());
            DisableCollider();
        }
    }

    private void HandleCollision(EnemyHealth objHealth)
    {
        // GameObject�� ���� ���� ����ϰ� ������ ���� �������� �� ���� ���� �ʾҴ��� Ȯ��
        if (objHealth.giveUpwardForce && Input.GetAxis("Vertical") < 0 && !mov.IsGrounded)
        {
            // direction ������ �������� ����
            Direction = Vector2.up;

            // downwardStrike�� true�� ����
            DownwardStrike = true;

            // collided�� true�� ����
            Collided = true;
        }
        if (Input.GetAxis("Vertical") > 0 && !mov.IsGrounded)
        {
            // direction ������ �Ʒ������� ����
            Direction = Vector2.down;

            // collided�� true�� ����
            Collided = true;
        }
        // ���� ������ ǥ�� ���� �������� Ȯ��
        if ((Input.GetAxis("Vertical") <= 0 && mov.IsGrounded) || Input.GetAxis("Vertical") == 0)
        {
            // �÷��̾ ������ ���� �ִ��� Ȯ��
            if (!mov.IsFacingRight) //(transform.parent.localScale.x < 0)
            {
                // direction ������ ���������� ����
                Direction = Vector2.right;
            }
            else
            {
                // direction ������ �������� ����
                Direction = Vector2.left;
            }

            // collided�� true�� ����
            Collided = true;
        }
        // damageAmount��ŭ �������� ����
        objHealth.Damage(damageAmount);

        // ���� ���� �浹 �� ���� ���� �Ҹ����� ��Ȱ��ȭ�ϴ� �ڷ�ƾ ȣ��
        StartCoroutine(NoLongerColliding());
    }

    // HandleMovement �޼����� �̵� ���� �Ҹ����� ��Ȱ��ȭ�ϴ� �ڷ�ƾ
    public IEnumerator NoLongerColliding()
    {
        // MeleeAttackManager ��ũ��Ʈ���� ������ �ð�(�⺻��: 0.1��) ���� ���
        yield return new WaitForSeconds(movementTime);

        // collided �Ҹ��� ��Ȱ��ȭ
        Collided = false;

        // downwardStrike �Ҹ��� ��Ȱ��ȭ
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
