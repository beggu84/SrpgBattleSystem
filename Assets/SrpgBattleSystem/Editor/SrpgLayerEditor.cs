using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SrpgLayer))]
public class SrpgLayerEditor : Editor
{
    private SrpgLayer _target;

    void OnEnable()
    {
        _target = (SrpgLayer)target;
    }

    public override void OnInspectorGUI()
    {
        GUI.enabled = false;

        EditorGUILayout.IntField("Z-Index Start", _target.zIndexStart);
        EditorGUILayout.Toggle("Touchable", _target.touchable);

        GUI.enabled = true;

        Undo.RecordObject(_target, _target.name);

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayout.LabelField("Properties");

            GUILayoutOption typeWidth = GUILayout.Width(100);
            if (_target.propertySchemes.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Type", typeWidth);
                EditorGUILayout.LabelField("Name");
                EditorGUILayout.EndHorizontal();

                for(int i=0; i<_target.propertySchemes.Count; i++)
                {
                    SrpgPropertyScheme scheme = _target.propertySchemes[i];

                    bool stop = false;
                    EditorGUILayout.BeginHorizontal();
                    {
                        scheme.type = (SrpgPropertyScheme.PropType)EditorGUILayout.EnumPopup(scheme.type, typeWidth);
                        scheme.name = EditorGUILayout.TextField(scheme.name);
                        if (GUILayout.Button("-", GUILayout.Width(15), GUILayout.Height(14)))
                        {
                            _target.propertySchemes.RemoveAt(i);
                            SrpgTile[] tiles = _target.GetComponentsInChildren<SrpgTile>();
                            foreach (SrpgTile tile in tiles)
                                tile.RemoveProperty(scheme.name);

                            stop = true;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (stop)
                        break;
                }
            }

            if (GUILayout.Button("Add"))
                _target.propertySchemes.Add(new SrpgPropertyScheme(SrpgPropertyScheme.PropType.Bool, "Name"));
        }
        EditorGUILayout.EndVertical();
    }
}
