using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float jumpPower;
    public float jumpStamina;
    public float dashPower;
    public float dashStamina;
    public bool isDashing = false;
    private Vector2 curMovementInput;
    public LayerMask groundLayerMask;

    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;
    public float maxXLook;
    private float camCurXRot;
    public float camDistance;
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
        Cursor.lockState = CursorLockMode.Locked;
        Vector3 camPos = cameraContainer.transform.position;
        camPos.z -= camDistance;
        cameraContainer.transform.position = camPos;
    }
    private void Update()
    {
        if (isDashing)
        {
            PlayerManager.Instance.Player.condition.stamina.Subtract(dashStamina * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        if (canLook)
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
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovementInput = Vector2.zero;
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
        if (context.phase == InputActionPhase.Started && isGrounded() && PlayerManager.Instance.Player.condition.UseStamina(jumpStamina))
        {
            Jump(this.jumpPower);
            //_rigidbody.AddForce(Vector2.up * jumpPower, ForceMode.Impulse);
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed && isGrounded() && !isDashing)
        {
            moveSpeed *= dashPower;
            isDashing = true;
        }
        if(context.phase == InputActionPhase.Canceled && isDashing)
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
