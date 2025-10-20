using UnityEngine;

public class ShopButton : MonoBehaviour
{
    public int WeaponID;
    public Shopping shopping;

    public void BuyWeapon()
    {
        shopping.BuyWeapon(WeaponID);
    }
}
