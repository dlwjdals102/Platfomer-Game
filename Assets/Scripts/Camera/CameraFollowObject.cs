using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float _flipYRotationTime = 0.5f;

    private Coroutine _turnCoroutine;

    private PlayerMovement mov;

    private bool _isFacingRight;

    private void Awake()
    {
        mov = _playerTransform.gameObject.GetComponent<PlayerMovement>();
        _isFacingRight = mov.IsFacingRight;
    }

    private void Update()
    {
        // CameraFollowObject가 플레이어의 위치를 ​​따르도록 만듭니다.
        transform.position = _playerTransform.position; 
        // 여기에서 지금 카메라 스왑할때 플레이어가 점프를 하면 y값이 고정된 상태로 더이상 안움직여서 화면이 위로찍힘, 나중에 버그픽스해야댐
                
    }

    public void CallTurn()
    {
        _turnCoroutine = StartCoroutine(FilpYLerp());
    }

    private IEnumerator FilpYLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float yRotation = 0f;

        float elapsedTime = 0f;
        while (elapsedTime < _flipYRotationTime)
        {
            elapsedTime += Time.deltaTime;

            //lerp the y rotation
            yRotation = Mathf.Lerp(startRotation, endRotationAmount, (elapsedTime / _flipYRotationTime));
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            yield return null;
        }
    }

    private float DetermineEndRotation()
    {
        _isFacingRight = !_isFacingRight;

        if (_isFacingRight)
        {
            return 180f;
        }
        else
        {
            return 0f;
        }
    }

}
