using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    public float checkRate = 0.05f; // 얼마나 자주 레이를 쏠 건지
    private float lastCheckTime;    // 마지막 측정 시간
    public float maxCheckDistance;  // 얼마나 멀리 떨어져 있는지
    public LayerMask layerMask;     // 어떤 레이어가 달려있는 게임 오브젝트를 추출할건지

    public GameObject curInteractGameObject;    // 인터랙션된 게임 오브젝트의 정보
    private IInteractable curInteractable;

    public TextMeshProUGUI promptText;  // 아이템 프롬프트
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

            // ray의 오리진, 방향은 카메라의 방향을 그대로 씀
            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            // 충돌이 됐을 때
            if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                if (hit.collider.gameObject != curInteractGameObject)
                {
                    curInteractGameObject = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponent<IInteractable>();
                    SetPromptText();
                }
            }
            // 충돌이 없을 때
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
        // Started: 눌렀을 때, curInteractable != null: 빈 공간이 아닐 때
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            curInteractable.OnInteract();
            curInteractGameObject = null;
            curInteractable = null;
            promptText.gameObject.SetActive(false);
        }
    }
}
