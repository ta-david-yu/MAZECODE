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

        Confirm,
        Cancel,

        Empty,

        NumOfCmd
    }

    [SerializeField]
    private CommandInputDriver m_InputDriver;

    [SerializeField]
    private BinarySignalReader m_Reader;

    public SequenceTree SeqTree { get; private set; } = new SequenceTree();

    private void Awake()
    {
        SeqTree.PushNewCommand(new List<SignalType>() { SignalType.Long }, Command.Cancel);
        SeqTree.PushNewCommand(new List<SignalType>() { SignalType.Short }, Command.Confirm);
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
        var cmd = SeqTree.GetCommand(seq);

        switch (cmd)
        {
            case Command.Walk:
                if (m_InputDriver.State == CommandInputDriver.MovementState.Idle)
                {
                    m_InputDriver.State = CommandInputDriver.MovementState.Walking;
                }
                else
                {
                    m_InputDriver.State = CommandInputDriver.MovementState.Idle;
                }
                break;
            case Command.Left:
                m_InputDriver.Direction = CommandInputDriver.MoveDirection.Left;
                break;
            case Command.Up:
                m_InputDriver.Direction = CommandInputDriver.MoveDirection.Up;
                break;
            case Command.Right:
                m_InputDriver.Direction = CommandInputDriver.MoveDirection.Right;
                break;
            case Command.Down:
                m_InputDriver.Direction = CommandInputDriver.MoveDirection.Down;
                break;
            case Command.Confirm:
                break;
            case Command.Cancel:
                break;
            case Command.Empty:
                break;
        }
    }
}
