using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(AgentController))]
public class CommandInputDriver : MonoBehaviour
{
    private static readonly Vector2[] s_Dirs = new Vector2[] { Vector2.right, Vector2.up, -Vector2.right, -Vector2.up };

    public enum MovementState
    {
        Idle,
        Walking
    }

    public enum MoveDirection
    {
        Right = 0,
        Up,
        Left,
        Down
    }

    [SerializeField]
    private AgentController m_Agent;

    private MovementState m_State = MovementState.Idle;
    public MovementState State
    {
        get
        {
            return m_State;
        }

        set
        {
            var prev = m_State;
            m_State = value;

            if (prev != value)
            {
                OnMovementStateChanged.Invoke(prev, value);
            }

            OnReceivedCommand.Invoke();
        }
    }

    private MoveDirection m_Direction = MoveDirection.Up;
    public MoveDirection Direction
    {
        get
        {
            return m_Direction;
        }

        set
        {
            var prev = m_Direction;
            m_Direction = value;

            if (prev != value)
            {
                OnDirectionChanged.Invoke(prev, value);
            }

            OnReceivedCommand.Invoke();
        }
    }

    public event Action<MovementState, MovementState> OnMovementStateChanged = delegate { };
    public event Action<MoveDirection, MoveDirection> OnDirectionChanged = delegate { };
    public event Action OnReceivedCommand = delegate { };

    // Update is called once per frame
    void Update()
    {
        var dir = s_Dirs[(int)Direction];

        m_Agent.Face(dir);

        switch (State)
        {
            case MovementState.Idle:
                m_Agent.Move(Vector2.zero);
                break;
            case MovementState.Walking:
                m_Agent.Move(dir);
                break;
        }
    }
}