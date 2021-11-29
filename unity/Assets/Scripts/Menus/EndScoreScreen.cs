using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndScoreScreen : MonoBehaviour
{
    public TextMeshPro TotalScore;
    public TextMeshPro Greats;
    public TextMeshPro Goods;
    public TextMeshPro Bads;

    public void setValues(float totalScore, int greats, int goods, int bads)
    {
        TotalScore.text = totalScore.ToString();
        Greats.text = greats.ToString();
        Goods.text = goods.ToString();
        Bads.text = bads.ToString();
    }
}
