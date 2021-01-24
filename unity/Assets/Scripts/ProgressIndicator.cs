using Microsoft.MixedReality.Toolkit.UI;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ProgressIndicator : MonoBehaviour
{

    public GameObject ProgressObject; // Object that this script is assigned to
    private IProgressIndicator progressIndicator; // a component of ProgressObject
    private bool doIndicate = false; // conform to MRTK documentation on progres indicators
    private int totalScore;
    private int maxTotalScore;
    public GameObject pTotal; // The progress indicator's text holder

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
    // conforms to the MRTK documentation on progress indicators
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

    // updates (but not display) the current progress. 
    private float progress = 0;
    public void SetProgress(float currentProgress)
    {
        progress = currentProgress;
    }

    // updates and displays the current and total score on the progressindicator's text field
    public void SetTotalScore(int currentTotalScore, int currentMaxTotalScore)
    {
        totalScore = currentTotalScore;
        maxTotalScore = currentMaxTotalScore;
        pTotal.GetComponent<TextMeshPro>().text = totalScore + "/" + maxTotalScore;
    }


    // async task to display the progress of the indicator. See MRTK documentation on progress indicators for more details.
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