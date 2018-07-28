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
    
//    //左
//    public void MoveToLeft(float fillTime)
//    {
//        MoveTo(_candyObject.X-1, _candyObject.Y, fillTime);	
//    }
//    //右
//    public void MoveToRight(float fillTime)
//    {
//        MoveTo(_candyObject.X+1, _candyObject.Y, fillTime);	
//    }
//    //向左下 移动一格
//    public void MoveToBelowLeft( float fillTime)
//    {
//        MoveTo(_candyObject.X-1, _candyObject.Y+1, fillTime);	
//    }
//    //向右下 移动一格
//    public void MoveToBelowRight( float fillTime)
//    {
//        MoveTo(_candyObject.X+1, _candyObject.Y+1, fillTime);	
//    }
    
    public void MoveToCandyAndReplace(CandyObject candy, float fillTime)
    {
//        逻辑更新 地图上的位置
//        GameManager.Inst.UpdateCandyPositionInMap(_candyObject, candy.X, candy.Y);
        
        MoveTo(candy.X, candy.Y, fillTime);
        Destroy(candy.gameObject);
    }
    
    public void MoveTo(int x, int y, float time)
    {
//        Debug.Log("MoveTo x = " + x + "  y=" + y);
        //逻辑更新 地图上的位置
        GameManager.Inst.UpdateCandyPositionInMap(_candyObject, x, y);

        //开始动画
        if (moveCoroutine != null)
        {
            Debug.Log("stop Coroutine");
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = MoveCoroutine(x, y, time);
        StartCoroutine(moveCoroutine);
    }

    private IEnumerator MoveCoroutine(int x, int y, float time)
    {
        //动画处理
        Vector3 startPosition = transform.position;
        Vector3 endPosition = GameManager.Inst.CorrectPostion(x, y);

        for (float t = 0; t < time; t += Time.deltaTime)
        {
            transform.position
                = Vector3.Lerp(startPosition, endPosition, t / time);
            yield return 0; //每帧
        }
        //保证位置一定对。
//        Debug.Log("保证位置一定对。");
        transform.position = endPosition;
    }
    
    
    
    //在交换的时候,如果两个物品交换不能触发消除. 需要回退
    public void MoveToAndReturn(int x, int y, float time)
    {
        //开始动画
        if (moveCoroutine != null)
        {
            Debug.Log("stop moveAndReturn Coroutine");
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = MoveAndReturnCoroutine(x, y, time);
        StartCoroutine(moveCoroutine);
    }

    private IEnumerator MoveAndReturnCoroutine(int x, int y, float time)
    {
        //动画处理
        Vector3 startPosition = transform.position;
        Vector3 endPosition = GameManager.Inst.CorrectPostion(x, y);
//        Vector3 endPosition = GameManager.Inst.CorrectPostion(_candyObject.X, _candyObject.Y);

        for (float t = 0; t < time; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, t / time );
            yield return 0;
        }
        
        yield return new WaitForSeconds(0.3f);
        //返回
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(endPosition, startPosition, t / time );
            yield return 0;
        }
        
        //保证位置一定对。
//        Debug.Log("保证位置一定对。");
        transform.position = startPosition; //回到开始位置
    }






}