using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;
using System.IO;
using NativeWebSocket;
using System.Security.Permissions;


namespace PoseTeacher
{
    public class Graphtest
    {
        public GameObject gobject;
        public GameObject graphContainer;
        //public GameObject coordobject;

        public Text coord;
        public int vectorElementsN = 200;
        LineRenderer lineRenderer, lineRenderer_coordx, lineRenderer_coordy;
        Vector3[] values, valuesDynamic, values_x, values_y, start_axis_x, start_axis_y, end_axis_x, end_axis_y, coordx, coordy;
        float begin_x, begin_y, end_x, end_y, step_x, step_y;
        float similarityScore;

        // Start is called before the first frame update

        public Graphtest(float similarityScoreExtern)
        {
            gobject = GameObject.Find("Dataline");
            graphContainer = GameObject.Find("GraphTestContainer");
            similarityScore = similarityScoreExtern;
            lineRenderer = gobject.GetComponent<LineRenderer>();
            Start_plot(similarityScore);

        }
        public void Start_plot(float similarityScoreExtern)
        {

            //float similarityScoreExtern = 0.0f;
            //lineRenderer = gobject.GetComponent<LineRenderer>();

            begin_x = 0.0f;
            begin_y = 0.0f;
            end_x = 1.0f;
            end_y = 1.0f;
            step_x = (end_x - begin_x) / 10; //gap between vertical lines
            step_y = (end_y - begin_y) / 10; //gap between horisontal lines

            for (int i = 0; i < 11; i++)
            {
                //vertical lines of a grid
                GameObject vert = GameObject.Instantiate(gobject);
                vert.transform.parent = graphContainer.transform;
                vert.GetComponent<Renderer>().material.color = Color.white;
                LineRenderer lineRenderer_vert = vert.GetComponent<LineRenderer>();
                lineRenderer_vert.widthMultiplier = 0.1f;
                lineRenderer_vert.startColor = Color.white;
                lineRenderer_vert.endColor = Color.white;
                lineRenderer_vert.positionCount = 2;
                lineRenderer_vert.SetPosition(0, new Vector3(begin_x + i * step_x, begin_y, 8.45f));
                lineRenderer_vert.SetPosition(1, new Vector3(begin_x + i * step_x, end_y, 8.45f));

                //horisontal lines of a grid
                GameObject hor = GameObject.Instantiate(gobject);
                hor.transform.parent = graphContainer.transform;
                if (i == 0 || i == 10)
                {
                    hor.GetComponent<Renderer>().material.color = Color.green;
                }
                else
                {
                    hor.GetComponent<Renderer>().material.color = Color.white;
                }
                if (i == 5)
                {
                    hor.GetComponent<Renderer>().material.color = Color.yellow;
                    //hor.GetComponent<Renderer>().material.mainTexture 
                }
                LineRenderer lineRenderer_hor = hor.GetComponent<LineRenderer>();
                lineRenderer_hor.widthMultiplier = 0.1f;
                lineRenderer_hor.startColor = Color.white;
                lineRenderer_hor.endColor = Color.white;
                lineRenderer_hor.positionCount = 2;
                lineRenderer_hor.SetPosition(0, new Vector3(begin_x, begin_y + i * step_y, 8.45f));
                lineRenderer_hor.SetPosition(1, new Vector3(end_x, begin_y + i * step_y, 8.45f));

            }

            //float similarityScoreExtern = (float)PoseteacherMain.similarityScoreExtern;
            valuesDynamic = new Vector3[vectorElementsN];
            for (int i = 0; i < vectorElementsN; i++)
            {
                valuesDynamic[i] = new Vector3((float)i / vectorElementsN, (float)similarityScoreExtern, 0.0f);
            }
        }
            public void Update_plot(double similarityScoreExtern)
            {
                Color c1 = Color.red;
                Color c2 = Color.red;
                
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
                lineRenderer.widthMultiplier = 0.4f;
                //draw a plot line



                for (int i = 0; i < vectorElementsN; i++)
                {
                    if (i == vectorElementsN - 1)
                    {
                        valuesDynamic[i] = new Vector3((float)i / vectorElementsN, (float)similarityScoreExtern, 0.0f);
                    }
                    else
                    {
                        valuesDynamic[i] = new Vector3((float)i / vectorElementsN, valuesDynamic[i + 1].y, 0.0f);
                    }
                    lineRenderer.SetPositions(valuesDynamic);
                    lineRenderer.SetColors(c1, c2);
                
            }
        }


        //GameObject xaxis = Instantiate(coordobject);
        //LineRenderer lineRenderer_x = xaxis.GetComponent<LineRenderer>();
        //GameObject yaxis = Instantiate(coordobject);
        //LineRenderer lineRenderer_y = yaxis.GetComponent<LineRenderer>();
        //lineRenderer_x.widthMultiplier = 0.2f;
        //lineRenderer_y.widthMultiplier = 0.2f;
        ////lineRenderer_x.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        ////lineRenderer_y.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        //values_x = new Vector3[vectorElementsN];
        //values_y = new Vector3[vectorElementsN];
        //for (int i = 0; i < vectorElementsN; i++)
        //{

        //    values_x[i] = new Vector3((float)i / vectorElementsN, 0.0f, 8.0f);
        //    values_y[i] = new Vector3(0.0f, (float)i / vectorElementsN, 8.0f);

        //    lineRenderer_x.SetPositions(values_x);
        //    lineRenderer_y.SetPositions(values_y);
        //}


        //    Transform canvas = newobject.transform.Find("TextContent");
        //    text_0.transform.parent = canvas;
        //GameObject xaxis = Instantiate(gobject);
        //Transform canvas = newobject.transform.Find("Backplate");
        //xaxis.transform.parent = canvas;
        //LineRenderer lineRenderer_x = xaxis.GetComponent<LineRenderer>();
        //lineRenderer_x.widthMultiplier = 0.2f;
        //lineRenderer_x.startColor = Color.black;
        //lineRenderer_x.endColor = Color.black;
        //lineRenderer_x.positionCount = 2;
        //lineRenderer_x.SetPosition(0, new Vector3((float)0.0f, (float)0.0f, 0.7f));
        //lineRenderer_x.SetPosition(1, new Vector3((float)1.0f, (float)0.0f, 0.7f));

        //////line renderer for yaxis
        //////Instantiate(gobject, new Vector3(0, 0, 0), Quaternion.identity);
        //GameObject yaxis = Instantiate(gobject);
        ////GameObject yaxis = Instantiate(gobject);
        ////Transform canvas = newobject.transform.Find("Backplate");
        //yaxis.transform.parent = canvas;
        //LineRenderer lineRenderer_y = yaxis.GetComponent<LineRenderer>();
        //lineRenderer_y.widthMultiplier = 0.2f;
        //lineRenderer_y.startColor = Color.black;
        //lineRenderer_y.endColor = Color.black;
        //lineRenderer_y.positionCount = 2;
        //lineRenderer_y.SetPosition(0, new Vector3((float)0.0f, (float)0.0f, 0.7f));
        //lineRenderer_y.SetPosition(1, new Vector3((float)0.0f, (float)1.0f, 0.0f));
        //line renderer for a plot line



        //line renderer for xaxis
        //Instantiate(gobject, new Vector3(0, 0, 0), Quaternion.identity);
        //GameObject xaxis = Instantiate(gobject);
        //LineRenderer lineRenderer_x = xaxis.GetComponent<LineRenderer>();
        //lineRenderer_x.widthMultiplier = 0.2f;
        //lineRenderer_x.startColor = Color.black;
        //lineRenderer_x.endColor = Color.black;
        //lineRenderer_x.positionCount = 2;
        //lineRenderer_x.SetPosition(0, new Vector3(0, -0.5f, 0.0f));
        //lineRenderer_x.SetPosition(1, new Vector3(1.5f, -0.5f, 0.0f));

        ////line renderer for yaxis
        ////Instantiate(gobject, new Vector3(0, 0, 0), Quaternion.identity);
        //GameObject yaxis = Instantiate(gobject);
        //LineRenderer lineRenderer_y = yaxis.GetComponent<LineRenderer>();
        //lineRenderer_y.widthMultiplier = 0.2f;
        //lineRenderer_y.startColor = Color.black;
        //lineRenderer_y.endColor = Color.black;
        //lineRenderer_y.positionCount = 2;
        //lineRenderer_y.SetPosition(0, new Vector3(0, -0.5f, 0.0f));
        //lineRenderer_y.SetPosition(1, new Vector3(0, 0.7f, 0.0f));


        //step_x = end_x / 10; //gap between vertical lines
        //step_y = (end_y + 0.5f) / 10; //gap between horisontal lines
        //end_x = 1.5f;
        //end_y = 0.7f;
        //draw a grid
        //for (int i = 1; i < 11; i++)
        //{
        //    //vertical lines of a grid
        //    GameObject vert = Instantiate(gobject);
        //    LineRenderer lineRenderer_vert = vert.GetComponent<LineRenderer>();
        //    lineRenderer_vert.widthMultiplier = 0.2f;
        //    lineRenderer_vert.startColor = Color.black;
        //    lineRenderer_vert.endColor = Color.black;
        //    lineRenderer_vert.positionCount = 2;
        //    lineRenderer_vert.SetPosition(0, new Vector3(i * step_x, -0.5f, 0.0f));
        //    lineRenderer_vert.SetPosition(1, new Vector3(i * step_x, end_y, 0.0f));

        //    //horisontal lines of a grid
        //    GameObject hor = Instantiate(gobject);
        //    LineRenderer lineRenderer_hor = hor.GetComponent<LineRenderer>();
        //    lineRenderer_hor.widthMultiplier = 0.2f;
        //    lineRenderer_hor.startColor = Color.black;
        //    lineRenderer_hor.endColor = Color.black;
        //    lineRenderer_hor.positionCount = 2;
        //    lineRenderer_hor.SetPosition(0, new Vector3(0, -0.5f + i * step_y, 0.0f));
        //    lineRenderer_hor.SetPosition(1, new Vector3(end_x, -0.5f + i * step_y, 0.0f));

        //}

        //Color c1 = Color.red;
        //Color c2 = Color.red;
        //lineRenderer.startColor = Color.red;
        //lineRenderer.endColor = Color.red;
        //lineRenderer.widthMultiplier = 0.5f;
        //values = new Vector3[200];
        ////draw a plot line
        //for (int i = 0; i < 200; i++)
        //{
        //    values[i] = new Vector3((float)i / 200, Mathf.Sin((float)i / 10) / 4, 0.0f);
        //    lineRenderer.SetPositions(values);
        //    lineRenderer.SetColors(c1, c2);
        //}


        //in case to set text objects for all labels x and y
        //for (int i = 0; i < 10; i++)
        //{
        //    Text text_0 = Instantiate(coord, new Vector3(0, 0f, 0.0f), Quaternion.identity) as Text;
        //    text_0.text = i.ToString();
        //    //text_0.transform.localScale = new Vector3(0.1f, 0.1f, 0.0f);
        //    Transform canvas = newobject.transform.Find("TextContent");
        //    text_0.transform.parent = canvas;
        //    text_0.transform.position = new Vector3(1.21f + i * 0.08f, -0.55f, 0.7f);
        //    text_0.fontSize = 1;
        //}



        //attempts to set text mesh pro from code
        //GameObject label = Instantiate(gobject);
        //TextMeshPro m_textMeshPro = label.AddComponent<TextMeshPro>();
        //m_textMeshPro.SetText("Hi");
        //m_textMeshPro.GetComponent<RectTransform>().sizeDelta = new Vector2(0.1f, 0.1f);
        //m_textMeshPro.
        //TextMeshPro textmeshPro = gobject.GetComponent<TextMeshPro>();
        //textmeshPro.SetText("Hi");

        //draw labels
        //void DrawText()
        //{

        //    for (int i = 0; i < 10; i++)
        //    {

        //        var pos_x = Camera.main.WorldToScreenPoint(new Vector3(i * step_x, -0.5f, 0.0f));
        //        string text_x = i.ToString();
        //        var textSize_x = GUI.skin.label.CalcSize(new GUIContent(text_x));
        //        GUI.contentColor = Color.red;
        //        Debug.Log(text_x);
        //        GUI.Label(new Rect(pos_x.x, -1f, textSize_x.x, textSize_x.y), text_x);

        //        var pos_y = Camera.main.WorldToScreenPoint(new Vector3(0, -0.5f + i * step_y, 0.0f));
        //        string text_y = i.ToString();
        //        var textSize_y = GUI.skin.label.CalcSize(new GUIContent(text_y));
        //        GUI.contentColor = Color.red;
        //        GUI.Label(new Rect(0, pos_y.y, textSize_y.x, textSize_y.y), text_y);
        //    }
        //}
        //Insert the empty game object with the TextMesh attached
        //public GameObject distanceText;

        //Instantiates the Object
        //Text text_0 = Instantiate(coord, new Vector3(0, 0f, 0.0f), Quaternion.identity) as Text;
        //text_0.text = "hello";
        ////text_0.transform.localScale = new Vector3(0.1f, 0.1f, 0.0f);
        //Transform canvas = newobject.transform.Find("TextContent");
        //text_0.transform.parent = canvas;
        //text_0.transform.position = new Vector3(1.5f, -0.3f, 0.7f);
        //text_0.fontSize = 1;





        //text_0.fontSize = text_0.fontSize * 7;

        //text_0.transform.localScale = text_0.transform.localScale / 7;
        //text_0.horizontalOverflow = HorizontalWrapMode.Overflow;
        //text_0.verticalOverflow = VerticalWrapMode.Overflow;
        //UnityEngine.UI.Text DescriptionText = text_0.GetComponent<UnityEngine.UI.Text>();
        //DescriptionText.text = "hello";
        //DescriptionText.transform.position = new Vector3(0, -0.5f, 0.0f); 
        //DescriptionText.transform.rotation = Quaternion.identity;

        //GameObject tempTextBox = Instantiate(gobject, pos, Quaternion.identity) as GameObject;
        //tempTextBox.GetComponent<Text>().text = "hello";
        //Grabs the TextMesh component from the game object
        //TextMesh theText = tempTextBox.transform.getComponent<TextMesh>();

        //Sets the text.
        //theText.text = "The Text";
        //void OnGUI()
        //{

        //DrawText();

        //for (int i = 0; i < 10; i++)
        //{

        //    var pos_x = Camera.main.WorldToScreenPoint(new Vector3(i * step_x, -0.5f, 0.0f));
        //    string text_x = i.ToString();
        //    var textSize_x = GUI.skin.label.CalcSize(new GUIContent(text_x));
        //    GUI.contentColor = Color.red;
        //    Debug.Log(text_x);
        //    GUI.Label(new Rect(pos_x.x, -1f, textSize_x.x, textSize_x.y), text_x);

        //    var pos_y = Camera.main.WorldToScreenPoint(new Vector3(0, -0.5f + i * step_y, 0.0f));
        //    string text_y = i.ToString();
        //    var textSize_y = GUI.skin.label.CalcSize(new GUIContent(text_y));
        //    GUI.contentColor = Color.red;
        //    GUI.Label(new Rect(0, pos_y.y, textSize_y.x, textSize_y.y), text_y);
        //}
        //}


    }
    //private void OnGUI()
    //{
    //    DrawText();
    //}

    // Update is called once per frame



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
}