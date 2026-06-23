using UnityEngine;

public class StopLight : MonoBehaviour
{
    [Header("Light Timing")]
    public float greenTime = 5f;
    public float redTime = 5f;

    [Header("Current State")]
    public bool isGreen = true;

    private float timer;

    private void Start()
    {
        timer = greenTime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            isGreen = !isGreen;

            if (isGreen)
            {
                timer = greenTime;
                Debug.Log("GREEN LIGHT");
            }
            else
            {
                timer = redTime;
                Debug.Log("RED LIGHT");
            }
        }
    }
}