using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graphtest : MonoBehaviour
{
    public GameObject gobject;
    LineRenderer lineRenderer, lineRenderer_coordx, lineRenderer_coordy;
    Vector3[] values, values_x, values_y, start_axis_x, start_axis_y, end_axis_x, end_axis_y, coordx, coordy;
    float end_x, end_y, step_x, step_y;
    // Start is called before the first frame update
    void Start()
    {
        //line renderer for a plot line
        lineRenderer = gobject.GetComponent<LineRenderer>();

        //line renderer for xaxis
        //Instantiate(gobject, new Vector3(0, 0, 0), Quaternion.identity);
        GameObject xaxis = Instantiate(gobject);
        LineRenderer lineRenderer_x = xaxis.GetComponent<LineRenderer>();
        lineRenderer_x.widthMultiplier = 0.2f;
        lineRenderer_x.startColor = Color.black;
        lineRenderer_x.endColor = Color.black;
        lineRenderer_x.positionCount = 2;
        lineRenderer_x.SetPosition(0, new Vector3(0, -0.5f, 0.0f));
        lineRenderer_x.SetPosition(1, new Vector3(1.5f, -0.5f, 0.0f));

        //line renderer for yaxis
        //Instantiate(gobject, new Vector3(0, 0, 0), Quaternion.identity);
        GameObject yaxis = Instantiate(gobject);
        LineRenderer lineRenderer_y = yaxis.GetComponent<LineRenderer>();
        lineRenderer_y.widthMultiplier = 0.2f;
        lineRenderer_y.startColor = Color.black;
        lineRenderer_y.endColor = Color.black;
        lineRenderer_y.positionCount = 2;
        lineRenderer_y.SetPosition(0, new Vector3(0, -0.5f, 0.0f));
        lineRenderer_y.SetPosition(1, new Vector3(0, 0.7f, 0.0f));
   

        lineRenderer.widthMultiplier = 0.1f;
        end_x = 1.5f;
        end_y = 0.7f;
        step_x = end_x / 10;
        step_y = end_y / 10;

        values = new Vector3[200];


        //draw a grid
        for (int i = 1; i < 11; i++)
        {
            GameObject vert = Instantiate(gobject);
            LineRenderer lineRenderer_vert = vert.GetComponent<LineRenderer>();
            lineRenderer_vert.widthMultiplier = 0.2f;
            lineRenderer_vert.startColor = Color.black;
            lineRenderer_vert.endColor = Color.black;
            lineRenderer_vert.positionCount = 2;
            lineRenderer_vert.SetPosition(0, new Vector3(i * step_x, -0.5f, 0.0f));
            lineRenderer_vert.SetPosition(1, new Vector3(i * step_x, end_y, 0.0f));


            GameObject hor = Instantiate(gobject);
            LineRenderer lineRenderer_hor = hor.GetComponent<LineRenderer>();
            lineRenderer_hor.widthMultiplier = 0.2f;
            lineRenderer_hor.startColor = Color.black;
            lineRenderer_hor.endColor = Color.black;
            lineRenderer_hor.positionCount = 2;
            lineRenderer_hor.SetPosition(0, new Vector3(0, -0.5f + i * step_y, 0.0f));
            lineRenderer_hor.SetPosition(1, new Vector3(end_x, -0.5f + i * step_y, 0.0f));

        }


        //draw a plot line
        for (int i = 0; i < 200; i++)
        {
            values[i] = new Vector3((float)i / 200, Mathf.Sin((float)i / 10) / 4, 0.0f);
            lineRenderer.SetPositions(values);
        }

        //draw labels
        for (int i = 0; i < 10; i++)
        {
            var pos_x = Camera.main.WorldToScreenPoint(new Vector3(i * step_x, -0.5f, 0.0f));
            string text_x = i.ToString();
            var textSize_x = GUI.skin.label.CalcSize(new GUIContent(text_x));
            GUI.contentColor = Color.red;
            GUI.Label(new Rect(pos_x.x, -1f, textSize_x.x, textSize_x.y), text_x);

            var pos_y = Camera.main.WorldToScreenPoint(new Vector3(0, -0.5f + i * step_y, 0.0f));
            string text_y = i.ToString();
            var textSize_y = GUI.skin.label.CalcSize(new GUIContent(text_y));
            GUI.contentColor = Color.red;
            GUI.Label(new Rect(0, pos_y.y, textSize_y.x, textSize_y.y), text_y);
        }

        //values = new Vector3[200];
        //values_x = new Vector3[400];
        //values_y = new Vector3[400];
        //coordx = new Vector3[100];
        //coordy = new Vector3[100];
        //for (int i = 0; i < 100; i++)
        //{
        //    coordx[i] = new Vector3(0, ((float)i - 50f) / 100, 0.0f);
        //    coordy[i] = new Vector3((float)i / 100, -50f, 0.0f);
        //    lineRenderer_coordx.SetPositions(coordx);
        //    lineRenderer_coordy.SetPositions(coordy);
        //}
        //for (int i = 0; i < 400; i++)
        //{
        //    values_x[i] = new Vector3(0, ((float)i - 100) / 200, 0.0f);
        //    lineRenderer_x.SetPositions(values_x);
        //}

        //for (int i = 0; i < 300; i++)
        //{
        //    values_y[i] = new Vector3((float)i / 400, -0.5f, 0.0f);
        //    lineRenderer_y.SetPositions(values_y);
        //}

        //for (int i = 0; i < 200; i++)
        //{
        //    values[i] = new Vector3((float)i / 200, Mathf.Sin((float)i / 10) / 4, 0.0f);
        //    lineRenderer.SetPositions(values);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
}
