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
            }
			
        }

//        Destroy(candies[0, 4].gameObject);
//        Destroy(candies[1, 4].gameObject);
//        Destroy(candies[2, 4].gameObject);
//        Destroy(candies[3, 4].gameObject);
//        Destroy(candies[4, 4].gameObject);
//        Destroy(candies[5, 4].gameObject);
//        Destroy(candies[6, 4].gameObject);
//        Destroy(candies[7, 4].gameObject);
//        Destroy(candies[8, 4].gameObject);
//        Destroy(candies[9, 4].gameObject);
        
//        createCandy(0, 4, CandyType.BARRIER);
        createCandy(1, 4, CandyType.BARRIER);
        createCandy(2, 4, CandyType.BARRIER);
        createCandy(3, 4, CandyType.BARRIER);
//        createCandy(4, 4, CandyType.BARRIER);
        createCandy(5, 4, CandyType.BARRIER);
        createCandy(6, 4, CandyType.BARRIER);
        createCandy(7, 4, CandyType.BARRIER);
        createCandy(8, 4, CandyType.BARRIER);
//        createCandy(9, 4, CandyType.BARRIER);

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
        
        //地图上 从下往上 倒数第二行开始 从右向左扫描 为了右边推进!
        
        for (int currentMapX = xCol-1; currentMapX >=0 ; currentMapX--) // 水平方向 从左到右
        {
            for (int currentMapY = yRow-2; currentMapY >= 0; currentMapY--) //垂直方向 从下到上
            {
                //当前元素位置的对象
                CandyObject currentCandy = CurrentCandy(currentMapX, currentMapY);
                //当前必须可以移动
                if (currentCandy.HasMove())
                {
//                    Debug.Log("========currentCandy.HasMove()======== currentCandy.x = " + currentCandy.X + "  currentCandy.Y = " + currentCandy.Y);
                    //正下方 必须是空的.才能移动过去
                    CandyObject belowDirectly = BelowDirectlyCandy(currentMapX, currentMapY);
                    if (belowDirectly != null && belowDirectly.CandyType == CandyType.EMPTY) //正下方 是空的
//                    if (BelowDirectly(currentMapX, currentMapY))
                    {
                        currentCandy.CandyMoved.MoveToCandyAndReplace(belowDirectly, fillTime); //动画和基础组件上更新位置 且会覆盖原有位置
                        createCandy(currentMapX, currentMapY, CandyType.EMPTY); //创建空位

                        filledFinished = false; //填充未结束。还有空位
                    }
                    else
                    {
                        //下方不是空的时，
                        //检查左下和右下 向上查找 如果找到一个非空且不可移动的.那么就表示可以移动到改位置

                        //右边 
                        CandyObject right = RightCandy(currentMapX, currentMapY);
                    
                        if (CanFindBarrierAbove(right))
                        {
//                            Debug.Log("right  x= " + right.X + "  y=" + right.Y);
                            currentCandy.CandyMoved.MoveToCandyAndReplace(right, fillTime);
                            createCandy(currentMapX, currentMapY, CandyType.EMPTY);

                            filledFinished = false;
                            continue;
//                            break;
                        }  
                        
//                        //左边
                        CandyObject left = LeftCandy(currentMapX, currentMapY);
                        if (CanFindBarrierAbove(left))
                        {
                            currentCandy.CandyMoved.MoveToCandyAndReplace(left, fillTime);
                            createCandy(currentMapX, currentMapY, CandyType.EMPTY);
                            
                            filledFinished = false;
                            continue;
                        }
                    
                        //左下
                        CandyObject belowLeft = BelowLeftCandy(currentMapX, currentMapY);
                        if (CanFindBarrierAbove(belowLeft))
                        {
                            currentCandy.CandyMoved.MoveToCandyAndReplace(belowLeft, fillTime);
                            createCandy(currentMapX, currentMapY, CandyType.EMPTY);

                            filledFinished = false;
                            continue;
                        }

                    
                        //右下
                        CandyObject belowRight = BelowRightCandy(currentMapX, currentMapY);
                        if (CanFindBarrierAbove(belowRight))
                        {
                            currentCandy.CandyMoved.MoveToCandyAndReplace(belowRight, fillTime);
                            createCandy(currentMapX, currentMapY, CandyType.EMPTY);

                            filledFinished = false;
                            continue;
                        }
                    
   
                    }
                }
                ///当前元素的正下方
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
                CandyObject fall = createCandy(x, -1, CandyType.NORMAL);
                fall.CandyMoved.MoveTo(x, 0, fillTime);
               
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
        CandyObject create = candy.transform.GetComponent<CandyObject>();
        create.Init(x, y, this, type);
        
        //放入二维数组
        
        //最上层的天空中,需要随机设置颜色 且不要放到地图数组中!
        if (y < 0) 
        {
            create.Color.SetColor((CandyCategory.ColorType) Random.Range(0, create.Color.NumColors));
        }
        else
        {
            //放到数组中 原来的需要删除了
//            CandyObject old = candies[x, y];
//            if (old != null)
//            {
//                Destroy(old.gameObject);
//            }
            candies[x, y] = create;
        }

        return create;


    }
    
    //当前地图位置中
    private CandyObject CurrentCandy(int x, int y)
    {
        if (0 <= x && x < xCol && 0 <= y && y<yRow)
        {
            return candies[x, y];
        }
        return null;
    }

    
    //正下方位置
    private CandyObject BelowDirectlyCandy(int x, int y)
    {     
        return CurrentCandy(x, y+1);
    }
  
    //左
    private CandyObject LeftCandy(int x, int y)
    {
        return CurrentCandy(x-1, y);
    }
    //右
    private CandyObject RightCandy(int x, int y)
    {
        return CurrentCandy(x+1, y);
    }
    
    //左下
    private CandyObject BelowLeftCandy(int x, int y)
    {
        return CurrentCandy(x-1, y+1);
    }
    
    //右下
    private CandyObject BelowRightCandy(int x, int y)
    {
        return CurrentCandy(x+1, y+1);
    }


    //从该糖果坐标 向上查找,找到一个Barrier 阻挡类型的
    private bool CanFindBarrierAbove(CandyObject currentCandy)
    {
        if (currentCandy != null && currentCandy.CandyType == CandyType.EMPTY)
        {
            int x = currentCandy.X;
            int y = currentCandy.Y;
            for (int aboveY = y; aboveY >= 0; aboveY--)
            {
                //向上遍历
                CandyObject candyAbove = candies[x, aboveY];          
                if (candyAbove.CandyType == CandyType.BARRIER)
                {
                    return true;
                }
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

    public void UpdateCandyPositionInMap(CandyObject o, int x, int y)
    {

//        Debug.Log("UpdateCandyPositionInMap x = " + x + "  y=" + y);
//        Debug.Log("UpdateCandyPositionInMap x = " + o.X + "  y=" + o.Y);
//        Destroy(candies[x, y].gameObject);
        candies[x, y] = o;
        o.X = x;
        o.Y = y;
    }
    
    private void ExchangeCandyObjectPosition(CandyObject o1, CandyObject o2)
    {
        if (o1.HasMove() && o2.HasMove())
        {
//            LogicExchangeCandyObjectPositionInTiledMap(o1, o2);
//        逻辑上当前位置和正下方的交换位置 地图二维数组
//            candies[o1.X, o1.Y] = o2;
//            candies[o2.X, o2.Y] = o1;

            int tempX = o1.X;
            int tempY = o1.Y;
            
            o1.CandyMoved.MoveTo(o2.X, o2.Y, fillTime);
            
            o2.CandyMoved.MoveTo(tempX, tempY, fillTime);
    
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