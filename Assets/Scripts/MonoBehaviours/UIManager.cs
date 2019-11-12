using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public Image PlayerHealthBar;

	public void SetHealthPerc(float percentage)
	{
		//Debug.Log("Setting the health. New percentage: " + percentage);

		PlayerHealthBar.rectTransform.localScale = new Vector3(percentage, 1f, 1f);
	}
}
