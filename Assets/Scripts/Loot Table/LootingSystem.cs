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

    public GameObject chestPrefab;
    public Sprite chestOpenedSprite;

    [Header("Automatically adjusted")]
    public int score;
    public int total;
    public int randomValue;

    public int tableLength;

    private SpriteRenderer spriteRenderer;
    public bool pressingSpace = false;

    [SerializeField]
    private bool canLoot = false;
    public GameObject interactedChest;

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
        if (Input.GetKeyDown(KeyCode.Space) && canLoot)
        {
            LootChest();

            canLoot = false;

            Debug.Log("You opened a chest!");
            spriteRenderer = interactedChest.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = chestOpenedSprite;
            interactedChest.tag = "Opened";
        }

        if (Input.GetButtonDown("Jump"))
        {
            pressingSpace = true;
        }

        if (Input.GetButtonUp("Jump"))
        {
            pressingSpace = false;
        }
    }

    void LootChest()
    {
        randomValue = Random.Range(0, total);
        Debug.Log($"Random value is {randomValue}");

        for (int i = 0; i < table.Length; i++)
        {
            if (randomValue <= table[i])
            {
                int pointsGained = 10 * (i + 1);

                Debug.Log($"Random value is below {table[i]}!");
                score += pointsGained;
                Debug.Log($"{table[i]} was number {i + 1} in the table, so the player gets {pointsGained} points");
                scoreText.text = $"Score:   {score}";
                return;
            }
            else
            {
                randomValue -= table[i];
                Debug.Log($"Random value is above {table[i]}, so it is been substracted to {randomValue}");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Chest")
        {
            canLoot = true;
            interactedChest = other.gameObject;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Chest")
        {
            canLoot = false;
        }
    }
}
