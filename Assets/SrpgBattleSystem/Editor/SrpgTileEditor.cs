using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SrpgTile))]
public class SrpgTileEditor : Editor
{
    private SrpgTile _target;

    void OnEnable()
    {
        _target = (SrpgTile)target;
    }

    public override void OnInspectorGUI()
    {
        GUI.enabled = false;

        _target.x = EditorGUILayout.IntField("X", _target.x);
        _target.y = EditorGUILayout.IntField("Y", _target.y);

        GUI.enabled = true;

        EditorGUILayout.Space();

        SrpgLayer parentLayer = _target.transform.parent.GetComponent<SrpgLayer>();
        if (parentLayer == null || parentLayer.propertySchemes.Count == 0)
            return;

        Undo.RecordObject(_target, _target.name);

        EditorGUILayout.BeginVertical("box");
        {
            EditorGUILayout.LabelField("Layer Properties");

            GUILayoutOption valueWidth = GUILayout.Width(100);
            foreach (SrpgPropertyScheme scheme in parentLayer.propertySchemes)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(scheme.name);

                    SrpgPropertyImpl prop = null;
                    if (!_target.TryGetProperty(scheme.name, out prop))
                    {
                        if (GUILayout.Button("Use", GUILayout.Width(100), GUILayout.Height(14)))
                        {
                            prop = _target.AddProperty(scheme.name);

                            if (scheme.type == SrpgPropertyScheme.PropType.Bool)
                                prop.b = false;
                            else if (scheme.type == SrpgPropertyScheme.PropType.Int)
                                prop.n = 0;
                            else if (scheme.type == SrpgPropertyScheme.PropType.Float)
                                prop.f = 0f;
                            else if (scheme.type == SrpgPropertyScheme.PropType.String)
                                prop.s = "";
                            else
                                Debug.LogError("Wrong type property!");
                        }
                    }
                    else
                    {
                        if (scheme.type == SrpgPropertyScheme.PropType.Bool)
                            prop.b = EditorGUILayout.Toggle(prop.b, valueWidth);
                        else if (scheme.type == SrpgPropertyScheme.PropType.Int)
                            prop.n = EditorGUILayout.IntField(prop.n, valueWidth);
                        else if (scheme.type == SrpgPropertyScheme.PropType.Float)
                            prop.f = EditorGUILayout.FloatField(prop.f, valueWidth);
                        else if (scheme.type == SrpgPropertyScheme.PropType.String)
                            prop.s = EditorGUILayout.TextField(prop.s, valueWidth);
                        else
                            Debug.LogError("Wrong type property!");
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Apply to same tiles"))
            {
                SrpgTile[] layerTiles = parentLayer.GetComponentsInChildren<SrpgTile>();
                foreach(SrpgTile tile in layerTiles)
                {
                    if (tile == _target)
                        continue;

                    if(tile.IsSameTile(_target))
                        tile.CopyProperties(_target.properties);
                }
            }
        }
        EditorGUILayout.EndVertical();
    }
}
