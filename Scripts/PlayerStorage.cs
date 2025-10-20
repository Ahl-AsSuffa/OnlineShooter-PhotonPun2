using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStorage : MonoBehaviour
{
    public PlayerStats playerStats;
    public GameObject WeaponSlot;
    public Transform StorageContent, EquippedContent;
    public LoadWeapon loadWeapon;
    public ConnectToServer connectToServer;
    public void CreateAllWeapons()
    {

        HashSet<int> equippedWeaponIDs = new HashSet<int>();
        for (int i = 0; i < playerStats.equippedWeapons.Count; i++)
        {
            equippedWeaponIDs.Add(playerStats.equippedWeapons[i].weaponID);
        }

        Debug.Log("Создание склада...");
        for (int i = 0; i < playerStats.playerWeapons.Count; i++)
        {
            // Если оружие уже экипировано — пропускаем
            if (equippedWeaponIDs.Contains(playerStats.playerWeapons[i].weaponID))
                continue;

            GameObject currentWeaponslot = Instantiate(WeaponSlot, StorageContent.transform);
            Image weaponicon = currentWeaponslot.transform.Find("WeaponIcon").GetComponent<Image>();
            Text weaponName = currentWeaponslot.transform.Find("WeaponName").GetComponent<Text>();
            Image equippedBtn = currentWeaponslot.transform.Find("EquipBtn").GetComponent<Image>();
            Image UnEquipdBtn = currentWeaponslot.transform.Find("UnEquipBtn").GetComponent<Image>();
            WeaponSlot weaponSlot = currentWeaponslot.GetComponent<WeaponSlot>();
            weaponSlot.CurrentWeaponId = i;

            int weaponID = playerStats.playerWeapons[i].weaponID;
            GameObject weaponObject = loadWeapon.Weapons.FirstOrDefault(w => w.GetComponent<WeaponStats>().WeaponID == weaponID);
            WeaponStats weaponStats = weaponObject.GetComponent<WeaponStats>();

            weaponicon.sprite = weaponStats.WeaponIcon;
            weaponName.text = weaponStats.WeaponName;
            weaponSlot.currentWeaponStats = weaponStats;
            equippedBtn.gameObject.SetActive(true);
            UnEquipdBtn.gameObject.SetActive(false);
            Debug.Log("Создан складской слот: " + i);
        }

        for (int i = 0; i < playerStats.equippedWeapons.Count; i++)
        {
            GameObject currentWeaponslot = Instantiate(WeaponSlot, EquippedContent.transform);
            Image weaponicon = currentWeaponslot.transform.Find("WeaponIcon").GetComponent<Image>();
            Text weaponName = currentWeaponslot.transform.Find("WeaponName").GetComponent<Text>();
            Image equippedBtn = currentWeaponslot.transform.Find("EquipBtn").GetComponent<Image>();
            Image UnEquipdBtn = currentWeaponslot.transform.Find("UnEquipBtn").GetComponent<Image>();
            WeaponSlot weaponSlot = currentWeaponslot.GetComponent<WeaponSlot>();
            weaponSlot.CurrentWeaponId = playerStats.equippedWeapons[i].weaponID;

            WeaponStats weaponStats = loadWeapon.Weapons[playerStats.equippedWeapons[i].weaponID].GetComponent<WeaponStats>();
            weaponicon.sprite = weaponStats.WeaponIcon;
            weaponName.text = weaponStats.WeaponName;
            weaponSlot.currentWeaponStats = weaponStats;
            equippedBtn.gameObject.SetActive(false);
            UnEquipdBtn.gameObject.SetActive(true);
            Debug.Log("Создан экипированный слот: " + i);
        }
    }
    public void CreateWeapon(int WeaponId)
    {
        bool weaponExists = playerStats.playerWeapons.Exists(w => w.weaponID == WeaponId);
        if (weaponExists)
        {
            Debug.LogWarning("Оружие с ID " + WeaponId + " уже существует в инвентаре игрока.");
            string errorText2 = "У вас на складе уже есть такое оружие!";
            connectToServer.StartErrorText(Color.green, errorText2, 0);
            return;
        }

        GameObject currentWeaponslot = Instantiate(WeaponSlot, StorageContent.transform);
        Image weaponicon = currentWeaponslot.transform.Find("WeaponIcon").GetComponent<Image>();
        Text weaponName = currentWeaponslot.transform.Find("WeaponName").GetComponent<Text>();
        Image equippedBtn = currentWeaponslot.transform.Find("EquipBtn").GetComponent<Image>();
        Image UnEquipdBtn = currentWeaponslot.transform.Find("UnEquipBtn").GetComponent<Image>();
        WeaponSlot weaponSlot = currentWeaponslot.GetComponent<WeaponSlot>();
        weaponSlot.CurrentWeaponId = WeaponId;

        WeaponStats weaponStats = loadWeapon.Weapons[WeaponId].GetComponent<WeaponStats>();
        weaponicon.sprite = weaponStats.WeaponIcon;
        weaponName.text = weaponStats.WeaponName;
        weaponSlot.currentWeaponStats = weaponStats;
        equippedBtn.gameObject.SetActive(true);
        UnEquipdBtn.gameObject.SetActive(false);
        Debug.Log("Создан: " + WeaponId);
    }
    public void DeleteWeapon(int WeaponId)
    {
        foreach (Transform child in StorageContent)
        {
            WeaponSlot weaponSlot = child.GetComponent<WeaponSlot>();
            if (weaponSlot != null && weaponSlot.CurrentWeaponId == WeaponId)
            {
                Destroy(child.gameObject);
                playerStats.DeleteWeapon(WeaponId);
                Debug.Log("Удалено оружие с ID: " + WeaponId);
                break;
            }
        }
    }
    public void BuyWeapon(int WeaponId, string WeaponName, int Price, bool IsDonateBalance)
    {
        StartCoroutine(StartBuyWeapon(WeaponId, WeaponName, Price, IsDonateBalance));
    }
    private IEnumerator StartBuyWeapon(int WeaponId, string WeaponName, int Price, bool IsDonateBalance)
    {
        yield return StartCoroutine(playerStats.serverClientConnect.ApplyPlayerStats(playerStats.serverClientConnect.Username));
        if (!IsDonateBalance)
        {
            if (playerStats.Balance >= Price)
            {
                bool weaponExists = playerStats.playerWeapons.Exists(w => w.weaponID == WeaponId);
                if (weaponExists)
                {
                    Debug.LogWarning("Оружие с ID " + WeaponId + " уже существует в инвентаре игрока.");
                    string errorText2 = "У вас на складе уже есть такое оружие!";
                    connectToServer.StartErrorText(Color.green, errorText2, 0);
                    yield break;
                }
                playerStats.Balance -= Price;
                playerStats.ResetMenuUi();
                playerStats.SetNewWeapon(WeaponId, WeaponName);

                GameObject currentWeaponslot = Instantiate(WeaponSlot, StorageContent.transform);
                Image weaponicon = currentWeaponslot.transform.Find("WeaponIcon").GetComponent<Image>();
                Text weaponName = currentWeaponslot.transform.Find("WeaponName").GetComponent<Text>();
                Image equippedBtn = currentWeaponslot.transform.Find("EquipBtn").GetComponent<Image>();
                Image UnEquipdBtn = currentWeaponslot.transform.Find("UnEquipBtn").GetComponent<Image>();
                WeaponSlot weaponSlot = currentWeaponslot.GetComponent<WeaponSlot>();
                weaponSlot.CurrentWeaponId = WeaponId;

                WeaponStats weaponStats = loadWeapon.Weapons[WeaponId].GetComponent<WeaponStats>();
                weaponicon.sprite = weaponStats.WeaponIcon;
                weaponName.text = weaponStats.WeaponName;
                weaponSlot.currentWeaponStats = weaponStats;
                equippedBtn.gameObject.SetActive(true);
                UnEquipdBtn.gameObject.SetActive(false);
                Debug.Log("Создан: " + WeaponId);
                string errorText = "Покупка произведена успешно!";
                connectToServer.StartErrorText(Color.green, errorText, 1);
                playerStats.SavePlayerStats();
            }
            else
            {
                string errorText = "Не хватает валюты!";
                connectToServer.StartErrorText(Color.red, errorText, 0);
            }
        }
        else
        {
            if (playerStats.JoskiFightCoins >= Price)
            {
                bool weaponExists = playerStats.playerWeapons.Exists(w => w.weaponID == WeaponId);
                if (weaponExists)
                {
                    Debug.LogWarning("Оружие с ID " + WeaponId + " уже существует в инвентаре игрока.");
                    string errorText2 = "У вас на складе уже есть такое оружие!";
                    connectToServer.StartErrorText(Color.green, errorText2, 0);
                    yield break;
                }
                playerStats.JoskiFightCoins -= Price;
                playerStats.ResetMenuUi();
                playerStats.SetNewWeapon(WeaponId, WeaponName);

                GameObject currentWeaponslot = Instantiate(WeaponSlot, StorageContent.transform);
                Image weaponicon = currentWeaponslot.transform.Find("WeaponIcon").GetComponent<Image>();
                Text weaponName = currentWeaponslot.transform.Find("WeaponName").GetComponent<Text>();
                Image equippedBtn = currentWeaponslot.transform.Find("EquipBtn").GetComponent<Image>();
                Image UnEquipdBtn = currentWeaponslot.transform.Find("UnEquipBtn").GetComponent<Image>();
                WeaponSlot weaponSlot = currentWeaponslot.GetComponent<WeaponSlot>();
                weaponSlot.CurrentWeaponId = WeaponId;

                WeaponStats weaponStats = loadWeapon.Weapons[WeaponId].GetComponent<WeaponStats>();
                weaponicon.sprite = weaponStats.WeaponIcon;
                weaponName.text = weaponStats.WeaponName;
                weaponSlot.currentWeaponStats = weaponStats;
                equippedBtn.gameObject.SetActive(true);
                UnEquipdBtn.gameObject.SetActive(false);
                Debug.Log("Создан: " + WeaponId);
                string errorText = "Покупка произведена успешно!";
                connectToServer.StartErrorText(Color.green, errorText, 1);
                playerStats.SavePlayerStats();
            }
            else
            {
                string errorText = "Не хватает валюты!";
                connectToServer.StartErrorText(Color.red, errorText, 0);
            }
        }
    }
}
