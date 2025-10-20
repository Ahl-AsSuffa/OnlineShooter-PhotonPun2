using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    public string WeaponName = "��������";
    public int WeaponID = 0;

    // ���� � ���������
    [Header("���� � ���������")]
    public float Damage = 20f;
    public float Range = 100f;

    [Header("Recoil Settings")]
    public float recoilUp = 5f;             // ������������ ������
    public float recoilSide = 2f;           // �������������� ������
    public float recoilReturnSpeed = 5f;    // ��� ������ ������������ ������
    public float spread = 1f; 
    [Header("������ ������")]
    public Sprite WeaponIcon;

    // ���������������� (��������� � �������)
    [Header("����������������")]
    public float RateOfFire = 0.5f;

    // ������ ��������
    [Header("������ �������� / ���������")]
    public int MaxAmmo = 12;
    public int CurrentAmmo = 12;
    public int Price = 0;

    // ����� �����������
    [Header("����� �����������")]
    public float ReloadTime = 1.5f;

    // ��� ������
    public enum WeaponType { Pistol,Submachine_gun ,Rifle, Shotgun, Melee, SniperRifle }
    public WeaponType Type = WeaponType.Pistol;

    //������� ��������
    [Header("������� ��������")]
    public ParticleSystem ShootParticles;
    public ParticleSystem ShootFireParticles;
    public GameObject impactEffectPlayer, imapctEffectWall;

    [Header("��������� �����")]
    public float HeadMultiplier,HandMultiplier, LowerArmMultiplier, 
        UpperArmMultiplier, ClavicleMultiplier, chestMultiplier, BellyMultiplier, GroinMultiplier, FootMultiplier, CalfMultiplier, ThighMultiplier;

    [Header("�����")]
    public AudioClip shootAudio,ReloadAudio, NoAmmos;

    public bool Reloading = false;
}
