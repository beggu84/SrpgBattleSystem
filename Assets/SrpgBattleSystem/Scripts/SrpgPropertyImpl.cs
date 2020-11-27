using UnityEngine;

[System.Serializable]
public class SrpgPropertyImpl
{
    [SerializeField]
    private string _name;
    public string name
    {
        get { return _name; }
        set { _name = value; }
    }

    [SerializeField]
    private bool _b;
    public bool b
    {
        get { return _b; }
        set { _b = value; }
    }

    [SerializeField]
    private int _n;
    public int n
    {
        get { return _n; }
        set { _n = value; }
    }

    [SerializeField]
    private float _f;
    public float f
    {
        get { return _f; }
        set { _f = value; }
    }

    [SerializeField]
    private string _s;
    public string s
    {
        get { return _s; }
        set { _s = value; }
    }

    public SrpgPropertyImpl(string name)
    {
        _name = name;
    }

    public void CopyValuesTo(SrpgPropertyImpl other)
    {
        other.b = _b;
        other.n = _n;
        other.f = _f;
        other.s = _s;
    }
}
