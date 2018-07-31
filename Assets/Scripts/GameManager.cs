using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
    private static GameManager _inst;
    public static GameManager Inst
    {
        get
        {
            return _inst;
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

    [Serializable]
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
        EMPTY_SLOT,
        NORMAL,
        BARRIER,
        ROW_CLEAR,
        COL_CLEAR,
        RAINBOWCANDY,
        COUNT
    }

    public Dictionary<CandyType, GameObject> candyPrefabDict;

   
    [Serializable]
    public struct CandyPrefab
    {
        public CandyType candyType;
        public GameObject prefab;
    }

    public CandyPrefab[] CandyPrefabs;

    private CandyObject[,] candiesInMap; //二维数组 地图中的糖果

    public float fillTime;

    private CandyObject pressedCandy;
    private CandyObject enteredCandy;
    // Use this for initialization

    private void Awake()
    {
        _inst = this;
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
        candiesInMap = new CandyObject[xCol, yRow];
        
        for (int x = 0; x < xCol; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                CreateEmptyCandy(x, y);
            }
        }

//        for (int i = 1; i < 9; i++) 
//        {
//            CreateBarrierCandy(i, 4);
//            
//        }
//        CreateBarrierCandy(9, 4);
        
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
//        yield return new WaitForSeconds(fillTime);
//        yield return new WaitForSeconds(fillTime);
//        yield return new WaitForSeconds(fillTime);
        bool needRefill = true;
        while (needRefill)
        {
            yield return new WaitForSeconds(fillTime*2);
            while (!FillFinished())
            {
                yield return new WaitForSeconds(fillTime);
            }
            yield return new WaitForSeconds(fillTime);
            needRefill = ClearAllMatchedCandies();
//            Debug.Log(" ==== needRefill ==== " + needRefill);
            yield return new WaitForSeconds(fillTime);
            
        }
        Debug.Log("FillAll");
    }

    //相同candy列表
    public List<CandyObject> sameCandyList;
    //要消除的candy列表
    public List<CandyObject> boomList;
    
    //填充完成 三个相同的检测 并放入消除列表
    public bool ClearAllMatchedCandies()
    {
//        List<CandyObject> filterList = new List<CandyObject>();
        bool needRefill = false;
        for (int y = yRow-1; y >=0; y--)
        {
            for (int x = 0; x < xCol; x++)
            {
                CandyObject current = candiesInMap[x, y];
                
//                if (filterList.Contains(current)) break;
                
                List<CandyObject> matchCandies = MatchCandies(current);
                if (matchCandies != null)
                {
                    if (matchCandies.Count == 4 || matchCandies.Count == 3)
                    {
                        Debug.Log(" matchCandies.Count ~~~~~~ 11 = " + matchCandies.Count);
                        matchCandies.Remove(current);
                        Debug.Log(" matchCandies.Count ~~~~~~ 22 = " + matchCandies.Count);
                    }
//                    filterList.AddRange(matchCandies);
                    foreach (CandyObject matchCandy in matchCandies)
                    {
                        Debug.Log(" foreach  ======= ");
                        if (ClearCandy(matchCandy))
                        {
                            needRefill = true;
                        }
                    }
                }

            }
            
        }
//        foreach (CandyObject current in candiesInMap)
//        {
////            needRefill = CleanMatchedCandiesIn(current);
//            List<CandyObject> matchCandies = MatchCandies(current);
//            if (matchCandies != null)
//            {
//                foreach (CandyObject matchCandy in matchCandies)
//                {
//                    if (ClearCandy(matchCandy))
//                    {
//                        needRefill = true;
//                    }
//                }
//            }
//        }
        Debug.Log("ClearAllMatchedCandies " + needRefill);
        return needRefill;
    }

    public bool CleanMatchedCandiesIn(CandyObject current)
    {
        bool needRefill = false;
        List<CandyObject> matchCandies = MatchCandies(current);
        if (matchCandies != null)
        {
            foreach (CandyObject matchCandy in matchCandies)
            {
                if (ClearCandy(matchCandy))
                {
                    needRefill = true;
                }
            }
        }

        return needRefill;
    }
    
    

    //填充相同Item列表
    public void FindSameCandyList(CandyObject current)
    {
        if (!sameCandyList.Contains(current))
        {
            sameCandyList.Add(current);
            CandyObject[] aroundCandies = {UpCandy(current), DownCandy(current), RightCandy(current), LeftCandy(current) };
            foreach (CandyObject around in aroundCandies)
            {
                if (around != null && current.HasCategroy() && around.HasCategroy() && around.Category.Color.Equals(current.Category.Color))
                {
                    FindSameCandyList(around);
                }
            }
        }
    }

    //匹配方法 非空表示 可以触发消除的
    public List<CandyObject> MatchCandies(CandyObject current)
    {
        sameCandyList.Clear();
        boomList.Clear();
//            Debug.Log("x= " + candy.X + "  y= " + candy.Y);
        FindSameCandyList(current);
        //计数器
//        int rowCount = 0;
//        int columnCount = 0;
        //临时列表
        List<CandyObject> rowTempList = new List<CandyObject> (); //一行
        List<CandyObject> columnTempList = new List<CandyObject> (); //一列

        int currentX = current.X;
        int currentY = current.Y;
        foreach (CandyObject sameCandy in sameCandyList) //颜色相同 包含自己, 且是以自己为基准的
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

        if (rowTempList.Count == 5 )
        {
            //直线5个 变彩虹
//            Debug.Log("直线5个 变彩虹");
            boomList.AddRange(rowTempList);
//            CreateRainbowCandy(current);
        }
        else if (columnTempList.Count == 5)
        {
            //直线5个 变彩虹
//            Debug.Log("直线5个 变彩虹");
            boomList.AddRange(columnTempList);
//            CreateRainbowCandy(current);
            
        }
        else if (rowTempList.Count >= 3 && columnTempList.Count >= 3)
        {
//            Debug.Log("横竖都是3~4个 L 和 T , 总共5~6个 炸弹");
            //横竖都是3~4个 L 和 T , 总共5~6个 炸弹
            boomList.AddRange(rowTempList);
            boomList.Remove(current); //重复交错的地方
            boomList.AddRange(columnTempList);
//            CreateRainbowCandy(current);
        }
        else if(rowTempList.Count == 4)
        {
            //直线4个 行火箭筒
//            Debug.Log("row 直线4个 行火箭筒 x="+current.X + "  y="+current.Y);
            boomList.AddRange(rowTempList);
            
//            boomList.Remove(current);
            
//            TransfromToRainbowCandy(current);
        }
        else if(columnTempList.Count == 4)
        {
            //直线4个 行火箭筒
//            Debug.Log("column 直线4个 行火箭筒 x="+current.X + "  y="+current.Y);
            boomList.AddRange(columnTempList);
            
//            boomList.Remove(current);
            
//            ColorType colorType = ColorType.YELLOW
//            current.Category.SetColor();
//            skyCandy.Category.SetColor(colorType, colorSpriteDict[colorType]);
//            TransfromToRainbowCandy(current);
        }
        else if (rowTempList.Count == 3)
        {
            //普通三个 
            boomList.AddRange(rowTempList);
        }
        else if ( columnTempList.Count == 3)
        {
            //普通三个   
            boomList.AddRange(columnTempList);
        }
        else
        {
            return null;
        }
//        }else if (rowTempList.Count == 2 && columnTempList.Count == 2 && sameCandyList.Count == 4)
//        {
            //飞机 逻辑复杂 暂时不做
//        }
        
        
//        bool horizontalBoom = false;
//        if (rowTempList.Count == 3)
//        {
//            boomList.AddRange(rowTempList);
//            horizontalBoom = true;
//        }
//
//        if (columnTempList.Count == 3)
//        {
//            if (horizontalBoom)
//            {
//                //当存在有水平消除 需要删除重复添加的对象.
//                boomList.Remove(current); 
//            }
//            boomList.AddRange(columnTempList);
//        }

        if (boomList.Count != 0)
        {
            //创建临时的BoomList
            List<CandyObject> tempBoomList = new List<CandyObject> ();
            //转移到临时列表
            tempBoomList.AddRange (boomList);

            return tempBoomList;
        }

        return null;
    }


    public bool ClearCandy(CandyObject candy)
    {
        if (candy.HasClear() && !candy.Clean.IsClearing)
        {
//            candiesInMap[candy.X, candy.Y] = null;
            CreateEmptyCandy(candy.X, candy.Y);
            candy.Clean.Cleanup(fillTime);
//            Debug.Log("Clear true");
            return true;
        }

        return false;
    }


    //分步填充
    public bool FillFinished()
    {
        
        //单次填充是否完成
        bool filledFinished = true;
        
        //地图上 从下往上 倒数第二行开始 
        for (int currentMapY = 1; currentMapY < yRow; currentMapY++) //垂直方向 从下到上
        {
//            bool rightCandyHasMoved = false; //水平方向上, 右边的已经移动过的标记
            for (int currentMapX = xCol-1; currentMapX >= 0; currentMapX--) // 水平方向  从右向左扫描 为了向右边推进!
            {
//                Debug.Log("x = " +  currentMapX + "  y=" + currentMapY);
                //当前元素位置的对象
                CandyObject currentCandy = CurrentCandyInMap(currentMapX, currentMapY);
                //当前必须可以移动
                if (currentCandy.HasMove())
                {
//                    Debug.Log("========currentCandy.HasMove()========   " + currentCandy.CandyType + " x = " + currentCandy.X + "  Y = " + currentCandy.Y);
                    //正下方 必须是空的.才能移动过去
                    CandyObject downCandy = DownCandy(currentCandy);
                    if (downCandy != null && downCandy.CandyType == CandyType.EMPTY_SLOT) //正下方 是空的
//                    if (BelowDirectly(currentMapX, currentMapY))
                    {
                        currentCandy.Movement.MoveToCandyAndReplace(downCandy, fillTime); //动画和基础组件上更新位置 且会覆盖原有位置
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
                                currentCandy.Movement.MoveToCandyAndReplace(candy, fillTime);
                                CreateEmptyCandy(currentMapX, currentMapY);

                                filledFinished = false;
                                break;
                            }
                            
                        }
                        
                        //下方不是空的时，
                        //检查左下和右下 向上查找 如果找到一个非空且不可移动的.那么就表示可以移动到改位置

//                        //右边 
//                        CandyObject right = RightCandy(currentCandy);
//                    
//                        if (CanFindBarrierAbove(right))
//                        {
////                            Debug.Log("right  x= " + right.X + "  y=" + right.Y);
//                            currentCandy.Movement.MoveToCandyAndReplace(right, fillTime);
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
//                            currentCandy.Movement.MoveToCandyAndReplace(belowRight, fillTime);
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
//                            currentCandy.Movement.MoveToCandyAndReplace(left, fillTime);
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
//                            currentCandy.Movement.MoveToCandyAndReplace(belowLeft, fillTime);
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
//            candiesInSky[x]
            //查看下方是否有空位.
            //    有的话就创建一个天空中的糖果 
            //    然后下落
            CandyObject topCandy = candiesInMap[x, yRow-1]; //最上面的糖果
            if (topCandy.CandyType == CandyType.EMPTY_SLOT) //如果是一个空槽 创建一个天空中的糖果 然后移动并替换下方
            {
                CandyObject skyCandy = CreateSkyCandy(x);
//                skyCandy.Movement.MoveTo(topCandy.X, topCandy.Y, fillTime);
                skyCandy.Movement.MoveToCandyAndReplace(topCandy, fillTime);
                filledFinished = false;
            }
        }
        return filledFinished;
    }


    //创建天空中的糖果 不需要放入数组
//    public CandyObject CreateSkyCandy(int x, int y)
    public CandyObject CreateSkyCandy(int x)
    {
//        yRow 不要放入数组
        CandyObject skyCandy = CreateCandy(x, yRow, CandyType.NORMAL);
        
        //最上层的天空中,需要随机设置颜色 且不要放到地图数组中! 后续让他移动进数组
        ColorType colorType = (ColorType) Random.Range(0, NumColors);
        skyCandy.Category.SetColor(colorType, colorSpriteDict[colorType]);
        
        return skyCandy;
    } 
    
    //创建空位槽 在地图上创建, 需要放入数组
    public CandyObject CreateEmptyCandy(int x, int y)
    {
        CandyObject emptyCandy = CreateCandy(x, y, CandyType.EMPTY_SLOT);
        candiesInMap[x, y] = emptyCandy;
        return emptyCandy;
    }
    
    //创建一个普通的糖果 在地图上创建, 需要放入数组
    public CandyObject CreateNormalCandy(int x, int y)
    {
        CandyObject normalCandy = CreateCandy(x, y, CandyType.NORMAL);
        candiesInMap[x, y] = normalCandy;
        return normalCandy;

    }
    
    //创建一个阻挡 在地图上创建, 需要放入数组
    public CandyObject CreateBarrierCandy(int x, int y)
    {
        //由于是在空白地图上创建阻挡, 之前位置上一定已经有空位槽类型的糖果了, 故需要删除之前的空位槽糖果!
        CandyObject empty = CurrentCandyInMap(x, y);
        if (empty != null)
        {
            Destroy(empty.gameObject);
        }
        
        CandyObject barrierCandy = CreateCandy(x, y, CandyType.BARRIER);
//        Debug.Log("x = " + x + "  y=" + y);
        candiesInMap[x, y] = barrierCandy;
        return barrierCandy;
    }
    
    
    //创建一个彩虹的糖果 在地图上创建, 需要放入数组
    public CandyObject TransfromToRainbowCandy(CandyObject current)
    {
        //替换这个位置的糖果
        int x = current.X;
        int y = current.Y;
//        current.Init(x, y, CandyType.RAINBOWCANDY);
//        current.Category.SetColor();
        
        Destroy(current);
        CandyObject rainbowCandy = CreateCandy(x, y, CandyType.RAINBOWCANDY);
        candiesInMap[x, y] = rainbowCandy;
        return current;
    }
    

    public CandyObject CreateCandy(int x, int y, CandyType type)
    {
        GameObject obj = Instantiate(candyPrefabDict[type], CorrectPostion(x, y), Quaternion.identity);
        //设置父对象
        obj.transform.parent = transform;
        CandyObject create = obj.transform.GetComponent<CandyObject>();
        create.Init(x, y, type);
        return create;


    }
    
    //当前地图位置中
    private CandyObject CurrentCandyInMap(int x, int y)
    {
        if (0<=x && x<xCol && 0<=y && y<yRow)
        {
            return candiesInMap[x, y];
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
        if (currentCandy != null && currentCandy.CandyType == CandyType.EMPTY_SLOT)
        {
//            int x = currentCandy.X;
//            int y = currentCandy.Y;
            for (int aboveY = currentCandy.Y; aboveY < yRow; aboveY++)
            {
                //向上遍历
                CandyObject candyAbove = CurrentCandyInMap(currentCandy.X, aboveY);// candiesInMap[x, aboveY];          
                if (candyAbove.CandyType == CandyType.BARRIER)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    
    public Vector3 CorrectPostion(int x, int y) {
        return new Vector3(transform.position.x - xCol * 0.5f + x, transform.position.y - yRow * 0.5f + y);
    }



    public void UpdateCandyPositionInMap(CandyObject o, int x, int y)
    {
        candiesInMap[x, y] = o;
        o.X = x;
        o.Y = y;
    }
    
    private void ExchangeCandyObjectPosition(CandyObject a, CandyObject b)
    {
        if (a.HasMove() && b.HasMove())
        {


            int ax = a.X;
            int ay = a.Y;
            
            int bx = b.X;
            int by = b.Y;
            
//            c1.Movement.MoveTo(c2x, c2y, fillTime);  
//            c2.Movement.MoveTo(c1x, c1y, fillTime);
//            逻辑地图坐标 和 基础组件坐标 更新
            UpdateCandyPositionInMap(a, bx, by);
            UpdateCandyPositionInMap(b, ax, by);

            List<CandyObject> aMatchCandies = MatchCandies(a);
            List<CandyObject> bMatchCandies = MatchCandies(b);
            
            if (aMatchCandies != null || bMatchCandies != null) //移动后可以触发消除
            {
                a.Movement.MoveTo(bx, by, fillTime);  
                b.Movement.MoveTo(ax, ay, fillTime);
                CleanMatchedCandiesIn(a);
                CleanMatchedCandiesIn(b);
                StartCoroutine(FillAll());
            }
            else
            {
                //逻辑上移回去!
//                c1.Movement.MoveTo(c1x, c1y, fillTime); 
//                c2.Movement.MoveTo(c2x, c2y, fillTime);
                UpdateCandyPositionInMap(a, ax, ay);
                UpdateCandyPositionInMap(b, bx, by);
                
                Debug.Log("动画上 移回去!");
                a.Movement.MoveToAndReturn(bx, by, fillTime);  
                b.Movement.MoveToAndReturn(ax, ay, fillTime);
            }
            
            pressedCandy = null;
            enteredCandy = null;
        }
    }


    public void PressCandy(CandyObject o)
    {
        pressedCandy = o;
    }

    public void EnteredCandy(CandyObject o)
    {
        if (pressedCandy != null)
        {
            enteredCandy = o;
            ReleaseCandy();

        }
    }

    public void ReleaseCandy()
    {
        //相邻 且 不同色
        if (IsNeighbour(pressedCandy, enteredCandy) && pressedCandy.Category.Color != enteredCandy.Category.Color)
        {
            ExchangeCandyObjectPosition(pressedCandy, enteredCandy);
        }
    }
    private bool IsNeighbour(CandyObject o1, CandyObject o2)
    {
        return o1.X == o2.X && Mathf.Abs(o1.Y - o2.Y) == 1 ||
               o1.Y == o2.Y && Mathf.Abs(o1.X - o2.X) == 1;
    }
}