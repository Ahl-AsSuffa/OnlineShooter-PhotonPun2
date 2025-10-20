using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawned : MonoBehaviourPunCallbacks
{
    public GameObject AllPlayersPanel;
    public GameObject PlayerConnectedUI;
    public Text PlayerNickNameText;
    public string PlayerNickName;
    public Image IsRoomMasterImg, PlayerAchievment, PlayerRank;
    public bool IsRoomMaster = false;
    public new PhotonView photonView;
    void Start()
    {
        AllPlayersPanel = GameObject.FindGameObjectWithTag("AllPlayer");
        foreach (var player in PhotonNetwork.PlayerList)
        {
            CreatePlayerUI(player);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // ������� UI ��� ������ ������
        CreatePlayerUI(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // ������� UI ��� ����������� ������
        RemovePlayerUI(otherPlayer);
    }

    private void CreatePlayerUI(Player photonPlayer)
    {
        // ������� ����� prefab UI ��� ������
        GameObject newPlayerUI = Instantiate(PlayerConnectedUI, AllPlayersPanel.transform);
        newPlayerUI.transform.SetParent(AllPlayersPanel.transform, false);

        // ���� ������ ���������� � prefab-�
        Text playerNameText = newPlayerUI.transform.Find("PlayerNickName").GetComponent<Text>();
        Image isRoomMasterImg = newPlayerUI.transform.Find("IsRoomMaster").GetComponent<Image>();

        // ������������� �������
        playerNameText.text = photonPlayer.NickName;

        // ��������, �������� �� ����� �������� �������
        bool isRoomMaster = (PhotonNetwork.MasterClient == photonPlayer);
        isRoomMasterImg.gameObject.SetActive(isRoomMaster);

        // ��������� ������ �� ������ UI ��� ����������� �������������
        newPlayerUI.name = photonPlayer.NickName; // ��� �������� ������
    }

    private void RemovePlayerUI(Player photonPlayer)
    {
        // ������� � ������� UI ��� ����������� ������
        Transform playerUIToRemove = AllPlayersPanel.transform.Find(photonPlayer.NickName);
        if (playerUIToRemove != null)
        {
            Destroy(playerUIToRemove.gameObject);
        }
    }
}
