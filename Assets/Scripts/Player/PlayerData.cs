using UnityEngine;

[CreateAssetMenu(menuName = "Player Data")] // 프로젝트 메뉴에서 마우스 오른쪽 클릭하여 Create/Player/Player Data를 통해 새 playerData 객체를 생성하고 플레이어에 드래그 앤 드롭
public class PlayerData : ScriptableObject
{
    [Header("중력")]
    [HideInInspector] public float gravityStrength; // 원하는 jumpHeight와 jumpTimeToApex에 필요한 아래로 향하는 중력(force).
    [HideInInspector] public float gravityScale; // 플레이어의 중력 강도 (ProjectSettings/Physics2D에서 설정한 중력의 배수).
                                                 // 또한 플레이어의 rigidbody2D.gravityScale에 설정되는 값.
    [Space(5)]
    public float fallGravityMult; // 낙하 시 플레이어의 gravityScale에 적용되는 배수.
    public float maxFallSpeed; // 플레이어가 낙하할 때 최대 낙하 속도(종단 속도).
    [Space(5)]
    public float fastFallGravityMult; // 낙하 중 아래 방향 입력 시 gravityScale에 적용되는 더 큰 배수.
                                      // Celeste 같은 게임에서 볼 수 있으며, 플레이어가 더 빠르게 낙하할 수 있도록 함.
    public float maxFastFallSpeed; // 빠른 낙하를 수행할 때 최대 낙하 속도(종단 속도).

    [Space(20)]

    [Header("달리기")]
    public float runMaxSpeed; // 플레이어가 도달하길 원하는 목표 속도.
    public float runAcceleration; // 플레이어가 최대 속도에 도달하는 가속도. runMaxSpeed로 설정하면 즉시 가속, 0으로 설정하면 가속 없음.
    [HideInInspector] public float runAccelAmount; // 실제로 적용되는 힘 (speedDiff에 곱해짐).
    public float runDecceleration; // 현재 속도에서 플레이어가 감속하는 속도. runMaxSpeed로 설정하면 즉시 감속, 0으로 설정하면 감속 없음.
    [HideInInspector] public float runDeccelAmount; // 실제로 적용되는 힘 (speedDiff에 곱해짐).
    [Space(5)]
    [Range(0f, 1)] public float accelInAir; // 공중에서 가속률에 적용되는 배수.
    [Range(0f, 1)] public float deccelInAir;
    [Space(5)]
    public bool doConserveMomentum = true;

    [Space(20)]

    [Header("점프")]
    public float jumpHeight; // 플레이어 점프의 높이.
    public float jumpTimeToApex; // 점프 힘을 가하고 원하는 점프 높이에 도달하는 데 걸리는 시간. 이 값은 플레이어의 중력 및 점프 힘도 제어.
    [HideInInspector] public float jumpForce; // 플레이어가 점프할 때 실제로 적용되는 위로 향하는 힘.

    [Header("모든 점프")]
    public float jumpCutGravityMult; // 플레이어가 점프 버튼을 해제할 때 중력을 증가시키는 배수.
    [Range(0f, 1)] public float jumpHangGravityMult; // 점프 정점(최대 높이)에 가까울 때 중력을 감소시킴.
    public float jumpHangTimeThreshold; // 플레이어가 추가 "점프 유지"를 경험하는 속도(정점 근처에서 y 속도가 0에 가까움).
    [Space(0.5f)]
    public float jumpHangAccelerationMult;
    public float jumpHangMaxSpeedMult;

    [Header("벽 점프")]
    public Vector2 wallJumpForce; // 벽 점프 시 플레이어에게 실제로 적용되는 힘.
    [Space(5)]
    [Range(0f, 1f)] public float wallJumpRunLerp; // 벽 점프 시 플레이어의 움직임에 영향을 줄임.
    [Range(0f, 1.5f)] public float wallJumpTime; // 벽 점프 후 플레이어의 움직임이 느려지는 시간.
    public bool doTurnOnWallJump; // 벽 점프 방향으로 플레이어를 회전시킬지 여부.

    [Space(20)]

    [Header("슬라이드")]
    public float slideSpeed;
    public float slideAccel;

    [Header("보조 기능")]
    [Range(0.01f, 0.5f)] public float coyoteTime; // 플랫폼에서 떨어진 후 여전히 점프할 수 있는 유예 시간.
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime; // 점프 입력 후 점프 조건(예: 접지됨)이 충족될 때 점프를 자동으로 수행할 수 있는 유예 시간.

    [Space(20)]

    [Header("대쉬")]
    public int dashAmount;
    public float dashSpeed;
    public float dashSleepTime; // 대쉬 버튼을 눌렀을 때 게임이 멈추는 시간.
    [Space(5)]
    public float dashAttackTime;
    [Space(5)]
    public float dashEndTime; // 대쉬 종료 후 정지 상태로 부드럽게 전환되는 시간.
    public Vector2 dashEndSpeed; // 대쉬 종료 시 플레이어를 느리게 만들어 응답성을 향상.
    [Range(0f, 1f)] public float dashEndRunLerp; // 대쉬 중 플레이어 움직임의 영향을 줄임.
    [Space(5)]
    public float dashRefillTime;
    [Space(5)]
    [Range(0.01f, 0.5f)] public float dashInputBufferTime;

    [Header("체력")]
    public int maxHealth;       // 최대체력
    public int currentHealth;   // 현재체력
    public int extraHealth;     // 추가로 얻은 체력


    private void OnValidate()
    {
        // 중력 강도를 공식 (gravity = 2 * jumpHeight / timeToJumpApex^2)을 사용해 계산
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        // 리지드바디의 중력 비율(프로젝트 설정/Physics2D의 중력 값 기준)을 계산
        gravityScale = gravityStrength / Physics2D.gravity.y;

        // 가속 및 감속 값을 공식 (amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed)을 사용해 계산
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        // 점프 힘을 공식 (initialJumpVelocity = gravity * timeToJumpApex)을 사용해 계산
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        #region 변수 범위
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
        #endregion
    }
}
