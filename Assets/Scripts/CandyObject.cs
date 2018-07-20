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
            if (HasMove())
			{
				_x = value;
			}
		}
	}

	private int _y;
	public int Y
	{
		get { return _y; }
		set
		{
			if (HasMove())
			{
				_y = value;
			}
		}
	}

	private GameManager.CandyType _candyType;

	public GameManager.CandyType CandyType
	{
		get { return _candyType; }
	}

	[HideInInspector]
	public GameManager _gameManager;


	private CandyMoved _candyMoved;

	public CandyMoved CandyMoved
	{
		get { return _candyMoved; }
	}

	private CandyCategory _color;

	public CandyCategory Color
	{
		get { return _color; }
	}

	private void Awake()
	{
        _candyMoved = GetComponent<CandyMoved>();
		_color = GetComponent<CandyCategory>();
	}
	
	
	public bool HasMove()
	{
        return _candyMoved != null;
	}
	public bool HasColor()
	{
		return _color != null;
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
		Debug.Log("enter");
		_gameManager.EnteredCandy(this);
//		throw new System.NotImplementedException();
	}
	
	private void OnMouseDown()
	{
//		Destroy(this.gameObject);
		Debug.Log("down");
		_gameManager.PressCandy(this);
//		throw new System.NotImplementedException();
	}

	private void OnMouseUp()
	{
		Debug.Log("up");
		_gameManager.ReleaseCandy();
//		throw new System.NotImplementedException();
	}
}
