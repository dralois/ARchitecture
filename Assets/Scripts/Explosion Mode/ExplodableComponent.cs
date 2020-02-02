using UnityEngine;

public class ExplodableComponent: MonoBehaviour
{

	private bool _isSetup;
	private int _explodePos;
	private float _outModifier;
	private Vector3 _baseOffset;

	public ExplodableRoot Root { get; private set; } = null;

	public void SetupExplodable(ExplodableRoot explodeBase, int childPos, float outModifier)
	{
		Root = explodeBase;
		_explodePos = childPos;
		_outModifier = outModifier;
		_baseOffset = Root.transform.InverseTransformVector(transform.position - Root.transform.position);
		_isSetup = true;
	}

	public void EnterExplosionMode(Vector3 normal)
	{
		if(_isSetup)
		{
			Root.EnterExplosionMode(normal);
		}
	}

	public void ExitExplosionMode()
	{
		if (_isSetup)
		{
			Root.ExitExplosionMode();
		}
	}

	public void SetExploded(bool exploded, Vector3 normal)
	{
		if (_isSetup)
		{
			if (exploded)
			{
				transform.position = transform.position + normal * _explodePos * _outModifier * GameManager.Instance.IFCScale;
			}
			else
			{
				transform.position = Root.transform.position + Root.transform.TransformVector(_baseOffset);
			}
		}
	}

	private void OnDestroy()
	{
		ExitExplosionMode();
	}

}
