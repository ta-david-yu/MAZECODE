using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BinarySignalReader;

public class SequenceNode
{
    public SequenceNode LongSignalNode { get; set; } = null;
    public SequenceNode ShortSignalNode { get; set; } = null;

    public CommandController.Command Command { get; set; } = CommandController.Command.Empty;

    public SequenceNode(SequenceNode longNode, SequenceNode shortNode, CommandController.Command cmd)
    {

    }
}
