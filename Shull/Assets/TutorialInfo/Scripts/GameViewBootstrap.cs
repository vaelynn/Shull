using UnityEngine;

public static class GameViewBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureGameViewSeesXander()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Transform target = FindXanderLikeTarget();
        if (target == null)
        {
            Debug.LogWarning("GameViewBootstrap: Could not find xander/player. Spawning visible fallback cube.");
            SpawnFallbackCube(mainCamera);
            return;
        }

        ThirdPersonCamera follow = mainCamera.GetComponent<ThirdPersonCamera>();
        if (follow == null)
        {
            follow = mainCamera.gameObject.AddComponent<ThirdPersonCamera>();
        }

        follow.SetTarget(target);

        Bounds targetBounds = GetTargetBounds(target);
        SpawnTargetMarker(targetBounds.center + Vector3.up * targetBounds.extents.y);

        float viewDistance = Mathf.Max(6f, targetBounds.extents.magnitude * 3f);
        Vector3 lookPoint = targetBounds.center + Vector3.up * (targetBounds.extents.y * 0.4f);
        Vector3 cameraPosition = lookPoint + new Vector3(0f, targetBounds.extents.y * 0.8f, -viewDistance);

        mainCamera.nearClipPlane = 0.01f;
        mainCamera.farClipPlane = 5000f;
        mainCamera.transform.position = cameraPosition;
        mainCamera.transform.LookAt(lookPoint);
        Debug.Log("GameViewBootstrap: Camera locked to target " + target.name + " at distance " + viewDistance);
    }

    private static Transform FindXanderLikeTarget()
    {
        GameObject byTag = GameObject.FindGameObjectWithTag("Player");
        if (byTag != null)
        {
            return byTag.transform;
        }

        GameObject byExactName = GameObject.Find("xander");
        if (byExactName != null)
        {
            return byExactName.transform;
        }

        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLowerInvariant().Contains("xander"))
            {
                return obj.transform;
            }
        }

        return null;
    }

    private static void SpawnFallbackCube(Camera mainCamera)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "VisibilityProbeCube";
        cube.transform.position = new Vector3(0f, 1f, 0f);
        cube.transform.localScale = new Vector3(2f, 2f, 2f);

        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.magenta;
        }

        mainCamera.transform.position = new Vector3(0f, 3f, -8f);
        mainCamera.transform.LookAt(cube.transform.position);
    }

    private static Bounds GetTargetBounds(Transform target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(target.position, new Vector3(1f, 2f, 1f));
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void SpawnTargetMarker(Vector3 markerPosition)
    {
        GameObject existing = GameObject.Find("VisibilityProbeSphere");
        if (existing != null)
        {
            Object.Destroy(existing);
        }

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "VisibilityProbeSphere";
        marker.transform.position = markerPosition;
        marker.transform.localScale = Vector3.one * 0.5f;

        Renderer renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }
    }
}
