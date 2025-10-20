using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using Photon.Pun;

[System.Serializable]
public class LoginRequest
{
    public string Username;
    public string Password;
}
[Serializable]
public class RegisterRequest
{
    public string Username;
    public string Password;
    [JsonProperty("StatsJson")]
    public string StatsJson;
    public bool IsBanned = false;
    public string BanReason = "";
    public string Role = "player";
}
[System.Serializable]
public class PlayerResponse
{
    public int Id;
    public string Username;
    [JsonProperty("statsJson")]
    public string StatsJson;
    public bool IsBanned;
    public string BanReason;
    public string Role = "player";
}
[Serializable]
public class UpdateStatsRequest
{
    public string Username;
    [JsonProperty("StatsJson")]
    public string StatsJson;
}
public class ServerClientConnect : MonoBehaviour
{
    public ConnectToServer connectToServer;
    public InputField loginInput, passInput;
    public PlayerStats playerStats;
    public string Username = "PlayerUserName";
    public string Password = "";
    private void Start()
    {
        if (loginInput == null)
            return;
        Username = PlayerPrefs.GetString("username");
        if(Username != "")
            loginInput.text = Username;
        Password = PlayerPrefs.GetString("password");
        if(Password != "")
        {
            passInput.text = Password;
            PlayerLogin();
        }
    }
    public void NewPlayerRegister()
    {
        string username = loginInput.text.Trim();
        string password = passInput.text.Trim();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            connectToServer.StartErrorText(Color.red, "Логин и/или пароль не заполнены!",0);
            return;
        }
        RegisterPlayer(username, password);
    }
    private string apiUrl = "https://gamedbmygameserver.ru:5500/api/players";
    //private string apiUrl = "http://localhost:5500/api/players";
    private void RegisterPlayer(string username, string password)
    {
        PlayerStatsData data = new PlayerStatsData();
        RegisterRequest newPlayer = new RegisterRequest
        {
            Username = username,
            Password = password,
            StatsJson = JsonConvert.SerializeObject(data),
            IsBanned = false,
            BanReason = "",
            Role = "player"
        };
        string json = JsonConvert.SerializeObject(newPlayer);

        // Добавляем "/register" в url, чтобы попасть в нужный POST метод контроллера
        StartCoroutine(PostRequest(apiUrl + "/register", json));
    }

    private IEnumerator PostRequest(string url, string json)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            connectToServer.StartErrorText(Color.green, "Успешная регистрация!", 1);
            PlayerPrefs.SetString("username",loginInput.text);
            Username = PlayerPrefs.GetString("username");
            connectToServer.AutorizePanel.SetActive(false);
            connectToServer.Autorized = 1;
            connectToServer.OnJoinedLobby();
            StartCoroutine(ApplyPlayerStats(Username));
        }
        else if (request.responseCode == 409) // HTTP 409 Conflict
        {
            connectToServer.StartErrorText(Color.red, "Такой логин уже существует!", 1);
        }
        else
        {
            Debug.LogError("Ошибка при регистрации: " + request.error);
        }
    }
    public void PlayerLogin()
    {
        string username = loginInput.text.Trim();
        string password = passInput.text.Trim();
        StartCoroutine(LoginPlayer(username, password)); // обязательно через StartCoroutine, иначе IEnumerator не запустится
    }

    private IEnumerator LoginPlayer(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            connectToServer.StartErrorText(Color.red, "Логин и пароль не могут быть пустыми!", 1);
            yield break;
        }

        LoginRequest loginRequest = new LoginRequest
        {
            Username = username,
            Password = password
        };

        string json = JsonUtility.ToJson(loginRequest);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl + "/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                HandleLoginResponse(request.downloadHandler.text);

                connectToServer.StartErrorText(Color.green, "Вход выполнен!", 1);
                PlayerPrefs.SetString("username", loginInput.text.Trim());
                Username = PlayerPrefs.GetString("username");
                PlayerPrefs.SetString("password",passInput.text.Trim());
                connectToServer.AutorizePanel.SetActive(false);
                connectToServer.Autorized = 1;
                connectToServer.OnJoinedLobby();
            }
            else if (request.responseCode == 404)
            {
                connectToServer.StartErrorText(Color.red, "Пользователь не найден!", 1);
            }
            else if (request.responseCode == 401)
            {
                connectToServer.StartErrorText(Color.red, "Неверный пароль!", 1);
            }
            else
            {
                Debug.LogError("Ошибка при входе: " + request.error);
                connectToServer.StartErrorText(Color.red, "Ошибка при входе. Попробуйте позже.", 1);
            }
        }
    }

    private void HandleLoginResponse(string json)
    {
        PlayerResponse response = JsonConvert.DeserializeObject<PlayerResponse>(json);

        if (!string.IsNullOrEmpty(response.StatsJson))
        {
            PlayerStatsData stats = JsonConvert.DeserializeObject<PlayerStatsData>(response.StatsJson);
            playerStats.ApplyStats(stats);
            playerStats.Role = response.Role;
            playerStats.IsBanned = response.IsBanned;
            playerStats.BanReason = response.BanReason;

            if(playerStats.IsBanned)
            {
                connectToServer.BanPanel.SetActive(true);
                connectToServer.BanReasonText.text = response.BanReason;
            }
        }
        else
        {
            Debug.LogWarning("StatsJson пустой или отсутствует");
        }
    }
    public void UpdatePlayerStats(string username, PlayerStatsData updatedStats)
    {
        username = PlayerPrefs.GetString("username");
        UpdateStatsRequest request = new UpdateStatsRequest
        {
            Username = username,
            StatsJson = JsonUtility.ToJson(updatedStats)
        };
        string json = JsonUtility.ToJson(request);
        StartCoroutine(PostUpdateStats(apiUrl + "/updateStats", json));
    }

    private IEnumerator PostUpdateStats(string url, string json)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Статистика успешно обновлена на сервере");
        }
        else
        {
            Debug.LogError("Ошибка при обновлении статистики: " + request.error);
        }
    }
    public IEnumerator ApplyPlayerStats(string username)
    {
        string url = apiUrl + "/getplayerstats?username=" + UnityWebRequest.EscapeURL(username);
        Debug.Log("PlayerLogin = " + username);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;

                // Распарсить ответ
                PlayerResponse response = JsonConvert.DeserializeObject<PlayerResponse>(json);

                if (!string.IsNullOrEmpty(response.StatsJson))
                {
                    PlayerStatsData stats = JsonConvert.DeserializeObject<PlayerStatsData>(response.StatsJson);
                    playerStats.ApplyStats(stats);
                    playerStats.Role = response.Role;
                    playerStats.IsBanned = response.IsBanned;
                    playerStats.BanReason = response.BanReason;
                    Debug.Log("Статистика успешно применена");
                    PhotonNetwork.LocalPlayer.NickName = playerStats.PlayerNickName;
                    playerStats.SetPlayerData();
                }
                else
                {
                    Debug.LogWarning("StatsJson пустой или отсутствует");
                }
            }
            else
            {
                Debug.LogError("Ошибка при получении статистики: " + request.error);
            }
        }
    }
}
