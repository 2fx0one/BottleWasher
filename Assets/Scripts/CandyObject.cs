using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CandyObject : MonoBehaviour
{
//	private CandyPostion _postion;
	
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

	private GameManager.CandyType _candyType;

	public GameManager.CandyType CandyType
	{
		get { return _candyType; }
	}

//	[HideInInspector]
//	public GameManager _gameManager;
//

	private CandyMoved _movement;

	public CandyMoved Movement
	{
		get { return _movement; }
	}

	private CandyCategory _category;

	public CandyCategory Category
	{
		get { return _category; }
	}

	public CandyClean Clean
	{
		get { return _clean; }
	}

	private CandyClean _clean;
	
	private void Awake()
	{
        _movement = GetComponent<CandyMoved>();
		_category = GetComponent<CandyCategory>();
		_clean = GetComponent<CandyClean>();
	}
	
	
	public bool HasMove()
	{
        return _movement != null;
	}
	public bool HasCategroy()
	{
		return _category != null;
	}

	public bool HasClear()
	{
		return _clean != null;
	}
	
	public void Init(int x, int y, GameManager.CandyType candyType)
	{
		this._x = x;
		this._y = y;
		this._candyType = candyType;
	}
	

	private void OnMouseEnter()
	{
		Debug.Log("OnMouseEnter  x = "+ _x + "   y = " + _y);
		GameManager.Inst.EnteredCandy(this);
	}
	
	private void OnMouseDown()
	{
//		Destroy(this.gameObject);
		Debug.Log("OnMouseDown x = " + _x + "   y = " + _y);
		GameManager.Inst.PressCandy(this);
	}

//	private void OnMouseUp()
//	{
//		Debug.Log("OnMouseUp UpUpUp x = "+ _x + "   y = " + _y);
//		GameManager.Inst.ReleaseCandy();
//		throw new System.NotImplementedException();
//	}
}
