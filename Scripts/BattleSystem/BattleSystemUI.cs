using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSystemUI : MonoBehaviour
{
    public TMP_Text name_text;
    public TMP_Text level_text;
    public Slider hpSlider;

    public void InitialiseHUD(Unit unit)
    {
        name_text.text = unit.unitName;
        level_text.text = "Lvl " + unit.unitLevel;
        hpSlider.maxValue = unit.maxHealth;
        hpSlider.value = unit.currentHealth;
    }

    public void UpdateHealthUI(int healthValue)
    {
        hpSlider.value = healthValue;
    }


}
