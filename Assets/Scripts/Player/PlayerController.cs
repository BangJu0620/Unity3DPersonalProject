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
    // ����
    public float jumpPower;
    public float jumpStamina;
    // �޸���
    public float dashPower;
    public float dashStamina;
    // �Ŵ޸���
    public float climbSpeed;
    public float hangStamina;
    public float climbStamina;
    // bool��
    public bool isDashing = false;
    public bool isHanging = false;
    public bool isClimbing = false;
    public bool isDoubleJump = false;   // ���� ���� ������ ����°�
    public bool canDoubleJump = false;  // ���� ���� ������ ������ �����ΰ�
    // �̵� ���(�Ŵ޸��� �� ���)
    private bool isXLocked;
    private bool isZLocked;
    // ���� �ִ��� Ȯ���� �� ���̾�
    public LayerMask groundLayerMask;
    // ���� �پ����� Ȯ���� �� ����
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
                // �Ŵ޸��� ����
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
        // ī�޶� ���� ������
        camCurXRot += mouseDelta.y * lookSensitivity;
        // ��, �Ʒ��� �ִ� ���� ����
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        // ���� ��ǥ�� ������� ����
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

        // �������°� ���߱�
        _rigidbody.useGravity = !isHanging;
        // �÷��̾� ���� ����
        LockPlayerRotation(isHanging);
    }

    public void Jump(float jumpPower)
    {
        _rigidbody.AddForce(Vector2.up * jumpPower, ForceMode.Impulse);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // InputActionPhase.Performed : ��� ������ ���� ��
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
        // ���콺�� ��� �����ϱ� ������ �Է¹�����, �ƴҶ� ���� �ʿ䰡 ����
        mouseDelta = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        // InputActionPhase.Started : ������ ��
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
        // ������ ���̸� ���� ���� �پ��ִ��� Ȯ��
        // �÷��̾� ������ ���ϱ� ���� ���̾��ũ ����
        Ray[] rays = new Ray[4]
        {
            // �׶��忡�� ���̸� ��� �Ǹ� �׶��带 ��������
            // ������ ���ϴ� ��츦 �����ϱ� ���� ��¦ ���� �÷��ֱ�
            // x, z �� ���ʿ� 1���� �� 4��
            new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.3f), Vector3.down),
            new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.3f), Vector3.down),
            new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.3f), Vector3.down),
            new Ray(transform.position + (-transform.right * 0.2f) + (transform.up * 0.3f), Vector3.down)
        };
        
        // ray 4���� �� ���� �ϳ��� �ε����� true ��ȯ
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
            // �Ŵ޸���, ���� �� ������
            isHanging = !isHanging;
            // �������°� ���߱�
            _rigidbody.useGravity = !isHanging;
            // �÷��̾� ���� ����
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
            // z ��ġ ����
            isZLocked = isActive;
        }
        else
        {
            // x ��ġ ����
            isXLocked = isActive;
        }
        //lookSensitivity = baseLookSensitivity * num;
    }

    bool isWall()
    {
        // ������ ���̸� ���� ���� �ִ��� Ȯ��
        // �÷��̾� ������ ���ϱ� ���� ���̾��ũ ����
        Ray[] rays = new Ray[3]
        {
            new Ray(transform.position + (transform.up * 0.4f), transform.forward),
            new Ray(transform.position + (transform.up * 0.8f), transform.forward),
            new Ray(transform.position + (transform.up * 1.2f), transform.forward),
        };

        // ray 3���� �� ���� �ϳ��� �ε����� true ��ȯ
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
