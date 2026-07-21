using UnityEngine;
using UnityEngine.AI;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]private Transform target;

    [SerializeField]private Vector3 offset = new Vector3(0f, 15f, -10f);

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = target.position + offset;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
