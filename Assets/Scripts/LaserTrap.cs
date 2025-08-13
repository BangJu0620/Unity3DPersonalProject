using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LaserTrap : MonoBehaviour
{
    public LayerMask playerLayerMask;
    public float distance;
    public Vector3 origin;
    public TextMeshProUGUI warningText;

    void Start()
    {
        
    }

    void Update()
    {
        Debug.DrawRay(transform.position + origin, Vector3.forward, Color.red);

        if (Physics.Raycast(transform.position + origin, Vector3.forward, distance, playerLayerMask))
        {
            // 경고 띄우기
            Debug.Log("Player 감지");
            warningText.gameObject.SetActive(true);
        }
        else
        {
            warningText.gameObject.SetActive(false);
        }
    }
}
