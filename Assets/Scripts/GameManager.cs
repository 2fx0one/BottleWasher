using System.Collections;
using System.Collections.Generic;
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
        ROW_CLEAN,
        COL_CLEAN,
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

	private CandyBase[,] candies; //二维数组

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
		candies = new CandyBase[xCol, yRow];
		for (int x = 0; x < xCol; x++)
		{
			for (int y = 0; y < yRow; y++)
			{
				GameObject candy = Instantiate(candyPrefabDict[CandyType.NORMAL], Vector3.zero, Quaternion.identity);
				candy.transform.SetParent(transform);
				candies[x, y]  = candy.transform.GetComponent<CandyBase>();
				candies[x, y].Init(x, y, this, CandyType.NORMAL);

				if (candies[x, y].HasMove())
				{
					Debug.Log("xxx");
					candies[x, y].CandyMoved.Move(x, y);
				}

				if (candies[x, y].HasColor())
				{
					candies[x, y].Color.SetColor((CandyCategory.ColorType)(Random.Range(0, candies[x, y].Color.NumColors)));
				}
			}
			
		}
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Vector3 CorrectPostion(int x, int y) {
        return new Vector3(this.transform.position.x - this.xCol * 0.5f + x, this.transform.position.y + yRow * 0.5f - y);

    }
}
