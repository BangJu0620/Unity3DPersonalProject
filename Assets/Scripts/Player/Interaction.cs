using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    public float checkRate = 0.05f; // �󸶳� ���� ���̸� �� ����
    private float lastCheckTime;    // ������ ���� �ð�
    public float maxCheckDistance;  // �󸶳� �ָ� ������ �ִ���
    public LayerMask layerMask;     // � ���̾ �޷��ִ� ���� ������Ʈ�� �����Ұ���

    public GameObject curInteractGameObject;    // ���ͷ��ǵ� ���� ������Ʈ�� ����
    private IInteractable curInteractable;

    public TextMeshProUGUI promptText;  // ������ ������Ʈ
    private Camera camera;

    void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        if (Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

            // ray�� ������, ������ ī�޶��� ������ �״�� ��
            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            // �浹�� ���� ��
            if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                if (hit.collider.gameObject != curInteractGameObject)
                {
                    curInteractGameObject = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponent<IInteractable>();
                    SetPromptText();
                }
            }
            // �浹�� ���� ��
            else
            {
                curInteractGameObject = null;
                curInteractable = null;
                promptText.gameObject.SetActive(false);
            }
        }
    }

    private void SetPromptText()
    {
        promptText.gameObject.SetActive(true);
        promptText.text = curInteractable.GetInteractPrompt();
    }

    public void OnInteractInput(InputAction.CallbackContext context)
    {
        // Started: ������ ��, curInteractable != null: �� ������ �ƴ� ��
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            curInteractable.OnInteract();
            curInteractGameObject = null;
            curInteractable = null;
            promptText.gameObject.SetActive(false);
        }
    }
}
