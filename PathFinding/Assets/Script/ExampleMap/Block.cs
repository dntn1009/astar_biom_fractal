using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DefineEnumHelper;

public class Block
{
    public eOreType _type
    {
        get;set;
    }
    public bool _isView
    {
        get;set;
    }

    public GameObject _oreBlock
    {
        get;set;
    }

    public Block(eOreType t, bool v, GameObject ore)
    {
        _type = t;
        _isView = v;
        _oreBlock = ore;
    }
}
