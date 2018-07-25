using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
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


    public int xCol;
    public int yRow;

    public GameObject gridPrefab;
    
    //糖果颜色
    public enum ColorType
    {
        YELLOW,
        PURPLE,
        RED,
        BLUE,
        GREEN,
        PINK,
        ANY,
        COUNT
    }

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    }

    public ColorSprite[] colorSprites; //unity 面板拖放
    
    public Dictionary<ColorType, Sprite> colorSpriteDict;
    public int NumColors
    {
        get { return colorSprites.Length; }
    }
    //===============================

    //糖果状态
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

    private void Awake()
    {
        _instance = this;
        sameCandyList = new List<CandyObject> ();
        boomList = new List<CandyObject> ();
    }

    void InitGame()
    {
        colorSpriteDict = new Dictionary<ColorType, Sprite>();
        for (int i = 0; i < colorSprites.Length; i++)
        {
            if (!colorSpriteDict.ContainsKey(colorSprites[i].color))
            {
                colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
            }
        }
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
        candies = new CandyObject[xCol, yRow+1];  //+1 天空中一排的位置
        for (int x = 0; x < xCol; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                CreateCandy(x, y, CandyType.EMPTY);
            }
			
        }
        
        CreateCandy(0, 0, CandyType.BARRIER);
        CreateCandy(9, 10, CandyType.BARRIER);
        
//        CreateCandy(0, 4, CandyType.BARRIER);
//        CreateCandy(1, 4, CandyType.BARRIER);
//        CreateCandy(2, 4, CandyType.BARRIER);
//        CreateCandy(3, 4, CandyType.BARRIER);
//        CreateCandy(4, 4, CandyType.BARRIER);
//        CreateCandy(5, 4, CandyType.BARRIER);
//        CreateCandy(6, 4, CandyType.BARRIER);
//        CreateCandy(7, 4, CandyType.BARRIER);
//        CreateCandy(8, 4, CandyType.BARRIER);
//        createCandy(9, 4, CandyType.BARRIER);
    }
    void Start () 
    {

        InitGame();
        StartCoroutine(FillAll());
        

    }


    public IEnumerator FillAll()
    {
        bool needRefill = true;
        while (needRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (!FillFinished())
            {
                yield return new WaitForSeconds(fillTime);
            }
            yield return new WaitForSeconds(fillTime);
            needRefill = ClearAllMatchedCandies();
            
        }
        Debug.Log("FillAll");
//        
        

        

    }

    //相同candy列表
    public List<CandyObject> sameCandyList;
    //要消除的candy列表
    public List<CandyObject> boomList;
    
    //三个相同的检测 并放入消除列表
    public bool ClearAllMatchedCandies()
    {
        bool needRefill = false;
        foreach (CandyObject current in candies)
        {
            List<CandyObject> matchCandies = MatchCandies(current);
            if (matchCandies != null)
            {
                foreach (var matchCandy in matchCandies)
                {
                    if (ClearCandy(matchCandy))
                    {
                        needRefill = true;
                    }
                }
            }
        }
        return needRefill;
    }

    
//    public void BoomAllSameCandy()
//    {
//        
//        //
//        foreach (CandyObject candy in candies)
//        {
////            sameCandyList.Clear();
////            boomList.Clear();
////            Debug.Log("x= " + candy.X + "  y= " + candy.Y);
////            FillSameItemsList(candy);
////            MatchCandies(candy);
////            Debug.Log(sameCandyList.Count);
////            if (boomList.Count > 0)
////            {
////                foreach (var o in boomList)
////                {
////                    Destroy(o.gameObject);
////                }
////            }
//        }
//    }
//    
    //填充相同Item列表
    public void FindSameCandyList(CandyObject current)
    {
        if (this.sameCandyList.Contains(current))
        {
            return;
        }
        
        this.sameCandyList.Add(current);
        CandyObject[] tempList = 
            {UpCandy(current), DownCandy(current), RightCandy(current), LeftCandy(current) };
//        Debug.Log(" Length = " + tempList.Length);
        foreach (CandyObject neighour in tempList)
        {
//            if (neighour != null)
//            {
////                Debug.Log("a = " + o.Category + "  b= " + current.Category);
//            }
            if (neighour != null && current.HasCategroy() && neighour.HasCategroy() && neighour.Category.Color.Equals(current.Category.Color))
            {
                FindSameCandyList(neighour);
            }
        }
    }

    //匹配方法
    public List<CandyObject> MatchCandies(CandyObject current)
    {
        this.sameCandyList.Clear();
        this.boomList.Clear();
//            Debug.Log("x= " + candy.X + "  y= " + candy.Y);
        FindSameCandyList(current);
        //计数器
//        int rowCount = 0;
//        int columnCount = 0;
        //临时列表
        List<CandyObject> rowTempList = new List<CandyObject> ();
        List<CandyObject> columnTempList = new List<CandyObject> ();

        int currentX = current.X;
        int currentY = current.Y;
        foreach (CandyObject sameCandy in this.sameCandyList) //颜色相同 包含自己
        {
            //如果在同一行 
            if (currentY == sameCandy.Y)
            {
                rowTempList.Add(sameCandy);
            }
            
            //同一列
            if (currentX == sameCandy.X)
            {
                columnTempList.Add(sameCandy);
            }
        }
        
        //是否有水平消除
        bool horizontalBoom = false;
        if (rowTempList.Count >= 3)
        {
            this.boomList.AddRange(rowTempList);
            horizontalBoom = true;
        }

        if (columnTempList.Count >= 3)
        {
            if (horizontalBoom)
            {
                boomList.Remove(current);
            }
            this.boomList.AddRange(columnTempList);
        }

        if (boomList.Count != 0)
        {
            //创建临时的BoomList
            List<CandyObject> tempBoomList = new List<CandyObject> ();
            //转移到临时列表
            tempBoomList.AddRange (boomList);
            //TODO 开启协同 删除

//            StartCoroutine(DoBoomList(tempBoomList));
//            foreach (var boomCandy in boomList)
//            {
//                ClearCandy(boomCandy);
//            }
            return tempBoomList;
        }

        return null;
    }


    public bool ClearCandy(CandyObject candy)
    {
        if (candy.HasClear() && !candy.Clear.IsClearing)
        {
            candy.Clear.Clear();
            CreateCandy(candy.X, candy.Y, CandyType.EMPTY);
            return true;
        }

        return false;
    }

//    IEnumerator DoBoomList(List<CandyObject> tempBoomList)
//    {
//        yield return new WaitForSeconds (0.5f);
//        foreach (var boomCandy in tempBoomList)
//        {
//            Destroy(boomCandy.gameObject);
//        }   
//        yield return new WaitForSeconds (0.38f);
//        FillFinished();
//        Debug.Log("FillFinished() ");
////        while (!FillFinished()) ;
////        StartCoroutine(drop());
//    }
//
//    private IEnumerator drop()
//    {
//        while (!FillFinished())
//        {
//            yield return new WaitForSeconds(fillTime);
//        }
//    }

    //分步填充
    public bool FillFinished()
    {
        
        //单次填充是否完成
        bool filledFinished = true;
        
        //地图上 从下往上 倒数第二行开始 
        for (int currentMapY = 1; currentMapY < yRow; currentMapY++) //垂直方向 从下到上
        {
            bool rightCandyHasMoved = false; //水平方向上, 右边的已经移动过的标记
            for (int currentMapX = xCol-1; currentMapX >= 0; currentMapX--) // 水平方向  从右向左扫描 为了向右边推进!
            {
                Debug.Log("x = " +  currentMapX + "  y=" + currentMapY);
                //当前元素位置的对象
                CandyObject currentCandy = CurrentCandyInMap(currentMapX, currentMapY);
                //当前必须可以移动
                if (currentCandy.HasMove())
                {
                    // Debug.Log("========currentCandy.HasMove()======== currentCandy.x = " + currentCandy.X + "  currentCandy.Y = " + currentCandy.Y);
                    //正下方 必须是空的.才能移动过去
                    CandyObject belowDirectly = DownCandy(currentCandy);
                    if (belowDirectly != null && belowDirectly.CandyType == CandyType.EMPTY) //正下方 是空的
//                    if (BelowDirectly(currentMapX, currentMapY))
                    {
                        currentCandy.CandyMoved.MoveToCandyAndReplace(belowDirectly, fillTime); //动画和基础组件上更新位置 且会覆盖原有位置
                        CreateEmptyCandy(currentMapX, currentMapY); //创建空位

                        filledFinished = false; //填充未结束。还有空位
                    }
                    else
                    {
                        
                        CandyObject[] tempList = 
                        {
                            RightCandy(currentCandy), //右边 
                            DownRightCandy(currentCandy),  //右下
                            //LeftCandy(currentCandy),  //左边 不处理左边 来回跳跃
                            DownLeftCandy(currentCandy)  //左下
                        };

                        foreach (var candy in tempList)
                        {
                            if (CanFindBarrierAbove(candy))
                            {
                                currentCandy.CandyMoved.MoveToCandyAndReplace(candy, fillTime);
                                CreateEmptyCandy(currentMapX, currentMapY);

                                filledFinished = false;
                                break;
                            }
                            
                        }
                        
                        
                        //下方不是空的时，
                        //检查左下和右下 向上查找 如果找到一个非空且不可移动的.那么就表示可以移动到改位置

                        //右边 
//                        CandyObject right = RightCandy(currentCandy);
//                    
//                        if (CanFindBarrierAbove(right))
//                        {
////                            Debug.Log("right  x= " + right.X + "  y=" + right.Y);
//                            currentCandy.CandyMoved.MoveToCandyAndReplace(right, fillTime);
//                            CreateEmptyCandy(currentMapX, currentMapY);
//
//                            filledFinished = false;
//                            rightCandyHasMoved = true;
//                            continue;
//                        }
//                        
//                        //右下
//                        
//                        CandyObject belowRight = DownRightCandy(currentCandy);
//                        if (CanFindBarrierAbove(belowRight))
//                        {
//                            currentCandy.CandyMoved.MoveToCandyAndReplace(belowRight, fillTime);
//                            CreateEmptyCandy(currentMapX, currentMapY);
//
//                            filledFinished = false;
//                            
//                            continue;
//                        }
//                        //!!  bug:来回左右移动!  向左边移动后, 右边空位 但是遍历方向是从右向左. 循环无法结束.   {从右向左} 与 {左移动} 冲突 故而注释
//                        //左边
//                        CandyObject left = LeftCandy(currentCandy);
//                        if (!rightCandyHasMoved && CanFindBarrierAbove(left))
//                        {
////                            if (left.X == 2 && left.Y == 5)
////                            {
////                                Debug.Log("left xxx");
////                            }
//                            currentCandy.CandyMoved.MoveToCandyAndReplace(left, fillTime);
//                            CreateEmptyCandy(currentMapX, currentMapY);
//                            
//                            filledFinished = false;
//                            break; //必须break 否则会 来回左右移动!
//                        }
//                    
//                        //左下
//                        CandyObject belowLeft = DownLeftCandy(currentCandy);
//                        if (CanFindBarrierAbove(belowLeft))
//                        {
//                            currentCandy.CandyMoved.MoveToCandyAndReplace(belowLeft, fillTime);
//                            CreateEmptyCandy(currentMapX, currentMapY);
//
//                            filledFinished = false;
//                            continue;
//                        }

                    

                    
                    }
                }
            }
        }

        //地图内的最上排特殊情况
//		int y = -1;
        for (int x = 0; x < xCol; x++)
        {
            int y = yRow-1; //yRow 表示天空 那一层
            CandyObject emptyCandy = candies[x, y];
			
            if (emptyCandy.CandyType == CandyType.EMPTY)
            {
//                Destroy(emptyCandy.gameObject);
                CandyObject fall = CreateCandy(x, yRow, CandyType.NORMAL); //yRow 表示天空 那一层
                fall.CandyMoved.MoveTo(x, y, fillTime);
//                fall.CandyMoved.MoveToCandyAndReplace(emptyCandy, fillTime);
               
                filledFinished = false;
            }
        }
        return filledFinished;
    }


    public CandyObject CreateEmptyCandy(int x, int y)
    {
        return CreateCandy(x, y, CandyType.EMPTY);
    }

    public CandyObject CreateCandy(int x, int y, CandyType type)
    {
//        if (type == CandyType.BARRIER) //初始化的时候
//        {
//            
//        }
        GameObject candy = Instantiate(candyPrefabDict[type], CorrectPostion(x, y), Quaternion.identity);
        //设置父对象
        candy.transform.parent = transform;
        CandyObject create = candy.transform.GetComponent<CandyObject>();
        create.Init(x, y, this, type);
        
        //放入二维数组
        
        //最上层的天空中,需要随机设置颜色 且不要放到地图数组中!
        if (y == yRow)
        {
            ColorType colorType = (ColorType) Random.Range(0, this.NumColors);
//            colorSpriteDict[colorType]
            create.Category.SetColor(colorType, colorSpriteDict[colorType]);
        }
        else
        {
            //放到数组中 原来的需要删除了
//            CandyObject old = candies[x, y];
//            if (old != null)
//            {
//                Destroy(old.gameObject);
//            }
//            Destroy(candies[x, y].gameObject);
            candies[x, y] = create;
        }

        return create;


    }
    
    //当前地图位置中
    private CandyObject CurrentCandyInMap(int x, int y)
    {
        if (0 <= x && x < xCol && 0 <= y && y<yRow)
        {
            return candies[x, y];
        }
        return null;
    }

    
    //上
    private CandyObject UpCandy(CandyObject o)
    {
        return CurrentCandyInMap(o.X, o.Y + 1);
    }
    
    //下
    private CandyObject DownCandy(CandyObject o)
    {     
        return CurrentCandyInMap(o.X, o.Y - 1);
    }
  
    //左
    private CandyObject LeftCandy(CandyObject o)
    {
        return CurrentCandyInMap(o.X-1, o.Y);
    }
    //右
    private CandyObject RightCandy(CandyObject o)
    {
        return CurrentCandyInMap(o.X+1, o.Y);
    }
    
    
    //左下
    private CandyObject DownLeftCandy(CandyObject o)
    {
        return CurrentCandyInMap(o.X-1, o.Y-1);
    }
    
    //右下
    private CandyObject DownRightCandy(CandyObject o)
    {
        return CurrentCandyInMap(o.X+1, o.Y-1);
    }


    //从该糖果坐标 向上查找,找到一个Barrier 阻挡类型的
    private bool CanFindBarrierAbove(CandyObject currentCandy)
    {
        if (currentCandy != null && currentCandy.CandyType == CandyType.EMPTY)
        {
            int x = currentCandy.X;
            int y = currentCandy.Y;
            for (int aboveY = 0; aboveY < y; aboveY++)
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
        return new Vector3(this.transform.position.x - this.xCol * 0.5f + x, this.transform.position.y - yRow * 0.5f + y);
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
            
            if (MatchCandies(o1) != null && MatchCandies(o2) != null) //移动时可以触发消除
            {
                
            }
            else
            {
                //无法触发消除
                
            }
            
            ClearAllMatchedCandies();
            StartCoroutine(FillAll());

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