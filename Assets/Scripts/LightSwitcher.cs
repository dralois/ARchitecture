using UnityEngine;

public class LightSwitcher : MonoBehaviour
{

	[SerializeField] private GameObject _nightPrefab = null;
	[SerializeField] private GameObject _dayPrefab = null;
	[SerializeField] private Light _dayLight = null;

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
					_nightPrefab.SetActive(false);
					_dayPrefab.SetActive(true);
					_dayLight.enabled = true;
					break;
				}
			case GameManager.LightMood.Night:
				{
					_nightPrefab.SetActive(true);
					_dayPrefab.SetActive(false);
					_dayLight.enabled = false;
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
