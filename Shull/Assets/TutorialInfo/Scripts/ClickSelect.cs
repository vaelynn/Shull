using UnityEngine;
using UnityEngine.EventSystems;

public class ClickSelect : MonoBehaviour
{
    [SerializeField] private Camera sourceCamera;
    [SerializeField] private ThirdPersonCamera thirdPersonCamera;
    [SerializeField] private LayerMask raycastLayers = ~0;
    [SerializeField] private float maxDistance = 500f;

    public Transform Selected { get; private set; }

    private void Awake()
    {
        if (sourceCamera == null)
        {
            sourceCamera = Camera.main;
        }

        if (thirdPersonCamera == null)
        {
            thirdPersonCamera = GetComponent<ThirdPersonCamera>();
        }
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (sourceCamera == null)
        {
            return;
        }

        Ray ray = sourceCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, raycastLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        PlayerMovement player = hit.transform.GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            Selected = player.transform;
            if (thirdPersonCamera != null)
            {
                thirdPersonCamera.SetTarget(Selected);
            }
            return;
        }

        Selected = hit.transform;
    }
}
