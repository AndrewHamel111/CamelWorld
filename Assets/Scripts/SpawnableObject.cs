using UnityEngine;

public abstract class SpawnableObject : MonoBehaviour
{
    [SerializeField]
    private bool _persistent;
    public bool Persistent => _persistent;

    [SerializeField]
    private ObjectSpawner _spawner;

    public virtual void Init(ObjectSpawner spawner)
    {
        _spawner = spawner;
    }

    public void Destroy()
    {
        _spawner.InvokeRespawn();
        Destroy(this.gameObject);
    }
}
