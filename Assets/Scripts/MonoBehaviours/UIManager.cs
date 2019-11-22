using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public Image PlayerHealthBar;
	public TextMeshProUGUI Score;

	public void SetScoreUIValue(int newScore)
	{
		Score.text = newScore.ToString();
	}

	public void SetHealthUIValue(float percentage)
	{
		PlayerHealthBar.rectTransform.localScale = new Vector3(percentage, 1f, 1f);
	}
}
