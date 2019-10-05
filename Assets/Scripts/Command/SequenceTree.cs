using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceTree
{
    public SequenceNode RootNode { get; private set; } = new SequenceNode(null, null, CommandController.Command.Empty);

    public bool PushNewCommand(List<BinarySignalReader.SignalType> sequence, CommandController.Command cmd)
    {
        var currNode = RootNode;

        foreach (var signal in sequence)
        {
            if (signal == BinarySignalReader.SignalType.Long)
            {
                if (currNode.LongSignalNode == null)
                {
                    currNode.LongSignalNode = new SequenceNode(null, null, CommandController.Command.Empty);
                }
                currNode = currNode.LongSignalNode;
            }
            else
            {
                if (currNode.ShortSignalNode == null)
                {
                    currNode.ShortSignalNode = new SequenceNode(null, null, CommandController.Command.Empty);
                }
                currNode = currNode.ShortSignalNode;
            }
        }

        if (currNode.Command == CommandController.Command.Empty)
        {
            currNode.Command = cmd;
            return true;
        }
        else
        {
            return false;
        }
    }

    public CommandController.Command GetCommand(List<BinarySignalReader.SignalType> sequence)
    {
        var currNode = RootNode;

        foreach (var signal in sequence)
        {
            if (signal == BinarySignalReader.SignalType.Long)
            {
                if (currNode.LongSignalNode == null)
                {
                    return CommandController.Command.Empty;
                }
                currNode = currNode.LongSignalNode;
            }
            else
            {
                if (currNode.ShortSignalNode == null)
                {
                    return CommandController.Command.Empty;
                }
                currNode = currNode.ShortSignalNode;
            }
        }
        return currNode.Command;
    }

}
