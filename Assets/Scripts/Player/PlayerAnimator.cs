using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private PlayerMovement mov;
    private PlayerAttack attack;
    public Animator anim;
    private SpriteRenderer spriteRend;

    //private DemoManager demoManager;

    [Header("이동 기울기")]
    [SerializeField] private float maxTilt;
    [SerializeField][Range(0, 1)] private float tiltSpeed;

    [Header("파티클 효과")]
    [SerializeField] private GameObject jumpFX;
    [SerializeField] private GameObject landFX;
    private ParticleSystem _jumpParticle;
    private ParticleSystem _landParticle;

    public bool startedJumping { private get; set; }
    public bool justLanded { private get; set; }

    public bool startedAttacking { private get; set; }

    public float currentVelY;

    private void Start()
    {
        mov = GetComponent<PlayerMovement>();
        attack = GetComponent<PlayerAttack>();
        spriteRend = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        //demoManager = FindObjectOfType<DemoManager>();

        _jumpParticle = jumpFX.GetComponent<ParticleSystem>();
        _landParticle = landFX.GetComponent<ParticleSystem>();
    }
    private void Update()
    {
        
    }

    private void LateUpdate()
    {
        #region 기울기
        float tiltProgress;

        int mult = -1;

        if (mov.IsSliding)
        {
            tiltProgress = 0.25f;
        }
        else
        {
            tiltProgress = Mathf.InverseLerp(-mov.Data.runMaxSpeed, mov.Data.runMaxSpeed, mov.RB.velocity.x);
            mult = (mov.IsFacingRight) ? 1 : -1;
        }

        float newRot = ((tiltProgress * maxTilt * 2) - maxTilt);
        float rot = Mathf.LerpAngle(spriteRend.transform.localRotation.eulerAngles.z * mult, newRot, tiltSpeed);
        //spriteRend.transform.localRotation = Quaternion.Euler(0, 0, rot * mult);
        #endregion

        CheckAnimationState();

        ParticleSystem.MainModule jumpPSettings = _jumpParticle.main;
        //jumpPSettings.startColor = new ParticleSystem.MinMaxGradient(demoManager.SceneData.foregroundColor);
        ParticleSystem.MainModule landPSettings = _landParticle.main;
        //landPSettings.startColor = new ParticleSystem.MinMaxGradient(demoManager.SceneData.foregroundColor);
    }

    private void CheckAnimationState()
    {
        if (startedJumping)
        {
            anim.SetTrigger("Jump");
            GameObject obj = Instantiate(jumpFX, transform.position - (Vector3.up * transform.localScale.y / 2), Quaternion.Euler(-90, 0, 0));
            Destroy(obj, 1);
            startedJumping = false;
            return;
        }

        if (justLanded)
        {
            anim.SetTrigger("Land");
            GameObject obj = Instantiate(landFX, transform.position - (Vector3.up * transform.localScale.y / 1.5f), Quaternion.Euler(-90, 0, 0));
            Destroy(obj, 1);
            justLanded = false;
            return;
        }

        if (startedAttacking)
        {
            anim.SetTrigger("Attack");
            startedAttacking = false;
            return;
        }

        // 이동 애니메이션
        anim.SetFloat("velocityX", Mathf.Abs(mov.RB.velocity.x));

        // 점프 애니메이션
        anim.SetBool("grounded", mov.IsGrounded);
    }
}
