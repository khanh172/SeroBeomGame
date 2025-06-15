using System;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string content;
}
