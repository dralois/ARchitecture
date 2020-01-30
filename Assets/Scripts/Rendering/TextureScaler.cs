using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Camera))]
public class TextureScaler : MonoBehaviour
{

	[SerializeField] private Camera _masterARCam = null;
	[SerializeField] private Material _transferMaterial = null;
	[SerializeField] [Range(0, 32)] private int _depthBits = 0;

	private ARCameraManager _ar = null;
	private RenderTexture _rt = null;
	private Camera _cam = null;

	private void Awake()
	{
		// Rendertexture mit Default-Descriptor holen
		var desc = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGB32, _depthBits, 0);
		_rt = RenderTexture.GetTemporary(desc);
		// Der Kamera zuweisen
		_cam = GetComponent<Camera>();
		_cam.targetTexture = _rt;
		// Dem Material zuweisen
		_transferMaterial.SetTexture("_BaseMap", _rt);
		// Event abonnieren
		_ar = _masterARCam.GetComponent<ARCameraManager>();
		_ar.frameReceived += ARFrameRecieved;
	}

	private void ARFrameRecieved(ARCameraFrameEventArgs frameData)
	{
		// Display Transform Matrix setzen
		if (frameData.displayMatrix.HasValue)
		{
			_transferMaterial.SetMatrix("_UnityDisplayTransform", frameData.displayMatrix.Value);
		}
		// Projection Matrix setzen
		if (frameData.projectionMatrix.HasValue)
		{
			_cam.projectionMatrix = frameData.projectionMatrix.Value;
		}
	}

	private void OnDestroy()
	{
		// Rendertexture freigeben
		RenderTexture.ReleaseTemporary(_rt);
		// Event entfernen
		_ar.frameReceived -= ARFrameRecieved;
	}

}
