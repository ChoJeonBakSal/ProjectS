using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossBar : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI textMeshProUGUI;
    [SerializeField] public Image Image1;
    [SerializeField] public Image Image2;
    [SerializeField] public Text HpCount;

    private float bossFixHp;
    private float bossCurrentHp;
    private int currentSegment;
    private float segmentHp = 100f; // HP per segment
    private bool isFirstHit = true; // ù ��° Ÿ���� Ȯ���ϱ� ���� �÷���

    private void OnEnable()
    {
        // Initialize the boss health
        bossFixHp = 5000f; // Example value, adjust as needed
        bossCurrentHp = bossFixHp;
        currentSegment = Mathf.FloorToInt(bossFixHp / segmentHp);
        textMeshProUGUI.text = $"{bossCurrentHp.ToString("F0")} / {bossFixHp.ToString("F0")}";
        UpdateHpCount();

        // Initialize colors
        Image1.color = GetRandomColor();
        Image2.color = GetRandomColorExcluding(Image1.color); // �� �̹����� �ʱ� ������ �ٸ��� ����
    }

    void Start()
    {
        // ���� ü�� �ʱ�ȭ�� OnEnable���� ó����
    }

    void Update()
    {
        if (textMeshProUGUI == null)
            Debug.Log("No TextMeshProUGUI found");
    }

    public void RefreshBossHp(float currenthp)
    {
        bossCurrentHp = currenthp;
        hpProgress(bossCurrentHp);
        textMeshProUGUI.text = $"{bossCurrentHp.ToString("F0")} / {bossFixHp.ToString("F0")}";
    }

    private void hpProgress(float currenthp)
    {
        int newSegment = Mathf.FloorToInt(currenthp / segmentHp);
        float currentUnitHealth = currenthp % segmentHp;

        if (newSegment < currentSegment)
        {
            if (!isFirstHit)
            {
                // If we moved to a new segment and it's not the first hit, update the colors
                Image2.color = Image1.color;
                Image1.color = GetRandomColorExcluding(Image2.color);
            }
            currentSegment = newSegment;
        }

        isFirstHit = false; // ù ��° Ÿ���� �Ϸ�Ǿ����� ǥ��

        Image2.fillAmount = currentUnitHealth / segmentHp;
        UpdateHpCount();
    }

    private void UpdateHpCount()
    {
        HpCount.text = $"x {currentSegment}";
    }

    private Color GetRandomColor()
    {
        return new Color(Random.value, Random.value, Random.value);
    }

    private Color GetRandomColorExcluding(Color excludeColor)
    {
        Color newColor;
        do
        {
            newColor = new Color(Random.value, Random.value, Random.value);
        } while (newColor == excludeColor);
        return newColor;
    }
}
