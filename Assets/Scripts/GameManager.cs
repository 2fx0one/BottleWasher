using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }
    private void Awake()
    {
        _instance = this;
    }

    public int xCol;
    public int yRow;

    public GameObject gridPrefab;

    public enum CandyType //基础分类
    {
        EMPTY,
        NORMAL,
        BARRIER,
        ROW_CLEAR,
        COL_CLEAR,
        RAINBOWCANDY,
        COUNT
    }

    public Dictionary<CandyType, GameObject> candyPrefabDict;

    [System.Serializable]
    public struct CandyPrefab
    {
        public CandyType candyType;
        public GameObject prefab;
    }

    public CandyPrefab[] CandyPrefabs;

    private CandyObject[,] candies; //二维数组

    public float fillTime;

    private CandyObject pressedCandy;
    private CandyObject enteredCandy;
    // Use this for initialization
    void Start () 
    {
        //candy dict
        candyPrefabDict = new Dictionary<CandyType, GameObject>();
        for (int i = 0; i < CandyPrefabs.Length; i++)
        {
            if (!candyPrefabDict.ContainsKey(CandyPrefabs[i].candyType))
            {
                candyPrefabDict.Add(CandyPrefabs[i].candyType, CandyPrefabs[i].prefab);
            }
        }
	    
        //map
        for (int x = 0; x < xCol; x++) {
            for (int y = 0; y < yRow; y++) {
                GameObject grid = Instantiate(gridPrefab, CorrectPostion(x, y), Quaternion.identity);
                grid.transform.SetParent(transform);
            }
        }
		
        //candy init
        candies = new CandyObject[xCol, yRow];
        for (int x = 0; x < xCol; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                createCandy(x, y, CandyType.EMPTY);
//				GameObject candy = Instantiate(candyPrefabDict[CandyType.NORMAL], Vector3.zero, Quaternion.identity);
//				candy.transform.SetParent(transform);
//				candies[x, y]  = candy.transform.GetComponent<CandyBase>();
//				candies[x, y].Init(x, y, this, CandyType.NORMAL);
//
//				if (candies[x, y].HasMove())
//				{
//					candies[x, y].CandyMoved.Move(x, y);
//				}
//
//				if (candies[x, y].HasColor())
//				{
//					candies[x, y].Color.SetColor((CandyCategory.ColorType)(Random.Range(0, candies[x, y].Color.NumColors)));
//				}
            }
			
        }

        Destroy(candies[4, 4].gameObject);
        createCandy(4, 4, CandyType.BARRIER);

//		FillAll();
        StartCoroutine(FillAll());

    }


    public IEnumerator FillAll()
    {
        while (!FillFinished())
        {
            yield return new WaitForSeconds(fillTime);
        }
    }

    //分步填充
    public bool FillFinished()
    {
        //单次填充是否完成
        bool filledFinished = true;
        
        //地图上 从下往上 倒数第二行开始
        for (int currentMapY = yRow-2; currentMapY >= 0; currentMapY--)
        {
            for (int currentMapX = 0; currentMapX < xCol; currentMapX++)
            {
                //当前元素位置的对象
                CandyObject currentCandy = CurrentCandy(currentMapX, currentMapY); //candies[currentX, currentY];
                if (currentCandy.HasMove()) //当前元素可移动
                {
                    CandyObject belowDirectly= this.BelowDirectly(currentMapX, currentMapY);// candies[x, y + 1]; //当前元素的正下方

                    if ( belowDirectly.CandyType == CandyType.EMPTY) //正下方 是空的
                    {
//                        ExchangeCandyObjectPosition(currentCandy, belowDirectly); //逻辑上当前位置和正下方的交换位置 地图二维数组
//                        currentCandy.CandyMoved.Move(belowDirectly.X, belowDirectly.Y, fillTime); //动画上更新位置
//                        candies[belowDirectly.X, belowDirectly.Y] = currentCandy;
//                        candies[currentCandy.X, currentCandy.Y] = belowDirectly;
//                        belowDirectly.X = x;
//                        belowDirectly.Y = y;
//                        Destroy(belowDirectly.gameObject);
//                        createCandy(currentX, currentY, CandyType.EMPTY);
                        
                        currentCandy.CandyMoved.MoveDown(currentMapX, currentMapY, fillTime); //动画和基础组件上更新位置
                        candies[belowDirectly.X, belowDirectly.Y] = currentCandy; //地图上位置
                        Destroy(belowDirectly.gameObject); //删除下方
                        createCandy(currentMapX, currentMapY, CandyType.EMPTY); //创建空位
                        
                        filledFinished = false; //填充未结束。还有空位
                    }
                    else 
                    {
                        //当前是可移动的对象， 下方不是空的时，检查左下和右下
                        
                        //左下
                        //向上查找 如果找到一个非空且不可移动的.那么就表示可以移动到改位置
                        CandyObject belowLeft = BelowLeft(currentMapX, currentMapY);
                        if (belowLeft != null && belowLeft.CandyType == CandyType.EMPTY && CanFindBarrierAbove(belowLeft.X, belowLeft.Y))
                        {
//                            LogicExchangeCandyObjectPositionInTiledMap(currentCandy, belowLeft);
//                            currentCandy.CandyMoved.Move(belowLeft.X, belowLeft.Y, fillTime); //动画上更新位置
//                            Destroy(belowLeft.gameObject);
//                            createCandy(x, y, CandyType.EMPTY);
                            
                            currentCandy.CandyMoved.MoveDownLeft(currentMapX, currentMapY, fillTime);
//                            candies[belowLeft.X, belowLeft.Y] = currentCandy;
                            Destroy(belowLeft.gameObject);
                            createCandy(currentMapX, currentMapY, CandyType.EMPTY);
                            filledFinished = false;
                        }
                        //右下
                        CandyObject belowRight = BelowRight(currentMapX, currentMapY);
                        //在左下位置 向上查找 如果找到一个非空且不可移动的.那么就表示可以移动到改位置
                        if (belowRight != null &&  belowRight.CandyType == CandyType.EMPTY && CanFindBarrierAbove(belowRight.X, belowRight.Y))
                        {
//                            LogicExchangeCandyObjectPositionInTiledMap(currentCandy, belowRight);
//                            currentCandy.CandyMoved.Move(belowRight.X, belowRight.Y, fillTime); //动画上更新位置
//                            Destroy(belowRight.gameObject);
//                            createCandy(x, y, CandyType.EMPTY);
                            
                            currentCandy.CandyMoved.MoveDownRight(currentMapX, currentMapY, fillTime);
//                            candies[belowRight.X, belowRight.Y] = currentCandy;
                            Destroy(belowRight.gameObject);
                            createCandy(currentMapX, currentMapY, CandyType.EMPTY);
                  
                            filledFinished = false;
                        }
 
                        
                        
//                        for (int down = -1; down <= 1; down++)
//                        {
//                            if (down != 0)
//                            {
//                                int downX = currentX + down;
//                                if (downX >= 0 && downX < xCol) //边界判断
//                                {
//                                    CandyObject downCandy = candies[downX, currentY + 1]; //左下和右下
//                                    if (downCandy.CandyType == CandyType.EMPTY)
//                                    {
//                                        bool canfill = true;
//                                        //向上
//                                        for (int aboveY = currentY; aboveY >= 0; aboveY--)
//                                        {
//                                            CandyObject candyAbove = candies[downX, aboveY];
//                                            if (candyAbove.HasMove())
//                                            {
//                                                break;
//                                            }
//                                            else if (candyAbove.CandyType != CandyType.EMPTY)
//                                            {
//                                                canfill = false;
//                                                break;
//                                            }
//                                        }
//
//                                        if (!canfill)
//                                        {
//                                            Destroy(downCandy.gameObject);
//                                            currentCandy.CandyMoved.Move(downX, currentY + 1, fillTime);
//                                            candies[downX, currentY + 1] = currentCandy;
//                                            createCandy(currentX, currentY, CandyType.EMPTY);
//                                            filledFinished = false;
//                                        }
//                                    }
//                                }
//                            }
//                        }
                    }

                }
				
            }

        }

        //最上排特殊情况
//		int y = -1;
        for (int x = 0; x < xCol; x++)
        {
            CandyObject emptyCandy = candies[x, 0];
			
            if (emptyCandy.CandyType == CandyType.EMPTY)
            {
                Destroy(emptyCandy.gameObject);
                GameObject o = Instantiate(candyPrefabDict[CandyType.NORMAL], CorrectPostion(x, -1), Quaternion.identity);
                //设置父对象
                o.transform.parent = transform;

                CandyObject newCandy = o.GetComponent<CandyObject>();
                candies[x, 0] = newCandy;
                newCandy.Init(x, -1, this, CandyType.NORMAL);
                newCandy.Color.SetColor((CandyCategory.ColorType)Random.Range(0, newCandy.Color.NumColors));
                newCandy.CandyMoved.MoveTo(x, 0, fillTime);
                filledFinished = false;
            }

        }
        return filledFinished;
    }


   


    public CandyObject createCandy(int x, int y, CandyType type)
    {
        GameObject candy = Instantiate(candyPrefabDict[type], CorrectPostion(x, y), Quaternion.identity);
		
        //设置父对象
        candy.transform.parent = transform;
		
        //放入二维数组
        candies[x, y]  = candy.transform.GetComponent<CandyObject>();
        candies[x, y].Init(x, y, this, type);

        return candies[x, y];
    }
    
    //正下方位置
    private CandyObject CurrentCandy(int x, int y)
    {
        if (0 <= x && x < xCol && 0 <= y && y<yRow)
        {
            return candies[x, y];
        }
        return null;
//        int belowX = x;
//        int belowY = y;
//        Debug.Log(belowX + " " + belowY);
//        if (belowX )
//        return candies[belowX, belowY];
    }

    
    //正下方位置
    private CandyObject BelowDirectly(int x, int y)
    {     
        int belowX = x;
        int belowY = y + 1;
        Debug.Log(belowX + " " + belowY);
        return CurrentCandy(belowX, belowY);
    }
    
    //右下
    private CandyObject BelowRight(int x, int y)
    {
        int belowX = x + 1;
        int belowY = y + 1;
        Debug.Log(belowX + " " + belowY);
        return CurrentCandy(belowX, belowY);
//        return x >= xCol || y >= yRow ? null : candies[belowX, belowY];
//        int x = o.X + 1;
//        int y = o.Y + 1;
//        Debug.Log(x + " " + y);
//        return x >= xCol || y >= yRow ? null : candies[x, y];
    }

    //左下
    private CandyObject BelowLeft(int x, int y)
    {
        int belowX = x - 1;
        int belowY = y + 1;
        Debug.Log(belowX + " " + belowY);
        return CurrentCandy(belowX, belowY);
//        return x < 0 || y >= yRow ? null : candies[belowX, belowY];
//        int x = o.X - 1;
//        int y = o.Y + 1;
//        return x < 0 || y >= yRow ? null : candies[x, y];
    }


    
    //从该坐标 向上查找,找到一个Barrier 阻挡类型的
    private bool CanFindBarrierAbove(int x, int y)
    {
        for (int aboveY = y; aboveY >= 0; aboveY--)
        {
            CandyObject candyAbove = candies[x, aboveY];
            if (candyAbove.HasMove())
            {
                return false;
            }
            
            if (candyAbove.CandyType == CandyType.BARRIER)
            {
                return true;
            }
        }

        return false;
    }
    
    
    public Vector3 CorrectPostion(int x, int y) {
        return new Vector3(this.transform.position.x - this.xCol * 0.5f + x, this.transform.position.y + yRow * 0.5f - y);
    }

    private bool IsNeighbour(CandyObject o1, CandyObject o2)
    {
        return o1.X == o2.X && Mathf.Abs(o1.Y - o2.Y) == 1 ||
               o1.Y == o2.Y && Mathf.Abs(o1.X - o2.X) == 1;
    }

    public void UpdateCandyPositionInMap(CandyObject o)
    {
        candies[o.X, o.Y] = o;
    }
    
    private void ExchangeCandyObjectPosition(CandyObject o1, CandyObject o2)
    {
        if (o1.HasMove() && o2.HasMove())
        {
//            LogicExchangeCandyObjectPositionInTiledMap(o1, o2);
//        逻辑上当前位置和正下方的交换位置 地图二维数组
            candies[o1.X, o1.Y] = o2;
            candies[o2.X, o2.Y] = o1;

            int tempX = o1.X;
            int tempY = o1.Y;
            
            o1.CandyMoved.MoveTo(o2.X, o2.Y, fillTime);
            
            o2.CandyMoved.MoveTo(tempX, tempY, fillTime);

            //动画上更新位置
//        if (o1.HasMove())
//        {
//        }
//        else
//        {
//            o1.transform.position = CorrectPostion(o2.X, o2.Y);
//
//        }

//        if (o2.HasMove()){
//        }
//        else
//        {
//            o1.transform.position = CorrectPostion(tempX, tempY);
//        }
//            

        }
    }

//逻辑移动 在 位置数组中移动
    private void LogicExchangeCandyObjectPositionInTiledMap(CandyObject o1, CandyObject o2)
    {
        candies[o1.X, o1.Y] = o2;
        candies[o2.X, o2.Y] = o1;
        
//        int tempX = o1.X;
//        int tempY = o1.Y;
//        o1.X = o2.X;
//        o1.Y = o2.Y;
//        o2.X = tempX;
//        o2.Y = tempY;



    }

    public void PressCandy(CandyObject o)
    {
        pressedCandy = o;
    }

    public void EnteredCandy(CandyObject o)
    {
        enteredCandy = o; 
    }

    public void ReleaseCandy()
    {
        if (IsNeighbour(pressedCandy, enteredCandy))
        {
            ExchangeCandyObjectPosition(pressedCandy, enteredCandy);
        }
    }
}