using UnityEngine;

[System.Serializable]
public class SrpgPropertyScheme
{
    public enum PropType
    {
        Bool = 0,
        Int,
        Float,
        String
    }

    [SerializeField]
    private PropType _type;
    public PropType type
    {
        get { return _type; }
        set { _type = value; }
    }

    [SerializeField]
    private string _name;
    public string name
    {
        get { return _name; }
        set { _name = value; }
    }

    public SrpgPropertyScheme(PropType type, string name)
    {
        _type = type;
        _name = name;
    }
}
