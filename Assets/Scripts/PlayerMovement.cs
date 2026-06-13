using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 8f;

    private LeaderController leader;

    void Start()
    {
        leader = GetComponent<LeaderController>();
    }

    void Update()
    {
        if (leader != null && leader.isDead)
            return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}