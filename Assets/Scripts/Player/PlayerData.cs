using UnityEngine;

[CreateAssetMenu(menuName = "Player Data")] // ������Ʈ �޴����� ���콺 ������ Ŭ���Ͽ� Create/Player/Player Data�� ���� �� playerData ��ü�� �����ϰ� �÷��̾ �巡�� �� ���
public class PlayerData : ScriptableObject
{
    [Header("�߷�")]
    [HideInInspector] public float gravityStrength; // ���ϴ� jumpHeight�� jumpTimeToApex�� �ʿ��� �Ʒ��� ���ϴ� �߷�(force).
    [HideInInspector] public float gravityScale; // �÷��̾��� �߷� ���� (ProjectSettings/Physics2D���� ������ �߷��� ���).
                                                 // ���� �÷��̾��� rigidbody2D.gravityScale�� �����Ǵ� ��.
    [Space(5)]
    public float fallGravityMult; // ���� �� �÷��̾��� gravityScale�� ����Ǵ� ���.
    public float maxFallSpeed; // �÷��̾ ������ �� �ִ� ���� �ӵ�(���� �ӵ�).
    [Space(5)]
    public float fastFallGravityMult; // ���� �� �Ʒ� ���� �Է� �� gravityScale�� ����Ǵ� �� ū ���.
                                      // Celeste ���� ���ӿ��� �� �� ������, �÷��̾ �� ������ ������ �� �ֵ��� ��.
    public float maxFastFallSpeed; // ���� ���ϸ� ������ �� �ִ� ���� �ӵ�(���� �ӵ�).

    [Space(20)]

    [Header("�޸���")]
    public float runMaxSpeed; // �÷��̾ �����ϱ� ���ϴ� ��ǥ �ӵ�.
    public float runAcceleration; // �÷��̾ �ִ� �ӵ��� �����ϴ� ���ӵ�. runMaxSpeed�� �����ϸ� ��� ����, 0���� �����ϸ� ���� ����.
    [HideInInspector] public float runAccelAmount; // ������ ����Ǵ� �� (speedDiff�� ������).
    public float runDecceleration; // ���� �ӵ����� �÷��̾ �����ϴ� �ӵ�. runMaxSpeed�� �����ϸ� ��� ����, 0���� �����ϸ� ���� ����.
    [HideInInspector] public float runDeccelAmount; // ������ ����Ǵ� �� (speedDiff�� ������).
    [Space(5)]
    [Range(0f, 1)] public float accelInAir; // ���߿��� ���ӷ��� ����Ǵ� ���.
    [Range(0f, 1)] public float deccelInAir;
    [Space(5)]
    public bool doConserveMomentum = true;

    [Space(20)]

    [Header("����")]
    public float jumpHeight; // �÷��̾� ������ ����.
    public float jumpTimeToApex; // ���� ���� ���ϰ� ���ϴ� ���� ���̿� �����ϴ� �� �ɸ��� �ð�. �� ���� �÷��̾��� �߷� �� ���� ���� ����.
    [HideInInspector] public float jumpForce; // �÷��̾ ������ �� ������ ����Ǵ� ���� ���ϴ� ��.

    [Header("��� ����")]
    public float jumpCutGravityMult; // �÷��̾ ���� ��ư�� ������ �� �߷��� ������Ű�� ���.
    [Range(0f, 1)] public float jumpHangGravityMult; // ���� ����(�ִ� ����)�� ����� �� �߷��� ���ҽ�Ŵ.
    public float jumpHangTimeThreshold; // �÷��̾ �߰� "���� ����"�� �����ϴ� �ӵ�(���� ��ó���� y �ӵ��� 0�� �����).
    [Space(0.5f)]
    public float jumpHangAccelerationMult;
    public float jumpHangMaxSpeedMult;

    [Header("�� ����")]
    public Vector2 wallJumpForce; // �� ���� �� �÷��̾�� ������ ����Ǵ� ��.
    [Space(5)]
    [Range(0f, 1f)] public float wallJumpRunLerp; // �� ���� �� �÷��̾��� �����ӿ� ������ ����.
    [Range(0f, 1.5f)] public float wallJumpTime; // �� ���� �� �÷��̾��� �������� �������� �ð�.
    public bool doTurnOnWallJump; // �� ���� �������� �÷��̾ ȸ����ų�� ����.

    [Space(20)]

    [Header("�����̵�")]
    public float slideSpeed;
    public float slideAccel;

    [Header("���� ���")]
    [Range(0.01f, 0.5f)] public float coyoteTime; // �÷������� ������ �� ������ ������ �� �ִ� ���� �ð�.
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime; // ���� �Է� �� ���� ����(��: ������)�� ������ �� ������ �ڵ����� ������ �� �ִ� ���� �ð�.

    [Space(20)]

    [Header("�뽬")]
    public int dashAmount;
    public float dashSpeed;
    public float dashSleepTime; // �뽬 ��ư�� ������ �� ������ ���ߴ� �ð�.
    [Space(5)]
    public float dashAttackTime;
    [Space(5)]
    public float dashEndTime; // �뽬 ���� �� ���� ���·� �ε巴�� ��ȯ�Ǵ� �ð�.
    public Vector2 dashEndSpeed; // �뽬 ���� �� �÷��̾ ������ ����� ���伺�� ���.
    [Range(0f, 1f)] public float dashEndRunLerp; // �뽬 �� �÷��̾� �������� ������ ����.
    [Space(5)]
    public float dashRefillTime;
    [Space(5)]
    [Range(0.01f, 0.5f)] public float dashInputBufferTime;

    [Header("ü��")]
    public int maxHealth;       // �ִ�ü��
    public int currentHealth;   // ����ü��
    public int extraHealth;     // �߰��� ���� ü��


    private void OnValidate()
    {
        // �߷� ������ ���� (gravity = 2 * jumpHeight / timeToJumpApex^2)�� ����� ���
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        // ������ٵ��� �߷� ����(������Ʈ ����/Physics2D�� �߷� �� ����)�� ���
        gravityScale = gravityStrength / Physics2D.gravity.y;

        // ���� �� ���� ���� ���� (amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed)�� ����� ���
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        // ���� ���� ���� (initialJumpVelocity = gravity * timeToJumpApex)�� ����� ���
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        #region ���� ����
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
        #endregion
    }
}
