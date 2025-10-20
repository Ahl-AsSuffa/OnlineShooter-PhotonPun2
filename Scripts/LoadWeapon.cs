using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class LoadWeapon : MonoBehaviour
{
    public int WeaponID;                        // ID �������� ������
    public GameObject[] Weapons;                // ������� ���� ������
    public AnimatorOverrideController[] WeaponAnimOverrides;
    public Transform weaponHolder;              // ���� ����� ��������� ������ (��������, � ����)
    public GameObject CurrentWeapon;            // �������� ������ � �����

    public PlayerStats playerStats;
    public WeaponStats weaponStats;
    public Animator upperBodyAnimator; // �������� ������� ����� ����
    public PhotonView photonView;
    public Transform AllWeaponsPanelContent;
    public GameObject WeaponFastSlotPrefabUI;

    void Start()
    {
        if (photonView.IsMine && !playerStats.IsMenu)
        {
            StartCoroutine(Starting());
        }
    }
    private IEnumerator Starting()
    {
        yield return new WaitForSeconds(2);
        if (photonView.IsMine && !playerStats.IsMenu)
        {
            WeaponID = playerStats.equippedWeapons[0].weaponID;
            photonView.RPC("SynchEquipWeapon", RpcTarget.AllBuffered, WeaponID);
            UpdateWeaponSeePanel();
        }
    }
    public void EquipWeapon(int id)
    {
        photonView.RPC("SynchEquipWeapon", RpcTarget.All,id);
    }
    [PunRPC]
    public void SynchEquipWeapon(int id)
    {
        Debug.Log("������������ + ���� - "+ id);
        // ������� ������� ������
        if (CurrentWeapon != null)
            Destroy(CurrentWeapon);

        // ��������� ID � ������� ����� ������
        WeaponID = id;
        CurrentWeapon = Instantiate(Weapons[WeaponID], weaponHolder);
        CurrentWeapon.transform.localPosition = Vector3.zero;
        CurrentWeapon.transform.localRotation = Quaternion.identity;

        weaponStats = CurrentWeapon.GetComponent<WeaponStats>();

        // ������� �������� (Clone)
        CurrentWeapon.name = Weapons[WeaponID].name;

        // ��������� override-����������
        if (WeaponAnimOverrides[WeaponID] != null)
        {
            upperBodyAnimator.runtimeAnimatorController = WeaponAnimOverrides[WeaponID];
        }
        else
        {
            Debug.LogWarning("� ������ ��� AnimatorOverrideController!");
        }
    }
    private void UpdateWeaponSeePanel()
    {
        for (int i = 0; i < playerStats.equippedWeapons.Count; i++)
        {
            GameObject weaponFastSlotPrefab = Instantiate(WeaponFastSlotPrefabUI, AllWeaponsPanelContent.transform);
            Image weaponIcon = weaponFastSlotPrefab.transform.Find("WeaponIcon").GetComponent<Image>();
            Text WeaponNumer = weaponFastSlotPrefab.transform.Find("WeaponNumber").GetComponent<Text>();

            weaponIcon.sprite = Weapons[playerStats.equippedWeapons[i].weaponID].GetComponent<WeaponStats>().WeaponIcon;
            int SlotNumber = i +1;
            WeaponNumer.text = SlotNumber.ToString();
        }
    }
}
