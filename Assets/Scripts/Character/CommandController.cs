using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BinarySignalReader;

[RequireComponent(typeof(CommandInputDriver))]
public class CommandController : MonoBehaviour
{
    public enum Command
    {
        Walk = 0,
        Left,
        Up,
        Right,
        Down,

        NumOfCmd
    }

    [SerializeField]
    private CommandInputDriver m_InputDriver;

    [SerializeField]
    private BinarySignalReader m_Reader;

    public List<List<SignalType>> Seqs { get; set; } = new List<List<SignalType>>();

    public List<SignalType> WalkSeq { get; set; } = new List<SignalType>();
    public List<SignalType> TurnRightSeq { get; set; } = new List<SignalType>();
    public List<SignalType> TurnLeftSeq { get; set; } = new List<SignalType>();
    public List<SignalType> TurnUpSeq { get; set; } = new List<SignalType>();
    public List<SignalType> TurnDownSeq { get; set; } = new List<SignalType>();

    private void Awake()
    {
        for (Command cmd = Command.Walk; cmd < Command.NumOfCmd; cmd++)
        {
            Seqs.Add(new List<SignalType>());
        }
    }

    private void Update()
    {
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
    }

    private void OnEnable()
    {
        m_Reader.OnEndSignalSequence += handleOnEndSeq;
    }

    private void OnDisable()
    {
        m_Reader.OnEndSignalSequence -= handleOnEndSeq;
    }

    private void handleOnEndSeq(List<SignalType> seq)
    {
        // TODO: to be fixed, a binary tree
        if (matchSequences(Seqs[(int)Command.Walk], seq))
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
        else if (matchSequences(Seqs[(int)Command.Left], seq))
        {
            m_InputDriver.Direction = CommandInputDriver.MoveDirection.Left;
        }
        else if (matchSequences(Seqs[(int)Command.Up], seq))
        {
            m_InputDriver.Direction = CommandInputDriver.MoveDirection.Up;
        }
        else if (matchSequences(Seqs[(int)Command.Right], seq))
        {
            m_InputDriver.Direction = CommandInputDriver.MoveDirection.Right;
        }
        else if (matchSequences(Seqs[(int)Command.Down], seq))
        {
            m_InputDriver.Direction = CommandInputDriver.MoveDirection.Down;
        }
    }

    private bool matchSequences(List<SignalType> a, List<SignalType> b)
    {
        if (a.Count == b.Count)
        {
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
