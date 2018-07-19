using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyMoved : MonoBehaviour
{
	private CandyBase _candyBase;

	private void Awake()
	{
		_candyBase = GetComponent<CandyBase>();
	}

	public void Move(int x, int y)
	{
		_candyBase.X = x;
		_candyBase.Y = y;
        _candyBase.transform.position = _candyBase.gameManager.CorrectPostion(x, y);
	}
	
	
}
