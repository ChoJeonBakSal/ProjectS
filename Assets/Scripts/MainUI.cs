using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    // Human Set
    private float MaxHpHuman;
    public float _crtHpHuman;
    public Image BarHpHuman;

    // Wolf Set
    private float MaxHpWolf;
    public float _crtHpWolf;
    public Image BarHpWolf;

    private float MaxSkillGauge = 100;
    public float _crtSkillGauge;
    public Image BarSkillGauge;

    public void SetInitialValues(float humanHp, float wolfHp, float crtUltimate)
    {
        // Human Set
        MaxHpHuman = humanHp;
        _crtHpHuman = MaxHpHuman;

        // Wolf Set
        MaxHpWolf = wolfHp;
        _crtHpWolf = MaxHpWolf;

        //Skill Set
        _crtSkillGauge = crtUltimate;
    }
 
    void Update()
    {
        BarHpHuman.fillAmount = _crtHpHuman / MaxHpHuman;
        BarHpWolf.fillAmount = _crtHpWolf / MaxHpWolf;
        BarSkillGauge.fillAmount = _crtSkillGauge / MaxSkillGauge;
        if(BarSkillGauge.fillAmount >= 1)
        {
            DBCharacterPC.Instance.UltimateSkillEffectOnOff(true);
        } else
        {
            DBCharacterPC.Instance.UltimateSkillEffectOnOff(false);
        }
    }

    public void AddSkillGauge(float amount)
    {
        _crtSkillGauge += amount;
        if (_crtSkillGauge > MaxSkillGauge)
        {
            _crtSkillGauge = MaxSkillGauge;
        }
    }
}
