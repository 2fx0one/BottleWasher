using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//移动组件
public class CandyMoved : MonoBehaviour
{
	private CandyObject _candyObject;

	private IEnumerator moveCoroutine;
	
	private void Awake()
	{
		_candyObject = GetComponent<CandyObject>();
	}

	public void Move(int x, int y, float time)
	{
		if (moveCoroutine != null)
		{
			StopCoroutine(moveCoroutine);
		}

		moveCoroutine = MoveCoroutine(x, y, time);
		StartCoroutine(moveCoroutine);
//		_candyObject.X = x;
//		_candyObject.Y = y;
//        _candyObject.transform.position = _candyObject._gameManager.CorrectPostion(x, y);
	}

	private IEnumerator MoveCoroutine(int x, int y, float time)
	{
		//把当前组件的candy的X Y 更新
		_candyObject.X = x;
		_candyObject.Y = y;

		Vector3 startPosition = transform.position;
		Vector3 endPosition = _candyObject._gameManager.CorrectPostion(x, y);

		for (float t = 0; t < time; t += Time.deltaTime)
		{
			_candyObject.transform.position
				= Vector3.Lerp(startPosition, endPosition, t / time);
			yield return 0; //每帧
		}
		
		//保证位置一定对。
		_candyObject.transform.position = _candyObject._gameManager.CorrectPostion(x, y);
	}
	
	
}
