using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class DemoSceneSetup
{
    private const string PlayerModelPath = "Assets/Scenes/SHULL - ASSETS/player/model/xander.fbx";
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

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

        EditorApplication.delayCall += PlaceXanderIfMissing;
    }

    [MenuItem("Shull/Place Xander on Terrain")]
    public static void PlaceXanderOnTerrainMenu()
    {
        PlaceXanderIfMissing(force: true);
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

        if (!force && FindXander() != null)
        {
            return;
        }

        if (force && FindXander() != null)
        {
            Selection.activeGameObject = FindXander();
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

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Selection.activeGameObject = xander;
        SceneView.lastActiveSceneView?.FrameSelected();

        Debug.Log("Placed xander on terrain at " + xander.transform.position);
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
