using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SrpgMap))]
public class SrpgMapEditor : Editor
{
	private SrpgMap _target;

    void OnEnable()
    {
        _target = (SrpgMap)target;
    }

    public override void OnInspectorGUI()
    {
        GUI.enabled = false;

        //EditorGUILayout.EnumPopup("View Type", _srpgMap.viewType);

        EditorGUILayout.Space();

        EditorGUILayout.IntField("Map Width", _target.mapWidth);
        EditorGUILayout.IntField("Map Height", _target.mapHeight);

        EditorGUILayout.Space();

        EditorGUILayout.IntField("Tile Width", _target.tileWidth);
        EditorGUILayout.IntField("Tile Height", _target.tileHeight);

        //EditorGUILayout.Space();

        //EditorGUILayout.IntField("Altitude", _target.altitude);

        GUI.enabled = true;
    }
}
