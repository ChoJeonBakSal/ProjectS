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

    public void SetInitialValues(float humanHp, float wolfHp)
    {
        // Human Set
        MaxHpHuman = humanHp;
        _crtHpHuman = MaxHpHuman;

        // Wolf Set
        MaxHpWolf = wolfHp;
        _crtHpWolf = MaxHpWolf;
    }

    void Update()
    {
        BarHpHuman.fillAmount = _crtHpHuman / MaxHpHuman;
        BarHpWolf.fillAmount = _crtHpWolf / MaxHpWolf;
    }
}
