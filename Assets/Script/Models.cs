using System;
using System.Collections.Generic;


[System.Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

[System.Serializable]
public class ChatRequest
{
    public ChatMessage[] messages;
    public string model;
    public float temperature;
    public int max_completion_tokens;
    public float top_p;
    public bool stream;
    // UnityのJsonUtilityは null をサポートしないので stop は削除
}