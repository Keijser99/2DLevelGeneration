using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Looting : MonoBehaviour
{
    private enum Loot
    {
        CoinOne, CoinTwo, CoinThree, CoinFour, Diamond, Elixer
    }
    private int enumLength;

    public string lootName;
    public int lootChange;

    public Looting(string name, int chance)
    {
        lootName = name;
        lootChange = chance;
    }

    [Range(0, 100)]
    [Tooltip("Should be ranked from highest to lowest chance")]
    public int[] chances;
    public int chanceSum;
    public int total;

    private int lootNumber;

    public Text scoreText;

    [Header("Automatically adjusted")]
    public int score;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < System.Enum.GetValues(typeof(Loot)).Length; i++)
        {
            enumLength++;
        }
        Debug.Log($"Length of enum is {enumLength}");

        for (int i = 0; i < chances.Length; i++)
        {
            chanceSum++;
        }
        Debug.Log($"Length of changes list is {chanceSum}");
        Debug.Log($"Last chance in chances list is {chances[chanceSum -1]}");

        foreach (int chance in chances)
        {
            total += chance;
        }
        Debug.Log($"Total percentage of changes: {total}");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lootNumber = DecideLoot();
            Debug.Log($"Loot number is {lootNumber}");

            for (int i = 0; i < chanceSum; i++)
            {
                if (lootChange == i)
                {
                    Debug.Log($"The prize is: {(Loot)i}");
                }
            }

        }
    }

    private int DecideLoot()
    {
        int randomValue = Random.Range(0, total);
        Debug.Log($"Random value is {randomValue}");

        for (int i = 0; i < chanceSum-1; i++)
        {
            if (randomValue > chances[i])
            {
                return i;
            }
            else
            {
                Debug.Log($"Random value is not above {chances[i]}, so I'll try lower");
            }
        }
        //if (randomValue < chances[chanceSum-1]) //the last number in the chances list
        //{
        //    DecideLoot();
        //}
        return 0;

        /*
         * Get a random value
         * Go through predetermined intervals to see where the random value fall under
         * Handle the prize according to the correct interval
         * 
         * i.e random value is 87
         * Is it above 40 (change 60%)?
         * yes
         * 
         * 
         * 
         * i.e. random value is 22
         * Is it above 40 (change 60%)?
         * no
         * 
         * Is it above 30?
         * no
         * 
         * Is is above 20?
         * yes
         */
    }
}
