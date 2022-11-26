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
    [SerializeField] 
    private float _maxPlayerSpeed;
    
    [SerializeField] 
    private float _sprintBonus;
    
    [SerializeField] 
    private float _sprintAnimationPeriod;
    
    [SerializeField] 
    private float _jumpSpeed;
    
    [SerializeField] 
    private float _rollAngle;
    
    [SerializeField] 
    private float _duckGunAngle;
    
    [SerializeField] 
    private float _cameraRollFactor;
    
    [SerializeField, Range(0.0f,1.0f)]
    private float _lookSensitivity;

    [SerializeField] 
    private float _gravity = -9.81f;
    
    [SerializeField] 
    private float _groundDistance = 0.1f;
    
    [SerializeField] 
    private float _chestReach = 3.0f;
    
    [SerializeField] 
    private LayerMask _groundMask;
    
    [SerializeField] 
    private LayerMask _interactableMask;


    [SerializeField] LayerMask _targets;

    // movement variables
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _sprintStartTime;

    // weapon variables
    private bool _canFire;
    private bool _canReload;
    [SerializeField]
    private Weapon _weapon;

    // other variables
    LootChest _currentChest;
    LootChest _raycastChest;
    LootChest _triggerChest;

    // input management
    private InputManager _inputManager;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _fireHeld;
    private bool _jumpNext;
    private bool _sprintHeld;
    private bool _duckHeld;
    private float _xRotation;

    // unity components
    [SerializeField]
    CharacterController _controller;

    [SerializeField]
    CapsuleCollider _capsuleCollider;

    [SerializeField]
    Camera _playerCamera;

    [SerializeField]
    Transform _groundCheck;

    // unity references
    // HUD components
    [SerializeField]
    Text _clipText;

    [SerializeField]
    Text _reserveText;

    [SerializeField]
    Text _interactText;

    private GameManager _gameManager;

    private void Awake()
    {
        _inputManager = new InputManager();
        _inputManager.PlayerControls.Jump.performed += OnJump;
        _inputManager.PlayerControls.Fire.performed += OnFire;
        _inputManager.PlayerControls.Sprint.performed += OnSprint;
        _inputManager.PlayerControls.Reload.performed += OnReload;
        _inputManager.PlayerControls.Interact.performed += OnInteract;
        _xRotation = 0;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnFire(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (_canFire)
        {
            Shoot();
        }
    }

    private void OnReload(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (_canReload)
        {
            _weapon.PlayReloadAnimation();
        }
    }

    private void OnInteract(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _currentChest = (_raycastChest != null && !_raycastChest.IsOpen) ? _raycastChest : (_triggerChest != null && !_triggerChest.IsOpen) ? _triggerChest : null;

        if (_currentChest != null)
        {
            _currentChest.OnChestOpened();
            _currentChest = null;
        }
    }

    private void OnSprint(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        _sprintStartTime = Time.time;
    }

    private void OnJump(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (_isGrounded)
        {
            _jumpNext = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //canFire = true;
        _gameManager = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCanBools();

        // update HUD elements
        _clipText.text = string.Format("{0}", _weapon.Clip);
        _reserveText.text = string.Format("{0}", _weapon.Reserve);
        _interactText.gameObject.SetActive(_currentChest != null);

        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = 0.0f;
        }
        _velocity.y += _gravity * Time.deltaTime;

        // some checking raycasts
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward * _playerCamera.farClipPlane);
        RaycastHit info;
        if (Physics.Raycast(ray, out info, _chestReach, _interactableMask))
        {
            if (info.collider.gameObject.tag == "Chest")
            {
                _raycastChest = info.collider.gameObject.GetComponent<LootChest>();
            }
            else
            {
                _raycastChest = null;
            }
        }
        else
        {
            _raycastChest = null;
        }

        // move player
        float playerSpeed = _sprintHeld ? _maxPlayerSpeed * _sprintBonus : _maxPlayerSpeed;
        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        _controller.Move(move * playerSpeed * Time.deltaTime);

        // apply vertical movement
        if (_jumpNext)
        {
            _velocity.y = _jumpSpeed;
            _jumpNext = false;
        }
        _controller.Move(_velocity * Time.deltaTime);

        // read the movement value
        _moveInput = _inputManager.PlayerControls.Move.ReadValue<Vector2>();

        _lookInput = _inputManager.PlayerControls.Look.ReadValue<Vector2>();
        _lookInput *= _lookSensitivity;
        _xRotation -= _lookInput.y;

        //fireHeld = inputManager.PlayerControls.Fire.ReadValue<float>() > 0;

        // perform look
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        // adding slight roll to the perspective with motion
        if (_moveInput.x < 0)
        {
            _playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0, _rollAngle * _cameraRollFactor);
        }
        else if (_moveInput.x > 0)
        {
            _playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0 - _rollAngle * _cameraRollFactor);
        }
        else
        {
            _playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);
        }

        this.transform.Rotate(Vector3.up * _lookInput.x);

        _sprintHeld = _inputManager.PlayerControls.Sprint.phase == UnityEngine.InputSystem.InputActionPhase.Started;
        _duckHeld = _inputManager.PlayerControls.Duck.phase == UnityEngine.InputSystem.InputActionPhase.Started;
        _fireHeld = _inputManager.PlayerControls.Fire.phase == UnityEngine.InputSystem.InputActionPhase.Started;

        float roll = _moveInput.x > 0 ? 0 - _rollAngle : _moveInput.x < 0 ? _rollAngle : 0;
        roll += _duckHeld ? _duckGunAngle : 0;
        // animate gun
        if (_sprintHeld && _moveInput.sqrMagnitude > 0)
        {
            if (_weapon.Type == WeaponType.PISTOL || _weapon.Type == WeaponType.SMG)
            {
                float theta = (Time.time - _sprintStartTime) * Mathf.PI * 2 / _sprintAnimationPeriod;
                Vector3 wepPos = new Vector3(0.5f, -0.5f, 0.8f);
                wepPos.y += 0.2f * Mathf.Sin(theta);
                wepPos.z -= 0.1f * Mathf.Sin(theta);
                float weaponZRot = 30 * Mathf.Sin(theta);

                _weapon.transform.localPosition = wepPos;
                _weapon.transform.localRotation = Quaternion.Euler(roll, -90f, weaponZRot + 50f);
            }
            else if (_weapon.Type == WeaponType.RIFLE)
            {
                float theta = (Time.time - _sprintStartTime) * Mathf.PI * 2 / _sprintAnimationPeriod;
                Vector3 wepPos = new Vector3(0.5f, -0.8f, 0.6f);
                wepPos.y += 0.1f * Mathf.Sin(theta);
                wepPos.x -= 0.5f * Mathf.Cos(theta / 2);
                float weaponZRot = 15 * Mathf.Sin(theta);

                _weapon.transform.localPosition = wepPos;
                _weapon.transform.localRotation = Quaternion.Euler(roll, -174f, weaponZRot);
            }
        }
        else
        {
            _weapon.transform.localPosition = new Vector3(0.5f, -0.5f, 0.8f);
            _weapon.transform.localRotation = Quaternion.Euler(roll, -90f, 0);
        }

        // Shrink player
        Vector3 v = transform.localScale;
        if (_duckHeld)
        {
            v.y = 0.6f;
            transform.localScale = v;
            _weapon.transform.localScale = new Vector3(1, 1.7f, 1);
            v = _weapon.transform.localPosition;
            v.x = 0.7f;
            _weapon.transform.localPosition = v;
        }
        else
        {
            v.y = 1f;
            transform.localScale = v;
            _weapon.transform.localScale = new Vector3(1, 1, 1);
            v = _weapon.transform.localPosition;
            v.x = 0.5f;
            _weapon.transform.localPosition = v;
        }

        // fire
        if (_canFire && _fireHeld && _weapon.FullAuto)
        {
            Shoot();
        }
    }

    private void FixedUpdate()
    {

    }

    private void LateUpdate()
    {
        if (_raycastChest != null)
        {
            _currentChest = _raycastChest;
        }
    }

    private void Shoot()
    {
        if (_weapon.Clip < 0)
        {
            _weapon.PlayReloadAnimation();
        }
        else
        {
            _weapon.PlayFireAnimation();

            // pick end point of laser
            Vector3 laserStart = _weapon.transform.position;
            Vector3 laserEnd = transform.position + (_playerCamera.transform.forward * _weapon.Range);

            Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward * _playerCamera.farClipPlane);
            RaycastHit info;
            if (Physics.Raycast(ray, out info, _playerCamera.farClipPlane, _targets))
            {
                // update laser end position to the point of contact
                laserEnd = info.point;

                if (info.transform.gameObject.tag == "BreakableObject")
                {
                    BreakableObject bo = info.transform.gameObject.GetComponent<BreakableObject>();
                    if (bo.PlaySoundOnBreak)
                    {
                        _gameManager.PlaySound(bo.BreakSound, bo.transform.position);
                    }

                    if (!bo.Persistent)
                    {
                        bo.Destroy();
                    }
                }
            }

            //Invoke("CanFire", weapon.fireDelay);
            _gameManager.DrawLine(laserStart, laserEnd, _weapon.TracerColor, _weapon.TracerHeight, _weapon.TracerLife);
        }
    }

    private void OnDrawGizmos()
    {
        //Ray ray = new Ray(this.weapon.bulletOrigin.position, this.playerCamera.transform.forward);
        //Gizmos.DrawLine(ray.origin, ray.origin + (ray.direction * 400));
        _currentChest = (_raycastChest != null) ? _raycastChest : _triggerChest;
        if (_currentChest != null)
        {
            Gizmos.DrawLine(this.transform.position, _currentChest.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Chest")
        {
            LootChest otherChest = other.gameObject.GetComponent<LootChest>();

            if (otherChest.IsOpen)
            { 
                return;
            }

            if (_triggerChest == null)
            {
                _triggerChest = otherChest;
            }
            else if ((this.transform.position - otherChest.transform.position).sqrMagnitude < (this.transform.position - _triggerChest.transform.position).sqrMagnitude)
            {
                // else if the new chest is closer, shift focus to that one
                _triggerChest = otherChest;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Chest")
        {
            if (other.gameObject.GetComponent<LootChest>() != _currentChest) return;
               
            //inChestRange = false;
            _currentChest = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Chest")
        {
            LootChest otherChest = other.gameObject.GetComponent<LootChest>();

            if (otherChest.IsOpen) return;

            if (_triggerChest == null)
                _triggerChest = otherChest;
            else if ((this.transform.position - otherChest.transform.position).sqrMagnitude < (this.transform.position - _triggerChest.transform.position).sqrMagnitude)
            {
                // else if the new chest is closer, shift focus to that one
                _triggerChest = otherChest;
            }
        }
    }

    private void UpdateCanBools()
    {
        // update canBools
        _canFire = !_weapon.IsFiring && !_weapon.IsReloading && !_sprintHeld && _weapon.Clip > 0;
        _canReload = !_weapon.IsFiring && !_weapon.IsReloading && _weapon.Clip < _weapon.ClipCapacity && _weapon.Reserve > 0;
    }

    private void OnEnable()
    {
        _inputManager.Enable();
    }

    private void OnDisable()
    {
        _inputManager.Disable();
    }
}
