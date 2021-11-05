using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    public Rigidbody2D rb;

    public Animator animator;

    Vector2 movement, otherPos;

    RebuildedHuntAndKillLevelGenerator HuntKillAlg;

    [SerializeField]
    private bool isMining = false;

    private void Start()
    {
        HuntKillAlg = gameObject.GetComponent<RebuildedHuntAndKillLevelGenerator>();
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
        otherPos = new Vector2(other.gameObject.transform.position.x, other.gameObject.transform.position.y);

        //Debug.Log($"I've hit a {other}");
        if (other.gameObject.CompareTag("Walls") && isMining)
        {
            Debug.Log($"I'm mining this {other.gameObject}");
            other.gameObject.SetActive(false);
            Debug.Log(otherPos);
            HuntKillAlg.MiningWalls(otherPos);
        }
    }
}