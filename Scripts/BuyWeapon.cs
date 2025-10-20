using UnityEngine;

public class BuyWeapon : MonoBehaviour
{
    public PlayerStorage playerStorage;
    public int WeaponPrice;
    public bool IsDonateBalance = false;
    public void BuyNewWeapon(int WeaponId)
    {
        WeaponStats weaponStats = playerStorage.loadWeapon.Weapons[WeaponId].GetComponent<WeaponStats>();
        string WeaponName = weaponStats.WeaponName;
        playerStorage.BuyWeapon(WeaponId, WeaponName, WeaponPrice, IsDonateBalance);
    }
}
