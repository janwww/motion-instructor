using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CLSCompliant(false)]
public class ProgressIndicator : MonoBehaviour
{

    public GameObject ProgressObject;
    private IProgressIndicator progressIndicator;
    private bool doIndicate = false;
    private int totalScore;
    private int maxTotalScore;
    public GameObject pTotal;

    // Start is called before the first frame update
    void Start()
    {
        progressIndicator = ProgressObject.GetComponent<IProgressIndicator>();
        Transform pTotalTr = ProgressObject.transform.Find("ProgressTextTotal");
        if (pTotalTr != null)
            pTotal = pTotalTr.gameObject;
        if (pTotal != null)
            pTotal.SetActive(false);
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

        if (ProgressObject.activeSelf && pTotal != null && pTotal.activeSelf)
        {
            pTotal.GetComponent<TextMeshPro>().text = totalScore + "/" + maxTotalScore;
        }

    }

    private float progress = 0;
    public void SetProgress(float currentProgress)
    {
        progress = currentProgress;
    }

    public void SetTotalScore(int currentTotalScore, int currentMaxTotalScore)
    {
        totalScore = currentTotalScore;
        maxTotalScore = currentMaxTotalScore;
        pTotal.GetComponent<TextMeshPro>().text = totalScore + "/" + maxTotalScore;
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
