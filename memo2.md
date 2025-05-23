---
marp: true
theme: default
paginate: true
header: "RPGゲームの道具屋さんを作ろう！ 🎮"
footer: "© 2024 プログラミング教室"
style: |
  section {
    font-size: 1.3em;
  }
  h1 {
    font-size: 1.8em;
  }
  h2 {
    font-size: 1.4em;
  }
  code {
    font-size: 0.7em;
  }
  .note {
    font-size: 0.7em;
    color: #666;
    margin-top: 1em;
  }
  .json {
    background-color: #f5f5f5;
    padding: 1em;
    border-radius: 5px;
    font-family: monospace;
  }
  .center {
    text-align: center;
  }
  .small {
    font-size: 0.8em;
  }
  .section-title {
    font-size: 2.2em;
    font-weight: bold;
    margin-bottom: 0.2em;
  }
  .section-num {
    display: inline-block;
    background: #eac4b8;
    color: #222;
    font-size: 2em;
    font-weight: bold;
    border-radius: 8px;
    padding: 0.1em 0.7em;
    margin-bottom: 0.5em;
    margin-right: 0.5em;
    vertical-align: middle;
  }
  .section-sub {
    font-size: 1em;
    color: #444;
    margin-top: 0.5em;
    margin-bottom: 0.5em;
  }
  .step {
    display: inline-block;
    background: #4CAF50;
    color: white;
    font-size: 1.2em;
    font-weight: bold;
    border-radius: 8px;
    padding: 0.1em 0.7em;
    margin-bottom: 0.5em;
  }
---
<span class="section-num">01</span>
<span class="section-title">はじめに</span>
<div class="section-sub">RPGゲームの道具屋さんの概要と、段階的な実装方法について説明します。</div>

---
# RPGゲームの道具屋さんを作ろう！ 🎮
## コピペで作るAI会話システム

<div class="note">
※ このスライドでは、段階的にコピペしていくことで
AIと会話できる道具屋さんを作ります。
</div>

---

# 実装の流れ 📝

1. 基本的な会話システム
2. 会話履歴の管理
3. API通信の実装
4. レスポンス処理
5. 商品購入機能
6. システムメッセージ

<div class="note">
※ 各ステップのコードを順番にコピペしていくことで、
完成したシステムが作れます。
</div>

---
<span class="section-num">02</span>
<span class="section-title">基本的な会話システム</span>
<div class="section-sub">最初のステップとして、UIと基本的な会話機能を実装します。</div>

---
# ステップ1：基本構造 🏗

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GroqChatClient : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private TMP_InputField inputField;    
    [SerializeField] private Button sendButton;           
    [SerializeField] private TextMeshProUGUI outputText;  

    private void Start()
    {
        SetupEventListeners();
    }
}
```

<div class="note">
※ このコードを新しいスクリプトGroqChatClient.csに
コピペしてください。
</div>

---

# イベントリスナーの設定 🎮

```csharp
private void SetupEventListeners()
{
    sendButton.onClick.AddListener(OnSendButtonClicked);
    inputField.onSubmit.AddListener(_ => OnSendButtonClicked());
}

private void OnSendButtonClicked()
{
    string message = inputField.text.Trim();
    if (string.IsNullOrEmpty(message)) return;

    inputField.text = "";
    AppendMessage("あなた", message);
}

private void AppendMessage(string sender, string message)
{
    outputText.text = message;
}
```

<div class="note">
※ このコードを前のコードの下に追加してください。
ボタンクリックとEnterキーでメッセージを送信できます。
</div>

---
<span class="section-num">03</span>
<span class="section-title">会話履歴の管理</span>
<div class="section-sub">会話の内容を保存する機能を追加します。</div>

---
# ステップ2：会話履歴クラス 📝

```csharp
[System.Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}
```

<div class="note">
※ このクラスをGroqChatClient.csの先頭に追加してください。
会話の内容を保存するためのデータ構造です。
</div>

---

# 会話履歴の実装 💭

```csharp
private List<ChatMessage> chatHistory = new List<ChatMessage>();

private void OnSendButtonClicked()
{
    string message = inputField.text.Trim();
    if (string.IsNullOrEmpty(message)) return;

    inputField.text = "";
    chatHistory.Add(new ChatMessage { 
        role = "user", 
        content = message 
    });
    AppendMessage("あなた", message);
}
```

<div class="note">
※ このコードを前のOnSendButtonClickedメソッドと
置き換えてください。
</div>

---
<span class="section-num">04</span>
<span class="section-title">API通信の実装</span>
<div class="section-sub">AIとの通信機能を追加します。</div>

---
# ステップ3：APIリクエストクラス 📤

```csharp
[System.Serializable]
public class ChatRequest
{
    public ChatMessage[] messages;
    public string model = "meta-llama/llama-4-scout-17b-16e-instruct";
    public float temperature = 1;
    public int max_completion_tokens = 1024;
}
```

<div class="note">
※ このクラスをGroqChatClient.csに追加してください。
AIへのリクエストの形式を定義します。
</div>

---

# API通信の実装 🌐

```csharp
[Header("API設定")]
public string apiKey = "あなたのAPIキー";

private IEnumerator CallGroqAPI()
{
    var request = CreateAPIRequest();
    yield return request.SendWebRequest();
    
    if (request.result == UnityWebRequest.Result.Success)
    {
        ProcessAPIResponse(request.downloadHandler.text);
    }
    else
    {
        Debug.LogError("API Error: " + request.error);
    }
}
```

<div class="note">
※ このコードをGroqChatClient.csに追加してください。
APIキーは後で設定します。
</div>

---

# リクエスト作成メソッド 🔧

```csharp
private UnityWebRequest CreateAPIRequest()
{
    string url = "https://api.groq.com/openai/v1/chat/completions";
    var data = new ChatRequest
    {
        messages = chatHistory.ToArray()
    };

    string json = JsonUtility.ToJson(data);
    var request = new UnityWebRequest(url, "POST");
    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    request.downloadHandler = new DownloadHandlerBuffer();
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Authorization", "Bearer " + apiKey);

    return request;
}
```

<div class="note">
※ このコードをGroqChatClient.csに追加してください。
APIリクエストを作成するメソッドです。
</div>

---
<span class="section-num">05</span>
<span class="section-title">レスポンス処理</span>
<div class="section-sub">AIからの返事を処理する機能を追加します。</div>

---
# ステップ4：レスポンスクラス 📥

```csharp
[System.Serializable]
public class CustomGroqResponse
{
    public string message;
    public string buy_item;
}
```

<div class="note">
※ このクラスをGroqChatClient.csに追加してください。
AIからの返事の形式を定義します。
</div>

---

# レスポンス処理の実装 🔄

```csharp
private void ProcessAPIResponse(string responseText)
{
    try
    {
        var response = JsonUtility.FromJson<CustomGroqResponse>(responseText);
        if (response != null)
        {
            chatHistory.Add(new ChatMessage { 
                role = "assistant", 
                content = response.message 
            });
            AppendMessage("AI", response.message);
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError("Parse Error: " + e.Message);
    }
}
```

<div class="note">
※ このコードをGroqChatClient.csに追加してください。
AIからの返事を処理するメソッドです。
</div>

---
<span class="section-num">06</span>
<span class="section-title">商品購入機能</span>
<div class="section-sub">商品の購入機能を追加します。</div>

---
# ステップ5：商品購入機能 🛍

```csharp
private List<string> purchasedItems = new List<string>();

private void ProcessAPIResponse(string responseText)
{
    try
    {
        var response = JsonUtility.FromJson<CustomGroqResponse>(responseText);
        if (response != null)
        {
            chatHistory.Add(new ChatMessage { 
                role = "assistant", 
                content = response.message 
            });
            
            string displayMessage = response.message;
            if (!string.IsNullOrEmpty(response.buy_item))
            {
                purchasedItems.Add(response.buy_item);
                displayMessage += "\n\n【購入した商品】";
                foreach (var item in purchasedItems)
                {
                    displayMessage += $"\n{item}";
                }
            }
            
            AppendMessage("AI", displayMessage);
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError("Parse Error: " + e.Message);
    }
}
```

<div class="note">
※ このコードで前のProcessAPIResponseメソッドを
置き換えてください。
商品の購入機能が追加されます。
</div>

---
<span class="section-num">07</span>
<span class="section-title">システムメッセージ</span>
<div class="section-sub">AIの設定を初期化します。</div>

---
# ステップ6：システムメッセージ ⚙️

```csharp
private void InitializeSystemMessage()
{
    var text = "背景：RPGゲームの道具屋さんの商人にゃんすけ。お店の名前は「猫の道具屋さん」" +
               "人柄：猫の種族で、語尾ににゃーをつける、かなり温厚な性格\n" +
               "売っているもの：いい感じの剣，20ルピー そこそこ強い防具50ルピーのみそれ以外の商品を選ばれた場合は、ないことを伝える。\n" +
               "やくわり：このお店では、商品を買うことができます。会話をしながら、ユーザーに問いかけをしてください。それ以外は出来ないで、断る。\n" +
               "ユーザーが断った場合はそのまま会話を終える。\n" +
               "回答は必ず以下のJSON形式で返してください：\n" +
               "{\"message\": \"会話の内容\", \"buy_item\": \"\"}\n";

    chatHistory.Add(new ChatMessage { role = "system", content = text });
}

private void Start()
{
    InitializeSystemMessage();
    SetupEventListeners();
}
```

<div class="note">
※ このコードをGroqChatClient.csに追加し、
Startメソッドを更新してください。
AIの設定が初期化されます。
</div>

---
<span class="section-num">08</span>
<span class="section-title">完成と実行</span>
<div class="section-sub">実装したシステムの実行方法を説明します。</div>

---
# 実行手順 📋

1. 新しいスクリプト`GroqChatClient.cs`を作成
2. 各ステップのコードを順番にコピペ
3. Unity EditorでUIコンポーネントを設定
4. APIキーを設定
5. 実行して動作確認

<div class="note">
※ 各ステップのコードは、前のコードの上に
追加していってください。
</div>

---

# UIの設定 🎨

1. Canvasを作成
2. 以下のUI要素を追加：
   - InputField (TMP)
   - Button
   - Text (TMP)
3. 各コンポーネントをInspectorで設定

<div class="note">
※ UIコンポーネントは、Inspectorで
GroqChatClientスクリプトに
適切に設定する必要があります。
</div>

---

# 注意点 ⚠️

1. コードの重複に注意
2. UIコンポーネントの設定を忘れずに
3. APIキーは適切に管理
4. エラー処理を確認

<div class="note">
※ 各ステップを順番に実装することで、
安全にシステムを構築できます。
</div>

---

# お疲れ様でした！ 👏

次回もお楽しみに！

<div class="note">
※ プログラミングは楽しいです！
一緒に学んでいきましょう。
</div>
