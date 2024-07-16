using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MonsterSpawnManager : MonoBehaviour
{
    public static MonsterSpawnManager Instance;

    [Serializable]
    public class MonsterSpawnSetting
    {
        public GameObject MonsterPrefab;
        public int SpawnCount;
    }

    [SerializeField] private GameObject Plane;
    [SerializeField] private MonsterSpawnSetting[] monsterSettings;
    [SerializeField] private int MaxWaveCount;

    private float planeWidth;
    private float planeHeight;
    private int WaveCount;

    private bool isSpawning;

    private List<Vector3> spawnPositionList = new List<Vector3>();

    private Vector3 spawnPoint;

    private List<MonsterView> monsterList = new List<MonsterView>();

    private void Awake()
    {
        Instance = this;

        InitSpawnPointList();
    }

    private void InitSpawnPointList()
    {
        Transform plane = Plane.transform;
        planeWidth = plane.localScale.x * 10f;
        planeHeight = plane.localScale.z * 10f;

        Vector3 Area1 = GetAreaCenterPosition(plane, planeWidth, planeHeight);
        Vector3 Area2 = GetAreaCenterPosition(plane, -planeWidth, planeHeight);  
        Vector3 Area3 = GetAreaCenterPosition(plane, -planeWidth, -planeHeight);
        Vector3 Area4 = GetAreaCenterPosition(plane, planeWidth, -planeHeight);

        spawnPositionList.Add(Area1);
        spawnPositionList.Add(Area2);
        spawnPositionList.Add(Area3);
        spawnPositionList.Add(Area4);

        WaveCount = 0;
    }

    private Vector3 GetAreaCenterPosition(Transform plane, float width, float height)
    {
        return new Vector3(plane.position.x - width / 4, plane.position.y, plane.position.z - height / 4);
    }

    private void Update()
    {
        if(monsterList.Count <= 0 && !isSpawning)
        {
            isSpawning = true;
            Invoke(nameof(StartMonsterSpawnWave), 5f);
        }
    }

    public void StartMonsterSpawnWave()
    {
        if (MaxWaveCount <= WaveCount) return;

        ChangeSpawnPointCenter();

        foreach (var child in monsterSettings)
        {
            if(child == null) continue;

            for (int i = 0; i < child.SpawnCount; i++)
            {
                Vector3 randomPosition = spawnPoint + new Vector3(
                    UnityEngine.Random.Range(-planeWidth / 4, planeWidth / 4),
                    0,
                    UnityEngine.Random.Range(-planeHeight / 4, planeHeight / 4));

                Quaternion randomRotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
                GameObject newMonster = Instantiate(child.MonsterPrefab, randomPosition, randomRotation);
                newMonster.transform.parent = transform;
            }
        }

        WaveCount++;
        isSpawning = false;
    }

    private void ChangeSpawnPointCenter()
    {
        int randomAreaIndex = UnityEngine.Random.Range(0, spawnPositionList.Count);

        spawnPoint = spawnPositionList[randomAreaIndex];
    }

    public void AddMonsterList(MonsterView newObject)
    {
        monsterList.Add(newObject);
    }

    public void RemoveMonsterList(MonsterView newObject)
    {
        monsterList.Remove(newObject);
    }
}
