using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    // by default objects will break after any damage
    public int hp = 1;
    public bool playSoundOnBreak;
    public AudioClip breakSound;
    public bool persistent;

    [SerializeField] public ObjectSpawner spawner;
}
