using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMatchGameManager : MonoBehaviourPunCallbacks
{
    [Header("СПАВН БУСТЕРОВ")]
    public bool BoostersSpawn = false;
    public GameObject[] AllBoosters;
    public int HowManyBoostersSpawned = 2;

    public string playerPrefabName = "Player"; // имя префаба
    public Transform[] spawnPoint; // точка спавна
    public Transform[] spawnPointsTeamA, spawnPointsTeamB;
    public bool PlayerSpawned = false;

    public GameObject AllPlayersPanel, AllPlayersStatsPanel, EndPanelStats, AllPlayersPanelTeamA,
        AllPlayersPanelTeamB, AllPlayersStatsPanelTeamA, AllPlayersStatsPanelTeamB, EndPanelStatsTeamA, EndPanelStatsTeamB;
    public GameObject PlayerConnectedUI, PlayerStatsUI, PlayerEndPanelStatsUi;
    public GameObject StartPanel, EndPanel;
    public Text PlayerNickNameText, PlayersTitleText;
    public Image IsRoomMasterImg, PlayerAchievment, PlayerRank;
    public GameObject StartBtn, MatchStartedPanel, AllStatsPanel;
    public int PlayersCount;
    public bool GameStarted = false, IsPolygon = false, UnlimitedTime = false, isMineMatch = false, IsTeamGame = false;
    public PhotonView photonnView;
    public float TimerToEndGame = 180;
    public Text TimerText;
    private double startTime;
    private bool isStarted = false, FirstUpdate = true;
    public Sprite[] AllRankSprites;
    [Header("UI References")]
    public Transform killLogContainer;
    public GameObject killLogPrefab;
    public float logLifetime = 5f;

    public List<Player> teamA = new List<Player>();
    public List<Player> teamB = new List<Player>();

    void Start()
    {
        FirstUpdate = true;
        if (PhotonNetwork.IsConnectedAndReady && !PlayerSpawned)
        {
            int RandomSpawnPoint = Random.Range(0, spawnPoint.Length);
            PhotonNetwork.Instantiate(playerPrefabName, spawnPoint[RandomSpawnPoint].position, spawnPoint[RandomSpawnPoint].rotation);
            Debug.Log("Создан игрок");
            PlayerSpawned = true;
        }
        AllPlayersCount();
        StartCoroutine(Starting());
    }
    private IEnumerator Starting()
    {
        yield return new WaitForSeconds(.25f);
        // Создаем UI для всех игроков в комнате
        if (!IsTeamGame)
        {
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                StartCoroutine(UpdateUI(player));
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            StartBtn.SetActive(true); // Активируем кнопку только у мастера
        }
        else
        {
            StartBtn.SetActive(false); // Скрываем кнопку у остальных игроков
        }
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnBoosters();
            photonnView.RPC("SynchronizeMatchTime", RpcTarget.AllBuffered, TimerToEndGame);
        }
    }
    private void Update()
    {
        if (Input.GetKey(KeyCode.Tab))
        {
            AllStatsPanel.SetActive(true);
        }
        else
        {
            AllStatsPanel.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            DistributePlayers();
        }

        if (!isStarted || UnlimitedTime) return;

        double timePassed = PhotonNetwork.Time - startTime;
        float timeLeft = Mathf.Max(0f, TimerToEndGame - (float)timePassed);

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        TimerText.text = "ДО ЗАВЕРШЕНИЯ МАТЧА: " + $"{minutes:00}:{seconds:00}";

        if (timeLeft <= 0)
        {
            isStarted = false;
            TimerEnded();
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        AllPlayersCount();
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(UpdateUI(newPlayer));
        }
    }
    public IEnumerator UpdateUI(Player player)
    {
        yield return new WaitForSeconds(1f);
        CreateOrUpdatePlayerUI(player);
        CreateOrUpdatePlayerStatsUI(player);
        CreateOrUpdateEndPanel(player);
        FirstUpdate = false;
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // Проверяем снова, кто теперь мастер
        StartBtn.SetActive(PhotonNetwork.IsMasterClient);
        StartCoroutine(UpdateUI(newMasterClient));
    }

    // Игрок покинул комнату
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        AllPlayersCount();
        if (IsTeamGame)
        {
            object teamObj;
            if (otherPlayer.CustomProperties.TryGetValue("Team", out teamObj))
            {
                string team = teamObj as string;
                Transform existingUI = null;
                Transform existingStatsUI = null;

                if (team == "A")
                {
                    existingUI = AllPlayersPanelTeamA.transform.Find(otherPlayer.NickName);
                    existingStatsUI = AllPlayersStatsPanelTeamA.transform.Find(otherPlayer.NickName);
                }
                else if (team == "B")
                {
                    existingUI = AllPlayersPanelTeamB.transform.Find(otherPlayer.NickName);
                    existingStatsUI = AllPlayersStatsPanelTeamB.transform.Find(otherPlayer.NickName);
                }

                if (existingUI != null)
                {
                    Destroy(existingUI.gameObject);
                }
                if (existingStatsUI != null)
                {
                    Destroy(existingStatsUI.gameObject);
                }
            }
        }
        else
        {
            Transform existingUI = AllPlayersPanel.transform.Find(otherPlayer.NickName);
            Transform existingStatsUI = AllPlayersStatsPanel.transform.Find(otherPlayer.NickName);
            if (existingUI != null)
            {
                Destroy(existingUI.gameObject);
            }
            if (existingStatsUI != null)
            {
                Destroy(existingStatsUI.gameObject);
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        // Обновляем UI, если свойства игрока изменились
        if(!FirstUpdate)
        {
            if (!GameStarted)
            {
                CreateOrUpdatePlayerUI(targetPlayer);
                CreateOrUpdateEndPanel(targetPlayer);
            }

            CreateOrUpdatePlayerStatsUI(targetPlayer);
        }
    }

    private void CreateOrUpdatePlayerUI(Player photonPlayer)
    {
        if (IsTeamGame)
        {
            object teamObj;
            if (photonPlayer.CustomProperties.TryGetValue("Team", out teamObj))
            {
                string team = teamObj as string;
                Transform existingUI = null;
                if (team == "A")
                {
                    existingUI = AllPlayersPanelTeamA.transform.Find(photonPlayer.NickName);
                }
                else if (team == "B")
                {
                    existingUI = AllPlayersPanelTeamB.transform.Find(photonPlayer.NickName);
                }

                if (existingUI != null)
                {
                    UpdatePlayerUI(existingUI.gameObject, photonPlayer);
                }
                else
                {
                    GameObject newPlayerUI = null;
                    if (team == "A")
                    {
                        newPlayerUI = Instantiate(PlayerConnectedUI, AllPlayersPanelTeamA.transform);
                    }
                    else if (team == "B")
                    {
                        newPlayerUI = Instantiate(PlayerConnectedUI, AllPlayersPanelTeamB.transform);
                    }
                    newPlayerUI.name = photonPlayer.NickName;
                    UpdatePlayerUI(newPlayerUI, photonPlayer);
                }
            }
        }
        else
        {
            Transform existingUI = AllPlayersPanel.transform.Find(photonPlayer.NickName);

            if (existingUI != null)
            {
                UpdatePlayerUI(existingUI.gameObject, photonPlayer);
            }
            else
            {
                GameObject newPlayerUI = Instantiate(PlayerConnectedUI, AllPlayersPanel.transform);
                newPlayerUI.name = photonPlayer.NickName;
                UpdatePlayerUI(newPlayerUI, photonPlayer);
            }
        }
    }

    private void UpdatePlayerUI(GameObject playerUI, Player photonPlayer)
    {
        if (playerUI == null)
        {
            Debug.LogError("playerUI is null!");
            return;
        }
        if (StartPanel.activeSelf == false)
            return;

        Transform nameTransform = playerUI.transform.Find("PlayerNickName");
        Transform masterTransform = playerUI.transform.Find("IsRoomMaster");
        Transform rankTransform = playerUI.transform.Find("Rank");

        if (nameTransform == null) return;
        if (masterTransform == null) return;
        if (rankTransform == null) return;
        Text rankText = null;
        if (rankTransform != null)
        {
            Transform rankTextTransform = rankTransform.Find("RankText");
            rankText = rankTextTransform.GetComponent<Text>();
        }

        if (nameTransform != null && masterTransform != null && rankText != null)
        {
            Text playerNameText = nameTransform.GetComponent<Text>();
            Image isRoomMasterImg = masterTransform.GetComponent<Image>();

            playerNameText.text = photonPlayer.NickName;
            if (playerNameText.text == "Player123")
                return;
            isRoomMasterImg.gameObject.SetActive(PhotonNetwork.MasterClient == photonPlayer);
            if (PhotonNetwork.MasterClient == photonPlayer)
                playerUI.transform.SetAsFirstSibling();

            int rank = photonPlayer.CustomProperties.ContainsKey("Rank") ? (int)photonPlayer.CustomProperties["Rank"] : 0;
            rankText.text = $"Ранг: {rank}";

            Image rankImg = rankTransform.GetComponent<Image>();
            if (rank != 0)
                rankImg.sprite = AllRankSprites[rank - 1];
        }
        else
        {
            Debug.LogError("UI components missing — cannot update player UI.");
        }
    }
    private void CreateOrUpdatePlayerStatsUI(Player photonPlayer)
    {
        if (IsTeamGame)
        {
            object teamObj;
            if (photonPlayer.CustomProperties.TryGetValue("Team", out teamObj))
            {
                string team = teamObj as string;
                Transform existingUI = null;
                if (team == "A")
                {
                    existingUI = AllPlayersStatsPanelTeamA.transform.Find(photonPlayer.NickName);
                }
                else if (team == "B")
                {
                    existingUI = AllPlayersStatsPanelTeamB.transform.Find(photonPlayer.NickName);
                }

                if (existingUI != null)
                {
                    UpdatePlayerStatsUI(existingUI.gameObject, photonPlayer);
                }
                else
                {
                    GameObject newPlayerUI = null;
                    if (team == "A")
                    {
                        newPlayerUI = Instantiate(PlayerStatsUI, AllPlayersStatsPanelTeamA.transform);
                    }
                    else if (team == "B")
                    {
                        newPlayerUI = Instantiate(PlayerStatsUI, AllPlayersStatsPanelTeamB.transform);
                    }
                    newPlayerUI.name = photonPlayer.NickName;
                    UpdatePlayerStatsUI(newPlayerUI, photonPlayer);
                }
            }
        }
        else
        {
            Transform existingUI = AllPlayersStatsPanel.transform.Find(photonPlayer.NickName);

            if (existingUI != null)
            {
                UpdatePlayerStatsUI(existingUI.gameObject, photonPlayer);
            }
            else
            {
                GameObject newPlayerUI = Instantiate(PlayerStatsUI, AllPlayersStatsPanel.transform);
                newPlayerUI.name = photonPlayer.NickName;
                UpdatePlayerStatsUI(newPlayerUI, photonPlayer);
            }
        }
    }
    private void UpdatePlayerStatsUI(GameObject playerUI, Player photonPlayer)
    {
        if (playerUI == null)
        {
            Debug.LogError("playerUI is null!");
            return;
        }

        Debug.Log("Id: " + photonPlayer.ActorNumber);
        Transform nameTransform = playerUI.transform.Find("PlayerNickName");
        Transform masterTransform = playerUI.transform.Find("IsRoomMaster");
        Transform rankTransform = playerUI.transform.Find("Rank");
        Transform KillsTextTransform = playerUI.transform.Find("Kills");
        Transform DeathsTextTransform = playerUI.transform.Find("Deaths");
        Transform DamageTextTransform = playerUI.transform.Find("Damage");
        Transform KDTextTransform = playerUI.transform.Find("KD");

        if (nameTransform == null) return;
        if (masterTransform == null) return;
        if (rankTransform == null) return;
        if (KillsTextTransform == null) return;
        if (DeathsTextTransform == null) return;
        if (DamageTextTransform == null) return;
        if (KDTextTransform == null) return;

        Text KillsText = KillsTextTransform.GetComponent<Text>();
        Text DeathsText = DeathsTextTransform.GetComponent<Text>();
        Text DamageText = DamageTextTransform.GetComponent<Text>();
        Text KDText = KDTextTransform.GetComponent<Text>();

        Text rankText = null;
        if (rankTransform != null)
        {
            Transform rankTextTransform = rankTransform.Find("RankText");
            rankText = rankTextTransform.GetComponent<Text>();
        }

        if (nameTransform != null && masterTransform != null && rankText != null)
        {
            Text playerNameText = nameTransform.GetComponent<Text>();
            Image isRoomMasterImg = masterTransform.GetComponent<Image>();

            playerNameText.text = photonPlayer.NickName;
            if (playerNameText.text == "Player123")
                return;
            isRoomMasterImg.gameObject.SetActive(PhotonNetwork.MasterClient == photonPlayer);

            int rank = photonPlayer.CustomProperties.ContainsKey("Rank") ? (int)photonPlayer.CustomProperties["Rank"] : 0;
            int kills = photonPlayer.CustomProperties.ContainsKey("KillsCurrentMatch") ? (int)photonPlayer.CustomProperties["KillsCurrentMatch"] : 0;
            int deaths = photonPlayer.CustomProperties.ContainsKey("DeathsCurrentMatch") ? (int)photonPlayer.CustomProperties["DeathsCurrentMatch"] : 0;
            float damage = photonPlayer.CustomProperties.ContainsKey("CurrentMatchDamage") ? (float)photonPlayer.CustomProperties["CurrentMatchDamage"] : 0;
            float KD = photonPlayer.CustomProperties.ContainsKey("CurrentMatchKD") ? (float)photonPlayer.CustomProperties["CurrentMatchKD"] : 0;
            Debug.Log("Updating rankText with value: " + rank);
            rankText.text = $"Ранг: {rank}";
            KillsText.text = kills.ToString();
            DeathsText.text = deaths.ToString();
            DamageText.text = damage.ToString("F1");
            KDText.text = KD.ToString("F2");
            Image rankImg = rankTransform.GetComponent<Image>();
            Debug.Log("CurrentRank: " + rank);
            if (rank != 0)
                rankImg.sprite = AllRankSprites[rank - 1];
        }
        else
        {
            Debug.LogError("UI components missing — cannot update player UI.");
        }
    }
    private void CreateOrUpdateEndPanel(Player photonPlayer)
    {
        if (IsTeamGame)
        {
            object teamObj;
            if (photonPlayer.CustomProperties.TryGetValue("Team", out teamObj))
            {
                string team = teamObj as string;
                Transform existingUI = null;
                if (team == "A")
                {
                    existingUI = EndPanelStatsTeamA.transform.Find(photonPlayer.NickName);
                }
                else if (team == "B")
                {
                    existingUI = EndPanelStatsTeamB.transform.Find(photonPlayer.NickName);
                }

                if (existingUI != null)
                {
                    UpdatePlayerEndPanel(existingUI.gameObject, photonPlayer);
                }
                else
                {
                    GameObject newPlayerUI = null;
                    if (team == "A")
                    {
                        newPlayerUI = Instantiate(PlayerEndPanelStatsUi, EndPanelStatsTeamA.transform);
                    }
                    else if (team == "B")
                    {
                        newPlayerUI = Instantiate(PlayerEndPanelStatsUi, EndPanelStatsTeamB.transform);
                    }
                    newPlayerUI.name = photonPlayer.NickName;
                    UpdatePlayerEndPanel(newPlayerUI, photonPlayer);
                }
            }
        }
        else
        {
            Transform existingUI = EndPanelStats.transform.Find(photonPlayer.NickName);

            if (existingUI != null)
            {
                UpdatePlayerEndPanel(existingUI.gameObject, photonPlayer);
            }
            else
            {
                GameObject newPlayerUI = Instantiate(PlayerEndPanelStatsUi, EndPanelStats.transform);
                newPlayerUI.name = photonPlayer.NickName;
                UpdatePlayerEndPanel(newPlayerUI, photonPlayer);
            }
        }
    }
    private void UpdatePlayerEndPanel(GameObject playerUI, Player photonPlayer)
    {
        if (playerUI == null)
        {
            Debug.LogError("playerUI is null!");
            return;
        }

        Debug.Log("Id: " + photonPlayer.ActorNumber);
        Transform nameTransform = playerUI.transform.Find("PlayerNickName");
        Transform masterTransform = playerUI.transform.Find("IsRoomMaster");
        Transform rankTransform = playerUI.transform.Find("Rank");
        Transform KillsTextTransform = playerUI.transform.Find("Kills");
        Transform DeathsTextTransform = playerUI.transform.Find("Deaths");
        Transform DamageTextTransform = playerUI.transform.Find("Damage");
        Transform KDTextTransform = playerUI.transform.Find("KD");
        Transform ShootsTextTransform = playerUI.transform.Find("Shoots");
        Transform MaxKillsBehindDeathTextTransform = playerUI.transform.Find("MaxKillsBehindDeath");
        Transform AccuracyTextTransform = playerUI.transform.Find("Accuracy");

        if (nameTransform == null) return;
        if (masterTransform == null) return;
        if (rankTransform == null) return;
        if (KillsTextTransform == null) return;
        if (DeathsTextTransform == null) return;
        if (DamageTextTransform == null) return;
        if (KDTextTransform == null) return;

        Text KillsText = KillsTextTransform.GetComponent<Text>();
        Text DeathsText = DeathsTextTransform.GetComponent<Text>();
        Text DamageText = DamageTextTransform.GetComponent<Text>();
        Text KDText = KDTextTransform.GetComponent<Text>();
        Text shootsText = ShootsTextTransform.GetComponent<Text>();
        Text MaxKillsBehindDeathText = MaxKillsBehindDeathTextTransform.GetComponent<Text>();
        Text AccuracyText = AccuracyTextTransform.GetComponent<Text>();

        if (nameTransform != null && masterTransform != null)
        {
            Text playerNameText = nameTransform.GetComponent<Text>();
            Image isRoomMasterImg = masterTransform.GetComponent<Image>();

            playerNameText.text = photonPlayer.NickName;
            if (playerNameText.text == "Player123")
                return;
            isRoomMasterImg.gameObject.SetActive(PhotonNetwork.MasterClient == photonPlayer);

            int rank = photonPlayer.CustomProperties.ContainsKey("Rank") ? (int)photonPlayer.CustomProperties["Rank"] : 0;
            int kills = photonPlayer.CustomProperties.ContainsKey("KillsCurrentMatch") ? (int)photonPlayer.CustomProperties["KillsCurrentMatch"] : 0;
            int deaths = photonPlayer.CustomProperties.ContainsKey("DeathsCurrentMatch") ? (int)photonPlayer.CustomProperties["DeathsCurrentMatch"] : 0;
            int currentMatchShoots = photonPlayer.CustomProperties.ContainsKey("CurrentMatchShoots") ? (int)photonPlayer.CustomProperties["CurrentMatchShoots"] : 0;
            int MaxKillsBehindDeath = photonPlayer.CustomProperties.ContainsKey("MaxKillsBehindDeath") ? (int)photonPlayer.CustomProperties["MaxKillsBehindDeath"] : 0;
            float damage = photonPlayer.CustomProperties.ContainsKey("CurrentMatchDamage") ? (float)photonPlayer.CustomProperties["CurrentMatchDamage"] : 0;
            float KD = photonPlayer.CustomProperties.ContainsKey("CurrentMatchKD") ? (float)photonPlayer.CustomProperties["CurrentMatchKD"] : 0;
            float accuracy = photonPlayer.CustomProperties.ContainsKey("CurrentMatchAccuracy") ? (float)photonPlayer.CustomProperties["CurrentMatchAccuracy"] : 0;

            KillsText.text = kills.ToString();
            DeathsText.text = deaths.ToString();
            DamageText.text = damage.ToString("F1");
            KDText.text = KD.ToString("F2");
            shootsText.text = currentMatchShoots.ToString();
            MaxKillsBehindDeathText.text = MaxKillsBehindDeath.ToString();
            AccuracyText.text = accuracy.ToString("F2");
            Image rankImg = rankTransform.GetComponent<Image>();
            if (rank != 0)
                rankImg.sprite = AllRankSprites[rank - 1];
        }
        else
        {
            Debug.LogError("UI components missing — cannot update player UI.");
        }
    }
    public void AllPlayersCount()
    {
        PlayersCount = PhotonNetwork.CurrentRoom.PlayerCount;
        PlayersTitleText.text = "ИГРОКОВ В КОМНАТЕ: " + PlayersCount.ToString();
    }
    public void StartGame()
    {
        photonnView.RPC("SynchronizeStartPanel", RpcTarget.AllBuffered);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
            // Мастер клиент устанавливает стартовое время
            startTime = PhotonNetwork.Time;
            photonView.RPC("StartTimer", RpcTarget.AllBuffered, startTime);
            DistributePlayers();
        }
    }

    [PunRPC]
    public void SynchronizeStartPanel()
    {
        StartPanel.SetActive(false);
        StartBtn.SetActive(false);
        MatchStartedPanel.SetActive(true);
        GameStarted = true;
        foreach (PlayerMovement pm in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            if (pm.GetComponent<PhotonView>().IsMine)
            {
                pm.IsPause = false;
                pm.CheckPause();
            }
        }
    }
    [PunRPC]
    void StartTimer(double masterStartTime)
    {
        startTime = masterStartTime;
        isStarted = true;
    }
    public void TimerEnded()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("⏰ Время вышло! Игра окончена.");
            photonView.RPC("KickAllPlayers", RpcTarget.All);
        }
        EndPanel.SetActive(true);
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            Player player = PhotonNetwork.PlayerList[i];
            CreateOrUpdateEndPanel(player);
        }
    }
    [PunRPC]
    void KickAllPlayers()
    {
        Debug.Log("⏰ Время вышло! Игрок будет кикнут.");

        foreach (PlayerStats playerStats in FindObjectsByType<PlayerStats>(FindObjectsSortMode.None))
        {
            if (playerStats.GetComponent<PhotonView>().IsMine)
            {
                playerStats.PlussedExperience += Random.Range(5, 50);
                playerStats.PlussedBalance += Random.Range(130, 550);
                playerStats.SetPlayerData();
                playerStats.SavePlayerStats();
            }
        }
        // Покидаем комнату
        foreach (PlayerMovement pm in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            if (pm.GetComponent<PhotonView>().IsMine)
            {
                pm.IsPause = true;
                pm.CheckPause();
            }
        }
        GameStarted = false;
    }

    [PunRPC]
    public void AddKillLog(string killer, string victim, string HitPoint, string WeaponSpriteName)
    {
        GameObject log = Instantiate(killLogPrefab, killLogContainer);
        Text text = log.GetComponentInChildren<Text>();
        Image weaponIcon = log.GetComponentInChildren<Image>();
        Sprite weaponSprite = LoadSpriteByName(WeaponSpriteName);
        Debug.Log("AddKillLog Sprite: " + weaponSprite.name);
        weaponIcon.sprite = weaponSprite;
        text.text = $"<color=red>{killer}</color> ЗАБИЛ ДО СМЕРТИ ➤ <color=blue>{victim}</color> в {HitPoint}";

        Destroy(log, logLifetime);
    }
    public void SendKillLog(string killer, string victim, string HitPoint, string WeaponSpriteName)
    {
        photonView.RPC("AddKillLog", RpcTarget.All, killer, victim, HitPoint, WeaponSpriteName);
    }
    Sprite LoadSpriteByName(string name)
    {
        return Resources.Load<Sprite>("WeaponIcons/" + name);
    }
    public void SpawnBoosters()
    {
        if (!BoostersSpawn)
            return;

        for (int i = 0; i < HowManyBoostersSpawned; i++)
        {
            string boosterName = AllBoosters[Random.Range(0, AllBoosters.Length)].name;
            PhotonNetwork.Instantiate("Boosters/" + boosterName, spawnPoint[Random.Range(0, spawnPoint.Length)].position, Quaternion.identity);
        }
    }
    public void Leave()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    [PunRPC]
    public void SynchronizeMatchTime(float MatchTimer)
    {
        TimerToEndGame = MatchTimer;
    }
    public void SetTeam(Player player)
    {
        int actorNumber = player.ActorNumber;
        photonnView.RPC("SynchSetTeam", RpcTarget.AllBuffered, actorNumber);
    }

    [PunRPC]
    public void SynchSetTeam(int actorNumber)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        StartCoroutine(UpdateUI(player));
        if (player.CustomProperties.TryGetValue("Team", out object team))
        {
            if (team as string == "A")
                teamA.Add(player);
            else if (team as string == "B")
                teamB.Add(player);
        }
    }

    [PunRPC]
    public void MovePlayerToTeamSpawn(int actorNumber, string team, int index)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
        {
            Transform[] spawnPoints = team == "A" ? spawnPointsTeamA : spawnPointsTeamB;
            if (index < spawnPoints.Length)
            {
                Transform targetPoint = spawnPoints[index];

                GameObject myPlayer = FindPlayerByActorNumber(actorNumber); // <— исправлено
                if (myPlayer != null)
                {
                    myPlayer.transform.position = targetPoint.position;

                    PlayerMovement playerMovement = myPlayer.GetComponent<PlayerMovement>();
                    if (playerMovement != null)
                    {
                        playerMovement.respawn();
                    }
                }
            }
        }
    }
    GameObject FindPlayerByActorNumber(int actorNumber)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var obj in players)
        {
            PhotonView view = obj.GetComponent<PhotonView>();
            if (view != null && view.Owner != null && view.Owner.ActorNumber == actorNumber)
            {
                return obj;
            }
        }
        return null;
    }
    public void DistributePlayers()
    {
        for (int i = 0; i < teamA.Count; i++)
        {
            photonView.RPC("MovePlayerToTeamSpawn", RpcTarget.All, teamA[i].ActorNumber, "A", i);
        }

        for (int i = 0; i < teamB.Count; i++)
        {
            photonView.RPC("MovePlayerToTeamSpawn", RpcTarget.All, teamB[i].ActorNumber, "B", i);
        }
    }

    public void CheckAllPlayers()
    {
        photonView.RPC("SynchCheckAllPlayers", RpcTarget.MasterClient);
    }
    [PunRPC]
    public void SynchCheckAllPlayers()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        bool isTeamAAlive = false;
        bool isTeamBAlive = false;

        foreach (PlayerStats playerStats in FindObjectsByType<PlayerStats>(FindObjectsSortMode.None))
        {
            if (playerStats == null || playerStats.photonView == null) continue;

            Player owner = playerStats.photonView.Owner;
            if (teamA.Contains(owner) && !playerStats.IsDead)
            {
                isTeamAAlive = true;
            }
            else if (teamB.Contains(owner) && !playerStats.IsDead)
            {
                isTeamBAlive = true;
            }
        }

        Debug.Log($"Team A alive: {isTeamAAlive}, Team B alive: {isTeamBAlive}");

        if (!isTeamAAlive)
        {
            Debug.Log("Team A проиграла");
            DistributePlayers();
            // Победа Team B
        }
        else if (!isTeamBAlive)
        {
            Debug.Log("Team B проиграла");
            DistributePlayers();
            // Победа Team A
        }
    }
}
