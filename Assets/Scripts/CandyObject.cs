using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyObject : MonoBehaviour
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

	private GameManager.CandyType _candyType;

	public GameManager.CandyType CandyType
	{
		get { return _candyType; }
	}

	[HideInInspector]
	public GameManager _gameManager;


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

	public CandyClear Clear
	{
		get { return _clear; }
	}

	private CandyClear _clear;
	
	private void Awake()
	{
        _movement = GetComponent<CandyMoved>();
		_category = GetComponent<CandyCategory>();
		_clear = GetComponent<CandyClear>();
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
		return _clear != null;
	}
	
	public void Init(int x, int y, GameManager gameManager, GameManager.CandyType candyType)
	{
		this._x = x;
		this._y = y;
		this._gameManager = gameManager;
		this._candyType = candyType;
	}
	

	private void OnMouseEnter()
	{
//		Debug.Log("enter");
		_gameManager.EnteredCandy(this);
//		throw new System.NotImplementedException();
	}
	
	private void OnMouseDown()
	{
//		Destroy(this.gameObject);
//		Debug.Log("down");
		_gameManager.PressCandy(this);
//		throw new System.NotImplementedException();
	}

	private void OnMouseUp()
	{
//		Debug.Log("up");
		_gameManager.ReleaseCandy();
//		throw new System.NotImplementedException();
	}
}
