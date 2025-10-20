using Photon.Pun;
using UnityEngine;
using ExitGames.Client.Photon;

public class MineMatchGameManager : MonoBehaviour
{
    public DeathMatchGameManager gameManager;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectTeam(string TeamName)
    {
        if (TeamName == "A")
        {
            Hashtable props = new Hashtable() { { "Team", "A" } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
        else if (TeamName == "B") 
        {
            Hashtable props = new Hashtable() { { "Team", "B" } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
        gameManager.StartCoroutine(gameManager.UpdateUI(PhotonNetwork.LocalPlayer));
        gameManager.SetTeam(PhotonNetwork.LocalPlayer);
    }
}
