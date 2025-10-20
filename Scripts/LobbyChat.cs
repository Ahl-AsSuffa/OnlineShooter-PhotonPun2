using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LobbyChat : MonoBehaviour, IChatClientListener
{
    [Header("UI")]
    public InputField inputField;
    public Transform chatContent;
    public GameObject chatMessagePrefab;

    [Header("Player")]
    public PlayerStats playerStats;

    private ChatClient chatClient;
    private string currentChannel = "LobbyChannel";

    [Serializable]
    public class ChatMessageData
    {
        public string msg;
        public int rank;
    }
    public void OnJoined()
    {
        Application.runInBackground = true;

        AuthenticationValues auth = new AuthenticationValues
        {
            UserId = playerStats.PlayerNickName // Важно!
        };

        chatClient = new ChatClient(this);
        chatClient.Connect(
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
            "1.0", auth
        );

        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    void Update()
    {
        if (chatClient != null)
            chatClient.Service();
    }

    public void SendChatMessage()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            ChatMessageData data = new ChatMessageData
            {
                msg = inputField.text,
                rank = playerStats.Rank
            };
            string json = JsonUtility.ToJson(data);
            chatClient.PublishMessage(currentChannel, json);
            inputField.text = "";
        }
    }

    public void OnConnected()
    {
        Debug.Log("Подключено к Photon Chat");
        chatClient.Subscribe(new string[] { currentChannel });
    }

    public void OnDisconnected() => Debug.Log("Отключено от Photon Chat");
    public void OnChatStateChange(ChatState state) { }
    public void OnSubscribed(string[] channels, bool[] results) => Debug.Log("Подписан на: " + string.Join(",", channels));
    public void OnUnsubscribed(string[] channels) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void DebugReturn(DebugLevel level, string message) => Debug.Log($"[PhotonChat][{level}]: {message}");

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < messages.Length; i++)
        {
            ChatMessageData data = JsonUtility.FromJson<ChatMessageData>(messages[i].ToString());

            GameObject chatMsg = Instantiate(chatMessagePrefab, chatContent);
            chatMsg.transform.Find("PlayerNickName").GetComponent<Text>().text = senders[i];
            chatMsg.transform.Find("MessageText").GetComponent<Text>().text = data.msg;
            chatMsg.transform.Find("RankImg").GetComponent<Image>().sprite = playerStats.RankSprites[data.rank - 1];
        }
    }

    public void OnUserSubscribed(string channel, string user) =>
        Debug.Log($"Пользователь {user} подписался на канал {channel}");

    public void OnUserUnsubscribed(string channel, string user) =>
        Debug.Log($"Пользователь {user} отписался от канала {channel}");

    private void OnEndEdit(string value)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage();
            inputField.ActivateInputField();
        }
    }
}
