using UnityEngine;

public class CrateItem : MonoBehaviour
{
    public string itemName;
    public Sprite itemIcon;
    public Rarity rarity;
}
public enum Rarity
{
    Common,
    Rare,
    Epic,
    Mythic,
    Legend,
}