using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaseOpenner : MonoBehaviour
{
    [Header("Цена открытия кейса")]
    public int OpenPrice = 20;
    public int WeaponId;
    public string WeaponName = "WeaponName";

    public RectTransform contentPanel;
    public GameObject itemPrefab, SpinPanel, RewardPanel;
    public List<CrateItem> crateItems;
    private List<CrateItem> spawnedItems = new List<CrateItem>();
    public float scrollSpeed = 1000f;
    public float deceleration = 50f;
    public int visibleItems = 20; // сколько пролетит до замедления
    public RectTransform selectorImage;
    private bool isSpinning = false;
    private float currentSpeed;
    private float targetPosition;
    public ScrollRect scrollRect;
    private Vector2 initialContentPosition;
    public PlayerStats playerStats;
    public PlayerStorage playerStorage;
    public Text WinRewardText;

    private void Start()
    {
        WeaponStats weaponStats = playerStorage.loadWeapon.Weapons[WeaponId].GetComponent<WeaponStats>();
        WeaponName = weaponStats.WeaponName;
        initialContentPosition = scrollRect.content.anchoredPosition;
    }
    public void OpenCase()
    {
        StartCoroutine(StartOpenCase());
    }
    private IEnumerator StartOpenCase()
    {
        yield return StartCoroutine(playerStats.serverClientConnect.ApplyPlayerStats(playerStats.serverClientConnect.Username));

        if (playerStats.JoskiFightCoins >= OpenPrice)
        {
            playerStorage.connectToServer.StartErrorText(Color.red, "", 3);
            SpinPanel.SetActive(true);
            playerStats.JoskiFightCoins -= OpenPrice;
            playerStats.ResetMenuUi();
            spawnedItems.Clear();
            for (int i = 0; i < 50; i++)
            {
                var itemData = GetRandomItemByRarity();
                spawnedItems.Add(itemData); // сохраняем предмет
                GameObject itemGO = Instantiate(itemPrefab, contentPanel);
                itemGO.transform.Find("Icon").GetComponent<Image>().sprite = itemData.itemIcon;
                itemGO.transform.Find("Name").GetComponent<Text>().text = itemData.itemName;
                itemGO.transform.Find("Frame").GetComponent<Image>().color = GetRarityColor(itemData.rarity);
            }
            playerStats.SavePlayerStats();
            StartSpin();
        }
        else
        {
            string errorText = "Не хватает валюты!";
            playerStorage.connectToServer.StartErrorText(Color.red, errorText, 0);
            yield break;
        }
    }

    CrateItem GetRandomItemByRarity()
    {
        int totalWeight = 0;
        foreach (var kvp in rarityChances)
            totalWeight += kvp.Value;

        int randomWeight = Random.Range(0, totalWeight);
        int currentWeight = 0;
        Rarity selectedRarity = Rarity.Common;

        foreach (var kvp in rarityChances)
        {
            currentWeight += kvp.Value;
            if (randomWeight < currentWeight)
            {
                selectedRarity = kvp.Key;
                break;
            }
        }

        // Теперь выберем случайный предмет с нужной редкостью
        List<CrateItem> filtered = crateItems.FindAll(i => i.rarity == selectedRarity);
        if (filtered.Count == 0) return crateItems[Random.Range(0, crateItems.Count)];
        return filtered[Random.Range(0, filtered.Count)];
    }
    private void StartSpin()
    {
        if (isSpinning) return;

        isSpinning = true;
        currentSpeed = scrollSpeed + Random.Range(-250,250);
        StartCoroutine(Spin());
    }

    IEnumerator Spin()
    {
        float totalDistance = 0;

        while (currentSpeed > 0)
        {
            float movement = currentSpeed * Time.deltaTime;
            totalDistance += movement;

            scrollRect.content.anchoredPosition -= new Vector2(movement, 0);
            currentSpeed -= deceleration * Time.deltaTime;

            yield return null;
        }

        isSpinning = false;

        // Определение предмета под селектором
        float selectorWorldX = selectorImage.position.x;

        CrateItem winningItem = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < contentPanel.childCount; i++)
        {
            Transform child = contentPanel.GetChild(i);
            float itemWorldX = child.position.x;
            float distance = Mathf.Abs(itemWorldX - selectorWorldX);

            if (distance < minDistance)
            {
                minDistance = distance;
                winningItem = spawnedItems[i];
            }
        }

        Debug.Log($"Ты выбил: {winningItem.itemName}");
        GetReward(winningItem.itemName);

        yield return new WaitForSeconds(2f);
        ClearItems();
        scrollRect.content.anchoredPosition = initialContentPosition;
    }

    Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return Color.gray;
            case Rarity.Rare: return Color.blue;
            case Rarity.Epic: return Color.magenta;
            case Rarity.Mythic: return new Color(1f, 0.5f, 0f);
            case Rarity.Legend: return Color.yellow;
            default: return Color.white;
        }
    }
    void ClearItems()
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        RewardPanel.SetActive(false);
        SpinPanel.SetActive(false);
    }
    private Dictionary<Rarity, int> rarityChances = new Dictionary<Rarity, int>()
{
    { Rarity.Common, 75 },
    { Rarity.Rare, 15 },
    { Rarity.Epic, 7 },
    { Rarity.Mythic, 3 },
    { Rarity.Legend, 2 }
};

    public void GetReward(string RewardName)
    {
        StartCoroutine(StartGetReward(RewardName));
    }
    private IEnumerator StartGetReward(string RewardName)
    {
        yield return StartCoroutine(playerStats.serverClientConnect.ApplyPlayerStats(playerStats.serverClientConnect.Username));
        RewardPanel.SetActive(true);
        switch (RewardName)
        {
            case "+ 400 БАЛАНС":
                playerStats.Balance += 400;
                break;
            case "+ 1200 БАЛАНС":
                playerStats.Balance += 1200;
                break;
            case "+ 5000 БАЛАНС":
                playerStats.Balance += 5000;
                break;
            case "+ 10000 БАЛАНС":
                playerStats.Balance += 10000;
                break;
            case "+ 20 ОПЫТА":
                playerStats.CurrentExperience += 20;
                break;
            case "+ 70 ОПЫТА":
                playerStats.CurrentExperience += 70;
                break;
            case "+ 200 ОПЫТА":
                playerStats.CurrentExperience += 200;
                break;
            case "+ 500 ОПЫТА":
                playerStats.CurrentExperience += 500;
                break;
            default:
                if(RewardName == WeaponName)
                {
                    playerStorage.CreateWeapon(WeaponId);
                    playerStats.SetNewWeapon(WeaponId, WeaponName);
                    playerStorage.connectToServer.StartErrorText(Color.red, "", 2);
                }
                break;
        }
        WinRewardText.text = "ПОЗДРАВЛЯЕМ, ВЫ ВЫИГРАЛИ: " + RewardName;
        playerStorage.connectToServer.StartErrorText(Color.red, "", 4); 
        playerStats.SavePlayerStats();
        playerStats.ResetMenuUi();
        playerStats.CheckNewRank();
    }
}
