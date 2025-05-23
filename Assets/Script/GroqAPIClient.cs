using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GroqChatClient : MonoBehaviour
{
    public string apiKey = "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";

    void Start()
    {
        StartCoroutine(CallGroqAPI("こんにちは！"));
    }

    IEnumerator CallGroqAPI(string userMessage)
    {
        string url = "https://api.groq.com/openai/v1/chat/completions";

        // リクエストオブジェクトを作成
        ChatRequest data = new ChatRequest
        {
            messages = new ChatMessage[]
            {
                new ChatMessage { role = "user", content = userMessage }
            },
            model = "meta-llama/llama-4-scout-17b-16e-instruct",
            temperature = 1,
            max_completion_tokens = 1024,
            top_p = 1,
            stream = false
        };

        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // ここでJSONをパース
                var response = JsonUtility.FromJson<GroqResponse>(request.downloadHandler.text);
                if (response != null && response.choices != null && response.choices.Length > 0)
                {
                    string reply = response.choices[0].message.content;
                    Debug.Log("AIの返答: " + reply);
                }
                else
                {
                    Debug.LogWarning("レスポンスのパースに失敗しました: " + request.downloadHandler.text);
                }
            }
            else
            {
                Debug.LogError("エラー: " + request.error);
                Debug.LogError("レスポンス: " + request.downloadHandler.text);
            }
        }
    }
}

// Groq APIレスポンス用クラス
[System.Serializable]
public class GroqResponse
{
    public string id;
    public string @object;
    public long created;
    public string model;
    public Choice[] choices;
    public Usage usage;
}

[System.Serializable]
public class Choice
{
    public int index;
    public Message message;
    public string finish_reason;
}

[System.Serializable]
public class Message
{
    public string role;
    public string content;
}

[System.Serializable]
public class Usage
{
    public int prompt_tokens;
    public int completion_tokens;
    public int total_tokens;
}
