using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    public Color color;
    public Vector3 start;
    public Vector3 end;
    public float size;
    public float laserLife;

    LineRenderer lineRenderer;

    private void SetAll(Color color, Vector3 start, Vector3 end, float size, float laserLife)
    {
        this.color = color;
        this.start = start;
        this.end = end;
        this.size = (size <= 0) ? 1.0f : size;
        this.laserLife = (laserLife <= 0) ? 1.0f : laserLife;
    }

    public void CreateLine(Color color, Vector3 start, Vector3 end, float size, float laserLife)
    {
        // update laser properties
        SetAll(color, start, end, size, laserLife);

        // Create the line renderer and set it's points
        lineRenderer = this.gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
        Vector3[] vector3s = new Vector3[2];
        vector3s[0] = start;
        vector3s[1] = end;
        lineRenderer.SetPositions(vector3s);

        // set the material / color
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = color;

        // set the size of the line and color
        //lineRenderer.startColor = lineRenderer.endColor = color;
        lineRenderer.startWidth = size;
        lineRenderer.endWidth = size;

        // make sure the line is enabled
        lineRenderer.enabled = true;

        // slowly shrink the laser until it despawns
        StartCoroutine(FadeLine());
    }

    IEnumerator FadeLine()
    {
        float life = laserLife;

        while(life > 0)
        {
            life -= Time.deltaTime;

            float t = life / laserLife;
            Color tColor = new Color(color.r, color.g, color.b, color.a * t);

            // fade color and width with time
            lineRenderer.startWidth = t * size;
            lineRenderer.endWidth = t * size;

            lineRenderer.material.color = tColor;
            //lineRenderer.startColor = lineRenderer.endColor = tColor;

            //lineRenderer.colorGradient = InterpolatedGradient(color);
            //lineRenderer.SetColors(color, color);

            yield return null;
        }

        Destroy(this.gameObject);
    }
}
