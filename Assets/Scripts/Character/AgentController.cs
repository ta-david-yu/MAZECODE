using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    [Header("Reference")]

    [SerializeField]
    private Transform m_Root;

    [Header("Settings")]

    [SerializeField]
    private float m_Speed = 1.0f;

    [SerializeField]
    private float m_RotateAngularSpeed = 90.0f;

    public void Move(Vector2 movement)
    {
        if (movement.sqrMagnitude > 1)
        {
            movement = movement.normalized;
        }
        movement *= m_Speed * Time.deltaTime;

        m_Root.transform.position += new Vector3(movement.x, movement.y, 0);
    }

    public void Face(Vector2 direction)
    {
        Vector2 forward = m_Root.right;
        var angle = Vector2.SignedAngle(forward, direction);
        var unsignedAngle = Mathf.Abs(angle);

        if (unsignedAngle > 0)
        {
            float maxAngularSpeed = m_RotateAngularSpeed * Time.deltaTime;
            if (unsignedAngle > maxAngularSpeed)
            {
                direction = forward.Rotate(maxAngularSpeed * Mathf.Sign(angle));
            }

            m_Root.right = direction;
        }
    }
}
