using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CommandInputDriver))]
public class DebugCommandController : MonoBehaviour
{
    [SerializeField]
    private CommandInputDriver m_InputDriver;

    private void Update()
    {
        float horiztonal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.RightArrow))
        {
            m_InputDriver.Direction = CommandInputDriver.MoveDirection.Right;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            m_InputDriver.Direction = CommandInputDriver.MoveDirection.Left;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            m_InputDriver.Direction = CommandInputDriver.MoveDirection.Up;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            m_InputDriver.Direction = CommandInputDriver.MoveDirection.Down;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (m_InputDriver.State == CommandInputDriver.MovementState.Idle)
            {
                m_InputDriver.State = CommandInputDriver.MovementState.Walking;
            }
            else
            {
                m_InputDriver.State = CommandInputDriver.MovementState.Idle;
            }
        }
    }
}
