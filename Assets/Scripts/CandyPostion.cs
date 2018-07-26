using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyPostion
{
    private int _x;
    public int X
    {
        get { return _x; }
        set
        {
//            if (HasMove())
//			{
            _x = value;
//			}
        }
    }

    private int _y;
    public int Y
    {
        get { return _y; }
        set
        {
//			if (HasMove())
//			{
            _y = value;
//			}
        }
    }
    
}