/*
    @DawnosaurDev가 youtube.com/c/DawnosaurStudios에서 제작
    확인해주셔서 감사합니다. 도움이 되었길 바랍니다!
    추가 질문이나 피드백이 있으면 Twitter로 연락하거나 YouTube에 댓글을 남겨주세요. :D

    이 스크립트를 자유롭게 사용하세요! 여러분이 만드는 게임을 보고 싶습니다!
*/

using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // 플레이어의 이동 매개변수를 저장하는 ScriptableObject입니다.
    // 이를 사용하지 않으려면 모든 매개변수를 이 스크립트에 직접 붙여넣고 관련 참조를 수동으로 변경해야 합니다.
    public PlayerData Data;

    #region 컴포넌트
    public Rigidbody2D RB { get; private set; }
    // 플레이어 애니메이션을 처리하는 스크립트입니다. 프로젝트에 통합할 때 모든 참조를 안전하게 제거할 수 있습니다.
    public PlayerAnimator AnimHandler { get; private set; }

    public PlayerHurt PlayerHurt { get; private set; }
    #endregion

    #region 상태 매개변수
    // 플레이어가 수행할 수 있는 행동을 제어하는 변수들입니다.
    // 다른 스크립트에서 읽을 수 있지만 내부에서만 값을 변경할 수 있습니다.
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsGrounded { get; private set; }
    

    // 타이머 (필드로 선언되었으며, 메서드를 사용해 bool을 반환할 수도 있습니다)
    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    // 점프 관련
    private bool _isJumpCut;
    private bool _isJumpFalling;

    // 벽 점프 관련
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;

    // 대쉬 관련
    private int _dashesLeft;
    private bool _dashRefilling;
    private Vector2 _lastDashDir;
    private bool _isDashAttacking;
    #endregion

    #region 입력 매개변수
    public Vector2 _moveInput;

    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    #endregion

    #region 체크 매개변수
    // 인스펙터에서 설정해야 하는 매개변수
    [Header("체크")]
    [SerializeField] private Transform _groundCheckPoint;
    // groundCheck 크기는 캐릭터 크기에 따라 다르며 일반적으로 너비(지면)와 높이(벽 체크)보다 약간 작게 설정합니다.
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region 레이어와 태그
    [Header("레이어 & 태그")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    #region 카메라 관련
    [Header("카메라 추적 오브젝트")]
    [SerializeField] private GameObject _cameraFollowGO;
    private CameraFollowObject _cameraFollowObject;
    private float _fallSpeedYDampingChangeThreshold;
    #endregion

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        AnimHandler = GetComponent<PlayerAnimator>();
        PlayerHurt = GetComponent<PlayerHurt>();
    }

    private void Start()
    {
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
        _cameraFollowObject = _cameraFollowGO.GetComponent<CameraFollowObject>();
        _fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
    }

    private void Update()
    {
        #region 타이머
        // 타이머를 감소시켜 일정 시간 이후 행동을 무효화
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;

        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
        #endregion

        #region 입력 처리
        // 플레이어의 입력을 처리합니다.
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        // 기존에서 if (!PlayerHurt.hit) 조건문 추가
        if (!PlayerHurt.isKnockback)
        {
            if (_moveInput.x != 0)
                CheckDirectionToFace(_moveInput.x > 0);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J))
            {
                OnJumpInput();
            }

            if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.J))
            {
                OnJumpUpInput();
            }

            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K))
            {
                OnDashInput();
            }
        }
        
        #endregion

        #region 충돌 체크
        if (!IsDashing && !IsJumping)
        {
            // 지면 체크
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
            {
                if (LastOnGroundTime < -0.1f)
                {
                    AnimHandler.justLanded = true;
                }

                LastOnGroundTime = Data.coyoteTime;
            }

            // 오른쪽 벽 체크
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                    || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
                LastOnWallRightTime = Data.coyoteTime;

            // 왼쪽 벽 체크
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
                LastOnWallLeftTime = Data.coyoteTime;

            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
        #endregion

        #region 점프 체크
        if (IsJumping && RB.velocity.y < 0)
        {
            // 플레이어가 점프 중이지만 낙하하는 경우, 점프 상태를 종료
            IsJumping = false;
            _isJumpFalling = true;
        }

        if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
        {
            // 벽 점프 시간이 초과되면 벽 점프 상태 종료
            IsWallJumping = false;
        }

        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
            // 지면에 있을 경우 점프 관련 변수를 초기화
            _isJumpCut = false;
            _isJumpFalling = false;
        }

        if (!IsDashing)
        {
            // 점프 가능 상태에서 점프 입력이 확인되면 점프 실행
            if (CanJump() && LastPressedJumpTime > 0)
            {
                IsJumping = true;
                IsWallJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;
                Jump();

                AnimHandler.startedJumping = true;
            }
            // 벽 점프 가능 상태에서 점프 입력이 확인되면 벽 점프 실행
            else if (CanWallJump() && LastPressedJumpTime > 0)
            {
                IsWallJumping = true;
                IsJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;

                _wallJumpStartTime = Time.time;
                _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

                WallJump(_lastWallJumpDir);
            }
        }
        #endregion

        #region 대쉬 체크
        if (CanDash() && LastPressedDashTime > 0)
        {
            // 대쉬 버튼 입력 후 잠깐 게임 멈춤 (박진감 추가 및 방향 입력 여유 제공)
            Sleep(Data.dashSleepTime);

            // 방향 입력이 없을 경우, 기본 방향으로 대쉬
            if (_moveInput != Vector2.zero)
                _lastDashDir = _moveInput;
            else
                _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;

            IsDashing = true;
            IsJumping = false;
            IsWallJumping = false;
            _isJumpCut = false;

            StartCoroutine(nameof(StartDash), _lastDashDir);
        }
        #endregion

        #region 슬라이드 체크
        if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
            IsSliding = true;
        else
            IsSliding = false;
        #endregion

        #region 중력
        if (!_isDashAttacking)
        {
            // 대쉬 공격 중이 아니라면 중력 설정
            if (IsSliding)
            {
                // 슬라이드 중에는 중력 0
                SetGravityScale(0);
            }
            else if (RB.velocity.y < 0 && _moveInput.y < 0)
            {
                // 빠른 낙하 중이라면 중력 증가
                SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
            }
            else if (_isJumpCut)
            {
                // 점프 버튼 해제 시 중력 증가
                SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
            {
                // 점프 정점 근처에서 중력 감소 (점프 유지 시간 효과)
                SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
            }
            else if (RB.velocity.y < 0)
            {
                // 낙하 중이라면 중력 증가
                SetGravityScale(Data.gravityScale * Data.fallGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else
            {
                // 기본 중력 설정 (지면에 있거나 상승 중)
                SetGravityScale(Data.gravityScale);
            }
        }
        else
        {
            // 대쉬 중일 때 중력 0
            SetGravityScale(0);
        }
        #endregion

        #region 카메라 
        //특정 속도 임계값으로 빠르게 떨어지는 경우
        if (RB.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping 
            && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        //가만히 서 있거나 위로 올라가면
        if (RB.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping
            && CameraManager.instance.LerpedFromPlayerFalling)
        {
            //재설정하면 다시 호출할 수 있습니다.
            CameraManager.instance.LerpedFromPlayerFalling = false;

            CameraManager.instance.LerpYDamping(false);
        }

        #endregion

        IsGrounded = LastOnGroundTime > 0;
    }

    private void FixedUpdate()
    {
        // 대쉬가 아닌 경우 달리기 처리
        if (!IsDashing && !PlayerHurt.isKnockback)
        {
            if (IsWallJumping)
                Run(Data.wallJumpRunLerp);
            else
                Run(1);
        }
        else if (_isDashAttacking)
        {
            Run(Data.dashEndRunLerp);
        }

        // 슬라이드 처리
        if (IsSliding)
            Slide();
    }

    #region 입력 콜백
    public void OnJumpInput()
    {
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }

    public void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            _isJumpCut = true;
    }

    public void OnDashInput()
    {
        LastPressedDashTime = Data.dashInputBufferTime;
    }
    #endregion

    #region 일반 메서드
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }

    private void Sleep(float duration)
    {
        // StartCoroutine를 호출하지 않고 게임을 멈춤
        StartCoroutine(nameof(PerformSleep), duration);
    }

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
    #endregion

    #region 이동 메서드
    private void Run(float lerpAmount)
    {
        // 이동 방향 및 목표 속도를 계산
        float targetSpeed = _moveInput.x * Data.runMaxSpeed;
        targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

        // 가속도 계산 및 적용
        float accelRate;
        if (LastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;

        float speedDif = targetSpeed - RB.velocity.x;
        float movement = speedDif * accelRate;
        RB.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Turn()
    {
        //Vector3 scale = transform.localScale;
        //scale.x *= -1;
        //transform.localScale = scale;

        if (IsFacingRight)
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 180f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);

            // turn the camera follow object
            _cameraFollowObject.CallTurn();
        }
        else
        {
            Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
            transform.rotation = Quaternion.Euler(rotator);
            _cameraFollowObject.CallTurn();
        }

        IsFacingRight = !IsFacingRight;

    }
    #endregion

    #region 점프 메서드
    private void Jump()
    {
        // 점프를 여러 번 호출하지 못하도록 설정
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        #region 점프 수행
        // 플레이어가 낙하 중이라면 적용되는 힘을 증가시킴
        // 이는 항상 동일한 점프 높이를 보장합니다.
        float force = Data.jumpForce;
        if (RB.velocity.y < 0)
            force -= RB.velocity.y;

        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        #endregion
    }

    private void WallJump(int dir)
    {
        // 벽 점프를 여러 번 호출하지 못하도록 설정
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        #region 벽 점프 수행
        Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
        force.x *= dir; // 벽의 반대 방향으로 힘을 가함

        if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
            force.x -= RB.velocity.x;

        if (RB.velocity.y < 0) // 플레이어가 낙하 중이라면 중력에 반대되는 힘을 가함
            force.y -= RB.velocity.y;

        RB.AddForce(force, ForceMode2D.Impulse);
        #endregion
    }
    #endregion

    #region 대쉬 메서드
    // 대쉬 코루틴
    private IEnumerator StartDash(Vector2 dir)
    {
        // 전체적으로 Celeste 스타일의 대쉬를 구현합니다. 
        // 더 물리 기반의 접근 방식을 원하면 점프 메서드와 유사한 방법을 시도할 수 있습니다.

        LastOnGroundTime = 0;
        LastPressedDashTime = 0;

        float startTime = Time.time;

        _dashesLeft--;
        _isDashAttacking = true;

        SetGravityScale(0);

        // 대쉬 "공격" 단계 동안 속도를 유지합니다.
        while (Time.time - startTime <= Data.dashAttackTime)
        {
            RB.velocity = dir.normalized * Data.dashSpeed;
            yield return null; // 다음 프레임까지 대기
        }

        startTime = Time.time;

        _isDashAttacking = false;

        // 대쉬 종료 단계에서 이동 속도를 제한하고 중력을 복구합니다.
        SetGravityScale(Data.gravityScale);
        RB.velocity = Data.dashEndSpeed * dir.normalized;

        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }

        // 대쉬 종료
        IsDashing = false;
    }

    // 플레이어가 대쉬를 다시 사용할 수 있도록 재충전
    private IEnumerator RefillDash(int amount)
    {
        _dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        _dashRefilling = false;
        _dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
    }
    #endregion

    #region 기타 이동 메서드
    private void Slide()
    {
        // 남아 있는 위로 향하는 힘을 제거하여 위로 미끄러지는 현상을 방지
        if (RB.velocity.y > 0)
        {
            RB.AddForce(-RB.velocity.y * Vector2.up, ForceMode2D.Impulse);
        }

        // 슬라이드는 Run과 유사하게 동작하지만 y축에서만 작동합니다.
        float speedDif = Data.slideSpeed - RB.velocity.y;
        float movement = speedDif * Data.slideAccel;

        // 과도한 보정을 방지하기 위해 힘을 제한
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        RB.AddForce(movement * Vector2.up);
    }
    #endregion

    #region 체크 메서드
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }

    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
            (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }

    private bool CanJumpCut()
    {
        return IsJumping && RB.velocity.y > 0;
    }

    private bool CanWallJumpCut()
    {
        return IsWallJumping && RB.velocity.y > 0;
    }

    private bool CanDash()
    {
        if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0 && !_dashRefilling)
        {
            StartCoroutine(nameof(RefillDash), 1);
        }

        return _dashesLeft > 0;
    }

    public bool CanSlide()
    {
        return LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0;
    }
    #endregion

    #region 에디터 메서드
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
    #endregion

}

