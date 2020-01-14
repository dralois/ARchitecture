using UnityEngine;

public class LightSwitcher : MonoBehaviour
{

	[SerializeField] private GameObject _nightLights = null;
	[SerializeField] private GameObject _dayLights = null;

	[ContextMenu("Mood Switch Day")]
	public void SwitchDay()
	{
		X_MoodChange(GameManager.LightMood.Day);
	}

	[ContextMenu("Mood Switch Night")]
	public void SwitchNight()
	{
		X_MoodChange(GameManager.LightMood.Night);
	}

	private void X_MoodChange(GameManager.LightMood mood)
	{
		switch (mood)
		{
			case GameManager.LightMood.Day:
				{
					_nightLights.SetActive(false);
					_dayLights.SetActive(true);
					break;
				}
			case GameManager.LightMood.Night:
				{
					_nightLights.SetActive(true);
					_dayLights.SetActive(false);
					break;
				}
		}
	}

	private void OnEnable()
	{
		GameManager.Instance.MoodChanged += X_MoodChange;
	}

	private void OnDisable()
	{
		GameManager.Instance.MoodChanged -= X_MoodChange;
	}

}
