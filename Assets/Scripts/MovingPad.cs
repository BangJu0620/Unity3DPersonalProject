using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MovingPad : MonoBehaviour
{
    public float speed;
    public float distance;
    public LayerMask playerLayerMask;

    private float newXPos;
    private Vector3 startPos;
    private bool isUpPos = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        newXPos = startPos.x + Mathf.PingPong(speed * Time.time, distance);
        transform.position = new Vector3(newXPos, startPos.y, startPos.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((playerLayerMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            foreach(var contact in collision.contacts)
            {
                if(Vector3.Dot(contact.normal, Vector3.up) < -0.7f)
                {
                    isUpPos = true;
                }
            }
            if(isUpPos) collision.transform.parent = transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if ((playerLayerMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            collision.transform.parent = null;
            isUpPos = false;
        }
    }
}
