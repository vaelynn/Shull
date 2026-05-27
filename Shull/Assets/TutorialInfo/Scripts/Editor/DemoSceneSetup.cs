using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DemoSceneSetup
{
    private const string PlayerModelPath = "Assets/Scenes/SHULL - ASSETS/player/model/xander.fbx";

    [MenuItem("Shull/Setup Game Demo Scene")]
    public static void SetupGameDemoScene()
    {
        EnsureGround();
        GameObject player = EnsurePlayer();
        EnsureCamera(player.transform);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = player;

        Debug.Log("Shull demo scene ready: press Play. Use WASD, Shift (crouch), Left Click (attack).");
    }

    private static void EnsureGround()
    {
        if (GameObject.Find("Ground") != null)
        {
            return;
        }

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(5f, 1f, 5f);
    }

    private static GameObject EnsurePlayer()
    {
        GameObject existing = GameObject.Find("Player");
        if (existing != null)
        {
            return existing;
        }

        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerModelPath);
        if (modelAsset == null)
        {
            Debug.LogError("Could not find xander.fbx at: " + PlayerModelPath);
            return new GameObject("Player");
        }

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 0f, 0f);

        if (player.GetComponent<CharacterController>() == null)
        {
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
        }

        if (player.GetComponent<Animator>() == null)
        {
            player.AddComponent<Animator>();
        }

        if (player.GetComponent<AutoScaleToHeight>() == null)
        {
            player.AddComponent<AutoScaleToHeight>();
        }

        if (player.GetComponent<PlayerMovement>() == null)
        {
            player.AddComponent<PlayerMovement>();
        }

        return player;
    }

    private static void EnsureCamera(Transform playerTarget)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("No Main Camera found in scene.");
            return;
        }

        ThirdPersonCamera thirdPersonCamera = mainCamera.GetComponent<ThirdPersonCamera>();
        if (thirdPersonCamera == null)
        {
            thirdPersonCamera = mainCamera.gameObject.AddComponent<ThirdPersonCamera>();
        }

        SerializedObject serializedCamera = new SerializedObject(thirdPersonCamera);
        SerializedProperty targetProperty = serializedCamera.FindProperty("target");
        if (targetProperty != null)
        {
            targetProperty.objectReferenceValue = playerTarget;
            serializedCamera.ApplyModifiedPropertiesWithoutUndo();
        }

        mainCamera.transform.position = playerTarget.position + new Vector3(0f, 2f, -4f);
        mainCamera.transform.LookAt(playerTarget.position + Vector3.up * 1.4f);
    }
}
