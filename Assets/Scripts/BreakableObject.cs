using UnityEngine;

public class BreakableObject : SpawnableObject
{
    // by default objects will break after any damage
    [SerializeField]
    private int _hp = 1;
    
    [SerializeField]
    private bool _playSoundOnBreak;
    public bool PlaySoundOnBreak => _playSoundOnBreak;

    [SerializeField]
    private AudioClip _breakSound;
    public AudioClip BreakSound => _breakSound;
}
