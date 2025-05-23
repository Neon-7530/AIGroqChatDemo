using System;

/// <summary>
/// チャット関連のデータモデルを定義するクラス
/// </summary>
namespace ChatModels
{
    /// <summary>
    /// チャットメッセージを表すクラス
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        public string role;
        public string content;
    }

    /// <summary>
    /// APIリクエスト用のデータクラス
    /// </summary>
    [Serializable]
    public class ChatRequest
    {
        public ChatMessage[] messages;
        public string model;
        public float temperature;
        public int max_completion_tokens;
        public float top_p;
        public bool stream;
    }

    /// <summary>
    /// Groq APIのレスポンスを表すクラス
    /// </summary>
    [Serializable]
    public class GroqResponse
    {
        public string id;
        public string @object;
        public long created;
        public string model;
        public Choice[] choices;
        public Usage usage;
    }

    /// <summary>
    /// APIレスポンスの選択肢を表すクラス
    /// </summary>
    [Serializable]
    public class Choice
    {
        public int index;
        public Message message;
    }

    /// <summary>
    /// メッセージを表すクラス
    /// </summary>
    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    /// <summary>
    /// API使用量を表すクラス
    /// </summary>
    [Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    /// <summary>
    /// カスタムレスポンスを表すクラス
    /// </summary>
    [Serializable]
    public class CustomGroqResponse
    {
        public string message;
        public string buy_item;
    }
} 