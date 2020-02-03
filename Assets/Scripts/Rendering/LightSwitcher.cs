using UnityEngine;

public class LightSwitcher : MonoBehaviour
{

	[SerializeField] private GameObject _nightLights = null;
	[SerializeField] private GameObject _dayLights = null;

	[ContextMenu("Mood Switch Day")]
	public void SwitchDay()
	{
		X_MoodChange(GameManager.LightMode.Day);
	}

	[ContextMenu("Mood Switch Night")]
	public void SwitchNight()
	{
		X_MoodChange(GameManager.LightMode.Night);
	}

	private void X_MoodChange(GameManager.LightMode mood)
	{
		switch (mood)
		{
			case GameManager.LightMode.Day:
				{
					_nightLights.SetActive(false);
					_dayLights.SetActive(true);
					break;
				}
			case GameManager.LightMode.Night:
				{
					_nightLights.SetActive(true);
					_dayLights.SetActive(false);
					break;
				}
		}
	}

	private void OnEnable()
	{
		GameManager.Instance.LightChanged += X_MoodChange;
	}

	private void OnDisable()
	{
		GameManager.Instance.LightChanged -= X_MoodChange;
	}

}
