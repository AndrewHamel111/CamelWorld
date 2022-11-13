using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] public int damage;
    [Tooltip("Maximum range of the weapon in Unity units")]
    [SerializeField] public float range;
    [Tooltip("WeaponType determines what sprint/draw animations play and the ammo type")]
    [SerializeField] public WeaponType type;
    [Tooltip("When true the fire button can be held to fire multiple shots in sequence")]
    [SerializeField] public bool fullAuto;
    [Tooltip("Capacity of a full clip")]
    [SerializeField] public int clipCapacity;
    [Tooltip("Maximum reserve ammunition of the weapon (LIKELY TO BE DEPRECATED)")]
    [SerializeField] public int maxReserve;

    [Header("Recoil and Spread")]
    [Tooltip("Time between shots")]
    [SerializeField] public float fireDelay;
    [Tooltip("Time that must pass before recoil is reset")]
    [SerializeField] public float recoilDelay;
    [Tooltip("Degrees the shots are rotated up (CONSISTENT)")]
    [SerializeField] private float kick; // degrees the shots are "rotated" up
    [Tooltip("Degrees the shots are rotated in random directions (RANDOM)")]
    [SerializeField] private float spread; // degrees the shots are "rotated" in a random direction

    [Header("Tracer Settings")]
    [Tooltip("Color of the tracer line")]
    [SerializeField] public Color tracerColor;
    [Tooltip("Height in Unity units of the tracer line")]
    [SerializeField] public float tracerHeight;
    [Tooltip("Duration/Lifespan of the tracer line")]
    [SerializeField] public float tracerLife;

    // runtime
    [Header("Runtime Variables")]
    public int clip;
    public int reserve;
    public bool isFiring = false;
    public bool isFireAnimationPlaying = false;
    public bool isReloading = false;

    [SerializeField] public float reloadAnimationTime;
    [SerializeField] public float fireAnimationTime;

    [Tooltip("Sound the weapon makes when firing")]
    [SerializeField] public AudioClip fireSound;
    [Tooltip("Allowed pitch variance of the weapon's fire sound")]
    [SerializeField] public float fireSoundPitchVariance;
    [SerializeField] public Animator animator;

    public void PlayFireAnimation()
    {
        clip--;
        isFiring = true;
        isFireAnimationPlaying = true;

        animator.Play("none");
        animator.Play("fire");

        GameManager.Instance.PlaySound(fireSound, this.transform.position);

        Invoke("FinishFire", fireDelay);
        //StartCoroutine(FinishFire());
        //Debug.Log("Fire");
    }

    public void PlayReloadAnimation()
    {
        isReloading = true;

        animator.Play("reload");

        //Invoke("Reload", reloadAnimationTime);
        StartCoroutine(FinishReload());
    }

    void FinishFire()
    {
        isFiring = false;
        ResetGunState();
    }

    // function waits until reload is finished to reset gun state
    IEnumerator FinishReload()
    {
        while(isReloading)
        {
            yield return null;
        }

        int bulletsNeeded = Mathf.Min(clipCapacity - clip, reserve);
        clip += bulletsNeeded;
        reserve -= bulletsNeeded;
        ResetGunState();
    }

    /*
    private void Reload()
    {
        isReloading = false;

        int bulletsNeeded = Mathf.Min(clipCapacity - clip, reserve);
        clip += bulletsNeeded;
        reserve -= bulletsNeeded;
        ResetGunState();
    }
    */

    private void ResetGunState()
    {
        if (clip == 0)
            animator.Play("empty");
        else
            animator.Play("none");
    }
}
