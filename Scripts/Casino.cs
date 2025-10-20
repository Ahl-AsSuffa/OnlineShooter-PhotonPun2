using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Casino : MonoBehaviour
{
    public Text BalanceText, ComputerNumberText, ErrorText;
    public PlayerStats playerStats;
    public InputField GetMoneyInput;
    public Animator ErrorPanelAnim;
    public void ClickCasino(int WhatIs)
    {
        StartCoroutine(StartClickCasino(WhatIs));
    }
    private IEnumerator StartClickCasino(int WhatIs)
    {
        string inputText = GetMoneyInput.text;

        // 1. ���������, ��� ������ �� ������
        if (string.IsNullOrEmpty(inputText) || string.IsNullOrWhiteSpace(inputText))
        {
            ErrorPanelAnim.SetTrigger("error");
            ErrorText.text = "����� ������ �� ����� ���� ������.";
            yield break;
        }
        // 2. ���������, ��� ������ �������� ������ �����
        int moneyValue = 0;
        if (IsNumeric(inputText))
        {
            moneyValue = int.Parse(inputText); // ���� ����� ������������� � �����
        }
        else
        {
            ErrorPanelAnim.SetTrigger("error");
            ErrorText.text = "����� �����!";
            yield break;
        }
        yield return StartCoroutine(playerStats.serverClientConnect.ApplyPlayerStats(playerStats.serverClientConnect.Username));

        if (moneyValue > playerStats.Balance)
        {
            ErrorPanelAnim.SetTrigger("error");
            ErrorText.text = "�� ������� �����!";
            yield break;
        }

        int ComputerNumber = Random.Range(-1, 1000001);
        if (WhatIs == 0)
        {
            if (ComputerNumber <= 499999)
            {
                playerStats.Balance += moneyValue;
                ComputerNumberText.text = "����� ����������: " + ComputerNumber.ToString() + " / �� ��������!";
            }
            else
            {
                playerStats.Balance -= moneyValue;
                ComputerNumberText.text = "����� ����������: " + ComputerNumber.ToString() + " / �� ��������� :(!";
            }
        }
        if (WhatIs == 1)
        {
            if (ComputerNumber >= 500000)
            {
                playerStats.Balance += moneyValue;
                ComputerNumberText.text = "����� ����������: " + ComputerNumber.ToString() + " / �� ��������!";
            }
            else
            {
                playerStats.Balance -= moneyValue;
                ComputerNumberText.text = "����� ����������: " + ComputerNumber.ToString() + " / �� ��������� :(!";
            }
        }
        ResetBalance();
    }
    public void ResetBalance()
    {
        BalanceText.text = "������: " + playerStats.Balance.ToString();
        playerStats.BalanceText.text = "������: " + playerStats.Balance.ToString();
        playerStats.SavePlayerStats();
    }
    private bool IsNumeric(string text)
    {
        int result;
        return int.TryParse(text, out result);
    }
}
