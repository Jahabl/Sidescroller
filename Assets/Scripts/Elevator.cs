using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Vector3 pointA;
    [SerializeField] private Vector3 pointB;
    private Vector3 target;
    private bool isMoving;

    private void Update()
    {
        if (isMoving)
        {
            if (Vector3.Distance(transform.position, target) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * moveSpeed);
            }
            else
            {
                transform.position = target;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isMoving = true;
            collision.gameObject.transform.parent = transform;

            if (transform.position == pointA)
            {
                target = pointB;
                Debug.Log("B");
            }
            else
            {
                target = pointA;
                Debug.Log("A");
            }

            Debug.Log(target);
        }
        else
        {
            isMoving = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.parent = null;
        }
    }
}
