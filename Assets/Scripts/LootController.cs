using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootController : MonoBehaviour
{
    public float amplitude;
    public float duration;
    public Vector3 destination;

    public void Launch(Vector3 destination, float amplitude = 1.0f, float duration = 1.0f)
    {
        this.destination = destination;
        this.amplitude = amplitude;
        this.duration = duration;
        StartCoroutine(LaunchLoot(this.transform.position));
    }

    IEnumerator LaunchLoot(Vector3 start)
    {
        float time = 0;

        while(time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            Vector3 p = Vector3.Lerp(start, destination, t);
            p.y += 4 * t * (1 - t) * amplitude;
            this.transform.position = p;

            yield return null;
        }

        this.transform.position = destination;
        Destroy(this); // remove the script
    }
}
