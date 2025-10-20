using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Shopping : MonoBehaviour
{
    public LoadWeapon loadWeapon;
    public PlayerStats playerStats;
    public Text MoneyText;
    public GameObject WeaponSlot;
    public GameObject ShopContent;
    private void Start()
    {
        if (loadWeapon.photonView.IsMine)
        {
            for (int i = 0; i < playerStats.equippedWeapons.Count; i++) 
            {
                GameObject weapon = Instantiate(WeaponSlot, ShopContent.transform);
                Text weaponNameText = weapon.transform.Find("WeaponName").GetComponent<Text>();
                Text priceText = weapon.transform.Find("Price").GetComponent <Text>();
                Image weaponIcon = weapon.transform.Find("WeaponIcon").GetComponent<Image>();

                WeaponStats weaponStats = loadWeapon.Weapons[playerStats.equippedWeapons[i].weaponID].GetComponent<WeaponStats>();
                weaponNameText.text = weaponStats.WeaponName;
                weaponIcon.sprite = weaponStats.WeaponIcon;
                priceText.text = "÷≈Õ¿: "+weaponStats.Price.ToString();
                ShopButton shopButton = weapon.transform.Find("BuyBtn").GetComponent<ShopButton>();
                shopButton.WeaponID = playerStats.equippedWeapons[i].weaponID;
                shopButton.shopping = this;
            }
        }
    }
    public void BuyWeapon(int WeaponID)
    {
        if (loadWeapon.photonView.IsMine)
        {
            WeaponStats weaponStats = loadWeapon.Weapons[WeaponID].GetComponent<WeaponStats>();
            if (weaponStats.Price <= playerStats.Money)
            {
                if (loadWeapon.CurrentWeapon.name == loadWeapon.Weapons[WeaponID].name)
                    return;

                playerStats.Money -= weaponStats.Price;
                MoneyText.text = "¡‡Î‡ÌÒ: " + playerStats.Money.ToString();

                loadWeapon.photonView.RPC("WeaponBuyed", RpcTarget.AllBuffered, WeaponID);
            }
        }
    }

    [PunRPC]
    private void WeaponBuyed(int WeaponId)
    {
        loadWeapon.EquipWeapon(WeaponId);
    }
}
