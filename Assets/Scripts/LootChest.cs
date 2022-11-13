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
    [SerializeField] public Vector3 lootOffset;
    [SerializeField] public float lootAngle = 45.0f;
    [SerializeField] public float lootDistance = 1.0f;
    [SerializeField] public float lootHeightMax = 1.0f;
    [SerializeField] public float lootLaunchTime = 0.2f;

    // CONFIG
    [Header("Configuration")]
    [SerializeField] public ChestTier chestTier;
    [SerializeField] public GameObject[] loot;


    // UNITY COMPONENTS
    Animator animator; // animator of child Chest mesh
    Light light; // reference to the box's light, disabled when opened.

    // RUNTIME VARIABLES
    public bool isOpen = false;
    
    void Start()
    {
        animator = this.gameObject.GetComponentInChildren<Animator>();
        light = this.gameObject.GetComponentInChildren<Light>();

        // TODO depending on the chest tier that is specified by chestTier, the mesh is replaced with the corresponding tier.
        // Then, the loot is determined based on the tier. That will in part be moved to the GameManager as well.
        switch(chestTier)
        {
            case ChestTier.BASIC:
                light.color = GameManager.Instance.chestColorByQuality[0];
                break;
            case ChestTier.GOOD:
                light.color = GameManager.Instance.chestColorByQuality[1];
                break;
            case ChestTier.GREAT:
                light.color = GameManager.Instance.chestColorByQuality[2];
                break;
        }
        
    }

    public void OnChestOpened()
    {
        isOpen = true;
        light.enabled = false;
        animator.SetTrigger("ChestOpened");

        // spawn the loot
        Vector3 pos = this.transform.position + lootOffset;
        int i = 0;
        int iE = loot.Length;
        foreach(GameObject go in loot)
        {
            GameObject spawned = Instantiate(go, pos, this.transform.rotation, this.transform);
            LootController l = spawned.GetComponent<LootController>();
            if (l != null)
            {
                //float theta = Random.Range(-lootAngle, lootAngle) * 5.0f;
                float theta = 2 * (float)(i)/(float)(iE) * lootAngle - lootAngle + Random.Range(-lootAngle / 15, lootAngle / 15);
                Vector3 lootDest = Quaternion.Euler(0, theta, 0) * this.transform.forward * lootDistance + this.transform.position;
                l.Launch(lootDest, lootHeightMax * Random.value, Random.Range(0.4f, 0.4f + lootLaunchTime));
            }
            else
                Debug.LogError("No Loot Controller on Loot object");

            i++;
        }
    }
}
