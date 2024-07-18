using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CSV ���� �а� List�� 
public class DBCharacterPC : MonoBehaviour
{
    #region CSV Set
    [SerializeField] private MainUI mainUI; 
    public TextAsset _playersInfoDbFile;
    public TextAsset _normalAttackDBFile;

    public List<string> TypeNameList = new List<string>();
    private static DBCharacterPC _instance;
    public static DBCharacterPC Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DBCharacterPC>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(DBCharacterPC).ToString());
                    _instance = singletonObject.AddComponent<DBCharacterPC>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }
    #endregion

    #region Player Set
    [SerializeField] private float _humanHp;
    [SerializeField] private float _wolfHp;
    public float HumanHp
    {
        get { return _humanHp; }
        set { _humanHp = value; }
    }
    public float WolfHp
    {
        get { return _wolfHp; }
        set { _wolfHp = value; }
    }
 
    #endregion
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        LoadPlayersInfoDb(_playersInfoDbFile);
        LoadNormalAttackDB(_normalAttackDBFile);
    }
    private void Start()
    {
        SetData();
        mainUI.SetInitialValues(HumanHp, WolfHp, 0); // �ʱⰪ ����, �ñر�� 0���� ����
    }
    public class ItemData
    {
        public int ID;
        public string Description;
        public string PairGroup;
        public string Type;
        public float Health;
        public float JmSpeed;
        public float AttackSpeed;
        public float BaseAttackDamage;
        public int NormalAttackID;
        public int SkillID;
    }

    public class NormalAttack
    {
        public int ID;
        public string Description;
        public float DamagePercet;
        public float SpecialMoveGaugeCharge;
        public float MaxSpecialMoveGaugeCharge;
    }

    public List<ItemData> dbList = new List<ItemData>();
    public List<NormalAttack> normalAttacks = new List<NormalAttack>();

    private void LoadPlayersInfoDb(TextAsset csvFile)
    {
        string[][] grid = CsvReadWrite.LoadTextFile(csvFile);
        for (int i = 1; i < grid.Length; i++)
        {
            if (grid[i].Length == 0 || string.IsNullOrWhiteSpace(grid[i][0]))
            {
                // �� �� �Ǵ� ù ��° ���� �� ��� ����
                continue;
            }

            ItemData row = new ItemData();
            row.ID = Int32.Parse(grid[i][0]);
            row.Description = grid[i][1];
            row.PairGroup = grid[i][2];
            row.Type = grid[i][3];
            row.Health = Int32.Parse(grid[i][4]);
            row.JmSpeed = Int32.Parse(grid[i][5]);
            row.AttackSpeed = float.Parse(grid[i][6]);
            row.BaseAttackDamage = Int32.Parse(grid[i][7]);
            row.NormalAttackID = Int32.Parse(grid[i][8]);
            row.SkillID = Int32.Parse(grid[i][9]);

            /*   row.Icon = Resources.Load<Sprite>("Images/Items/" + grid[i][2] + "/" + grid[i][4]); // ������ �ε�
               row.EquipCategory = grid[i][8];*/
            dbList.Add(row);
        }
    }

    private void LoadNormalAttackDB(TextAsset csvFile)
    {
        string[][] grid = CsvReadWrite.LoadTextFile(csvFile);
        for (int i = 1; i < grid.Length; i++)
        {
            if (grid[i].Length == 0 || string.IsNullOrWhiteSpace(grid[i][0]))
            {
                // �� �� �Ǵ� ù ��° ���� �� ��� ����
                continue;
            }

            NormalAttack row = new NormalAttack();
            row.ID = Int32.Parse(grid[i][0]);
            row.Description = grid[i][1];
            row.DamagePercet = float.Parse(grid[i][2]);
            row.SpecialMoveGaugeCharge = float.Parse(grid[i][6]);
            row.MaxSpecialMoveGaugeCharge = float.Parse(grid[i][7]);

            /*   row.Icon = Resources.Load<Sprite>("Images/Items/" + grid[i][2] + "/" + grid[i][4]); // ������ �ε�
               row.EquipCategory = grid[i][8];*/
            normalAttacks.Add(row);
        }
    }

    private void SetData() 
    {
        if (dbList.Count > 0)
        {
            HumanHp = dbList[0].Health;
            WolfHp = dbList[1].Health;
        }
    }

    // TEST
    public void HmTakeDamageButton()
    {
        HitDamageHuman(5); // Assuming you want to take 5 damage for example
    }    
    public void WolfTakeDamageButton()
    {
        HitDamageWolf(5); // Assuming you want to take 5 damage for example
    }
    public void AddSkillGaugeButton()
    {
        AddSkillGauge(5); // ���� ��� 5 ��ŭ ��ų ������ ����
    }
    // Human ���� �Ʒ� �Լ� �̱��� �ν����� ȣ�� ��Ű�� ü�� bar�� �ڵ����� ���� �� return �� Ȱ���ص� ���� �� �״� ��� ���
    public float HitDamageHuman(float damage)
    {
        HumanHp -= damage;
        mainUI._crtHpHuman = HumanHp;
        return HumanHp;
    }
    public float HitDamageWolf(float damage)
    {
        WolfHp -= damage;
        mainUI._crtHpWolf = WolfHp;
        return WolfHp;
    }
    // ���ο� ��ų ������ �߰� �޼ҵ�
    public void AddSkillGauge(float amount)
    {
        mainUI.AddSkillGauge(amount);
    }

    public int GetInfoDBID(string currentPlayerTag)
    {
        if (dbList.Count <= 0) return -1;

        if (currentPlayerTag == "Human")
        {
            return dbList[0].ID;
        }
        else
        {
            return dbList[1].ID;
        }
    }

    public float GetAttackDamageValue(int infoId)
    {
        foreach(var item in dbList)
        {
            if(item.ID == infoId)
            {
                return item.BaseAttackDamage;
            }
        }

        return -1f;  
    }

    public int GetNormalAttackID(int infoId)
    {
        foreach (var item in dbList)
        {
            if (item.ID == infoId) return item.NormalAttackID;
        }

        return -1;
    }

    public int GetSkillAttackID(int infoId)
    {
        foreach (var item in dbList)
        {
            if (item.ID == infoId) return item.SkillID;
        }

        return -1;
    }

    public float GetGaugeChargeValue(int attackId)
    {
        foreach(var item in normalAttacks)
        {
            if(item.ID == attackId) return item.SpecialMoveGaugeCharge;
        }

        return -1f;
    }

    public float GetMaxGagueChargeValue(int attackId)
    {
        foreach (var item in normalAttacks)
        {
            if (item.ID == attackId) return item.MaxSpecialMoveGaugeCharge;
        }

        return -1f;
    }
}
