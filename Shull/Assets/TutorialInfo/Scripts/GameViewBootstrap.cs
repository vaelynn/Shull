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
            return;
        }

        ThirdPersonCamera follow = mainCamera.GetComponent<ThirdPersonCamera>();
        if (follow == null)
        {
            follow = mainCamera.gameObject.AddComponent<ThirdPersonCamera>();
        }

        follow.SetTarget(target);

        Vector3 cameraPosition = target.position + new Vector3(0f, 2f, -5f);
        mainCamera.transform.position = cameraPosition;
        mainCamera.transform.LookAt(target.position + Vector3.up * 1.4f);
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
}
