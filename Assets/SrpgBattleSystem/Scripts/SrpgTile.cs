using System;
using System.Collections.Generic;
using UnityEngine;

public class SrpgTile : MonoBehaviour
{
    [SerializeField]
    private int _y = -1;
    public int y
    {
        get { return _y; }
        set { _y = value; }
    }

    [SerializeField]
    private int _x = -1;
    public int x
    {
        get { return _x; }
        set { _x = value; }
    }

    [SerializeField]
    private List<SrpgPropertyImpl> _properties = new List<SrpgPropertyImpl>();
    public List<SrpgPropertyImpl> properties
    {
        get { return _properties; }
    }

    public delegate bool MouseEventHandler(object sender, EventArgs e);
    public event MouseEventHandler clicked;

    public bool IsSameTile(SrpgTile other)
    {
        return (GetComponent<SpriteRenderer>().sprite.GetInstanceID()
            == other.GetComponent<SpriteRenderer>().sprite.GetInstanceID());
    }

    //public virtual bool IsTileWalkable()
    //{

    //}

    public bool TryGetProperty(string name, out SrpgPropertyImpl outProp)
    {
        outProp = null;

        foreach (SrpgPropertyImpl prop in _properties)
        {
            if(prop.name == name)
            {
                outProp = prop;
                return true;
            }
        }

        return false;
    }

    public SrpgPropertyImpl AddProperty(string name)
    {
        SrpgPropertyImpl prop = new SrpgPropertyImpl(name);
        _properties.Add(prop);
        return prop;
    }

    public void RemoveProperty(string name)
    {
        for(int i=0; i<_properties.Count; i++)
        {
            if (_properties[i].name == name)
            {
                _properties.RemoveAt(i);
                break;
            }
        }
    }

    public void CopyProperties(List<SrpgPropertyImpl> srcProperties)
    {
        _properties.Clear();

        foreach(SrpgPropertyImpl srcProp in srcProperties)
        {
            SrpgPropertyImpl newProp = new SrpgPropertyImpl(srcProp.name);
            srcProp.CopyValuesTo(newProp);
            _properties.Add(newProp);
        }
    }

    void OnMouseDown()
    {
        if (clicked != null)
            clicked.Invoke(this, new EventArgs());
    }
}
