using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    public PhotonView photonView;
    public float Health = 100, MaxHealth = 100;
    public Animator anim;
    public Text DamageText;
    public bool IsDead = false;
    public void GetDamage(float Damage)
    {
        photonView.RPC("SynchronizeHealth", RpcTarget.AllBuffered, Damage);
    }
    [PunRPC]
    private void SynchronizeHealth(float Damage)
    {
        if (IsDead)
            return;
        Debug.Log("Получила УРон");
        DamageText.text = Damage.ToString("F0");
        Health -= Damage;
        StartCoroutine(ResetText());
        if (Health > 0)
        {
            Health -= Damage;
        }
        else
        {
            Debug.Log("МишеньУмерла");
            IsDead = true;
            Health = MaxHealth;
            anim.SetBool("IsDead", true);
            StartCoroutine(ResetTarget());
        }
    }

    IEnumerator ResetText()
    {
        yield return new WaitForSeconds(1);
        DamageText.text = "";
    }
    IEnumerator ResetTarget()
    {
        yield return new WaitForSeconds(5f);
        anim.SetBool("IsDead", false);
        IsDead = false;
    }
}
