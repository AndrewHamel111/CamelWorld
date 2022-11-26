using System.Collections;
using UnityEngine;

// A major change is being made to the weapon class.
// The major change is now being reverted and it's worse than pulling teeth. Good luck :)

public enum WeaponType
{
    PISTOL, RIFLE, SMG
}

public class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [Tooltip("Damage of the weapon")]
    [SerializeField]
    private int _damage;
    
    [Tooltip("Maximum range of the weapon in Unity units")]
    [SerializeField]
    private float _range;
    public float Range => _range;

    [Tooltip("WeaponType determines what sprint/draw animations play and the ammo type")]
    [SerializeField]
    private WeaponType _type;
    public WeaponType Type => _type;
    
    [Tooltip("When true the fire button can be held to fire multiple shots in sequence")]
    [SerializeField]
    private bool _fullAuto;
    public bool FullAuto => _fullAuto;
    
    [Tooltip("Capacity of a full clip")]
    [SerializeField]
    private int _clipCapacity;
    public int ClipCapacity => _clipCapacity;
    
    [Tooltip("Maximum reserve ammunition of the weapon (LIKELY TO BE DEPRECATED)")]
    [SerializeField]
    private int _maxReserve;

    [Header("Recoil and Spread")]
    [Tooltip("Time between shots")]
    [SerializeField]
    private float _fireDelay;
    
    [Tooltip("Time that must pass before recoil is reset")]
    [SerializeField]
    private float _recoilDelay;
    
    [Tooltip("Degrees the shots are rotated up (CONSISTENT)")]
    [SerializeField]
    private float _kick; // degrees the shots are "rotated" up
    
    [Tooltip("Degrees the shots are rotated in random directions (RANDOM)")]
    [SerializeField]
    private float _spread; // degrees the shots are "rotated" in a random direction

    [Header("Tracer Settings")]
    [Tooltip("Color of the tracer line")]
    [SerializeField]
    private Color _tracerColor;
    public Color TracerColor => _tracerColor;

    [Tooltip("Height in Unity units of the tracer line")]
    [SerializeField]
    private float _tracerHeight;
    public float TracerHeight => _tracerHeight;

    [Tooltip("Duration/Lifespan of the tracer line")]
    [SerializeField]
    private float _tracerLife;
    public float TracerLife => _tracerLife;

    // runtime
    //[Header("Runtime Variables")]
    private int _clip;
    private int _reserve;
    private bool _isFiring = false;
    private bool _isFireAnimationPlaying = false;
    private bool _isReloading = false;

    public int Clip => _clip;
    public int Reserve => _reserve;
    public bool IsFiring => _isFiring;
    public bool IsReloading => _isReloading;

    [SerializeField]
    private float _reloadAnimationTime;
    
    [SerializeField]
    private float _fireAnimationTime;

    [Tooltip("Sound the weapon makes when firing")]
    [SerializeField]
    private AudioClip _fireSound;
    
    [Tooltip("Allowed pitch variance of the weapon's fire sound")]
    [SerializeField]
    private float _fireSoundPitchVariance;
    
    [SerializeField]
    private Animator _animator;

    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _clip = _clipCapacity;
        _reserve = _maxReserve;
    }

    public void PlayFireAnimation()
    {
        _clip--;
        _isFiring = true;
        _isFireAnimationPlaying = true;

        _animator.Play("none");
        _animator.Play("fire");

        _gameManager.PlaySound(_fireSound, this.transform.position);

        Invoke("FinishFire", _fireDelay);
    }

    public void PlayReloadAnimation()
    {
        _isReloading = true;

        _animator.Play("reload");

        // todo start some kind of timer which is longer than the reload animation and if it expires there was a problem reloading and we shouldn't softlock the player's state which currently would happen
    }

    void FinishFire()
    {
        _isFiring = false;
        ResetGunState();
    }

    private void ResetGunState()
    {
        if (_clip == 0)
            _animator.Play("empty");
        else
            _animator.Play("none");
    }

    public void OnReloadAnimationFinish()
    {
        _isReloading = false;

        int bulletsNeeded = Mathf.Min(_clipCapacity - _clip, _reserve);
        _clip += bulletsNeeded;
        _reserve -= bulletsNeeded;
        ResetGunState();
    }
}
