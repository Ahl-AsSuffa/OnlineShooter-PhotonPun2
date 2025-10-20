using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Shooting : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public LoadWeapon loadWeapon;
    public PlayerStats playerStats;

    private float shootCooldown;
    public Camera plCamera;

    public Text CurrentAmmosText;

    public LineRenderer lineRenderer;
    public float laserDuration = 0.1f;

    public Image HitImg;
    public AudioSource ShootAudoiSource;
    public GameObject PressFBtn;
    public AudioClip HitSound;

    void Start()
    {
        if (playerMovement.photonView.IsMine)  // Проверка на локальный игрок
        {
            plCamera = playerMovement.playerCamera;
            StartCoroutine(Starting());
        }
    }
    private IEnumerator Starting()
    {
        yield return new WaitForSeconds(2);
        if (playerMovement.photonView.IsMine)  // Проверка на локальный игрок
        {
            StartCoroutine(ResetUI());
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (playerMovement.photonView.IsMine)  // Проверка на локальный игрок
        {
            if (shootCooldown > 0f)
                shootCooldown -= Time.deltaTime;

            if (!playerMovement.IsPause && !playerStats.IsDead)
            {
                if (Input.GetKey(KeyCode.Mouse0) && shootCooldown <= 0f)
                {
                    if (loadWeapon.weaponStats.CurrentAmmo > 0 && !loadWeapon.weaponStats.Reloading)
                    {
                        Shoot();
                    }
                    else
                    {
                        playerMovement.Shooting = false;
                        playerMovement.BodyAnimator.ResetTrigger("shoot");
                    }
                    shootCooldown = loadWeapon.weaponStats.RateOfFire;

                    if (Input.GetKeyDown(KeyCode.Mouse0) && loadWeapon.weaponStats.CurrentAmmo == 0)
                        playerMovement.photonView.RPC("ShootAudio", RpcTarget.All, "NoAmmos");
                }
                else if (!Input.GetKey(KeyCode.Mouse0))
                {
                    playerMovement.Shooting = false;
                    playerMovement.BodyAnimator.ResetTrigger("shoot");
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    if (loadWeapon.weaponStats.CurrentAmmo != loadWeapon.weaponStats.MaxAmmo)
                    {
                        if (!playerMovement.Aiming && !loadWeapon.weaponStats.Reloading)
                        {
                            StartCoroutine(Reload());
                            loadWeapon.weaponStats.Reloading = true;
                        }
                    }
                }
            }
            TakeBooster();
            UpdateBooster();
        }
    }

    public void TakeBooster()
    {
        RaycastHit hit;
        int ignoreLayer = LayerMask.NameToLayer("IgnoreRaycast");
        int layerMask = ~(1 << ignoreLayer);
        if (Physics.Raycast(plCamera.transform.position, plCamera.transform.forward, out hit, 4, layerMask))
        {
            if (hit.collider.CompareTag("Booster"))
            {
                PressFBtn.SetActive(true);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Boosters currentBooster = hit.collider.transform.root.GetComponent<Boosters>();
                    if (currentBooster != null)
                    {
                        switch (currentBooster.BoosterName)
                        {
                            case "speed":
                                playerMovement.currentSpeedBoost = currentBooster.BoostValue;
                                SpeedBoosterText.gameObject.SetActive(true);
                                BoosterActive = true;
                                SpeedBoosterTimer = 30;
                                playerMovement.photonView.RPC("ShowSpeedTrail", RpcTarget.AllBuffered, true);
                                break;
                            case "jump":
                                playerMovement.currentJumpBoost = currentBooster.BoostValue;
                                JumpBoosterText.gameObject.SetActive(true);
                                BoosterActive = true;
                                JumpBoosterTimer = 30;
                                break;
                            case "scale":
                                playerMovement.transform.localScale = new Vector3(currentBooster.BoostValue, currentBooster.BoostValue, currentBooster.BoostValue);
                                ScaleBoosterText.gameObject.SetActive(true);
                                BoosterActive = true;
                                if (playerStats.Health! > 100)
                                    playerStats.Health += 300;
                                else
                                {
                                    playerStats.Health = 400;
                                }
                                playerStats.ResetUI();
                                ScaleBoosterTimer = 30;
                                break;
                        }
                        currentBooster.BoostGetted();
                    }
                }
            }
        }
        else
        {
            PressFBtn.SetActive(false);
        }
    }
    private void Shoot()
    {
        playerMovement.photonView.RPC("PlayEffects", RpcTarget.AllBuffered);
        playerMovement.photonView.RPC("ShootAudio", RpcTarget.All, "shoot");
        loadWeapon.weaponStats.CurrentAmmo--;
        playerStats.ShootedAmmos++;
        playerStats.CurrentMatchShoots++;
        CurrentAmmosText.text = loadWeapon.weaponStats.CurrentAmmo + " / " + loadWeapon.weaponStats.MaxAmmo;

        Vector3 origin = loadWeapon.weaponStats.ShootFireParticles.transform.position;
        Vector3 direction = new Vector3();
        if(!playerMovement.Aiming)
            direction = GetSpreadDirection(plCamera.transform.forward, loadWeapon.weaponStats.spread);
        else
            direction = plCamera.transform.forward;

        Vector3 endPoint;

        playerMovement.Shooting = true;
        playerMovement.BodyAnimator.SetTrigger("shoot");
        //Отдача
        float verticalRecoil = Random.Range(loadWeapon.weaponStats.recoilUp * 0.8f, loadWeapon.weaponStats.recoilUp * 1.2f);
        float horizontalRecoil = Random.Range(-loadWeapon.weaponStats.recoilSide, loadWeapon.weaponStats.recoilSide);
        playerMovement.ApplyRecoil(verticalRecoil, horizontalRecoil);
        int ignoreLayer = LayerMask.NameToLayer("IgnoreRaycast");
        int layerMask = ~(1 << ignoreLayer);
        RaycastHit hit;
        if (Physics.Raycast(plCamera.transform.position, direction, out hit, loadWeapon.weaponStats.Range, layerMask))
        {
            endPoint = hit.point;
            Debug.Log("Попал в " + hit.transform.name);
            // Если у объекта есть скрипт с жизнью, нанесём урон
            PhotonView target = hit.transform.root.GetComponentInParent<PhotonView>();
            if (target != null && !target.IsMine)
            {
                GameObject impactPlayer = Instantiate(loadWeapon.weaponStats.impactEffectPlayer, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactPlayer, 2f);
                StartCoroutine(HitUi());
                ShootAudoiSource.PlayOneShot(HitSound);
                PlayerStats hitPlayerStats = target.transform.root.GetComponentInParent<PlayerStats>();
                if (hitPlayerStats != null && hitPlayerStats.IsTeamMate)
                    return;
                else
                {
                    Debug.Log("TargetTut");
                    Target hitTarget = target.transform.root.GetComponentInParent<Target>();
                    if (hitTarget != null)
                    {
                        float TargetEndDamage = GetHitPoint(hit.transform.name, loadWeapon.weaponStats.Damage);
                        HitTarget(hitTarget, TargetEndDamage);
                        return;
                    }
                }

                if (hitPlayerStats == null)
                    return;

                float endDamage = GetHitPoint(hit.transform.name, loadWeapon.weaponStats.Damage);
                playerStats.Hits++;
                playerStats.CurrentMatchHits++;
                playerStats.CurrentMatchDamage += endDamage;
                playerStats.SetPlayerData();

                PhotonView currentPlayerView = GetComponent<PhotonView>();
                target.RPC("TakeDamage", target.Owner, endDamage, currentPlayerView.ViewID, hit.transform.name);
            }
            else
            {
                if (target != null && target.IsMine)
                {
                    Target hitTarget = target.transform.root.GetComponentInParent<Target>();
                    if (hitTarget != null)
                    {
                        GameObject impactPlayer = Instantiate(loadWeapon.weaponStats.impactEffectPlayer, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(impactPlayer, 2f);
                        StartCoroutine(HitUi());
                        ShootAudoiSource.PlayOneShot(HitSound);
                        float TargetEndDamage = GetHitPoint(hit.transform.name, loadWeapon.weaponStats.Damage);
                        HitTarget(hitTarget, TargetEndDamage);
                    }
                }
                else
                {
                    GameObject impactWall = Instantiate(loadWeapon.weaponStats.imapctEffectWall, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactWall, 2f);
                }
            }
        }
        else
        {
            endPoint = origin + direction * loadWeapon.weaponStats.Range;
        }
        playerMovement.photonView.RPC("ShowLaserRPC", RpcTarget.All, origin, endPoint);
    }
    public float SpeedBoosterTimer = 30, JumpBoosterTimer = 30, ScaleBoosterTimer = 30;
    public bool BoosterActive = false;
    public Text SpeedBoosterText, JumpBoosterText, ScaleBoosterText;
    public GameObject SpeedTrail;
    public void UpdateBooster()
    {
        if (!BoosterActive)
            return;

        if (ScaleBoosterTimer > 0)
            ScaleBoosterTimer -= Time.deltaTime;
        if (JumpBoosterTimer > 0)
            JumpBoosterTimer -= Time.deltaTime;
        if (SpeedBoosterTimer > 0)
            SpeedBoosterTimer -= Time.deltaTime;

        SpeedBoosterText.text = "БУСТЕР СКОРОСТИ ЗАКОНЧИТСЯ ЧЕРЕЗ: " + SpeedBoosterTimer.ToString("F1");
        JumpBoosterText.text = "БУСТЕР ПРЫЖКА ЗАКОНЧИТСЯ ЧЕРЕЗ: " + JumpBoosterTimer.ToString("F1");
        ScaleBoosterText.text = "БУСТЕР УВЕЛИЧЕНИЯ ЗАКОНЧИТСЯ ЧЕРЕЗ: " + ScaleBoosterTimer.ToString("F1");

        if (JumpBoosterTimer <= 0 && ScaleBoosterTimer <= 0 && SpeedBoosterTimer <= 0)
        {
            UpdateSpeedBoost();
            UpdateJumpBoost();
            UpdateScaleBoost();
            BoosterActive = false;
        }
    }

    [PunRPC]
    public void ShowLaserRPC(Vector3 start, Vector3 end)
    {
        StartCoroutine(ShowLaser(start, end));
    }
    [PunRPC]
    public void ShootAudio(string AudioName)
    {
        switch (AudioName)
        {
            case "shoot":
                ShootAudoiSource.PlayOneShot(loadWeapon.weaponStats.shootAudio);
                break;
            case "reload":
                ShootAudoiSource.PlayOneShot(loadWeapon.weaponStats.ReloadAudio);
                break;
            case "NoAmmos":
                ShootAudoiSource.PlayOneShot(loadWeapon.weaponStats.NoAmmos);
                break;
        }

    }
    IEnumerator Reload()
    {
        playerMovement.BodyAnimator.SetBool("reload", true);
        playerMovement.photonView.RPC("ShootAudio", RpcTarget.AllBuffered, "reload");
        yield return new WaitForSeconds(loadWeapon.weaponStats.ReloadTime);
        playerMovement.BodyAnimator.SetBool("reload", false);
        loadWeapon.weaponStats.CurrentAmmo = loadWeapon.weaponStats.MaxAmmo;
        StartCoroutine(ResetUI());
        loadWeapon.weaponStats.Reloading = false;
    }

    IEnumerator ResetUI()
    {
        yield return new WaitForSeconds(.1f);
        CurrentAmmosText.text = loadWeapon.weaponStats.CurrentAmmo + " / " + loadWeapon.weaponStats.MaxAmmo;
    }
    IEnumerator ShowLaser(Vector3 start, Vector3 end)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Устанавливаем начальный цвет с полной прозрачностью
        Color startColor = new Color(1f, 1f, 1f, 0.5f); // Начальный цвет с альфой 0.5
        Color endColor = new Color(1f, 1f, 1f, 0f);    // Конечный цвет с альфой 0

        // Явно сбрасываем цвет lineRenderer на начальное значение
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = startColor;

        float duration = laserDuration;
        float elapsed = 0f;

        // Плавное изменение прозрачности
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Color currentColor = Color.Lerp(startColor, endColor, t);
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;

            yield return null;
        }

        // Отключаем lineRenderer после завершения эффекта
        lineRenderer.enabled = false;
    }
    IEnumerator HitUi()
    {
        HitImg.enabled = true;
        yield return new WaitForSeconds(.2f);
        HitImg.enabled = false;
    }

    [PunRPC]
    public void PlayEffects()
    {
        loadWeapon.weaponStats.ShootParticles.Play();
        loadWeapon.weaponStats.ShootFireParticles.Play();
    }

    [PunRPC]
    public void TakeDamage(float Damage, int attackerViewId, string HitPoint)
    {
        playerStats.TakeDamage(Damage, attackerViewId, HitPoint);
    }
    private float GetHitPoint(string HitName, float WeaponStartDamage)
    {
        switch (HitName)
        {
            case "head":
                WeaponStartDamage *= loadWeapon.weaponStats.HeadMultiplier;
                break;
            case "hand_r":
            case "hand_l":
                WeaponStartDamage *= loadWeapon.weaponStats.HandMultiplier;
                break;
            case "lowerarm_r":
            case "lowerarm_l":
                WeaponStartDamage *= loadWeapon.weaponStats.LowerArmMultiplier;
                break;
            case "upperarm_r":
            case "upperarm_l":
                WeaponStartDamage *= loadWeapon.weaponStats.UpperArmMultiplier;
                break;
            case "clavicle_r":
            case "clavicle_l":
                WeaponStartDamage *= loadWeapon.weaponStats.ClavicleMultiplier;
                break;
            case "spine_03":
                WeaponStartDamage *= loadWeapon.weaponStats.chestMultiplier;
                break;
            case "spine_02":
                WeaponStartDamage *= loadWeapon.weaponStats.BellyMultiplier;
                break;
            case "spine_01":
                WeaponStartDamage *= loadWeapon.weaponStats.GroinMultiplier;
                break;
            case "foot_r":
            case "foot_l":
                WeaponStartDamage *= loadWeapon.weaponStats.FootMultiplier;
                break;
            case "calf_r":
            case "calf_l":
                WeaponStartDamage *= loadWeapon.weaponStats.CalfMultiplier;
                break;
            case "thigh_r":
            case "thigh_l":
                WeaponStartDamage *= loadWeapon.weaponStats.ThighMultiplier;
                break;
            default:
                WeaponStartDamage = loadWeapon.weaponStats.Damage;
                break;
        }
        return WeaponStartDamage;
    }

    [PunRPC]
    public void ShowSpeedTrail(bool SetActive)
    {
        SpeedTrail.SetActive(SetActive);
    }
    private void UpdateSpeedBoost()
    {
        playerMovement.currentSpeedBoost = 1;
        SpeedBoosterText.gameObject.SetActive(false);
        if (SpeedTrail.activeSelf)
            playerMovement.photonView.RPC("ShowSpeedTrail", RpcTarget.AllBuffered, false);
        SpeedBoosterTimer = 0;
    }
    private void UpdateJumpBoost()
    {
        playerMovement.currentJumpBoost = 1;
        JumpBoosterText.gameObject.SetActive(false);
        JumpBoosterTimer = 0;
    }
    private void UpdateScaleBoost()
    {
        playerMovement.transform.localScale = new Vector3(1, 1, 1);
        ScaleBoosterText.gameObject.SetActive(false);
        if (playerStats.Health > 300)
            playerStats.Health -= 300;
        else
        {
            playerStats.Health -= 100;
        }
        playerStats.ResetUI();
        ScaleBoosterTimer = 0;
    }

    private void HitTarget(Target hitTarget, float Damage)
    {
        hitTarget.GetDamage(Damage);
    }
    private Vector3 GetSpreadDirection(Vector3 forward, float spread)
    {
        // Создаем случайное смещение
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // Добавляем смещение к направлению
        Vector3 spreadDirection = forward + plCamera.transform.right * x + plCamera.transform.up * y;
        return spreadDirection.normalized;
    }
}
