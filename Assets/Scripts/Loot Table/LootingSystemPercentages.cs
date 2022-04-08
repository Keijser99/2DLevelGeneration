using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootingSystemPercentages : MonoBehaviour
{
    [Header("Tweakables")]
    [SerializeField]
    private int[] percentages =
    {
        10, // 4 coins
        20, // 3 coins
        30, //2 coins
        40 //1 coin
    };

    public Text scoreText;

    [Header("Automatically adjusted")]
    public int score, currentScore;
    public int total;
    public int randomValue;

    private int topValue;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < percentages.Length; i++)
        {
            total += percentages[i];
        }

        if (total != 100f)
        {
            Debug.LogWarning("Total should be 100. Check whether the sum of percentages is 100");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetLootValue();
        }
    }

    private void GetLootValue()
    {
        topValue = 0;
        score = 0;
        randomValue = Random.Range(0, total);
        Debug.Log($"Random Value = {randomValue}");

        for (int i = 0; i < percentages.Length; i++)
        {
            topValue += percentages[i];
            Debug.Log($"topValue = {topValue} and percentage[i] = {percentages[i]}");
            if (randomValue < topValue)
            {
                score += 1 + i;
                currentScore += score;
                scoreText.text = $"Score:   {currentScore}";
                Debug.Log($"i is {i}, so points gained = {score}");
                break;
            }
        }
    }
}