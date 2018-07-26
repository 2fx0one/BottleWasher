﻿using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;

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
        EMPTY_SLOT,
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
//        candiesInSky = new CandyObject[xCol];  // 天空中一排的位置
        for (int x = 0; x < xCol; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                CreateEmptyCandy(x, y);
            }
			
        }

        for (int i = 1; i < 9; i++) 
        {
            CreateBarrierCandy(i, 4);
            
        }
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
            Debug.Log(" ==== needRefill ==== " + needRefill);
            
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
        foreach (CandyObject current in candiesInMap)
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
        Debug.Log("ClearAllMatchedCandies " + needRefill);
        return needRefill;
    }
    

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

            return tempBoomList;
        }

        return null;
    }


    public bool ClearCandy(CandyObject candy)
    {
        if (candy.HasClear() && !candy.Clear.IsClearing)
        {
            candy.Clear.Clear();
            CreateEmptyCandy(candy.X, candy.Y);
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
//        yRow
        CandyObject skyCandy = CreateCandy(x, yRow, CandyType.NORMAL);
        
        //最上层的天空中,需要随机设置颜色 且不要放到地图数组中! 后续让他移动进数组
        ColorType colorType = (ColorType) Random.Range(0, this.NumColors);
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
        Debug.Log("x = " + x + "  y=" + y);
        candiesInMap[x, y] = barrierCandy;
        return barrierCandy;
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
        return new Vector3(this.transform.position.x - this.xCol * 0.5f + x, this.transform.position.y - yRow * 0.5f + y);
    }

    private bool IsNeighbour(CandyObject o1, CandyObject o2)
    {
        return o1.X == o2.X && Mathf.Abs(o1.Y - o2.Y) == 1 ||
               o1.Y == o2.Y && Mathf.Abs(o1.X - o2.X) == 1;
    }

    public void UpdateCandyPositionInMap(CandyObject o, int x, int y)
    {
        candiesInMap[x, y] = o;
        o.X = x;
        o.Y = y;
    }
    
    private void ExchangeCandyObjectPosition(CandyObject c1, CandyObject c2)
    {
        if (c1.HasMove() && c2.HasMove())
        {


            int c1x = c1.X;
            int c1y = c1.Y;
            
            int c2x = c2.X;
            int c2y = c2.Y;
            
            c1.Movement.MoveTo(c2x, c2y, fillTime);  
            c2.Movement.MoveTo(c1x, c1y, fillTime);
            
            if (MatchCandies(c1) != null || MatchCandies(c2) != null) //移动后可以触发消除
            {
                ClearAllMatchedCandies();
                StartCoroutine(FillAll());
            }
            else
            {
                //无法触发消除
//                c2.Movement.MoveTo(c2x, c2y, fillTime);  
//                c1.Movement.MoveTo(c1x, c1y, fillTime);
                
            }

        }
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