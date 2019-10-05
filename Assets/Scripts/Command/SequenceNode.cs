using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BinarySignalReader;

public class SequenceNode
{
    public SequenceNode LongSignalNode { get; set; } = null;
    public SequenceNode ShortSignalNode { get; set; } = null;
}
