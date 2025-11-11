using UnityEngine;
using UnityEditor;

public class NotesEditorWindow : EditorWindow
{
    private WorldNote selectedNote;

    [MenuItem("Own Tools/World Notes Editor")]
    public static void ShowWindow()
    {
        GetWindow<NotesEditorWindow>("World Notes Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("World Notes Editor", EditorStyles.boldLabel);

        selectedNote = EditorGUILayout.ObjectField("WorldNote Objekt", selectedNote, typeof(WorldNote), true) as WorldNote;

        if (selectedNote != null)
        {
            EditorGUILayout.LabelField("Notiztext", EditorStyles.boldLabel);
            selectedNote.noteText = EditorGUILayout.TextArea(selectedNote.noteText, GUILayout.Height(100));

            EditorGUILayout.HelpBox("Diese Notiz wird im Spiel angezeigt, wenn der Spieler in Reichweite ist und E drückt.", MessageType.Info);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(selectedNote);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Wähle ein WorldNote-Objekt aus der Szene oder einem Prefab aus.", MessageType.Info);
        }
    }
}
