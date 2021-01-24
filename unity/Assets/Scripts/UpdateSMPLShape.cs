using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class UpdateSMPLShape : MonoBehaviour
{
    // Enables setting blendshapes from MRTK sliders


    public void UpdateSMPLShapeParam1(GameObject slider)
    {
        float a = slider.GetComponent<PinchSlider>().SliderValue;
        SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
        if (a < 0.5)
        {
            renderer.SetBlendShapeWeight(2, (0.5f - a) * 200);
        }
        else
        {
            renderer.SetBlendShapeWeight(3, (a - 0.5f) * 200);
        }
        //Debug.Log(a);
    }

    public void UpdateSMPLShapeParam2(GameObject slider)
    {
        float a = slider.GetComponent<PinchSlider>().SliderValue;
        SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
        if (a < 0.5)
        {
            renderer.SetBlendShapeWeight(4, (0.5f - a) * 200);
        }
        else
        {
            renderer.SetBlendShapeWeight(5, (a - 0.5f) * 200);
        }

        //Debug.Log(a);
    }

    public void UpdateSMPLShapeParam3(GameObject slider)
    {
        float a = slider.GetComponent<PinchSlider>().SliderValue;
        SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
        if (a < 0.5)
        {
            renderer.SetBlendShapeWeight(6, (0.5f - a) * 200);
        }
        else
        {
            renderer.SetBlendShapeWeight(7, (a - 0.5f) * 200);
        }

        //Debug.Log(a);
    }

    public void UpdateSMPLShapeParam4(GameObject slider)
    {
        float a = slider.GetComponent<PinchSlider>().SliderValue;
        SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
        if (a < 0.5)
        {
            renderer.SetBlendShapeWeight(8, (0.5f - a) * 200);
        }
        else
        {
            renderer.SetBlendShapeWeight(9, (a - 0.5f) * 200);
        }

        //Debug.Log(a);
    }
}
