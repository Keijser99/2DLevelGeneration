using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootingSystem : MonoBehaviour
{
    [Header("Tweakables")]
    [SerializeField]
    private int[] table =
    {
        40,
        30,
        20,
        10
    };
    public int scoreSubtractor;

    public Text scoreText;

    [Header("Automatically adjusted")]
    public int score;
    public int total;
    public int randomValue;

    public int tableLength;

    // Start is called before the first frame update
    void Start()
    {
        tableLength = table.Length;

        foreach (int chance in table)
        {
            total += chance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LootChest();
        }
    }

    void LootChest()
    {
        randomValue = Random.Range(0, total);

        for (int i = 0; i < table.Length; i++)
        {
            if (randomValue <= table[i])
            {
                Debug.Log($"Reward: {table[i]}");
                score += scoreSubtractor - table[i];
                scoreText.text = $"Score:   {score}";
                return;
            }
            else
            {
                randomValue -= table[i];
            }
        }
    }
}
