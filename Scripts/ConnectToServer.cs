using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class ConnectToServer : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    public GameObject SuccesJoinedPanel, LoadingPanel,ExperiencePanel, TitlePanel, LostConnectionPanel, AutorizePanel, BanPanel;
    public Animator ErrorPanelAnim;
    public Text currentPing, NicknameText, ErrorText, RegionText;
    public string PlayerNickName, Promo = "promo2025";
    public InputField NickNameInput, CreateRoomInput, FindRoomInput, PromoInput;
    public LobbyChat lobbyChat;
    public Image CurrentRankImg, NextRankImg, CurrentExperienceFill, GettExperienceFill;
    public Text HowManyPlussedExperienceText, HowManyPlussedBalanceText, HowManyPlussedExperienceText2, HowManyPlussedJFCoinsText,
        BanReasonText;
    private float currentFillAmount;
    private float gettFillAmount;
    public Dropdown dropdown;
    public GameObject CreatingGamePanel;
    public AudioSource BGSound, ErrorSounds;

    public PlayerStats playerStats;
    public PlayerStorage playerStorage;
    public AudioClip[] AllAudioClips;
    public int Autorized = 0;
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("������������ � ������� Photon...");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        dropdown.value = 0;
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("������� ���������� � ������-�������!");
        RegionText.text = "REGION: " + PhotonNetwork.CloudRegion;
        // ����� ����� ����� � �������
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("������ � ������� �����, ���� �����������/�����������.");
        AutorizePanel.SetActive(true);
        LoadingPanel.SetActive(false);
        if (Autorized == 0)
            return;
        AutorizePanel.SetActive(false);
        LostConnectionPanel.SetActive(false);
        SuccesJoinedPanel.SetActive(true);
        TitlePanel.SetActive(true);
        GetNickName();
        InvokeRepeating(nameof(PrintPing), 1f, 5f);
        playerStats.MusicSource.Play();
        BGSound.Play();
        lobbyChat.OnJoined();
        CheckExperiencePane();
        playerStorage.CreateAllWeapons();
        Debug.Log("����� � �����. ������ � ������ / �������� ������.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"���������� �� �������. �������: {cause}");
        LostConnectionPanel.SetActive(true);
        StartCoroutine(ConnectToServerAfterLostConnection());
    }
    void PrintPing()
    {
        int ping = PhotonNetwork.GetPing();
        currentPing.text = "Ping: " + ping + "ms";
    }
    public void GetNickName()
    {
        PlayerNickName = playerStats.PlayerNickName;
        if (PlayerNickName == "")
        {
            PlayerNickName = "Player" + UnityEngine.Random.Range(0, 1000).ToString("F0");
            playerStats.PlayerNickName = PlayerNickName;
            PhotonNetwork.NickName = PlayerNickName;
            NicknameText.text = "�������: " + PlayerNickName;
            playerStats.SavePlayerStats();
        }
        else
        {
            PlayerNickName = playerStats.PlayerNickName;
            NicknameText.text = "�������: " + PlayerNickName;
        }
    }
    public void SetNickName()
    {
        if (NickNameInput.text != "")
        {
            string nick = NickNameInput.text;
            if (nick.Length > 15)
            {
                string errorText = "������� ������� �������. ����������� 14 ��������. ����� 14 - �����.";
                StartErrorText(Color.red, errorText,0);
                return;
            }
            PlayerNickName = NickNameInput.text;
            PhotonNetwork.NickName = PlayerNickName;
            NicknameText.text = "�������: " + PlayerNickName;
            playerStats.PlayerNickName = PlayerNickName;
            playerStats.SavePlayerStats();
            lobbyChat.OnJoined();
        }
        else
        {
            string errorText = "�����-�� ����� �������� ������� ����, �� � ����� ��� �������� �� ������. ���� �� ����� ���� ������, ��!";
            StartErrorText(Color.red, errorText, 0);
        }
    }
    public void Pay1000Rubley()
    {
        Application.OpenURL("https://www.donationalerts.com/r/al_khafan");
    }
    public void CreateRoom()
    {
        CreatingGamePanel.SetActive(true);
        if (CreateRoomInput == null)
        {
            Debug.LogError("CreateRoomInput �� ��������!");
            CreatingGamePanel.SetActive(false);
            return;
        }

        string roomName = CreateRoomInput.text;

        if (string.IsNullOrEmpty(roomName))
        {
            string errorText = "��� ������� �� ������ ���� ������!";
            CreatingGamePanel.SetActive(false);
            StartErrorText(Color.red, errorText, 0);
            return;
        }
        if (roomName.Length < 15)
        {
            Debug.Log("�������� �������: " + roomName);
        }
        else
        {
            CreatingGamePanel.SetActive(false);
            string errorText = "������� ������� �������� �������! �������� 14 ��������.";
            StartErrorText(Color.red, errorText,0);
        }

        RoomOptions roomOptions = new RoomOptions();
        switch (roomType)
        {
            case RoomType.Deathmatch:
                roomOptions.MaxPlayers = 10;
                roomOptions.IsVisible = true;   // �����: ����� ���� ����� � �����
                roomOptions.IsOpen = true;      // �����: ����� ����� ���� ��������������

                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "roomType", roomType.ToString() }
                };
                roomOptions.CustomRoomPropertiesForLobby = new string[] { "roomType" };
                PhotonNetwork.CreateRoom(roomName, roomOptions);
                break;
            case RoomType.Polygon:
                roomOptions.MaxPlayers = 5;
                roomOptions.IsVisible = true;   // �����: ����� ���� ����� � �����
                roomOptions.IsOpen = true;      // �����: ����� ����� ���� ��������������

                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "roomType", roomType.ToString() }
                };
                roomOptions.CustomRoomPropertiesForLobby = new string[] { "roomType" };
                PhotonNetwork.CreateRoom(roomName, roomOptions);
                break;
            case RoomType.GatherKeys:
                roomOptions.MaxPlayers = 10;
                roomOptions.IsVisible = true;   // �����: ����� ���� ����� � �����
                roomOptions.IsOpen = true;      // �����: ����� ����� ���� ��������������

                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "roomType", roomType.ToString() }
                };
                roomOptions.CustomRoomPropertiesForLobby = new string[] { "roomType" };
                PhotonNetwork.CreateRoom(roomName, roomOptions);
                break;
            case RoomType.Undermining:
                roomOptions.MaxPlayers = 10;
                roomOptions.IsVisible = true;   // �����: ����� ���� ����� � �����
                roomOptions.IsOpen = true;      // �����: ����� ����� ���� ��������������

                roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
                {
                    { "roomType", roomType.ToString() }
                };
                roomOptions.CustomRoomPropertiesForLobby = new string[] { "roomType" };
                PhotonNetwork.CreateRoom(roomName, roomOptions);
                break;
        }
    }
    public void SelectPolygonRoom()
    {
        roomType = RoomType.Polygon;
    }
    public void SelectDeathmatchRoom()
    {
        roomType = RoomType.Deathmatch;
    }
    public void SelectKeysGathering()
    {
        roomType = RoomType.GatherKeys;
    }
    public void SelectUndermining()
    {
        roomType = RoomType.Undermining;
    }
    public void FindRoom()
    {
        Debug.Log("������ ������ �����");

        if (FindRoomInput == null)
        {
            Debug.LogError("FindRoomInput �� ��������!");
            return;
        }

        string roomName = FindRoomInput.text;

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("��� ������� ������!");
            return;
        }

        if (roomName.Length >= 15)
        {
            string errorText = "������� ������� �������� �������! �������� 14 ��������.";
            StartErrorText(Color.red, errorText, 0);
            return;
        }

        bool roomFound = cachedRoomList.Any(room => room.Name == roomName && room.RemovedFromList == false);

        if (roomFound)
        {
            Debug.Log("������� �������, �����������...");
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.LogWarning("������� �� �������.");
            string errorText = "������� � ����� ������ �� �������.";
            StartErrorText(Color.red, errorText,0);
        }
    }
    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        cachedRoomList = roomList;
        Debug.Log("������ ������ �������. ���-�� ������: " + roomList.Count);
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("������� �������������� � �������: " + PhotonNetwork.CurrentRoom.Name);
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("roomType", out object value))
        {
            string typeStr = value.ToString();

            if (Enum.TryParse(typeStr, out RoomType roomType))
            {
                Debug.Log("������� ������� ����: " + roomType);

                if (roomType == RoomType.Polygon)
                {
                    PhotonNetwork.LoadLevel("Polygon");
                }
                else if (roomType == RoomType.Deathmatch)
                {
                    PhotonNetwork.LoadLevel("DeathMatch");
                }
                else if (roomType == RoomType.GatherKeys)
                {
                    PhotonNetwork.LoadLevel("GatherKeys");
                }
                else if (roomType == RoomType.Undermining)
                {
                    PhotonNetwork.LoadLevel("Undermining");
                }
            }
        }
    }
    public override void OnCreatedRoom()
    {
        Debug.Log("������� ������� �������: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("������ �������� �������: " + message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("������ ����������� � �������: " + message);
    }
    public enum RoomType
    {
        Deathmatch,
        Polygon,
        GatherKeys,
        Undermining
    }
    public RoomType roomType;

    public void CheckExperiencePane()
    {
        if(playerStats.PlussedExperience > 0 || playerStats.PlussedBalance > 0)
        {
            ExperiencePanel.SetActive(true);
            StartCoroutine(AnimateExperience());
            float summaryExperince = playerStats.CurrentExperience + playerStats.PlussedExperience;
            HowManyPlussedExperienceText.text = " + "+ playerStats.PlussedExperience.ToString("F0") + " �����" + " (" + summaryExperince + " / "+ playerStats.ExperienceToNextLevel.ToString("F0") + ")";
            CurrentRankImg.sprite = playerStats.RankSprites[playerStats.Rank - 1];
            NextRankImg.sprite = playerStats.RankSprites[playerStats.Rank];
            HowManyPlussedBalanceText.text = "+" + playerStats.PlussedBalance.ToString();
            HowManyPlussedExperienceText2.text = "+" + playerStats.PlussedExperience.ToString("F0");
            HowManyPlussedJFCoinsText.text = "+" + playerStats.PlussedJFCoins.ToString();
        }
    }
    public void CloseExperiencePanel()
    {
        ExperiencePanel.SetActive(false);
        playerStats.CurrentExperience += playerStats.PlussedExperience;
        playerStats.Balance += playerStats.PlussedBalance;
        playerStats.JoskiFightCoins += playerStats.PlussedJFCoins;
        playerStats.CheckNewRank();
        playerStats.PlussedExperience = 0;
        playerStats.PlussedBalance = 0;
        playerStats.PlussedJFCoins = 0;
        playerStats.SavePlayerStats();
    }

    private IEnumerator AnimateExperience()
    {
        // ������� �������� fillAmount
        float targetCurrentFill = playerStats.CurrentExperience / playerStats.ExperienceToNextLevel;
        float targetGettFill = (playerStats.CurrentExperience + playerStats.PlussedExperience) / playerStats.ExperienceToNextLevel;

        // ����� �������� (� ��������)
        float duration = 15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // �������� ������������ ����� ������� � ������� ���������
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetCurrentFill, elapsed / duration);
            gettFillAmount = Mathf.Lerp(gettFillAmount, targetGettFill, elapsed / duration);

            // ���������� UI
            CurrentExperienceFill.fillAmount = currentFillAmount;
            GettExperienceFill.fillAmount = gettFillAmount;

            // ���������� �������
            elapsed += Time.deltaTime;

            yield return null; // ���� ���������� �����
        }

        // ��������� ������ ������� �������� ����� ���������� ��������
        currentFillAmount = targetCurrentFill;
        gettFillAmount = targetGettFill;

        CurrentExperienceFill.fillAmount = currentFillAmount;
        GettExperienceFill.fillAmount = gettFillAmount;
    }
    private IEnumerator ConnectToServerAfterLostConnection()
    {
        yield return new WaitForSeconds(.5f);
        PhotonNetwork.ConnectUsingSettings();
    }

    public void StartErrorText(Color color, string currentText, int clipID)
    {
        ErrorSounds.PlayOneShot(AllAudioClips[clipID]);
        ErrorPanelAnim.SetTrigger("error");
        ErrorText.color = color;
        ErrorText.text = currentText;
    }

    public void SetPromo()
    {
        StartCoroutine(StartSetPromo());
    }
    private IEnumerator StartSetPromo()
    {
        yield return StartCoroutine(playerStats.serverClientConnect.ApplyPlayerStats(playerStats.serverClientConnect.Username));
        if (PlayerPrefs.GetInt(Promo) == 1)
        {
            string errorText = "�� ��� ������������ ������ ��������";
            StartErrorText(Color.red,errorText, 0);
            yield break;
        }
        if(PromoInput.text == Promo)
        {
            playerStats.Balance += 2000;
            playerStats.JoskiFightCoins += 1000;
            string errorText = "�������� ������� �����������!";
            StartErrorText(Color.green, errorText, 1);
            playerStats.ResetMenuUi();
            playerStats.SavePlayerStats();
            PlayerPrefs.SetInt(Promo, 1);
        }
    }
    private void OnDropdownValueChanged(int index)
    {
        switch (index)
        {
            case 0:
                SelectPolygonRoom();
                break;
            case 1:
                SelectDeathmatchRoom();
                break;
            case 2:
                SelectKeysGathering();
                break;
            case 3:
                SelectUndermining();
                break;
            default:
                Debug.Log("����������� �����");
                break;
        }
    }
    public void RestartMenu()
    {
        Application.Quit();
    }
}
