using UnityEngine;

public class FixedDistanceCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSmoothTime = 0.05f;

    private Vector3 offsetWorld;
    private Vector3 velocity;
    private Quaternion fixedRotation;

    private void Awake()
    {
        fixedRotation = transform.rotation;
    }

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        RebuildOffset();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = target.position + offsetWorld;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, followSmoothTime);
        transform.rotation = fixedRotation;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        RebuildOffset();
    }

    public void RebuildOffset()
    {
        if (target == null)
        {
            return;
        }

        offsetWorld = transform.position - target.position;
        fixedRotation = transform.rotation;
    }
}
