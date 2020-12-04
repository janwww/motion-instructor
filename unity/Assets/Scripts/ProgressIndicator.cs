using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CLSCompliant(false)]
public class ProgressIndicator : MonoBehaviour
{

    public GameObject ProgressObject;
    private IProgressIndicator progressIndicator;
    private bool doIndicate = false;

    // Start is called before the first frame update
    void Start()
    {
        progressIndicator = ProgressObject.GetComponent<IProgressIndicator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ProgressObject.activeSelf && doIndicate == false)
        {
            doIndicate = true;
            OpenProgressIndicator(progressIndicator);
        }
        else if (!ProgressObject.activeSelf)
        {
            doIndicate = false;
        }
    }

    private float progress = 0;
    public void SetProgress(float currentProgress)
    {
        progress = currentProgress;
        //Debug.Log("Progress Set to " + progress);
    }

    private async void OpenProgressIndicator(IProgressIndicator indicator)
    {
        await indicator.OpenAsync();
        while (doIndicate)
        {
            progressIndicator.Progress = progress;
            await Task.Yield();
        }
        await indicator.CloseAsync();
    }
}