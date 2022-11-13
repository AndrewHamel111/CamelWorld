using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public float respawnPeriod;
    [SerializeField] public GameObject prefab;

    public void AttemptRespawn()
    {
        if (transform.childCount == 0)
        {
            GameObject go = GameObject.Instantiate(prefab, transform);
            BreakableObject bo = go.GetComponent<BreakableObject>();
            if (bo != null)
            {
                bo.spawner = this;
            }
        }
    }

    public void InvokeRespawn()
    {
        Invoke("AttemptRespawn", respawnPeriod);
    }
}
