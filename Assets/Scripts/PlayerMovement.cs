using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody2D rb;

    public Animator animator;

    Vector2 movement;

    RebuildedHuntAndKillLevelGenerator huntKillAlg;

    [SerializeField]
    private bool isMining = false, canMine = false;

    Scene activeScene;

    private void Start()
    {
        huntKillAlg = RebuildedHuntAndKillLevelGenerator.Instance;

        activeScene = SceneManager.GetActiveScene();

        if (activeScene.name == "HuntAndKillLevelGeneration")
        {
            canMine = true;
        }
    }

    private void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);

        animator.SetFloat("Speed", movement.sqrMagnitude);

        if (Input.GetKey(KeyCode.Space))
        {
            isMining = true;
        }
        else isMining = false;

        
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerStay2D(Collider2D other)
    {

        if (other.gameObject.CompareTag("Walls") && isMining && canMine)
        {
            other.gameObject.SetActive(false);

            huntKillAlg.MiningWalls(other.gameObject);
        }
    }
}