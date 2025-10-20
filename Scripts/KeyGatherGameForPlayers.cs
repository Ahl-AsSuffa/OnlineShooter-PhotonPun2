using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class KeyGatherGameForPlayers : MonoBehaviour
{
    public int KeysCount;
    public Shooting shooting;
    public PhotonView photonView;
    public PlayerStats playerStats;
    public KeysGatherGame keysGatherGame;
    void Start()
    {
        if (photonView.IsMine)
        {
            photonView = GetComponent<PhotonView>();
            shooting = GetComponent<Shooting>();
            playerStats = GetComponent<PlayerStats>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine)
        {
            TakeGoldKey();
            DropKeys();
        }
    }

    public void TakeGoldKey()
    {
        if (playerStats.IsDead)
            return;

        RaycastHit hit;
        int ignoreLayer = LayerMask.NameToLayer("IgnoreRaycast");
        int layerMask = ~(1 << ignoreLayer);
        if (Physics.Raycast(shooting.plCamera.transform.position, shooting.plCamera.transform.forward, out hit, 4, layerMask))
        {
            if (hit.collider.CompareTag("GoldKey"))
            {
                shooting.PressFBtn.SetActive(true);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    PhotonView goldKeyPhoton = hit.collider.transform.root.GetComponent<PhotonView>();
                    if (goldKeyPhoton != null)
                    {
                        KeysCount++;
                        DestroyGoldKey(goldKeyPhoton.ViewID);
                        SetPlayerData();
                        goldKeyPhoton.gameObject.SetActive(false);
                    }
                }
            }
        }
        else
        {
            shooting.PressFBtn.SetActive(false);
        }
    }

    public void DestroyGoldKey(int photonViewId)
    {
        keysGatherGame.DestroyKey(photonViewId);
    }

    public void DropKeys()
    {
        if(playerStats.IsDead)
        {
            for(int i = 0; KeysCount > i; i++)
            {
                Vector3 randomPos = new Vector3(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(0, 1f), transform.position.z);
                keysGatherGame.CreateKey(randomPos);
                KeysCount--;
                SetPlayerData();
            }
        }
    }

    public void SetPlayerData()
    {
        Hashtable props = new Hashtable
        {
            { "KeysCount", KeysCount }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}
