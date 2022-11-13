using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO add weapon selection schmolies (look in inputmaster)
// TODO use the kick and spread floats to randomize shots
// TODO add bullet holes

public class PlayerController : MonoBehaviour
{
    // player constants
    [SerializeField] float maxPlayerSpeed;
    [SerializeField] float sprintBonus;
    [SerializeField] float sprintAnimationPeriod;
    [SerializeField] float jumpSpeed;
    [SerializeField] float rollAngle;
    [SerializeField] float duckGunAngle;
    [SerializeField] float cameraRollFactor;
    [SerializeField, Range(0.0f,1.0f)] float lookSensitivity;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float groundDistance = 0.1f;
    [SerializeField] float chestReach = 3.0f;
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask interactableMask;

    [SerializeField] LayerMask targets;

    // movement variables
    private Vector3 velocity;
    private bool isGrounded;
    public float sprintStartTime;

    // weapon variables
    private bool canFire;
    private bool canReload;
    public Weapon weapon;

    // other variables
    //bool inChestRange;
    LootChest currentChest;
    LootChest raycastChest;
    LootChest triggerChest;

    // input management
    private InputManager inputManager;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool fireHeld;
    private bool jumpNext;
    private bool sprintHeld;
    private bool duckHeld;
    private float xRotation;

    // unity components
    [SerializeField] CharacterController controller;
    [SerializeField] CapsuleCollider capsuleCollider;
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform groundCheck;

    // unity references
    // HUD components
    [SerializeField] Text clipText;
    [SerializeField] Text reserveText;
    [SerializeField] Text interactText;

    private void Awake()
    {
        inputManager = new InputManager();
        inputManager.PlayerControls.Jump.performed += OnJump;
        inputManager.PlayerControls.Fire.performed += OnFire;
        inputManager.PlayerControls.Sprint.performed += OnSprint;
        inputManager.PlayerControls.Reload.performed += OnReload;
        inputManager.PlayerControls.Interact.performed += OnInteract;
        xRotation = 0;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnFire(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (canFire)
        {
            Shoot();
        }
    }

    private void OnReload(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (canReload)
        {
            //Debug.Log("Reloading..");
            weapon.PlayReloadAnimation();
        }
    }

    private void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        currentChest = (raycastChest != null && !raycastChest.isOpen) ? raycastChest : (triggerChest != null && !triggerChest.isOpen) ? triggerChest : null;

        if (currentChest != null)
        {
            currentChest.OnChestOpened();
            currentChest = null;
        }
    }

    private void OnSprint(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        sprintStartTime = Time.time;
    }

    private void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isGrounded)
            jumpNext = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        //canFire = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCanBools();

        // update HUD elements
        clipText.text = string.Format("{0}", weapon.clip);
        reserveText.text = string.Format("{0}", weapon.reserve);
        interactText.gameObject.SetActive(currentChest != null);

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2.0f;
        }

        // some checking raycasts
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward * playerCamera.farClipPlane);
        RaycastHit info;
        if (Physics.Raycast(ray, out info, chestReach, interactableMask))
        {
            if (info.collider.gameObject.tag == "Chest")
            {
                raycastChest = info.collider.gameObject.GetComponent<LootChest>();
            }
            else
                raycastChest = null;
        }
        else
            raycastChest = null;

        // move player
        float playerSpeed = sprintHeld ? maxPlayerSpeed * sprintBonus : maxPlayerSpeed;
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * playerSpeed * Time.deltaTime);

        // apply vertical movement
        velocity.y += gravity * Time.deltaTime;
        if (jumpNext)
        {
            velocity.y = jumpSpeed;
            jumpNext = false;
        }
        controller.Move(velocity * Time.deltaTime);

        // read the movement value
        moveInput = inputManager.PlayerControls.Move.ReadValue<Vector2>();

        lookInput = inputManager.PlayerControls.Look.ReadValue<Vector2>();
        lookInput *= lookSensitivity;
        xRotation -= lookInput.y;

        //fireHeld = inputManager.PlayerControls.Fire.ReadValue<float>() > 0;

        // perform look
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        // adding slight roll to the perspective with motion
        if (moveInput.x < 0)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, rollAngle * cameraRollFactor);
        else if (moveInput.x > 0)
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0 - rollAngle * cameraRollFactor);
        else
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        this.transform.Rotate(Vector3.up * lookInput.x);

        sprintHeld = inputManager.PlayerControls.Sprint.phase == UnityEngine.InputSystem.InputActionPhase.Started;
        duckHeld = inputManager.PlayerControls.Duck.phase == UnityEngine.InputSystem.InputActionPhase.Started;
        fireHeld = inputManager.PlayerControls.Fire.phase == UnityEngine.InputSystem.InputActionPhase.Started;

        float roll = moveInput.x > 0 ? 0 - rollAngle : moveInput.x < 0 ? rollAngle : 0;
        roll += duckHeld ? duckGunAngle : 0;
        // animate gun
        if (sprintHeld && moveInput.sqrMagnitude > 0)
        {
            if (weapon.type == WeaponType.PISTOL || weapon.type == WeaponType.SMG)
            {
                float theta = (Time.time - sprintStartTime) * Mathf.PI * 2 / sprintAnimationPeriod;
                Vector3 wepPos = new Vector3(0.5f, -0.5f, 0.8f);
                wepPos.y += 0.2f * Mathf.Sin(theta);
                wepPos.z -= 0.1f * Mathf.Sin(theta);
                float weaponZRot = 30 * Mathf.Sin(theta);

                weapon.transform.localPosition = wepPos;
                weapon.transform.localRotation = Quaternion.Euler(roll, -90f, weaponZRot + 50f);
            }
            else if (weapon.type == WeaponType.RIFLE)
            {
                float theta = (Time.time - sprintStartTime) * Mathf.PI * 2 / sprintAnimationPeriod;
                Vector3 wepPos = new Vector3(0.5f, -0.8f, 0.6f);
                wepPos.y += 0.1f * Mathf.Sin(theta);
                wepPos.x -= 0.5f * Mathf.Cos(theta / 2);
                float weaponZRot = 15 * Mathf.Sin(theta);

                weapon.transform.localPosition = wepPos;
                weapon.transform.localRotation = Quaternion.Euler(roll, -174f, weaponZRot);
            }
        }
        else
        {
            weapon.transform.localPosition = new Vector3(0.5f, -0.5f, 0.8f);
            weapon.transform.localRotation = Quaternion.Euler(roll, -90f, 0);
        }

        // Shrink player
        if (duckHeld)
        {
            Vector3 v = transform.localScale;
            v.y = 0.6f;
            transform.localScale = v;
            weapon.transform.localScale = new Vector3(1, 1.7f, 1);
            v = weapon.transform.localPosition;
            v.x = 0.7f;
            weapon.transform.localPosition = v;
        }
        else
        {
            Vector3 v = transform.localScale;
            v.y = 1f;
            transform.localScale = v;
            weapon.transform.localScale = new Vector3(1, 1, 1);
            v = weapon.transform.localPosition;
            v.x = 0.5f;
            weapon.transform.localPosition = v;
        }

        // fire
        if (canFire && fireHeld && weapon.fullAuto)
        {
            //Debug.Log("We firing boys");
            Shoot();
        }
    }

    private void FixedUpdate()
    {

    }

    private void LateUpdate()
    {
        if (raycastChest != null)
            currentChest = raycastChest;
    }

    private void Shoot()
    {
        if (weapon.clip < 0)
        {
            weapon.PlayReloadAnimation();
        }
        else
        {
            weapon.PlayFireAnimation();

            // pick end point of laser
            Vector3 laserStart = weapon.transform.position;
            Vector3 laserEnd = transform.position + (playerCamera.transform.forward * weapon.range);

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward * playerCamera.farClipPlane);
            RaycastHit info;
            if (Physics.Raycast(ray, out info, playerCamera.farClipPlane, targets))
            {
                //Debug.Log("hit");

                // update laser end position to the point of contact
                laserEnd = info.point;

                if (info.transform.gameObject.tag == "BreakableObject")
                {
                    BreakableObject bo = info.transform.gameObject.GetComponent<BreakableObject>();
                    if (bo.playSoundOnBreak)
                        GameManager.Instance.PlaySound(bo.breakSound, bo.transform.position);

                    if (!bo.persistent)
                    {
                        bo.spawner.InvokeRespawn();
                        Destroy(bo.gameObject);
                    }
                }
            }

            //Invoke("CanFire", weapon.fireDelay);
            GameManager.Instance.DrawLine(laserStart, laserEnd, weapon.tracerColor, weapon.tracerHeight, weapon.tracerLife);
        }
    }

    private void OnDrawGizmos()
    {
        //Ray ray = new Ray(this.weapon.bulletOrigin.position, this.playerCamera.transform.forward);
        //Gizmos.DrawLine(ray.origin, ray.origin + (ray.direction * 400));
        currentChest = (raycastChest != null) ? raycastChest : triggerChest;
        if (currentChest != null)
            Gizmos.DrawLine(this.transform.position, currentChest.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Chest")
        {
            LootChest otherChest = other.gameObject.GetComponent<LootChest>();

            if (otherChest.isOpen) return;

            if (triggerChest == null)
                triggerChest = otherChest;
            else if ((this.transform.position - otherChest.transform.position).sqrMagnitude < (this.transform.position - triggerChest.transform.position).sqrMagnitude)
            {
                // else if the new chest is closer, shift focus to that one
                triggerChest = otherChest;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Chest")
        {
            if (other.gameObject.GetComponent<LootChest>() != currentChest) return;
               
            //inChestRange = false;
            currentChest = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Chest")
        {
            LootChest otherChest = other.gameObject.GetComponent<LootChest>();

            if (otherChest.isOpen) return;

            if (triggerChest == null)
                triggerChest = otherChest;
            else if ((this.transform.position - otherChest.transform.position).sqrMagnitude < (this.transform.position - triggerChest.transform.position).sqrMagnitude)
            {
                // else if the new chest is closer, shift focus to that one
                triggerChest = otherChest;
            }
        }
    }

    private void UpdateCanBools()
    {
        // update canBools
        canFire = !weapon.isFiring && !weapon.isReloading && !sprintHeld && weapon.clip > 0;
        canReload = !weapon.isFiring && !weapon.isReloading && weapon.clip < weapon.clipCapacity && weapon.reserve > 0;
    }

    private void OnEnable()
    {
        inputManager.Enable();
    }

    private void OnDisable()
    {
        inputManager.Disable();
    }
}
