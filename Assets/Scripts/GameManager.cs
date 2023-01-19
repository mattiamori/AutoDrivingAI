using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    int points;
    public TextMeshPro score;
    // Start is called before the first frame update
    private void Awake()
    {
        points = 0;
        score.text = "Punti: \n" + points.ToString();
        instance = this;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPoints(int x)
    {
        points = x;
        UpdateTextPoints();
        
    }

    public int GetPoints()
    {
        return points;
    }

    public void UpdateTextPoints()
    {
        score.text = "Punti: \n" + points.ToString();
    }
}
