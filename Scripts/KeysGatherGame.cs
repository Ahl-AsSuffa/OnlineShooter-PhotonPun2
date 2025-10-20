using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class KeysGatherGame : MonoBehaviourPunCallbacks
{
    public int MaxKeys = 19, currentKeys = 0;
    public PhotonView photoView;
    public DeathMatchGameManager gameManager;
    public GameObject KeyPrefab;
    public Text SpawnKeyTimerText;
    public float timerToSpawnNextKey;
    public bool TimerStarted = false;
    private double startTime;
    public bool PlayerAdded = false, EndGameTimerStarted = false;
    public GameObject KeysCountPanel, KeysCountPlayerUI;

    public GameObject EndGameTimer;
    public Text EndGameTimerText;
    public float EndGameTime;
    private double timerStartTime;
    private string EndGameString, KeysMore10Player;

    private Dictionary<string, int> playerKeys = new Dictionary<string, int>();

    private void Start()
    {
        if (PlayerAdded)
            return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            PhotonView view = player.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
            {
                if (player.GetComponent<KeyGatherGameForPlayers>() == null)
                {
                    var gatherScript = player.AddComponent<KeyGatherGameForPlayers>();
                    gatherScript.shooting = player.GetComponent<Shooting>();
                    gatherScript.photonView = view;
                    gatherScript.keysGatherGame = this;
                }
                PlayerAdded = true;
                break; // нашли своего — выходим
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (gameManager.GameStarted)
        {
            if (!TimerStarted)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    startTime = PhotonNetwork.Time;
                    photoView.RPC("StartSpawnKeyTimer", RpcTarget.AllBuffered, startTime);
                    TimerStarted = true;
                }
            }

            double timePassed = PhotonNetwork.Time - startTime;
            float timeLeft = Mathf.Max(0f, timerToSpawnNextKey - (float)timePassed);

            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            if (currentKeys < MaxKeys)
                SpawnKeyTimerText.text = $"До появление след.Ключа {currentKeys}/{MaxKeys}: " + $"{minutes:00}:{seconds:00}";
            else
                SpawnKeyTimerText.text = $"Максимальное количество ключей заспавнено {currentKeys}/{MaxKeys}. Далее дело за вами!";

            if (timeLeft <= 0)
            {
                if (currentKeys < MaxKeys)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        int RandomSpawn = Random.Range(0, gameManager.spawnPoint.Length);
                        PhotonNetwork.Instantiate(KeyPrefab.name, gameManager.spawnPoint[RandomSpawn].transform.position, Quaternion.identity);
                        currentKeys++;
                        photonView.RPC("SynchronizeKeysCount", RpcTarget.AllBuffered, currentKeys);
                    }
                    startTime = PhotonNetwork.Time;
                }
            }

            if (EndGameTimerStarted)
                CheckGameEndTimer();
        }
    }

    [PunRPC]
    public void StartSpawnKeyTimer(double startTimer)
    {
        this.startTime = startTimer;
    }

    public void DestroyKey(int viewId)
    {
        photonView.RPC("DestroyGoldKey", RpcTarget.MasterClient, viewId);
    }
    [PunRPC]
    private void DestroyGoldKey(int photonViewId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView goldKeyPhoton = PhotonNetwork.GetPhotonView(photonViewId);
        if (goldKeyPhoton != null)
        {
            PhotonNetwork.Destroy(goldKeyPhoton.gameObject);
        }
    }

    public void CreateKey(Vector3 createPos)
    {
        photonView.RPC("CreateGoldKey", RpcTarget.MasterClient, createPos);
    }
    [PunRPC]
    private void CreateGoldKey(Vector3 createPos)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(KeyPrefab.name, createPos, Quaternion.identity);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (gameManager.GameStarted)
            CreateOrUpdateKeysCountPanel(targetPlayer);
    }
    private void CreateOrUpdateKeysCountPanel(Player photonPlayer)
    {
        Transform existingUI = KeysCountPanel.transform.Find(photonPlayer.NickName);

        if (existingUI != null)
        {
            UpdatePlayerUI(existingUI.gameObject, photonPlayer);
        }
        else
        {
            GameObject newPlayerUI = Instantiate(KeysCountPlayerUI, KeysCountPanel.transform);
            newPlayerUI.name = photonPlayer.NickName;
            UpdatePlayerUI(newPlayerUI, photonPlayer);
        }
    }

    private void UpdatePlayerUI(GameObject playerUI, Player photonPlayer)
    {
        if (playerUI == null)
        {
            Debug.LogError("playerUI is null!");
            return;
        }

        Transform nameTransform = playerUI.transform.Find("Nickname");
        Transform KeysCount = playerUI.transform.Find("KeysCount");

        if (nameTransform == null) return;
        if (KeysCount == null) return;

        Text NickNameText = nameTransform.GetComponent<Text>();
        Text KeysCountText = KeysCount.GetComponent<Text>();

        NickNameText.text = photonPlayer.NickName;

        NickNameText.text = photonPlayer.NickName;

        int GetKeysCount = photonPlayer.CustomProperties.ContainsKey("KeysCount") ? (int)photonPlayer.CustomProperties["KeysCount"] : 0;
        KeysCountText.text = $"Ключи: {GetKeysCount} / 10";

        if (playerKeys.ContainsKey(photonPlayer.NickName))
        {
            playerKeys[photonPlayer.NickName] = GetKeysCount;
        }
        else
        {
            playerKeys.Add(photonPlayer.NickName, GetKeysCount);
        }
        CheckPlayersWithMoreThan10Keys();
    }
    public void CheckPlayersWithMoreThan10Keys()
    {
        foreach (KeyValuePair<string, int> entry in playerKeys)
        {
            string playerName = entry.Key;
            int keyCount = entry.Value;

            if (keyCount == 10)
            {
                Debug.Log($"{playerName} имеет больше 10 ключей: {keyCount} ключей");
                KeysMore10Player = playerName;
                photonView.RPC("StartTimer", RpcTarget.All, playerName);
            }
            else if (keyCount < 10)
            {
                if (KeysMore10Player == playerName)
                {
                    photonView.RPC("StopTimer", RpcTarget.All);
                }
            }
        }
    }

    [PunRPC]
    private void StartTimer(string PlayerNickName)
    {
        EndGameTimer.SetActive(true);
        EndGameString = $"Игрок '{PlayerNickName}' собрал 10 или более ключей. Игра завершится через: ";
        EndGameTimerStarted = true;
        timerStartTime = PhotonNetwork.Time;
    }
    [PunRPC]
    private void StopTimer()
    {
        EndGameTimer.SetActive(false);
        EndGameTimerStarted = false;
        EndGameTime = 30;
    }

    [PunRPC]
    private void SynchronizeKeysCount(int currentKeys)
    {
        this.currentKeys = currentKeys;
    }

    private void CheckGameEndTimer()
    {
        double currentTime = PhotonNetwork.Time;
        double elapsedTime = currentTime - timerStartTime;
        EndGameTimerText.text = EndGameString + elapsedTime.ToString("F1") + " / 30";

        if (elapsedTime >= EndGameTime)
        {
            // Таймер истек — выполните действия по завершению игры
            gameManager.TimerEnded();
            EndGameTimerStarted = false; // Останавливаем таймер
        }
    }
}
