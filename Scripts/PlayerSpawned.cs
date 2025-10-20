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
        // Создаем UI для нового игрока
        CreatePlayerUI(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Удаляем UI для покинувшего игрока
        RemovePlayerUI(otherPlayer);
    }

    private void CreatePlayerUI(Player photonPlayer)
    {
        // Создаем новый prefab UI для игрока
        GameObject newPlayerUI = Instantiate(PlayerConnectedUI, AllPlayersPanel.transform);
        newPlayerUI.transform.SetParent(AllPlayersPanel.transform, false);

        // Ищем нужные компоненты в prefab-е
        Text playerNameText = newPlayerUI.transform.Find("PlayerNickName").GetComponent<Text>();
        Image isRoomMasterImg = newPlayerUI.transform.Find("IsRoomMaster").GetComponent<Image>();

        // Устанавливаем никнейм
        playerNameText.text = photonPlayer.NickName;

        // Проверка, является ли игрок мастером комнаты
        bool isRoomMaster = (PhotonNetwork.MasterClient == photonPlayer);
        isRoomMasterImg.gameObject.SetActive(isRoomMaster);

        // Сохраняем ссылку на объект UI для дальнейшего использования
        newPlayerUI.name = photonPlayer.NickName; // Для удобства поиска
    }

    private void RemovePlayerUI(Player photonPlayer)
    {
        // Находим и удаляем UI для покинувшего игрока
        Transform playerUIToRemove = AllPlayersPanel.transform.Find(photonPlayer.NickName);
        if (playerUIToRemove != null)
        {
            Destroy(playerUIToRemove.gameObject);
        }
    }
}
