using UnityEngine;

public class WeaponStats : MonoBehaviour
{
    public string WeaponName = "Пистолет";
    public int WeaponID = 0;

    // Урон и дальность
    [Header("Урон и дальность")]
    public float Damage = 20f;
    public float Range = 100f;

    [Header("Recoil Settings")]
    public float recoilUp = 5f;             // вертикальная отдача
    public float recoilSide = 2f;           // горизонтальная отдача
    public float recoilReturnSpeed = 5f;    // как быстро возвращается камера
    public float spread = 1f; 
    [Header("Спрайт Оружия")]
    public Sprite WeaponIcon;

    // Скорострельность (выстрелов в секунду)
    [Header("Скорострельность")]
    public float RateOfFire = 0.5f;

    // Размер магазина
    [Header("Размер магазина / Стоимость")]
    public int MaxAmmo = 12;
    public int CurrentAmmo = 12;
    public int Price = 0;

    // Время перезарядки
    [Header("Время перезарядки")]
    public float ReloadTime = 1.5f;

    // Тип оружия
    public enum WeaponType { Pistol,Submachine_gun ,Rifle, Shotgun, Melee, SniperRifle }
    public WeaponType Type = WeaponType.Pistol;

    //Эффекты Стрельбы
    [Header("Эффекты Стрельбы")]
    public ParticleSystem ShootParticles;
    public ParticleSystem ShootFireParticles;
    public GameObject impactEffectPlayer, imapctEffectWall;

    [Header("Множители урона")]
    public float HeadMultiplier,HandMultiplier, LowerArmMultiplier, 
        UpperArmMultiplier, ClavicleMultiplier, chestMultiplier, BellyMultiplier, GroinMultiplier, FootMultiplier, CalfMultiplier, ThighMultiplier;

    [Header("Звуки")]
    public AudioClip shootAudio,ReloadAudio, NoAmmos;

    public bool Reloading = false;
}
