using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
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
		while (fill())
		{
			yield return new WaitForSeconds(fillTime);
		}
	}

	//分步填充
	public bool fill()
	{
		//单次填充是否完成
		bool filledNotFinished = false;
		for (int y = yRow-2; y >= 0; y--)
		{
			for (int x = 0; x < xCol; x++)
			{
				//当前元素位置的对象
				CandyObject candy = candies[x, y];
				if (candy.HasMove())
				{
					CandyObject below = candies[x, y + 1];

					if ( below.CandyType == CandyType.EMPTY)
					{
						candy.CandyMoved.Move(x, y+1, fillTime);
						candies[x, y + 1] = candy;
						createCandy(x, y, CandyType.EMPTY);
						filledNotFinished = true;
					}
				}
				
			}
		}
		
		//最上排特殊情况
//		int y = -1;
		for (int x = 0; x < xCol; x++)
		{
			CandyObject candy = candies[x, 0];
			
			if (candy.CandyType == CandyType.EMPTY)
			{
				GameObject o = Instantiate(candyPrefabDict[CandyType.NORMAL], CorrectPostion(x, -1), Quaternion.identity);
				//设置父对象
				o.transform.parent = transform;

				CandyObject newCandy = o.GetComponent<CandyObject>();
				candies[x, 0] = newCandy;
				newCandy.Init(x, -1, this, CandyType.NORMAL);
				newCandy.CandyMoved.Move(x, 0, fillTime);
				newCandy.Color.SetColor((CandyCategory.ColorType)Random.Range(0, newCandy.Color.NumColors));
				filledNotFinished = true;
				
//				candies[x, 0] = o.GetComponent<CandyObject>();
//				candies[x, 0].Init(x, -1, this, CandyType.NORMAL);
//				candies[x, 0].CandyMoved.Move(x, 0);
//				candies[x, 0].Color.SetColor((CandyCategory.ColorType)Random.Range(0, candies[x, 0].Color.NumColors));
//				filledNotFinished = true;
			}

		}

		return filledNotFinished;
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

//		if (candies[x, y].HasMove())
//		{
//			candies[x, y].CandyMoved.Move(x, y);
//		}
//
//		if (candies[x, y].HasColor())
//		{
//			candies[x, y].Color.SetColor((CandyCategory.ColorType)(Random.Range(0, candies[x, y].Color.NumColors)));
//		}
		

	}
    public Vector3 CorrectPostion(int x, int y) {
        return new Vector3(this.transform.position.x - this.xCol * 0.5f + x, this.transform.position.y + yRow * 0.5f - y);

    }
}
