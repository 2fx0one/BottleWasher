using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyBase : MonoBehaviour
{
	
	private int _x;
	public int X
	{
		get { return _x; }
		set { _x = value; }
	}

	private int _y;
	public int Y
	{
		get { return _y; }
		set { _y = value; }
	}

	private GameManager.CandyType _candyType;

	public GameManager.CandyType CandyType
	{
		get { return _candyType; }
	}

	[HideInInspector]
	public GameManager gameManager;

	public void Init(int x, int y, GameManager gameManager, GameManager.CandyType candyType)
	{
		this._x = x;
		this._y = y;
		this.gameManager = gameManager;
		this._candyType = candyType;
	}
	
}
