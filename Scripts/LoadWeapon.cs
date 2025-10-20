using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class LoadWeapon : MonoBehaviour
{
    public int WeaponID;                        // ID текущего оружия
    public GameObject[] Weapons;                // Префабы всех оружий
    public AnimatorOverrideController[] WeaponAnimOverrides;
    public Transform weaponHolder;              // Куда будет крепиться оружие (например, к руке)
    public GameObject CurrentWeapon;            // Активное оружие в сцене

    public PlayerStats playerStats;
    public WeaponStats weaponStats;
    public Animator upperBodyAnimator; // Аниматор верхней части тела
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
        Debug.Log("ПришелЗапрос + Айди - "+ id);
        // Удалить текущее оружие
        if (CurrentWeapon != null)
            Destroy(CurrentWeapon);

        // Сохранить ID и создать новое оружие
        WeaponID = id;
        CurrentWeapon = Instantiate(Weapons[WeaponID], weaponHolder);
        CurrentWeapon.transform.localPosition = Vector3.zero;
        CurrentWeapon.transform.localRotation = Quaternion.identity;

        weaponStats = CurrentWeapon.GetComponent<WeaponStats>();

        // Убираем приписку (Clone)
        CurrentWeapon.name = Weapons[WeaponID].name;

        // Назначить override-контроллер
        if (WeaponAnimOverrides[WeaponID] != null)
        {
            upperBodyAnimator.runtimeAnimatorController = WeaponAnimOverrides[WeaponID];
        }
        else
        {
            Debug.LogWarning("У оружия нет AnimatorOverrideController!");
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
