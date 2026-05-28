using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class DemoSceneSetup
{
    private const string PlayerModelPath = "Assets/Scenes/SHULL - ASSETS/player/model/xander.fbx";
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    private const string TerrainDataPath = "Assets/Scenes/SampleTerrain.asset";

    static DemoSceneSetup()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (!scene.path.Replace('\\', '/').EndsWith(SampleScenePath))
        {
            return;
        }

        EditorApplication.delayCall += () =>
        {
            EnsureDefaultTerrain();
            GameObject xander = FindXander();
            if (xander == null)
            {
                PlaceXanderIfMissing();
                xander = FindXander();
            }

            if (xander != null)
            {
                EnsureXanderComponents(xander);
                EnsureCameraTargetsXander(xander.transform);
            }
        };
    }

    [MenuItem("Shull/Place Xander on Terrain")]
    public static void PlaceXanderOnTerrainMenu()
    {
        EnsureDefaultTerrain();
        PlaceXanderIfMissing(force: true);
        GameObject xander = FindXander();
        if (xander != null)
        {
            EnsureXanderComponents(xander);
            EnsureCameraTargetsXander(xander.transform);
        }
    }

    [MenuItem("Shull/Configure Xander Components")]
    public static void ConfigureXanderComponentsMenu()
    {
        GameObject xander = FindXander();
        if (xander == null)
        {
            Debug.LogWarning("No xander/player found in current scene.");
            return;
        }

        EnsureXanderComponents(xander);
        EnsureCameraTargetsXander(xander.transform);
        Selection.activeGameObject = xander;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Configured xander with CharacterController and PlayerMovement.");
    }

    [MenuItem("Shull/Create Default Terrain")]
    public static void CreateDefaultTerrainMenu()
    {
        EnsureDefaultTerrain(forceFocus: true);
    }

    private static void PlaceXanderIfMissing()
    {
        PlaceXanderIfMissing(force: false);
    }

    private static void PlaceXanderIfMissing(bool force)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        EnsureDefaultTerrain();

        if (!force && FindXander() != null)
        {
            return;
        }

        if (force && FindXander() != null)
        {
            GameObject existing = FindXander();
            EnsureXanderComponents(existing);
            EnsureCameraTargetsXander(existing.transform);
            Selection.activeGameObject = existing;
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log("xander is already in the scene.");
            return;
        }

        GameObject modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerModelPath);
        if (modelAsset == null)
        {
            Debug.LogError("Could not find xander.fbx at: " + PlayerModelPath);
            return;
        }

        GameObject xander = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
        xander.name = "xander";
        xander.transform.position = GetTerrainSpawnPosition();
        xander.transform.rotation = Quaternion.identity;

        AutoScaleToHeight autoScale = xander.GetComponent<AutoScaleToHeight>();
        if (autoScale == null)
        {
            autoScale = xander.AddComponent<AutoScaleToHeight>();
        }

        autoScale.ApplyScale();
        EnsureXanderComponents(xander);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = xander;
        SceneView.lastActiveSceneView?.FrameSelected();
        EnsureCameraTargetsXander(xander.transform);

        Debug.Log("Placed xander on terrain at " + xander.transform.position);
    }

    private static void EnsureXanderComponents(GameObject xander)
    {
        if (!xander.CompareTag("Player"))
        {
            xander.tag = "Player";
        }

        if (xander.GetComponent<CharacterController>() == null)
        {
            CharacterController controller = xander.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
        }

        if (xander.GetComponent<PlayerMovement>() == null)
        {
            xander.AddComponent<PlayerMovement>();
        }

        if (xander.GetComponent<AutoScaleToHeight>() == null)
        {
            xander.AddComponent<AutoScaleToHeight>();
        }
    }

    private static void EnsureCameraTargetsXander(Transform xander)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        ThirdPersonCamera cameraFollow = mainCamera.GetComponent<ThirdPersonCamera>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<ThirdPersonCamera>();
        }

        SerializedObject serializedCameraFollow = new SerializedObject(cameraFollow);
        SerializedProperty targetProperty = serializedCameraFollow.FindProperty("target");
        if (targetProperty != null)
        {
            targetProperty.objectReferenceValue = xander;
            serializedCameraFollow.ApplyModifiedPropertiesWithoutUndo();
        }

        mainCamera.transform.position = xander.position + new Vector3(0f, 2f, -5f);
        mainCamera.transform.rotation = Quaternion.LookRotation((xander.position + Vector3.up * 1.4f) - mainCamera.transform.position);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static void EnsureDefaultTerrain(bool forceFocus = false)
    {
        Terrain existingTerrain = Object.FindObjectOfType<Terrain>();
        if (existingTerrain != null)
        {
            if (forceFocus)
            {
                Selection.activeGameObject = existingTerrain.gameObject;
                SceneView.lastActiveSceneView?.FrameSelected();
            }
            return;
        }

        TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainDataPath);
        if (terrainData == null)
        {
            terrainData = new TerrainData
            {
                heightmapResolution = 513
            };
            terrainData.size = new Vector3(500f, 50f, 500f);
            AssetDatabase.CreateAsset(terrainData, TerrainDataPath);
            AssetDatabase.SaveAssets();
        }

        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainObject.name = "Terrain";
        terrainObject.transform.position = Vector3.zero;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        if (forceFocus)
        {
            Selection.activeGameObject = terrainObject;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        Debug.Log("Created default terrain in SampleScene.");
    }

    private static GameObject FindXander()
    {
        GameObject byName = GameObject.Find("xander");
        if (byName != null)
        {
            return byName;
        }

        return GameObject.Find("Player");
    }

    private static Vector3 GetTerrainSpawnPosition()
    {
        Terrain terrain = Object.FindObjectOfType<Terrain>();
        if (terrain == null)
        {
            return new Vector3(0f, 0f, 0f);
        }

        Vector3 terrainPosition = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;
        Vector3 center = terrainPosition + new Vector3(terrainSize.x * 0.5f, 0f, terrainSize.z * 0.5f);
        center.y = terrain.SampleHeight(center) + terrainPosition.y;

        return center;
    }
}
