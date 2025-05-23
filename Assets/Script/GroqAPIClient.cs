using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class GroqChatClient : MonoBehaviour
{
    public string apiKey = "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";
    
    [SerializeField] private TMP_InputField inputField;    // 入力フィールド
    [SerializeField] private Button sendButton;           // 送信ボタン
    [SerializeField] private TextMeshProUGUI outputText;  // 出力テキスト

    private void Start()
    {
        // ボタンのクリックイベントを設定
        sendButton.onClick.AddListener(OnSendButtonClicked);
        
        // 入力フィールドのEnterキーイベントを設定
        inputField.onSubmit.AddListener(_ => OnSendButtonClicked());
    }

    private void OnSendButtonClicked()
    {
        string message = inputField.text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        // 入力フィールドをクリア
        inputField.text = "";
        
        // 送信ボタンを無効化
        sendButton.interactable = false;
        
        // ユーザーのメッセージを表示
        AppendMessage("あなた", message);
        
        // APIを呼び出し
        StartCoroutine(CallGroqAPI(message));
    }

    private void AppendMessage(string sender, string message)
    {
        // メッセージを追加
        outputText.text += $"{sender}: {message}\n\n";
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
                    AppendMessage("AI", reply);
                    Debug.Log("AIの応答: " + reply);
                }
                else
                {
                    Debug.LogWarning("レスポンスのパースに失敗しました: " + request.downloadHandler.text);
                    AppendMessage("システム", "エラー: レスポンスのパースに失敗しました");
                }
            }
            else
            {
                Debug.LogError("エラー: " + request.error);
                Debug.LogError("レスポンス: " + request.downloadHandler.text);
                AppendMessage("システム", "エラー: APIリクエストに失敗しました");
            }
        }

        // 送信ボタンを再度有効化
        sendButton.interactable = true;
    }
}
