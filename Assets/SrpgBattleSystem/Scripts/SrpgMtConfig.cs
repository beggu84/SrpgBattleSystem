using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SrpgMtConfig : ScriptableObject
{
    public Texture2D layerVisibleIcon = null;
    public Texture2D layerHiddenIcon = null;
    public List<Texture> sceneMenuIcons = new List<Texture>();
}
