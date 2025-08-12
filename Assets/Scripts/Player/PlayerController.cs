using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private Vector2 curMovementInput;
    // 점프
    public float jumpPower;
    public float jumpStamina;
    // 달리기
    public float dashPower;
    public float dashStamina;
    // 매달리기
    public float climbSpeed;
    public float hangStamina;
    public float climbStamina;
    // bool값
    public bool isDashing = false;
    public bool isHanging = false;
    public bool isClimbing = false;
    public bool isDoubleJump = false;   // 더블 점프 버프를 얻었는가
    public bool canDoubleJump = false;  // 현재 더블 점프가 가능한 상태인가
    // 이동 잠금(매달리기 때 사용)
    private bool isXLocked;
    private bool isZLocked;
    // 땅에 있는지 확인할 땅 레이어
    public LayerMask groundLayerMask;
    // 벽에 붙었는지 확인할 벽 정보
    public LayerMask wallLayerMask;
    public RaycastHit wall;

    [Header("Look")]
    public Transform cameraContainer;
    public Transform camera;
    public float minXLook;
    public float maxXLook;
    private float camCurXRot;
    public float camDistance;
    private float camDistanceX = 0.35f;
    private float camDistanceY = 0.25f;
    private float baseLookSensitivity;
    public float lookSensitivity;
    private Vector2 mouseDelta;
    public bool canLook = true;

    public Action inventory;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        baseLookSensitivity = lookSensitivity;
        Cursor.lockState = CursorLockMode.Locked;
        Vector3 camPos = camera.transform.position;
        camPos.x += camDistanceX * camDistance;
        camPos.y += camDistanceY * camDistance;
        camPos.z -= camDistance;
        camera.transform.position = camPos;
    }
    private void Update()
    {
        if (isDashing)
        {
            PlayerManager.Instance.Player.condition.DrainStamina(dashStamina);
        }
        if (isHanging)
        {
            PlayerManager.Instance.Player.condition.DrainStamina(hangStamina);
            if (isClimbing)
            {
                PlayerManager.Instance.Player.condition.DrainStamina(climbStamina);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isHanging)
        {
            Climb();
            if (!isWall())
            {
                // 매달리기 종료
                HangExit();
            }
        }
        else
        {
            Move();
        }
    }

    private void LateUpdate()
    {
        if (canLook && !isHanging)
        {
            CameraLook();
        }
    }

    void CameraLook()
    {
        // 카메라 상하 움직임
        camCurXRot += mouseDelta.y * lookSensitivity;
        // 위, 아래의 최대 범위 지정
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        // 로컬 좌표를 기반으로 설정
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);

        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);
    }

    void Move()
    {
        Vector3 dir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
        dir *= moveSpeed;
        dir.y = _rigidbody.velocity.y;

        _rigidbody.velocity = dir;
    }

    void Climb()
    {
        Vector3 dir = transform.up * curMovementInput.y + transform.right * curMovementInput.x;
        dir *= climbSpeed;
        //dir.y = _rigidbody.velocity.y;
        if (isXLocked)
        {
            dir.x = _rigidbody.velocity.x;
        }
        if (isZLocked)
        {
            dir.z = _rigidbody.velocity.z;
        }

        _rigidbody.velocity = dir;
    }

    void HangExit()
    {
        isHanging = false;
        isClimbing = false;

        // 떨어지는거 멈추기
        _rigidbody.useGravity = !isHanging;
        // 플레이어 방향 고정
        LockPlayerRotation(isHanging);
    }

    public void Jump(float jumpPower)
    {
        _rigidbody.AddForce(Vector2.up * jumpPower, ForceMode.Impulse);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // InputActionPhase.Performed : 계속 누르고 있을 때
        if (context.phase == InputActionPhase.Performed)
        {
            curMovementInput = context.ReadValue<Vector2>();
            if (isHanging)
            {
                isClimbing = true;
            }
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovementInput = Vector2.zero;
            isClimbing = false;
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // 마우스는 계속 존재하기 때문에 입력받을때, 아닐때 나눌 필요가 없다
        mouseDelta = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // InputActionPhase.Started : 눌렀을 때
        if (context.phase == InputActionPhase.Started && isGrounded() && !isWall())
        {
            if (PlayerManager.Instance.Player.condition.UseStamina(jumpStamina))
            {
                Jump(this.jumpPower);
            }
        }
        if (context.phase == InputActionPhase.Started && !isGrounded() && !isWall() && isDoubleJump && canDoubleJump)
        {
            if (PlayerManager.Instance.Player.condition.UseStamina(jumpStamina))
            {
                Jump(this.jumpPower);
                canDoubleJump = false;
            }
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started && isGrounded() && PlayerManager.Instance.Player.condition.DrainStamina(dashStamina))
        {
            moveSpeed *= dashPower;
            isDashing = !isDashing;
        }
        if ((context.phase == InputActionPhase.Canceled && isDashing) || !PlayerManager.Instance.Player.condition.DrainStamina(dashStamina))
        {
            moveSpeed /= dashPower;
            isDashing = false;
        }
    }

    bool isGrounded()
    {
        // 밑으로 레이를 쏴서 땅에 붙어있는지 확인
        // 플레이어 감지를 피하기 위해 레이어마스크 설정
        Ray[] rays = new Ray[4]
        {
            // 그라운드에서 레이를 쏘게 되면 그라운드를 지나가서
            // 인지를 못하는 경우를 예방하기 위해 살짝 위로 올려주기
            // x, z 축 양쪽에 1개씩 총 4개
            new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.3f), Vector3.down),
            new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.3f), Vector3.down),
            new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.3f), Vector3.down),
            new Ray(transform.position + (-transform.right * 0.2f) + (transform.up * 0.3f), Vector3.down)
        };
        
        // ray 4개를 다 쏴서 하나라도 부딪히면 true 반환
        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], 0.3f, groundLayerMask))
            {
                canDoubleJump = true;
                return true;
            }
        }

        return false;
    }

    public void OnHanging(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started && isWall())
        {
            // 매달리기, 해제 온 오프식
            isHanging = !isHanging;
            // 떨어지는거 멈추기
            _rigidbody.useGravity = !isHanging;
            // 플레이어 방향 고정
            LockPlayerRotation(isHanging);
        }
    }

    void LockPlayerRotation(bool isActive)
    {
        int num = 0;
        if (isActive) num = 0;
        else num = 1;
        //Quaternion playerRot = transform.rotation;
        //playerRot.y = Vector3.Dot(wall.normal, Vector3.forward);
        //transform.rotation = playerRot;
        Debug.Log(Vector3.Dot(wall.normal, Vector3.forward));
        if(Mathf.Abs(Vector3.Dot(wall.normal, Vector3.forward)) > 0.7f)
        {
            // z 위치 고정
            isZLocked = isActive;
        }
        else
        {
            // x 위치 고정
            isXLocked = isActive;
        }
        //lookSensitivity = baseLookSensitivity * num;
    }

    bool isWall()
    {
        // 앞으로 레이를 쏴서 벽이 있는지 확인
        // 플레이어 감지를 피하기 위해 레이어마스크 설정
        Ray[] rays = new Ray[3]
        {
            new Ray(transform.position + (transform.up * 0.4f), transform.forward),
            new Ray(transform.position + (transform.up * 0.8f), transform.forward),
            new Ray(transform.position + (transform.up * 1.2f), transform.forward),
        };

        // ray 3개를 다 쏴서 하나라도 부딪히면 true 반환
        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], out wall, 0.3f, wallLayerMask))
            {
                return true;
            }
        }

        return false;
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }

    void ToggleCursor()
    {
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }
}
