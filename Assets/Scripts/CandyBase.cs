using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyBase : MonoBehaviour
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
	public GameManager gameManager;


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
		this.gameManager = gameManager;
		this._candyType = candyType;
	}
	
}
