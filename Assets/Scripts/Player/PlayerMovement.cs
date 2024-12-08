/*
    @DawnosaurDev�� youtube.com/c/DawnosaurStudios���� ����
    Ȯ�����ּż� �����մϴ�. ������ �Ǿ��� �ٶ��ϴ�!
    �߰� �����̳� �ǵ���� ������ Twitter�� �����ϰų� YouTube�� ����� �����ּ���. :D

    �� ��ũ��Ʈ�� �����Ӱ� ����ϼ���! �������� ����� ������ ���� �ͽ��ϴ�!
*/

using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // �÷��̾��� �̵� �Ű������� �����ϴ� ScriptableObject�Դϴ�.
    // �̸� ������� �������� ��� �Ű������� �� ��ũ��Ʈ�� ���� �ٿ��ְ� ���� ������ �������� �����ؾ� �մϴ�.
    public PlayerData Data;

    #region ������Ʈ
    public Rigidbody2D RB { get; private set; }
    // �÷��̾� �ִϸ��̼��� ó���ϴ� ��ũ��Ʈ�Դϴ�. ������Ʈ�� ������ �� ��� ������ �����ϰ� ������ �� �ֽ��ϴ�.
    public PlayerAnimator AnimHandler { get; private set; }

    public PlayerHurt PlayerHurt { get; private set; }
    #endregion

    #region ���� �Ű�����
    // �÷��̾ ������ �� �ִ� �ൿ�� �����ϴ� �������Դϴ�.
    // �ٸ� ��ũ��Ʈ���� ���� �� ������ ���ο����� ���� ������ �� �ֽ��ϴ�.
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsWallJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsGrounded { get; private set; }
    

    // Ÿ�̸� (�ʵ�� ����Ǿ�����, �޼��带 ����� bool�� ��ȯ�� ���� �ֽ��ϴ�)
    public float LastOnGroundTime { get; private set; }
    public float LastOnWallTime { get; private set; }
    public float LastOnWallRightTime { get; private set; }
    public float LastOnWallLeftTime { get; private set; }

    // ���� ����
    private bool _isJumpCut;
    private bool _isJumpFalling;

    // �� ���� ����
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;

    // �뽬 ����
    private int _dashesLeft;
    private bool _dashRefilling;
    private Vector2 _lastDashDir;
    private bool _isDashAttacking;
    #endregion

    #region �Է� �Ű�����
    public Vector2 _moveInput;

    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    #endregion

    #region üũ �Ű�����
    // �ν����Ϳ��� �����ؾ� �ϴ� �Ű�����
    [Header("üũ")]
    [SerializeField] private Transform _groundCheckPoint;
    // groundCheck ũ��� ĳ���� ũ�⿡ ���� �ٸ��� �Ϲ������� �ʺ�(����)�� ����(�� üũ)���� �ణ �۰� �����մϴ�.
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region ���̾�� �±�
    [Header("���̾� & �±�")]
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    #region ī�޶� ����
    [Header("ī�޶� ���� ������Ʈ")]
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
        #region Ÿ�̸�
        // Ÿ�̸Ӹ� ���ҽ��� ���� �ð� ���� �ൿ�� ��ȿȭ
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;

        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
        #endregion

        #region �Է� ó��
        // �÷��̾��� �Է��� ó���մϴ�.
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        // �������� if (!PlayerHurt.hit) ���ǹ� �߰�
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

        #region �浹 üũ
        if (!IsDashing && !IsJumping)
        {
            // ���� üũ
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
            {
                if (LastOnGroundTime < -0.1f)
                {
                    AnimHandler.justLanded = true;
                }

                LastOnGroundTime = Data.coyoteTime;
            }

            // ������ �� üũ
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                    || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
                LastOnWallRightTime = Data.coyoteTime;

            // ���� �� üũ
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
                LastOnWallLeftTime = Data.coyoteTime;

            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
        #endregion

        #region ���� üũ
        if (IsJumping && RB.velocity.y < 0)
        {
            // �÷��̾ ���� �������� �����ϴ� ���, ���� ���¸� ����
            IsJumping = false;
            _isJumpFalling = true;
        }

        if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
        {
            // �� ���� �ð��� �ʰ��Ǹ� �� ���� ���� ����
            IsWallJumping = false;
        }

        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
        {
            // ���鿡 ���� ��� ���� ���� ������ �ʱ�ȭ
            _isJumpCut = false;
            _isJumpFalling = false;
        }

        if (!IsDashing)
        {
            // ���� ���� ���¿��� ���� �Է��� Ȯ�εǸ� ���� ����
            if (CanJump() && LastPressedJumpTime > 0)
            {
                IsJumping = true;
                IsWallJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;
                Jump();

                AnimHandler.startedJumping = true;
            }
            // �� ���� ���� ���¿��� ���� �Է��� Ȯ�εǸ� �� ���� ����
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

        #region �뽬 üũ
        if (CanDash() && LastPressedDashTime > 0)
        {
            // �뽬 ��ư �Է� �� ��� ���� ���� (������ �߰� �� ���� �Է� ���� ����)
            Sleep(Data.dashSleepTime);

            // ���� �Է��� ���� ���, �⺻ �������� �뽬
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

        #region �����̵� üũ
        if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
            IsSliding = true;
        else
            IsSliding = false;
        #endregion

        #region �߷�
        if (!_isDashAttacking)
        {
            // �뽬 ���� ���� �ƴ϶�� �߷� ����
            if (IsSliding)
            {
                // �����̵� �߿��� �߷� 0
                SetGravityScale(0);
            }
            else if (RB.velocity.y < 0 && _moveInput.y < 0)
            {
                // ���� ���� ���̶�� �߷� ����
                SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
            }
            else if (_isJumpCut)
            {
                // ���� ��ư ���� �� �߷� ����
                SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
            {
                // ���� ���� ��ó���� �߷� ���� (���� ���� �ð� ȿ��)
                SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
            }
            else if (RB.velocity.y < 0)
            {
                // ���� ���̶�� �߷� ����
                SetGravityScale(Data.gravityScale * Data.fallGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else
            {
                // �⺻ �߷� ���� (���鿡 �ְų� ��� ��)
                SetGravityScale(Data.gravityScale);
            }
        }
        else
        {
            // �뽬 ���� �� �߷� 0
            SetGravityScale(0);
        }
        #endregion

        #region ī�޶� 
        //Ư�� �ӵ� �Ӱ谪���� ������ �������� ���
        if (RB.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping 
            && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        //������ �� �ְų� ���� �ö󰡸�
        if (RB.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping
            && CameraManager.instance.LerpedFromPlayerFalling)
        {
            //�缳���ϸ� �ٽ� ȣ���� �� �ֽ��ϴ�.
            CameraManager.instance.LerpedFromPlayerFalling = false;

            CameraManager.instance.LerpYDamping(false);
        }

        #endregion

        IsGrounded = LastOnGroundTime > 0;
    }

    private void FixedUpdate()
    {
        // �뽬�� �ƴ� ��� �޸��� ó��
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

        // �����̵� ó��
        if (IsSliding)
            Slide();
    }

    #region �Է� �ݹ�
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

    #region �Ϲ� �޼���
    public void SetGravityScale(float scale)
    {
        RB.gravityScale = scale;
    }

    private void Sleep(float duration)
    {
        // StartCoroutine�� ȣ������ �ʰ� ������ ����
        StartCoroutine(nameof(PerformSleep), duration);
    }

    private IEnumerator PerformSleep(float duration)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1;
    }
    #endregion

    #region �̵� �޼���
    private void Run(float lerpAmount)
    {
        // �̵� ���� �� ��ǥ �ӵ��� ���
        float targetSpeed = _moveInput.x * Data.runMaxSpeed;
        targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

        // ���ӵ� ��� �� ����
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

    #region ���� �޼���
    private void Jump()
    {
        // ������ ���� �� ȣ������ ���ϵ��� ����
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;

        #region ���� ����
        // �÷��̾ ���� ���̶�� ����Ǵ� ���� ������Ŵ
        // �̴� �׻� ������ ���� ���̸� �����մϴ�.
        float force = Data.jumpForce;
        if (RB.velocity.y < 0)
            force -= RB.velocity.y;

        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        #endregion
    }

    private void WallJump(int dir)
    {
        // �� ������ ���� �� ȣ������ ���ϵ��� ����
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        #region �� ���� ����
        Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
        force.x *= dir; // ���� �ݴ� �������� ���� ����

        if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
            force.x -= RB.velocity.x;

        if (RB.velocity.y < 0) // �÷��̾ ���� ���̶�� �߷¿� �ݴ�Ǵ� ���� ����
            force.y -= RB.velocity.y;

        RB.AddForce(force, ForceMode2D.Impulse);
        #endregion
    }
    #endregion

    #region �뽬 �޼���
    // �뽬 �ڷ�ƾ
    private IEnumerator StartDash(Vector2 dir)
    {
        // ��ü������ Celeste ��Ÿ���� �뽬�� �����մϴ�. 
        // �� ���� ����� ���� ����� ���ϸ� ���� �޼���� ������ ����� �õ��� �� �ֽ��ϴ�.

        LastOnGroundTime = 0;
        LastPressedDashTime = 0;

        float startTime = Time.time;

        _dashesLeft--;
        _isDashAttacking = true;

        SetGravityScale(0);

        // �뽬 "����" �ܰ� ���� �ӵ��� �����մϴ�.
        while (Time.time - startTime <= Data.dashAttackTime)
        {
            RB.velocity = dir.normalized * Data.dashSpeed;
            yield return null; // ���� �����ӱ��� ���
        }

        startTime = Time.time;

        _isDashAttacking = false;

        // �뽬 ���� �ܰ迡�� �̵� �ӵ��� �����ϰ� �߷��� �����մϴ�.
        SetGravityScale(Data.gravityScale);
        RB.velocity = Data.dashEndSpeed * dir.normalized;

        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }

        // �뽬 ����
        IsDashing = false;
    }

    // �÷��̾ �뽬�� �ٽ� ����� �� �ֵ��� ������
    private IEnumerator RefillDash(int amount)
    {
        _dashRefilling = true;
        yield return new WaitForSeconds(Data.dashRefillTime);
        _dashRefilling = false;
        _dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
    }
    #endregion

    #region ��Ÿ �̵� �޼���
    private void Slide()
    {
        // ���� �ִ� ���� ���ϴ� ���� �����Ͽ� ���� �̲������� ������ ����
        if (RB.velocity.y > 0)
        {
            RB.AddForce(-RB.velocity.y * Vector2.up, ForceMode2D.Impulse);
        }

        // �����̵�� Run�� �����ϰ� ���������� y�࿡���� �۵��մϴ�.
        float speedDif = Data.slideSpeed - RB.velocity.y;
        float movement = speedDif * Data.slideAccel;

        // ������ ������ �����ϱ� ���� ���� ����
        movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        RB.AddForce(movement * Vector2.up);
    }
    #endregion

    #region üũ �޼���
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

    #region ������ �޼���
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

