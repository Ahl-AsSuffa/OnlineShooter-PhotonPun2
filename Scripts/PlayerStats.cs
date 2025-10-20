using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static PlayerStats;

public class PlayerStats : MonoBehaviour
{
    // Çäîğîâüå
    public float Health = 100f;
    public float MaxHealth = 100f;
    public Image HealthFill;

    // Ñòàìèíà / Âûíîñëèâîñòü (äëÿ ñïğèíòà, óêëîíåíèÿ, áëîêîâ) / Òî÷íîñòü / Âûïóùåííûå ïàòğîíû
    public float Stamina = 100f;
    public float MaxStamina = 100f;
    public float StaminaRegen = 10f;
    public Image StaminaRegenFill;
    public float Accuracy; public Text AccuracyText;
    public int ShootedAmmos;

    // Áğîíÿ
    public float Armor = 0f;

    // Îïûò / Óğîâåíü / Áàëàíñ
    public int Rank = 1; public Text RankText;
    public float CurrentExperience = 0f; public Text ExperienceText;
    public float ExperienceToNextLevel = 100f, PlussedExperience = 0;
    public int Money = 1;
    public int JoskiFightCoins = 0, Balance = 1000; public Text JoskiFightCoinsText, BalanceText;
    public int PlussedBalance =0, PlussedJFCoins = 0;
    public Image menuRankImg;
    public Sprite[] RankSprites;

    //KD / MaxKills / MaxDeaths / MaxMatchs
    public float KD = 0; public Text KDText;
    public int Kills, Deaths, Hits, MatchsCount, MaxKills, MaxKillsBehindDeath, MaxKillsBehindDeathCounter ,
        CurrentMatchShoots, CurrentMatchHits;
    public Text MatchsCountText, MaxKillsText;
    public int CurrentMatchKills, CurrentMatchDeaths;
    public float CurrentMatchDamage, CurrentMatchKD;

    // Óğîí áëèæíåãî áîÿ (èëè áàçîâûé óğîí áåç îğóæèÿ)
    public float BaseMeleeDamage = 10f;

    // Ñòàòóñ ôëàã
    public bool IsDead = false;
    public string Role = "player";
    public bool IsBanned = false;
    public string BanReason = "";

    //ÍèêÍåéì èãğîêà
    public string PlayerNickName = "";

    //×óâñòâèòåëüíîñòü / Ãğîìêîñòü Çâóêîâ / Ãğîìêîñòü Ìóçûêè
    public float MouseSensitivity = 2, AudioVolume = 1, MusicVolume = 1, AimMouseSentitivity = 1, SniperAimMouseSensitivity = .25f;

    public PhotonView photonView;

    public DeathMatchGameManager gameManager;
    public Animator GetDamagePanel;
    public bool IsMenu = false;
    public AudioSource MusicSource;
    public PlayerMovement playerMovement;
    public float DeadTime = 5f; //plMovementUnderMap
    public Text DeadText, WeaponNameText;
    public bool IsTeamMate = false;
    public Animator SeeWeaponPanel;
    public ServerClientConnect serverClientConnect;

    [System.Serializable]
    public class PlayerWeapon
    {
        public int weaponID;
        public string weaponName;
    }
    [SerializeField]
    public List<PlayerWeapon> playerWeapons = new List<PlayerWeapon>();
    [SerializeField]
    public List<PlayerWeapon> equippedWeapons = new List<PlayerWeapon>();
    private void Awake()
    {
        gameManager = FindAnyObjectByType<DeathMatchGameManager>();
        if (photonView.IsMine)
        {
            LoadPlayerSettings();
            if (IsMenu)
                return;

            string userName = PlayerPrefs.GetString("username");
            serverClientConnect.StartCoroutine(serverClientConnect.ApplyPlayerStats(userName));

            photonView.RPC("SetTeamMate", RpcTarget.AllBuffered);
            if (string.IsNullOrEmpty(PlayerNickName))
            {
                PlayerNickName = "Player" + Random.Range(0, 1000);
                PlayerPrefs.SetString("NickName", PlayerNickName);
            }

            PhotonNetwork.NickName = PlayerNickName;
            photonView.RPC("GetNickName", RpcTarget.OthersBuffered, PlayerNickName);
            ResetUI();
            Debug.Log("CurrentRank: " + Rank);
        }
    }

    [PunRPC]
    public void GetNickName(string nick)
    {
        PlayerNickName = nick;
    }
    // Ïîëó÷åíèå óğîíà
    public void TakeDamage(float amount, int attackerViewId, string HitPoint)
    {
        if (Health > 0 && !IsDead)
        {
            GetDamagePanel.SetTrigger("damaged");
            float damageAfterArmor = Mathf.Max(amount - Armor, 0);
            if (attackerViewId == 0)
                Die(0,"spine_02");
            Health -= damageAfterArmor;
            ResetUI();

            if (Health <= 0 && !IsDead)
            {
                IsDead = true;
                Die(attackerViewId, HitPoint);
                PhotonView attackerPhotonView = PhotonView.Find(attackerViewId);
                if (attackerPhotonView != null)
                {
                    attackerPhotonView.RPC("AddKill", attackerPhotonView.Owner);
                }
            }
        }
        else
        {
            return;
        }
    }
    [PunRPC]
    public void AddKill()
    {
        Kills++;
        MaxKillsBehindDeathCounter++;
        if(MaxKillsBehindDeathCounter > MaxKillsBehindDeath)
            MaxKillsBehindDeath = MaxKillsBehindDeathCounter;

        CurrentMatchKills++;
        SetPlayerData();
    }
    [PunRPC]
    public void AddMoney()
    {
        Money += 500;
    }
    private void Die(int attckedViewID, string HitPoint)
    {
        IsDead = true;
        photonView.RPC("SynchDeath", RpcTarget.All, IsDead);
        if(gameManager.IsTeamGame)
            gameManager.CheckAllPlayers();
        Deaths++;
        MaxKillsBehindDeathCounter = 0;
        CurrentMatchDeaths++;
        SetPlayerData();
        PhotonView attackerPhotonView = PhotonView.Find(attckedViewID);
        if (attackerPhotonView != null)
        {
            attackerPhotonView.RPC("AddMoney", attackerPhotonView.Owner);
            HitPoint = HitPointRashivrovka(HitPoint);
            PlayerMovement attackerPlMovement = attackerPhotonView.GetComponent<PlayerMovement>();
            playerMovement.gameManager.SendKillLog(attackerPhotonView.Owner.NickName, PlayerNickName, HitPoint, attackerPlMovement.loadWeapon.weaponStats.WeaponIcon.name);
            Debug.Log("DIE Sprite: " + playerMovement.loadWeapon.weaponStats.WeaponIcon.name);
        }
        else
        {
            Debug.Log("attackerPhotonView = null" + attckedViewID);
        }
    }
    public void Respawn()
    {
        int RandomSpawnPoint = Random.Range(0, gameManager.spawnPoint.Length);
        transform.position = gameManager.spawnPoint[RandomSpawnPoint].position;
        Health = MaxHealth;
        playerMovement.loadWeapon.weaponStats.CurrentAmmo = playerMovement.loadWeapon.weaponStats.MaxAmmo;
        Stamina = MaxStamina;
        StaminaRegenFill.fillAmount = Stamina / MaxStamina;
        ResetUI();
        playerMovement.shooting.CurrentAmmosText.text = "Ïàòğîíû: " + playerMovement.loadWeapon.weaponStats.CurrentAmmo + " / " + playerMovement.loadWeapon.weaponStats.MaxAmmo;
        IsDead = false;
        photonView.RPC("SynchDeath", RpcTarget.All, IsDead);
        DeadTime = 5;
    }
    public void ResetUI()
    {
        HealthFill.fillAmount = Health / MaxHealth;
    }

    public void SetPlayerData()
    {
        if (CurrentMatchDeaths == 0)
            CurrentMatchKD = CurrentMatchKills;
        else
            CurrentMatchKD = (float)CurrentMatchKills / (float)CurrentMatchDeaths;

        float saveAccuracy = 0;
        if (CurrentMatchHits > 0)
            saveAccuracy = ((float)CurrentMatchHits / (float)CurrentMatchShoots) * 100;
        else
            saveAccuracy = 100;

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            { "NickName", PlayerNickName },
            { "Rank", Rank},
            { "KillsCurrentMatch", CurrentMatchKills},
            { "DeathsCurrentMatch", CurrentMatchDeaths},
            { "CurrentMatchDamage", CurrentMatchDamage},
            { "CurrentMatchKD", CurrentMatchKD},
            { "CurrentMatchShoots", CurrentMatchShoots},
            { "MaxKillsBehindDeath", MaxKillsBehindDeath},
            { "CurrentMatchAccuracy", saveAccuracy},
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public void SavePlayerStats()
    {
        float saveAccuracy = Hits > 0 ? ((float)Hits / ShootedAmmos) * 100 : 100;
        float saveKD = Kills > 0 ? (float)Kills / Deaths : Kills;

        Debug.Log("KD: " + saveKD);

        PlayerStatsData data = new PlayerStatsData
        {
            Accuracy = saveAccuracy,
            Kills = this.Kills,
            Deaths = this.Deaths,
            Hits = this.Hits,
            ShootedAmmos = this.ShootedAmmos,
            Rank = this.Rank,
            Experience = this.CurrentExperience,
            ExperienceToNextLevel = this.ExperienceToNextLevel,
            PlussedExperience = this.PlussedExperience,
            JoskiFightCoins = this.JoskiFightCoins,
            Balance = this.Balance,
            PlussedBalance = this.PlussedBalance,
            PlussedJfCoins = this.PlussedJFCoins,
            KD = saveKD,
            PlayerNickName = this.PlayerNickName,
            MatchsCount = this.MatchsCount,
            MaxKills = this.MaxKills,
            playerWeapons = this.playerWeapons,
            equippedWeapons = this.equippedWeapons,
        };

        serverClientConnect.UpdatePlayerStats(serverClientConnect.Username,data);
    }
    public void ApplyStats(PlayerStatsData data)
    {
        this.Accuracy = data.Accuracy;
        this.Kills = data.Kills;
        this.Deaths = data.Deaths;
        this.Hits = data.Hits;
        this.ShootedAmmos = data.ShootedAmmos;
        this.Rank = data.Rank;
        this.CurrentExperience = data.Experience;
        this.ExperienceToNextLevel = data.ExperienceToNextLevel;
        this.PlussedExperience = data.PlussedExperience;
        this.JoskiFightCoins = data.JoskiFightCoins;
        this.Balance = data.Balance;
        this.PlussedBalance = data.PlussedBalance;
        this.PlussedJFCoins = data.PlussedJfCoins;
        this.KD = data.KD;
        this.PlayerNickName = data.PlayerNickName;
        this.MatchsCount = data.MatchsCount;
        this.MaxKills = data.MaxKills;
        this.playerWeapons = data.playerWeapons;
        this.equippedWeapons = data.equippedWeapons;
        this.IsBanned = data.IsBanned;
        this.BanReason = data.BanReason;
        this.Role = data.Role;

        ResetMenuUi();
        CheckNewRank();
    }

    public void SavePlayerSettings()
    {
        PlayerSettings data = new PlayerSettings
        {
            AudioVolume = this.AudioVolume,
            MusicVolume = this.MusicVolume,
            MouseSentitivity = this.MouseSensitivity,
            AimMouseSensitivity = this.AimMouseSentitivity,
            SniperAimMouseSensitivity = this.SniperAimMouseSensitivity,
        };

        JsonDataSaver.Save(data, "player_settings");
    }
    public void LoadPlayerSettings()
    {
        if (JsonDataSaver.Exists("player_settings"))
        {
            PlayerSettings data = JsonDataSaver.Load<PlayerSettings>("player_settings");

            this.AudioVolume = data.AudioVolume;
            this.MusicVolume = data.MusicVolume;
            this.MouseSensitivity = data.MouseSentitivity;
            this.AimMouseSentitivity = data.AimMouseSensitivity;
            SniperAimMouseSensitivity = data.SniperAimMouseSensitivity;

            UpdateVolumes();
        }
        else
        {
            Debug.Log("Ñîõğàíåíèÿ íàñòğîåê íå íàéäåíû.");
            SavePlayerSettings();
            LoadPlayerSettings();
        }
    }

    public void ResetMenuUi()
    {
        if (!IsMenu)
            return;
        AccuracyText.text = "ÒÎ×ÍÎÑÒÜ: " + Accuracy.ToString("F1") + "%";
        if(Rank < 80)
            RankText.text = "ĞÀÍÃ: " + Rank.ToString();
        else if (Rank >= 80)
            RankText.text = "ÌÀÊÑ. ĞÀÍÃ: " + Rank.ToString();
        ExperienceText.text = "ÎÏÛÒ: " + CurrentExperience.ToString("F0") + " / " + ExperienceToNextLevel.ToString("F0");
        JoskiFightCoinsText.text = "JOSKIFIGHT Êîèíû: " + JoskiFightCoins.ToString();
        BalanceText.text = "ÁÀËÀÍÑ: " + Balance.ToString();
        KDText.text = "ÊÄ: " + KD.ToString("F2");
        MatchsCountText.text = "ÊÎË-ÂÎ ÌÀÒ×ÅÉ: " + MatchsCount.ToString();
        MaxKillsText.text = "ÌÀÊÑ. ÑÅĞÈß ÓÁÈÉÑÒÂ: " + MaxKills.ToString();
        menuRankImg.sprite = RankSprites[Rank - 1];
    }
    public void CheckNewRank()
    {
        if (Rank >= 80)
        {
            CurrentExperience = 0;
            ExperienceToNextLevel = 0;
            Rank = 80;
            ResetMenuUi();
            return;
        }

        if (CurrentExperience >= ExperienceToNextLevel)
        {
            Rank++;
            CurrentExperience -= ExperienceToNextLevel;
            ExperienceToNextLevel += 20;
            SavePlayerStats();
            ResetMenuUi();
            if (CurrentExperience >= ExperienceToNextLevel)
                CheckNewRank();
        }
    }
    public void UpdateVolumes()
    {
        AudioListener.volume = AudioVolume;
        if (MusicSource != null)
            MusicSource.volume = MusicVolume;
    }

    public string HitPointRashivrovka(string HitPoint)
    {
        switch (HitPoint)
        {
            case "head":
                HitPoint = "ÃÎËÎÂÓ";
                break;
            case "hand_r":
                HitPoint = "ÏĞÀÂÓŞ ÊÈÑÒÜ";
                break;
            case "hand_l":
                HitPoint = "ËÅÂÓŞ ÊÈÑÒÜ";
                break;
            case "lowerarm_r":
                HitPoint = "ÏĞÅÄÏËÅ×ÜÅ (ÏĞÀÂÎÅ)";
                break;
            case "lowerarm_l":
                HitPoint = "ÏĞÅÄÏËÅ×ÜÅ (ËÅÂÎÅ)";
                break;
            case "upperarm_r":
                HitPoint = "ÏËÅ×Î (ÏĞÀÂÎÅ)";
                break;
            case "upperarm_l":
                HitPoint = "ÏËÅ×Î (ËÅÂÎÅ)";
                break;
            case "clavicle_r":
                HitPoint = "ÊËŞ×ÈÖÓ (ÏĞÀÂÓŞ)";
                break;
            case "clavicle_l":
                HitPoint = "ÊËŞ×ÈÖÓ (ËÅÂÓŞ)";
                break;
            case "spine_03":
                HitPoint = "ÃĞÓÄÍÎÉ ÎÒÄÅË";
                break;
            case "spine_02":
                HitPoint = "ÆÈÂÎÒ";
                break;
            case "spine_01":
                HitPoint = "ÏÀÕ";
                break;
            case "foot_r":
                HitPoint = "ÑÒÎÏÓ (ÏĞÀÂÓŞ)";
                break;
            case "foot_l":
                HitPoint = "ÑÒÎÏÓ (ËÅÂÓŞ)";
                break;
            case "calf_r":
                HitPoint = "ÃÎËÅÍÜ (ÏĞÀÂÓŞ)";
                break;
            case "calf_l":
                HitPoint = "ÃÎËÅÍÜ (ËÅÂÓŞ)";
                break;
            case "thigh_r":
                HitPoint = "ÁÅÄĞÎ (ÏĞÀÂÎÅ)";
                break;
            case "thigh_l":
                HitPoint = "ÁÅÄĞÎ (ËÅÂÎÅ)";
                break;
        }
        return HitPoint;
    }

    [PunRPC]
    public void SetTeamMate()
    {
        if(gameManager.IsPolygon)
        {
            IsTeamMate = true;
            Money = 999999;
        }
    }

    public void SetNewWeapon(int WeaponID, string WeaponName)
    {
        PlayerWeapon newWeapon = new PlayerWeapon
        {
            weaponID = WeaponID,
            weaponName = WeaponName,
        };
        playerWeapons.Add(newWeapon);
    }
    public void DeleteWeapon(int WeaponID)
    {
        for (int i = 0; i < playerWeapons.Count; i++)
        {
            if (playerWeapons[i].weaponID == WeaponID)
            {
                playerWeapons.RemoveAt(i);
                Debug.Log("Óäàëåíî îğóæèå ñ ID: " + WeaponID);
                break;
            }
        }
    }
    public void EquipNewWeapon(int WeaponID, string WeaponName)
    {
        PlayerWeapon newWeapon = new PlayerWeapon
        {
            weaponID = WeaponID,
            weaponName = WeaponName,
        };
        equippedWeapons.Add(newWeapon);
    }
    public void DeleteEquippedWeapon(int WeaponID)
    {
        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            if (equippedWeapons[i].weaponID == WeaponID)
            {
                equippedWeapons.RemoveAt(i);
                Debug.Log("Óäàëåíî îğóæèå ñ ID: " + WeaponID);
                break;
            }
        }
    }

    [PunRPC]
    public void SynchDeath(bool IsDead)
    {
        this.IsDead = IsDead;
    }
    private void OnApplicationQuit()
    {
        PlayerPrefs.SetString("password", "");
    }
}

[System.Serializable]
public class PlayerStatsData
{
    public float Accuracy = 100;
    public int ShootedAmmos = 0;

    public int Kills = 0;
    public int Deaths = 0;
    public int Hits = 0;
    public int Rank = 1;
    public float Experience = 0f;
    public float ExperienceToNextLevel = 100f;
    public float PlussedExperience = 0;
    public int JoskiFightCoins = 0, Balance = 1000, PlussedBalance = 0, PlussedJfCoins = 0;
    public float KD = 0;
    public int MatchsCount = 0;
    public int MaxKills = 0;
    public string Role = "player";
    public bool IsBanned = false;
    public string BanReason = "";

    public string PlayerNickName = "";

    public List<PlayerWeapon> playerWeapons = new List<PlayerWeapon>();
    public List<PlayerWeapon> equippedWeapons = new List<PlayerWeapon>();
}

[System.Serializable]
public class PlayerSettings
{
    public float AudioVolume = 1;
    public float MusicVolume = 1;
    public float MouseSentitivity = 2;
    public float AimMouseSensitivity = 1f;
    public float SniperAimMouseSensitivity = .25f;
}