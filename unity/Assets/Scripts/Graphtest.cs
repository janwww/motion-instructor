using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphtest : MonoBehaviour
{
    public GameObject gobject;
    LineRenderer lineRenderer;
    Vector3[] values;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = gobject.GetComponent<LineRenderer>();
        values = new Vector3[200];
        for (int i = 0; i < 200; i++)
        {
            values[i] = new Vector3((float)i / 200, Mathf.Sin((float)i / 10)/4, 0.0f);
            lineRenderer.SetPositions(values);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
}
