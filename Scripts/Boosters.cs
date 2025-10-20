using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Boosters : MonoBehaviour
{
    public string BoosterName; //speed / jump / scale / randomTeleport
    public PhotonView photonView;
    public float BoostValue;
    public DeathMatchGameManager gameManager;
    public ParticleSystem DestroyParticles;
    private void Start()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SynchronizeBoosValues", RpcTarget.AllBuffered);
            gameManager = FindAnyObjectByType<DeathMatchGameManager>();
        }
    }
    [PunRPC]
    public void SynchronizeBoosValues()
    {
        switch (BoosterName)
        {
            case "speed":
                BoostValue = Random.Range(4, 8);
                break;
            case "jump":
                BoostValue = Random.Range(4, 8);
                break;
            case "scale":
                BoostValue = Random.Range(2, 4);
                break;
        }
    }
    public void BoostGetted()
    {
        photonView.RPC("Destroythis", RpcTarget.AllBuffered);
    }
    [PunRPC]
    private void Destroythis()
    {
        DestroyParticles.Play();
        StartCoroutine(NextPos());
    }
    IEnumerator NextPos()
    {
        yield return new WaitForSeconds(.1f);
        int RandomTeleport = Random.Range(0, gameManager.spawnPoint.Length);
        transform.position = gameManager.spawnPoint[RandomTeleport].position;
    }
}
