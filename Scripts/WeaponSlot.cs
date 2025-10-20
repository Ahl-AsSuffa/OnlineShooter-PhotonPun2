using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour
{
    public int CurrentWeaponId;
    public WeaponStats currentWeaponStats;
    private Transform WeaponStatsPanel;
    private GameObject CanvasObject;
    private PlayerStats playerStats;
    private Transform StorageContent, EquippedContent;
    [SerializeField]
    private Text weaponName, Damage, Magazine, FireRate, Recoil, HeadDamage, clavicleDamage, GrudDamage, JivotDamage,
        GroinDamage, upperarmDamage, lowerarmDamage, handDamage, thighDamage, calfDamage, footDamage;
    public void GetCurrentWeaponStats()
    {
        if (CanvasObject == null)
            CanvasObject = GameObject.FindGameObjectWithTag("Canvas2");
        if (WeaponStatsPanel == null)
            WeaponStatsPanel = FindHiddenChildByTag(CanvasObject.transform, "WeaponStats");
        WeaponStatsPanel.gameObject.SetActive(true);

        if (weaponName == null || Magazine == null || FireRate == null || Recoil == null || HeadDamage == null || clavicleDamage == null || GrudDamage == null || JivotDamage == null || GroinDamage == null || upperarmDamage == null ||
            lowerarmDamage == null || handDamage == null || thighDamage == null || calfDamage == null || footDamage == null)
        {
            weaponName = WeaponStatsPanel.transform.Find("WeaponNameText").GetComponent<Text>();
            Damage = WeaponStatsPanel.transform.Find("Damage").GetComponent<Text>();
            FireRate = WeaponStatsPanel.transform.Find("FireRate").GetComponent<Text>();
            Magazine = WeaponStatsPanel.transform.Find("Magazine").GetComponent<Text>();
            Recoil = WeaponStatsPanel.transform.Find("Recoil").GetComponent<Text>();
            HeadDamage = WeaponStatsPanel.transform.Find("HeadDamage").GetComponent<Text>();
            clavicleDamage = WeaponStatsPanel.transform.Find("clavicleDamage").GetComponent<Text>();
            GrudDamage = WeaponStatsPanel.transform.Find("GrudDamage").GetComponent<Text>();
            JivotDamage = WeaponStatsPanel.transform.Find("JivotDamage").GetComponent<Text>();
            GroinDamage = WeaponStatsPanel.transform.Find("GroinDamage").GetComponent<Text>();
            upperarmDamage = WeaponStatsPanel.transform.Find("upperarmDamage").GetComponent<Text>();
            lowerarmDamage = WeaponStatsPanel.transform.Find("lowerarmDamage").GetComponent<Text>();
            handDamage = WeaponStatsPanel.transform.Find("handDamage").GetComponent<Text>();
            thighDamage = WeaponStatsPanel.transform.Find("thighDamage").GetComponent<Text>();
            calfDamage = WeaponStatsPanel.transform.Find("calfDamage").GetComponent<Text>();
            footDamage = WeaponStatsPanel.transform.Find("footDamage").GetComponent<Text>();
            Debug.Log("��� ���������� �������!");
            GetCurrentWeaponStats();
            return;
        }
        else
        {
            Debug.Log("����� ���������� ��������");
            float CurrentFireOfRate = 1 / currentWeaponStats.RateOfFire;
            float CurrentRecoil = currentWeaponStats.recoilUp * -100;
            weaponName.text = currentWeaponStats.WeaponName;
            Damage.text = "����: " + currentWeaponStats.Damage.ToString("F0");
            FireRate.text = "���������������� � ���: " + CurrentFireOfRate.ToString("F1");
            Magazine.text = "������� ��������: " + currentWeaponStats.MaxAmmo.ToString("F0");
            Recoil.text = "������: " + CurrentRecoil.ToString("F1");
            HeadDamage.text = "��������� � ������: X" + currentWeaponStats.HeadMultiplier.ToString("F1");
            clavicleDamage.text = "��������� � �������: X" + currentWeaponStats.ClavicleMultiplier.ToString("F1");
            GrudDamage.text = "��������� � �����: X" + currentWeaponStats.chestMultiplier.ToString("F1");
            JivotDamage.text = "��������� � �����: X" + currentWeaponStats.BellyMultiplier.ToString("F1");
            GroinDamage.text = "��������� � ���: X" + currentWeaponStats.GroinMultiplier.ToString("F1");
            upperarmDamage.text = "��������� � �����: X" + currentWeaponStats.UpperArmMultiplier.ToString("F1");
            lowerarmDamage.text = "��������� � ����������: X" + currentWeaponStats.LowerArmMultiplier.ToString("F1");
            handDamage.text = "��������� � �����: X" + currentWeaponStats.HandMultiplier.ToString("F1");
            thighDamage.text = "��������� � �����: X" + currentWeaponStats.ThighMultiplier.ToString("F1");
            calfDamage.text = "��������� � ������: X" + currentWeaponStats.CalfMultiplier.ToString("F1");
            footDamage.text = "��������� � �����: X" + currentWeaponStats.FootMultiplier.ToString("F1");
        }
    }

    Transform FindHiddenChildByTag(Transform parent, string tag)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true)) // true = ���� � �������
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
        }
        return null;
    }

    public void EquipCurrentWeapon()
    {
        if (playerStats == null)
            playerStats = FindAnyObjectByType<PlayerStats>();
        if(StorageContent == null || EquippedContent == null)
        {
            EquippedContent = GameObject.FindGameObjectWithTag("EquippedContent").transform;
            StorageContent = GameObject.FindGameObjectWithTag("StorageContent").transform;
        }

        if(playerStats.equippedWeapons.Count < 4)
        {
            playerStats.EquipNewWeapon(CurrentWeaponId, currentWeaponStats.WeaponName);
            gameObject.transform.SetParent(EquippedContent, false);
            playerStats.SavePlayerStats();
        }
        else
        {
            Debug.Log("��� ����������� 4 ������");
        }
    }
    public void UnEquipCurrentWeapon()
    {
        if (playerStats == null)
            playerStats = FindAnyObjectByType<PlayerStats>();
        if (StorageContent == null || EquippedContent == null)
        {
            EquippedContent = GameObject.FindGameObjectWithTag("EquippedContent").transform;
            StorageContent = GameObject.FindGameObjectWithTag("StorageContent").transform;
        }
        gameObject.transform.SetParent(StorageContent, false);
        gameObject.transform.SetAsFirstSibling();
        playerStats.DeleteEquippedWeapon(CurrentWeaponId);
        playerStats.SavePlayerStats();
    }
}
