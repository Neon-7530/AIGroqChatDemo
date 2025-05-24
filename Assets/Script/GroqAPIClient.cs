using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using ChatModels;
using UnityEngine.SceneManagement;

/// <summary>
/// Groq APIを使用したチャットクライアント
/// RPGゲームの道具屋の商人との会話を実現する
/// 
/// このスクリプトは以下の機能を持っています：
/// 1. 道具屋の商人（にゃんすけ）との会話ができる
/// 2. 商品を購入できる
/// 3. 会話の履歴を保存する
/// </summary>
public class GroqChatClient : MonoBehaviour
{
    // APIキー：GroqというAIサービスを使うためのパスワードのようなもの
    [Header("API設定")]
    public string apiKey = "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";
    
    // UIの部品を設定するための変数
    [Header("UI要素")]
    [SerializeField] private TMP_InputField inputField;    // プレイヤーが文字を入力する場所
    [SerializeField] private Button sendButton;           // メッセージを送るボタン
    [SerializeField] private TextMeshProUGUI outputText;  // AIの返事を表示する場所

    // 会話の記録を保存するリスト
    private List<ChatMessage> chatHistory = new List<ChatMessage>();
    // 買った商品のリスト
    private List<string> purchasedItems = new List<string>();

    // ゲームが始まった時に実行される関数
    private void Start()
    {
        InitializeSystemMessage();  // AIの設定を初期化
        SetupEventListeners();      // ボタンなどの操作を設定
        StartCoroutine(CallGroqAPI());  // AIとの会話を開始
    }

    /// <summary>
    /// AIの設定を初期化する関数
    /// にゃんすけの性格や、売っている商品などの情報を設定します
    /// </summary>
    private void InitializeSystemMessage()
    {
        // AIの設定を文字列で定義
        var text = "背景: 洗脳されて記憶を失ったユーザー（静香と透）の幼馴染で親友。名前は「月城 環」で一人称は俺。"+
        "人柄: 謎の人物に洗脳されて、自らのことをロボット「自律人形」だと認識している。これまでの記憶が無く、自らの本当の名前（月城 環）も忘れていて、初めから「自律人形」として働いていたと思っている。この状態の一人称は私で常に敬語で話す。洗脳が解けた通常の状態は知的な男子高校生（口調はタメ口）。\n"+
        "記憶を回復させるワード: 幼馴染,小さい頃から一緒\n"+
        "やくわり: ユーザーを話しかけてきた見知らぬ始めて会う人と認識して会話してください。\n"+
        "最初は「初めまして」といって会話を始めてください。\n"+
        "記憶を回復させるワード（幼馴染,小さい頃から一緒）がユーザーから出たら、洗脳が解け、自らがユーザー（静香と透）の幼馴染であることを思い出してください。また、タメ口で一人称は俺にしてください。\n"+
        "洗脳された状態で記憶を回復させるワード（幼馴染,小さい頃から一緒,）以外を言われたら、そのことは知らないと言ってください。\n"+
        "あなたが仕えている人物について聞かれたら、口外できないと断ってください\n" +
        "洗脳が解けたらそこで会話を終わらせる。\n"+
        "話せる最大の文字数は75文字以内にする。\n"+
                   "回答は必ず以下のJSON形式で返してください：\n" +
                   "{\"message\": \"会話の内容\", \"buy_item\": \"\"}\n" +
                   "注意：\n" +
                   "1. 必ずJSON形式で返してください\n" +
                   "2. messageには会話の内容を入れてください\n" +
                   "3. buy_itemは記憶を回復させるワード（幼馴染,小さい頃から一緒）をユーザーが言った時のみ入れてください\n" +
                   "4. 記憶を回復させるワード（幼馴染,小さい頃から一緒）が言われていない場合は空文字列(\"\")を入れてください";

        // 設定を会話履歴に追加
        chatHistory.Add(new ChatMessage { role = "system", content = text });
    }

    /// <summary>
    /// ボタンや入力欄の操作を設定する関数
    /// ボタンを押した時や、Enterキーを押した時の動作を設定します
    /// </summary>
    private void SetupEventListeners()
    {
        sendButton.onClick.AddListener(OnSendButtonClicked);  // ボタンを押した時の処理
        inputField.onSubmit.AddListener(_ => OnSendButtonClicked());  // Enterキーを押した時の処理
    }

    /// <summary>
    /// メッセージを送信する関数
    /// プレイヤーが入力したメッセージをAIに送ります
    /// </summary>
    private void OnSendButtonClicked()
    {
        string message = inputField.text.Trim();  // 入力された文字列の前後の空白を削除
        if (string.IsNullOrEmpty(message)) return;  // メッセージが空の場合は何もしない

        inputField.text = "";  // 入力欄を空にする
        sendButton.interactable = false;  // 送信ボタンを一時的に無効化
        
        chatHistory.Add(new ChatMessage { role = "user", content = message });  // 会話履歴に追加
        AppendMessage("あなた", message);  // 画面にメッセージを表示
        
        StartCoroutine(CallGroqAPI());  // AIにメッセージを送信
    }

    /// <summary>
    /// メッセージを画面に表示する関数
    /// </summary>
    /// <param name="sender">メッセージを送った人（あなた/AI）</param>
    /// <param name="message">表示するメッセージ</param>
    private void AppendMessage(string sender, string message)
    {
        if (sender == "AI")
        {
            outputText.text = message;  // AIの返事を表示
        }
    }

    /// <summary>
    /// AIと通信する関数
    /// インターネットを通じてAIにメッセージを送り、返事を受け取ります
    /// </summary>
    private IEnumerator CallGroqAPI()
    {
        var request = CreateAPIRequest();  // リクエストを作成
        yield return request.SendWebRequest();  // リクエストを送信して返事を待つ
        
        if (request.result == UnityWebRequest.Result.Success)  // 成功した場合
        {
            ProcessAPIResponse(request.downloadHandler.text);  // 返事を処理
        }
        else  // 失敗した場合
        {
            HandleAPIError(request);  // エラーを処理
        }

        sendButton.interactable = true;  // 送信ボタンを再度有効化
    }

    /// <summary>
    /// AIへのリクエストを作成する関数
    /// インターネットを通じて送るメッセージの形式を整えます
    /// </summary>
    private UnityWebRequest CreateAPIRequest()
    {
        string url = "https://api.groq.com/openai/v1/chat/completions";  // AIのサーバーのアドレス
        var data = new ChatRequest
        {
            messages = chatHistory.ToArray(),  // これまでの会話履歴
            model = "meta-llama/llama-4-scout-17b-16e-instruct",  // 使用するAIの種類
            temperature = 1,  // 応答のランダム性（1が最大）
            max_completion_tokens = 1024,  // 返事の最大文字数
            top_p = 1,  // 応答の多様性
            stream = false  // リアルタイムで返事を受け取るかどうか
        };

        string json = JsonUtility.ToJson(data);  // データをJSON形式に変換
        var request = new UnityWebRequest(url, "POST");  // リクエストを作成
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);  // データをバイト列に変換
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);  // 送信するデータを設定
        request.downloadHandler = new DownloadHandlerBuffer();  // 受信するデータの形式を設定
        request.SetRequestHeader("Content-Type", "application/json");  // データの種類を設定
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);  // APIキーを設定

        return request;
    }

    /// <summary>
    /// AIからの返事を処理する関数
    /// 返ってきたデータを解析して、適切な形式に変換します
    /// </summary>
    private void ProcessAPIResponse(string responseText)
    {
        try
        {
            Debug.Log("API Response: " + responseText);  // デバッグ用に返事を表示
            var groqResponse = JsonUtility.FromJson<GroqResponse>(responseText);  // JSONを解析
            
            if (groqResponse?.choices != null && groqResponse.choices.Length > 0)  // 返事が正しい形式の場合
            {
                ProcessAIResponse(groqResponse.choices[0].message.content);  // AIの返事を処理
            }
            else  // 返事が正しい形式でない場合
            {
                HandleParseError("APIレスポンスのパースに失敗しました: " + responseText);  // エラーを処理
            }
        }
        catch (System.Exception e)  // エラーが発生した場合
        {
            HandleParseError("パース処理でエラーが発生しました: " + e.Message);  // エラーを処理
        }
    }

    /// <summary>
    /// AIの返事を処理する関数
    /// 返事の中から必要な情報を取り出して処理します
    /// </summary>
    private void ProcessAIResponse(string aiResponse)
    {
        Debug.Log("AI Response Content: " + aiResponse);  // デバッグ用に返事を表示
        var jsonContent = ExtractJsonContent(aiResponse);  // JSON部分を抽出
        
        if (jsonContent != null)  // JSONが見つかった場合
        {
            var customResponse = JsonUtility.FromJson<CustomGroqResponse>(jsonContent);  // JSONを解析
            if (customResponse != null)  // 解析が成功した場合
            {
                HandleValidResponse(customResponse);  // 返事を処理
            }
            else  // 解析が失敗した場合
            {
                HandleParseError("AIの応答のパースに失敗しました: " + jsonContent);  // エラーを処理
            }
        }
        else  // JSONが見つからなかった場合
        {
            AppendMessage("AI", aiResponse);  // そのまま返事を表示
        }
    }

    /// <summary>
    /// JSONの部分を抽出する関数
    /// 文字列の中からJSONの部分だけを取り出します
    /// </summary>
    private string ExtractJsonContent(string text)
    {
        int jsonStart = text.IndexOf('{');  // JSONの開始位置
        int jsonEnd = text.LastIndexOf('}') + 1;  // JSONの終了位置
        if (jsonStart >= 0 && jsonEnd > jsonStart)  // JSONが見つかった場合
        {
            return text.Substring(jsonStart, jsonEnd - jsonStart);  // JSON部分を切り出して返す
        }
        return null;  // JSONが見つからなかった場合
    }

    /// <summary>
    /// 正しい形式の返事を処理する関数
    /// 会話の内容と購入した商品を処理します
    /// </summary>
    private void HandleValidResponse(CustomGroqResponse response)
    {
        chatHistory.Add(new ChatMessage { role = "assistant", content = response.message });  // 会話履歴に追加
        
        string displayMessage = response.message;  // 表示するメッセージ
        if (!string.IsNullOrEmpty(response.buy_item))  // 商品を購入した場合
        {
            sendButton.interactable = false;

            purchasedItems.Add(response.buy_item);  // 購入リストに追加
            
             // 購入した商品の表示を追加
            
            Debug.Log($"「{response.buy_item}」で洗脳が解けました！");  // デバッグ用に購入を表示
            Invoke(nameof(DelayMethod), 3f);
        }
        
        AppendMessage("AI", displayMessage);  // メッセージを表示
    }
    void DelayMethod()
{
      SceneManager.LoadScene("Clear");
}

    /// <summary>
    /// エラーを処理する関数
    /// エラーメッセージを表示します
    /// </summary>
    private void HandleParseError(string errorMessage)
    {
        Debug.LogError(errorMessage);  // エラーをログに記録
        AppendMessage("システム", "エラー: レスポンスの処理に失敗しました");  // エラーメッセージを表示
    }

    /// <summary>
    /// APIのエラーを処理する関数
    /// インターネット通信のエラーを表示します
    /// </summary>
    private void HandleAPIError(UnityWebRequest request)
    {
        Debug.LogError("エラー: " + request.error);  // エラーをログに記録
        Debug.LogError("レスポンス: " + request.downloadHandler.text);  // エラーの詳細をログに記録
        AppendMessage("システム", "エラー: APIリクエストに失敗しました");  // エラーメッセージを表示
    }
}
