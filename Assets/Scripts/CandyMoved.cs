using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyMoved : MonoBehaviour
{
	private CandyObject _candyObject;

	private void Awake()
	{
		_candyObject = GetComponent<CandyObject>();
	}

	public void Move(int x, int y)
	{
		_candyObject.X = x;
		_candyObject.Y = y;
        _candyObject.transform.position = _candyObject._gameManager.CorrectPostion(x, y);
	}
	
	
}
