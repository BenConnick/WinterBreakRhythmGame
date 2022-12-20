using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public Scorekeeper scorekeeper;

    public TextMeshProUGUI[] scoreLabels;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < scoreLabels.Length; i++)
        {
            scoreLabels[i].text = $"{scorekeeper.GetHits(i)} / {scorekeeper.GetTotalElapsed(i)}";
        }
    }
}
