using UnityEngine;

public class AutoScaleToHeight : MonoBehaviour
{
    [SerializeField] private float targetHeightMeters = 1.8f;
    [SerializeField] private bool applyOnStart = true;

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyScale();
        }
    }

    [ContextMenu("Apply Scale")]
    public void ApplyScale()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return;
        }

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }

        float currentHeight = combinedBounds.size.y;
        if (currentHeight <= 0.0001f)
        {
            return;
        }

        float scaleMultiplier = targetHeightMeters / currentHeight;
        transform.localScale *= scaleMultiplier;
    }
}
