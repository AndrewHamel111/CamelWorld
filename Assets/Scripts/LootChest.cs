using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
// - Make the chest tier swap out the mesh
// - Add a glow to unopened chests
// - Add sound effects

public enum ChestTier
{
    BASIC, GOOD, GREAT
}

public class LootChest : MonoBehaviour
{
    // CONSTANTS
    [Header("Constants")]
    [SerializeField] 
    private Vector3 _lootOffset;
    
    [SerializeField] 
    private float _lootAngle = 45.0f;
    
    [SerializeField] 
    private float _lootDistance = 1.0f;
    
    [SerializeField] 
    private float _lootHeightMax = 1.0f;
    
    [SerializeField] 
    private float _lootLaunchTime = 0.2f;

    // CONFIG
    [Header("Configuration")]
    [SerializeField] 
    private ChestTier _chestTier;
    
    [SerializeField] 
    private GameObject[] _loot;

    // UNITY COMPONENTS
    [SerializeField]
    private Animator _animator; // animator of child Chest mesh
    
    [SerializeField]
    private Light _light; // reference to the box's light, disabled when opened.

    // RUNTIME VARIABLES
    private bool _isOpen = false;
    public bool IsOpen => _isOpen;
    
    void Start()
    {
        // TODO depending on the chest tier that is specified by chestTier, the mesh is replaced with the corresponding tier.
        // Then, the loot is determined based on the tier. That will in part be moved to the GameManager as well.
        _light.color = GameManager.Instance.GetLightColorForChestQuality(_chestTier);
    }

    public void OnChestOpened()
    {
        _isOpen = true;
        _light.enabled = false;
        _animator.SetTrigger("ChestOpened");

        // spawn the loot
        Vector3 pos = this.transform.position + _lootOffset;
        int i = 0;
        int iE = _loot.Length;
        foreach(GameObject go in _loot)
        {
            GameObject spawned = Instantiate(go, pos, this.transform.rotation, this.transform);
            LootController l = spawned.GetComponent<LootController>();
            if (l != null)
            {
                //float theta = Random.Range(-lootAngle, lootAngle) * 5.0f;
                float theta = 2 * (float)(i)/(float)(iE) * _lootAngle - _lootAngle + Random.Range(-_lootAngle / 15, _lootAngle / 15);
                Vector3 lootDest = Quaternion.Euler(0, theta, 0) * this.transform.forward * _lootDistance + this.transform.position;
                l.Launch(lootDest, _lootHeightMax * Random.value, Random.Range(0.4f, 0.4f + _lootLaunchTime));
            }
            else
                Debug.LogError("No Loot Controller on Loot object");

            i++;
        }
    }
}
