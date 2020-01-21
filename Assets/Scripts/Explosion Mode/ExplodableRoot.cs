using UnityEngine;

public class ExplodableRoot : MonoBehaviour
{

	[SerializeField] private float _outModifier = .1f;

	private void Start()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			var explodable = transform.GetChild(i).GetComponent<ExplodableComponent>();
			explodable.SetupExplodable(this, transform.childCount - i, _outModifier);
		}
	}

	public void EnterExplosionMode(Vector3 normal)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			var explodable = transform.GetChild(i).gameObject.GetComponent<ExplodableComponent>();
			explodable.SetExploded(true, normal);
		}
	}

	public void ExitExplosionMode()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			var explodable = transform.GetChild(i).gameObject.GetComponent<ExplodableComponent>();
			explodable.SetExploded(false, Vector3.zero);
		}
	}

}
