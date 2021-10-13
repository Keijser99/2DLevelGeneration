using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewSceneReloader : MonoBehaviour
{
    [SerializeField]
    private int switchAlgorithmState = 1; // 1 is DrunkardWalk, -1 is otherAlgorithm

    [SerializeField]
    GameObject drunkardWalk, otherAlgorithm;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            switchAlgorithmState *= -1;
            Debug.Log("algorithm state changed to: " + switchAlgorithmState);

            if (switchAlgorithmState == -1) //switch to the other algorithm
            {
                ReloadScene();

                drunkardWalk.SetActive(false);
                otherAlgorithm.SetActive(true);
            }
            else if (switchAlgorithmState == 1) //switch to the Drunkard Walk algorithm
            {
                ReloadScene();

                otherAlgorithm.SetActive(false);
                drunkardWalk.SetActive(true);
            }
        }       
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
