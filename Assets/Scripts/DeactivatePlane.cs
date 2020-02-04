using UnityEngine;

public class DeactivatePlane : MonoBehaviour
{
	[SerializeField] private GameObject _planeObj = null;

	public void Deactivate()
	{
		_planeObj.SetActive(false);
	}
}
