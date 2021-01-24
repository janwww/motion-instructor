using UnityEngine;
using UnityEngine.UI;


namespace PoseTeacher
{
    //Class for visualisation of similarity score development over time
    public class Graph
    {
        public GameObject dataLine;
        public GameObject graphContainer;
        public Text coord; //object for the axis labels
        public int vectorElementsN = 200;
        LineRenderer lineRenderer;
        Vector3[] valuesDynamic;
        float begin_x, begin_y, end_x, end_y, step_x, step_y;
        //float similarityScore;

        public Graph(GameObject graphContainerIn, float similarityScoreIn = 0.0F)
        {
            //constructor. Initialise objects for the plot visualisation
            graphContainer = graphContainerIn;
            dataLine = graphContainer.transform.Find("Dataline").gameObject;

            //similarityScore = similarityScoreIn;
            lineRenderer = dataLine.GetComponent<LineRenderer>();
            Start_plot(similarityScoreIn);

        }

        public void Start_plot(float similarityScoreExtern)
        {
            //draws the grid and visualises time dependence of the score similarity
            begin_x = 0.0f;
            begin_y = 0.0f;
            end_x = 1.0f;
            end_y = 1.0f;
            step_x = (end_x - begin_x) / 10; //gap between vertical lines
            step_y = (end_y - begin_y) / 10; //gap between horisontal lines
            Color color_grid_line = Color.white;
            Color color_top_bottom = Color.green; //color of the middle line in the plot
            Color color_middle = Color.yellow; //color of levels 0 and 1 in the plot

            for (int i = 0; i < 11; i++)
            {
                //vertical lines of the grid
                GameObject vert = GameObject.Instantiate(dataLine);
                vert.transform.parent = graphContainer.transform;
                vert.GetComponent<Renderer>().material.color = color_grid_line;
                LineRenderer lineRenderer_vert = vert.GetComponent<LineRenderer>();
                lineRenderer_vert.widthMultiplier = 0.1f;
                lineRenderer_vert.startColor = color_grid_line;
                lineRenderer_vert.endColor = color_grid_line;
                lineRenderer_vert.positionCount = 2;
                lineRenderer_vert.SetPosition(0, new Vector3(begin_x + i * step_x, begin_y, 8.45f));
                lineRenderer_vert.SetPosition(1, new Vector3(begin_x + i * step_x, end_y, 8.45f));

                //horisontal lines of the grid
                GameObject hor = GameObject.Instantiate(dataLine);
                hor.transform.parent = graphContainer.transform;
                if (i == 0 || i == 10)
                {
                    //set the color of the bottom and top lines 
                    hor.GetComponent<Renderer>().material.color = color_top_bottom;
                }
                else
                {
                    //set the color of the main grid bacckground 
                    hor.GetComponent<Renderer>().material.color = color_grid_line;
                }
                if (i == 5)
                {
                    //set the color of the middle line
                    hor.GetComponent<Renderer>().material.color = color_middle;
                }

                //define parameters of the remaining majority of horisontal lines of the grid
                LineRenderer lineRenderer_hor = hor.GetComponent<LineRenderer>();
                lineRenderer_hor.widthMultiplier = 0.1f;
                lineRenderer_hor.startColor = color_grid_line;
                lineRenderer_hor.endColor = color_grid_line;
                lineRenderer_hor.positionCount = 2;
                lineRenderer_hor.SetPosition(0, new Vector3(begin_x, begin_y + i * step_y, 8.45f));
                lineRenderer_hor.SetPosition(1, new Vector3(end_x, begin_y + i * step_y, 8.45f));

            }

            valuesDynamic = new Vector3[vectorElementsN];
            for (int i = 0; i < vectorElementsN; i++)
            {
                valuesDynamic[i] = new Vector3((float)i / vectorElementsN, (float)similarityScoreExtern, 0.0f);
            }
        }

        public void Update_plot(double similarityScoreExtern)
        {
            //updates the line of similarity score versus time
            Color color_plot_line = Color.red;
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
                lineRenderer.startColor = color_plot_line;
                lineRenderer.endColor = color_plot_line;
            }
        }
    }
}