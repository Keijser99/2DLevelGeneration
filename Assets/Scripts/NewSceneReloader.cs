using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewSceneReloader : MonoBehaviour
{
    Scene activeScene;

    PlayerMovement player;

    private void Awake()
    {
        activeScene = SceneManager.GetActiveScene();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SwapScenes();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            SwapScenes();
        }
    }

    private void SwapScenes()
    {
        if (activeScene.name == "HuntAndKillLevelGeneration") //switch to the Drunkard Walk algorithm
        {
            SceneManager.LoadScene("DrunkardWalkLevelGeneration", LoadSceneMode.Single);
            Debug.Log("Swapped the algorithm to: The Drunkard Walk");
        }
        else if (activeScene.name == "DrunkardWalkLevelGeneration") //switch to the other algorithm
        {
            SceneManager.LoadScene("HuntAndKillLevelGeneration", LoadSceneMode.Single);
            Debug.Log("Swapped the algorithm to: The The Hunt-And-Kill Algorithm");
        }
    }
}
