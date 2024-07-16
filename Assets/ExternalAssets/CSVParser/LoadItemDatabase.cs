using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadItemDatabase : MonoBehaviour
{

    public TextAsset dbFile;

    public List<string> TypeNameList = new List<string>();

    private static LoadItemDatabase _instance;
    public static LoadItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LoadItemDatabase>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(LoadItemDatabase).ToString());
                    _instance = singletonObject.AddComponent<LoadItemDatabase>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadDb(dbFile);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public class ItemData
    {
        public int GlobalID;
        public int CategoryID;
        public string CategoryName;
        public int TypeID;
        public string TypeName;
        public string EquipCategory;
        //public IntVector2 Size;
        public Sprite Icon;
    }

    public List<ItemData> dbList = new List<ItemData>();

    private void LoadDb(TextAsset csvFile)
    {
        string[][] grid = CsvReadWrite.LoadTextFile(csvFile);
        for (int i = 1; i < grid.Length; i++)
        {
            if (grid[i].Length == 0 || string.IsNullOrWhiteSpace(grid[i][0]))
            {
                // 빈 줄 또는 첫 번째 셀이 빈 경우 무시
                continue;
            }

            ItemData row = new ItemData();
            row.GlobalID = Int32.Parse(grid[i][0]);
            row.CategoryID = Int32.Parse(grid[i][1]);
            row.CategoryName = grid[i][2];
            row.TypeID = Int32.Parse(grid[i][3]);
            row.TypeName = grid[i][4];
            TypeNameList.Add(row.TypeName);
            //row.Size = new IntVector2(Int32.Parse(grid[i][5]), Int32.Parse(grid[i][6]));
            row.Icon = Resources.Load<Sprite>("Images/Items/" + grid[i][2] + "/" + grid[i][4]); // 아이콘 로드
            row.EquipCategory = grid[i][8];
            dbList.Add(row);
        }
    }
}
