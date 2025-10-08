using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PatrolPathEditor : EditorWindow
{
    private GameObject patrolPath;
    private PatrolPath patrolPathScript;
    private GameObject enemyPrefab;
    private string patrolPathName = "PatrolPath";
    private List<PatrolPath> existingPatrolPaths = new List<PatrolPath>();
    private int selectedPathIndex = 0;

    [MenuItem("Own Tools/Enemy Creator")]
    public static void ShowWindow()
    {
        GetWindow<PatrolPathEditor>("Enemy Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Enemy Creator", EditorStyles.boldLabel);

        patrolPathName = EditorGUILayout.TextField("Patrol Path Name", patrolPathName);

        if (patrolPath == null)
        {
            if (GUILayout.Button("Create Patrol Path"))
            {
                CreatePatrolPath();
            }
        }
        else
        {
            if (GUILayout.Button("Add Patrol Point"))
            {
                AddPatrolPoint();
            }

            if (GUILayout.Button("Clear Patrol Points"))
            {
                ClearPatrolPoints();
            }

            if (GUILayout.Button("Save Patrol Path"))
            {
                SavePatrolPath();
            }
        }

        GUILayout.Space(10);

        GUILayout.Label("Enemy Prefab", EditorStyles.boldLabel);
        enemyPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", enemyPrefab, typeof(GameObject), false);


        GUILayout.Space(10);

        GUILayout.Label("Existing Patrol Paths", EditorStyles.boldLabel);
        UpdateExistingPatrolPaths();
        string[] pathNames = new string[existingPatrolPaths.Count];
        for (int i = 0; i < existingPatrolPaths.Count; i++)
        {
            pathNames[i] = existingPatrolPaths[i].name;
        }
        int newSelectedPathIndex = EditorGUILayout.Popup("Select Patrol Path", selectedPathIndex, pathNames);
        if (newSelectedPathIndex != selectedPathIndex)
        {
            selectedPathIndex = newSelectedPathIndex;
            HighlightSelectedPath();
        }


        if (enemyPrefab != null)
        {
            if (GUILayout.Button("Create Enemy"))
            {
                CreateEnemy();
            }
        }
    }

    private void CreatePatrolPath()
    {
        patrolPath = new GameObject(patrolPathName);
        patrolPathScript = patrolPath.AddComponent<PatrolPath>();
    }

    private void AddPatrolPoint()
    {
        GameObject newPoint = new GameObject("PatrolPoint");
        if (patrolPathScript.patrolPoints.Count > 0)
        {
            newPoint.transform.position = patrolPathScript.patrolPoints[patrolPathScript.patrolPoints.Count - 1].position;
        }
        else
        {
            newPoint.transform.position = patrolPath.transform.position;
        }
        newPoint.transform.parent = patrolPath.transform;
        patrolPathScript.patrolPoints.Add(newPoint.transform);

        // Wähle den neu erstellten Patrouillenpunkt aus
        Selection.activeGameObject = newPoint;
    }

    private void ClearPatrolPoints()
    {
        for (int i = patrolPathScript.patrolPoints.Count - 1; i >= 0; i--)
        {
            if (patrolPathScript.patrolPoints[i] != null)
            {
                DestroyImmediate(patrolPathScript.patrolPoints[i].gameObject);
            }
        }
        patrolPathScript.patrolPoints.Clear();
    }

    private void SavePatrolPath()
    {
        EditorUtility.SetDirty(patrolPath);
        AssetDatabase.SaveAssets();
    }

    private void CreateEnemy()
    {
        if (enemyPrefab != null)
        {
            GameObject newEnemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab);
            newEnemy.transform.position = patrolPath != null ? patrolPath.transform.position : Vector3.zero;
            Selection.activeGameObject = newEnemy;

            // Weist die Patrouillenpunkte dem neuen Gegner zu
            EnemyBehaviour enemyBehaviour = newEnemy.GetComponent<EnemyBehaviour>();
            if (enemyBehaviour != null && existingPatrolPaths.Count > 0)
            {
                //enemyBehaviour.patrolPath = existingPatrolPaths[selectedPathIndex];
            }
        }
        else
        {
            Debug.LogWarning("Enemy Prefab is not assigned.");
        }
    }

    private void UpdateExistingPatrolPaths()
    {
        existingPatrolPaths.Clear();
        PatrolPath[] paths = FindObjectsOfType<PatrolPath>();
        existingPatrolPaths.AddRange(paths);
    }

    private void HighlightSelectedPath()
    {
        foreach (var path in existingPatrolPaths)
        {
            path.isSelected = false;
        }
        if (existingPatrolPaths.Count > 0)
        {
            existingPatrolPaths[selectedPathIndex].isSelected = true;
        }
        SceneView.RepaintAll();
    }
}
