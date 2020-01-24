using UnityEngine;

[RequireComponent(typeof(Camera))]
public class TextureScaler : MonoBehaviour
{
	[SerializeField] private Material _transferMaterial = null;

	private RenderTexture _rt = null;
	private Camera _cam = null;

	private void Awake()
	{
		// Rendertexture mit Default-Descriptor holen
		var desc = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGB32, 0, 0);
		_rt = RenderTexture.GetTemporary(desc);
		// Der Kamera zuweisen
		_cam = GetComponent<Camera>();
		_cam.targetTexture = _rt;
		// Dem Material zuweisen
		_transferMaterial.SetTexture("_BaseMap", _rt);
	}

	private void OnDestroy()
	{
		RenderTexture.ReleaseTemporary(_rt);
	}

}
