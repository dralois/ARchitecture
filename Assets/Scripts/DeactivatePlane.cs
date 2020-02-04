using UnityEngine;

public class DeactivatePlane : MonoBehaviour
{
	[SerializeField] private GameObject _planeObj = null;

	public void Deactivate()
	{
		if (_planeObj)
		{
			_planeObj.SetActive(false);
		}
	}
}
