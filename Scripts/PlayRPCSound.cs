using Photon.Pun;
using UnityEngine;

public class PlayRPCSound : MonoBehaviour
{
    public PhotonView photonView;
    public AudioClip[] clips;
    public AudioSource audioSource;
    public void PlayRandomSound()
    {
        if(photonView.IsMine)
        {
            int randomCLip = Random.Range(0, clips.Length);
            photonView.RPC("PlaySound", RpcTarget.All, randomCLip);
        }
    }
    public void PlayAnywaySound(int SoundId)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("PlaySound", RpcTarget.All, SoundId);
        }
    }

    [PunRPC]
    public void PlaySound(int SoundID)
    {
        audioSource.PlayOneShot(clips[SoundID]);
    }
}
