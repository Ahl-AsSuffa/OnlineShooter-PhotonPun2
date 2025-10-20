using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public Animator LeggsAnimator, BodyAnimator, HandsAnimator;

    public Camera playerCamera; //Камера
    public float CustomWalkSpeed = 6f; //Обычная скорость задаваемая в инспекторе
    public float CustomRunSpeed = 12f; //Скорость бега задаваемая в инспекторе
    public float jumpPower = 7f; //Сила прыжка
    public float gravity = 10f; // Сила Гравитации
    public float lookSpeed = 2f; // Чувствительность мыши
    public float lookXLimit = 45f; //Ограничение взгляда вверх-вниз

    private float walkSpeed, runSpeed;
    public float currentSpeedBoost = 1, currentJumpBoost = 1;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    public LoadWeapon loadWeapon;
    public Shopping shopping;
    public Shooting shooting;

    public bool Run = false, Walk = false, Aiming = false, IsLocalPlayer = true, Shooting = false, IsPause = false;
    public GameObject[] localCharachterParts;

    public GameObject PlayerSpine;
    public Transform PlayerHead;

    private bool canMove = true;

    private float currentRecoilX = 0f;  // вертикально (вверх-вниз)
    private float currentRecoilY = 0f;  // горизонтально (влево-вправо)

    public PhotonView photonView;
    public Canvas canvas;
    public PlayerStats playerStats;
    public Image AimImg;
    public GameObject PausePanel, ShopPanel, DiePanel, SettingsPanel;
    public DeathMatchGameManager gameManager;
    private bool teleport = false;
    private void Awake()
    {
        if (!photonView.IsMine)
        {
            canvas.enabled = false;
            playerCamera.gameObject.SetActive(false);
        }
        else
        {
            lookSpeed = playerStats.MouseSensitivity;
            gameManager = FindAnyObjectByType<DeathMatchGameManager>();
            IsPause = !gameManager.GameStarted;
            if (IsPause)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    void Start()
    {
        if (photonView.IsMine)
        {
            characterController = GetComponent<CharacterController>();
            walkSpeed = CustomWalkSpeed;
            runSpeed = CustomRunSpeed;

            if (IsLocalPlayer)
            {
                for (int i = 0; i < localCharachterParts.Length; i++)
                {
                    localCharachterParts[i].SetActive(false);
                }
            }
            playerStats.StaminaRegenFill.fillAmount = playerStats.Stamina / playerStats.MaxStamina;
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            if (!teleport)
                Moving();
            PlayerDownUnderMap();
            UpdateInputBtns();
            UpdateAnimations();
            RegenStamina();
            UpdateFastSlots();
        }
    }

    private void Moving()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift) && playerStats.Stamina > 0;
        float speed = isRunning ? runSpeed * currentSpeedBoost : walkSpeed * currentSpeedBoost;

        float inputX = 0;
        float inputY = 0;

        if (!IsPause && !playerStats.IsDead && canMove && characterController.isGrounded)
        {
            inputX = Input.GetAxis("Vertical");
            inputY = Input.GetAxis("Horizontal");
        }

        float curSpeedX = speed * inputX;
        float curSpeedY = speed * inputY;

        float movementDirectionY = moveDirection.y;

        // Только если на земле — применяем ввод
        if (characterController.isGrounded)
        {
            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            if (Input.GetButton("Jump") && canMove)
            {
                if (playerStats.Stamina >= 10)
                {
                    moveDirection.y = jumpPower * currentJumpBoost;
                    playerStats.Stamina -= 10;
                }
            }
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Применяем гравитацию
        moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);

        if (canMove)
        {
            if (!IsPause && !playerStats.IsDead)
            {
                rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
                rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
                transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

                //Поворот верхней части тела
                PlayerSpine.transform.localEulerAngles = new Vector3(0, 0, rotationX);
                playerCamera.transform.position = PlayerHead.position;


                // Применяем отдачу (вращение камеры)
                rotationX += currentRecoilX;
                transform.Rotate(0, currentRecoilY, 0);
                // Затухание отдачи со временем
                if (loadWeapon.weaponStats != null)
                {
                    currentRecoilX = Mathf.Lerp(currentRecoilX, 0f, Time.deltaTime * loadWeapon.weaponStats.recoilReturnSpeed);
                    currentRecoilY = Mathf.Lerp(currentRecoilY, 0f, Time.deltaTime * loadWeapon.weaponStats.recoilReturnSpeed);
                }
            }
        }

        float movementMagnitude = new Vector2(inputX, inputY).magnitude;

        if (movementMagnitude > 0.1f)
        {
            if (isRunning && playerStats.Stamina > 0)
            {
                Run = true;
                Walk = false;
                playerStats.Stamina -= Time.deltaTime * 5;
                playerStats.StaminaRegenFill.fillAmount = playerStats.Stamina / playerStats.MaxStamina;
                StaminaRegenTimer = 3;
            }
            else
            {
                Run = false;
                Walk = true;
            }
        }
        else
        {
            Run = false;
            Walk = false;
        }
    }
    private void UpdateAnimations()
    {
        if (characterController.isGrounded)
        {
            LeggsAnimator.SetBool("run", Run);
            LeggsAnimator.SetBool("walk", Walk);
            BodyAnimator.SetBool("run", Run);
            BodyAnimator.SetBool("walk", Walk);
        }

        if (Input.GetKey(KeyCode.Mouse1) && !IsPause)
        {
            Aiming = true;
            switch (loadWeapon.weaponStats.Type)
            {
                case WeaponStats.WeaponType.SniperRifle:
                    lookSpeed = playerStats.SniperAimMouseSensitivity;
                    break;
                default:
                    lookSpeed = playerStats.AimMouseSentitivity;
                    break;
            }
            AimImg.enabled = false;
            BodyAnimator.SetBool("aim", true);
            if (loadWeapon.weaponStats.Type != WeaponStats.WeaponType.SniperRifle)
                playerCamera.fieldOfView = 30;
            else
                playerCamera.fieldOfView = 10;
        }
        else
        {
            Aiming = false;
            lookSpeed = playerStats.MouseSensitivity;
            AimImg.enabled = true;
            BodyAnimator.SetBool("aim", false);
            playerCamera.fieldOfView = 70;
        }
    }
    public void ApplyRecoil(float recoilX, float recoilY)
    {
        currentRecoilX += recoilX;
        currentRecoilY += recoilY;
    }

    float StaminaRegenTimer = 3f;
    public void RegenStamina()
    {
        if (StaminaRegenTimer > 0)
        {
            StaminaRegenTimer -= Time.deltaTime;
        }
        else
        {
            if (playerStats.Stamina < playerStats.MaxStamina)
            {
                playerStats.Stamina += playerStats.StaminaRegen;
                if (playerStats.Stamina > playerStats.MaxStamina)
                    playerStats.Stamina = playerStats.MaxStamina;
                playerStats.StaminaRegenFill.fillAmount = playerStats.Stamina / playerStats.MaxStamina;
                StaminaRegenTimer = .5f;
            }
        }
    }

    private void UpdateInputBtns()
    {
        if (!gameManager.GameStarted)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            IsPause = !IsPause;
            PausePanel.SetActive(IsPause);
            ShopPanel.SetActive(false);
            SettingsPanel.SetActive(false);
            if (IsPause)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            IsPause = !IsPause;
            ShopPanel.SetActive(IsPause);
            SettingsPanel.SetActive(false);
            if (IsPause)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                shopping.MoneyText.text = "Баланс: " + playerStats.Money.ToString();
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }


        if (Input.GetKey(KeyCode.LeftControl))
        {
            LeggsAnimator.SetBool("crouch", true);
            characterController.height = 1.4f;
            characterController.center = new Vector3(0, -0.1f, 0);
            runSpeed = 2.5f;
            walkSpeed = 2.5f;
        }
        else
        {
            LeggsAnimator.SetBool("crouch", false);
            characterController.height = 1.8f;
            characterController.center = new Vector3(0, -0.33f, 0);
            runSpeed = CustomRunSpeed;
            walkSpeed = CustomWalkSpeed;
        }
    }
    private void UpdateFastSlots()
    {
        if (IsPause)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (playerStats.equippedWeapons.Count < 1)
                return;
            if (loadWeapon.CurrentWeapon.GetComponent<WeaponStats>().WeaponName != playerStats.equippedWeapons[0].weaponName)
            {
                loadWeapon.EquipWeapon(playerStats.equippedWeapons[0].weaponID);
                playerStats.WeaponNameText.text = playerStats.equippedWeapons[0].weaponName;
                shooting.CurrentAmmosText.text = loadWeapon.weaponStats.CurrentAmmo.ToString() + " / " + loadWeapon.weaponStats.MaxAmmo.ToString();
                playerStats.SeeWeaponPanel.SetTrigger("changeWeapon");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (playerStats.equippedWeapons.Count < 2)
                return;
            if (loadWeapon.CurrentWeapon.GetComponent<WeaponStats>().WeaponName != playerStats.equippedWeapons[1].weaponName)
            {
                loadWeapon.EquipWeapon(playerStats.equippedWeapons[1].weaponID);
                playerStats.WeaponNameText.text = playerStats.equippedWeapons[1].weaponName;
                shooting.CurrentAmmosText.text = loadWeapon.weaponStats.CurrentAmmo.ToString() + " / " + loadWeapon.weaponStats.MaxAmmo.ToString();
                playerStats.SeeWeaponPanel.SetTrigger("changeWeapon");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (playerStats.equippedWeapons.Count < 3)
                return;
            if (loadWeapon.CurrentWeapon.GetComponent<WeaponStats>().WeaponName != playerStats.equippedWeapons[2].weaponName)
            {
                loadWeapon.EquipWeapon(playerStats.equippedWeapons[2].weaponID);
                playerStats.WeaponNameText.text = playerStats.equippedWeapons[2].weaponName;
                shooting.CurrentAmmosText.text = loadWeapon.weaponStats.CurrentAmmo.ToString() + " / " + loadWeapon.weaponStats.MaxAmmo.ToString();
                playerStats.SeeWeaponPanel.SetTrigger("changeWeapon");
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (playerStats.equippedWeapons.Count < 4)
                return;
            if (loadWeapon.CurrentWeapon.GetComponent<WeaponStats>().WeaponName != playerStats.equippedWeapons[3].weaponName)
            {
                loadWeapon.EquipWeapon(playerStats.equippedWeapons[3].weaponID);
                playerStats.WeaponNameText.text = playerStats.equippedWeapons[3].weaponName;
                shooting.CurrentAmmosText.text = loadWeapon.weaponStats.CurrentAmmo.ToString() + " / " + loadWeapon.weaponStats.MaxAmmo.ToString();
                playerStats.SeeWeaponPanel.SetTrigger("changeWeapon");
            }
        }
    }

    public void CheckPause()
    {
        if (IsPause)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public void LeaveRoom()
    {
        playerStats.SavePlayerStats();
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }
    bool hasFallen = false;
    public void PlayerDownUnderMap()
    {
        if (transform.position.y <= -100)
        {
            playerStats.TakeDamage(9999, 0, "spine_01");
        }

        if (playerStats.IsDead)
        {
            if (gameManager.IsTeamGame)
            {
                if (!hasFallen)
                {
                    hasFallen = true;
                    transform.rotation = Quaternion.Euler(90f, 0f, 90f);
                    characterController.height = .5f;
                    playerStats.DeadText.text = "Ты погиб. Жди нового раунда!";
                    DiePanel.SetActive(true);
                }
                return;
            }

            if (playerStats.DeadTime > 0f)
            {
                playerStats.DeadTime -= Time.deltaTime;
                playerStats.DeadText.text = "ВОЗРОЖДЕНИЕ ЧЕРЕЗ: " + playerStats.DeadTime.ToString("F1");

                if (!hasFallen)
                {
                    hasFallen = true;
                    transform.rotation = Quaternion.Euler(90f, 0f, 90f);
                    characterController.height = .5f;
                    DiePanel.SetActive(true);
                }
            }
            else
            {
                // Сброс при респауне
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                characterController.height = 1.8f;
                playerStats.Respawn();
                hasFallen = false;
                DiePanel.SetActive(false);
            }
        }
    }
    private bool justTeleported = false;
    private float teleportCooldown = 0.1f;
    public void respawn()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        characterController.height = 1.8f;
        playerStats.Respawn();
        hasFallen = false;
        DiePanel.SetActive(false);
    }
    public void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        if (justTeleported) return;

        if (other.CompareTag("Teleport1"))
        {
            teleport = true;
            Transform teleport2 = GameObject.FindGameObjectWithTag("Teleport2").transform;
            StartCoroutine(Teleport(teleport2.position));
            transform.rotation = transform.rotation = Quaternion.Euler(0f, teleport2.rotation.y - 90, 0f);
            Debug.Log("Телепорт к 2 " + other.name);
        }
        else if (other.CompareTag("Teleport2"))
        {
            teleport = true;
            Transform teleport1 = GameObject.FindGameObjectWithTag("Teleport1").transform;
            StartCoroutine(Teleport(teleport1.position));
            transform.rotation = transform.rotation = Quaternion.Euler(0f, teleport1.rotation.y - 90, 0f);
            Debug.Log("Телепорт к 1 " + other.name);
        }
    }
    private IEnumerator Teleport(Vector3 targetPosition)
    {
        justTeleported = true;
        transform.position = targetPosition;
        yield return new WaitForSeconds(teleportCooldown);
        justTeleported = false;
        teleport = false;
    }
}
