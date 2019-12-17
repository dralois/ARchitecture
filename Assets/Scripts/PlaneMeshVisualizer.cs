using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

#if !UNITY_EDITOR
[RequireComponent(typeof(ARPlaneMeshVisualizer), typeof(MeshRenderer), typeof(ARPlane))]
#endif
public class PlaneMeshVisualizer : MonoBehaviour
{

#region Fields

	[SerializeField] private float _featheringWidth = 0.2f;
	[SerializeField] private Material _shadowMaterial = null;

	private List<Vector3> _planeUVs = new List<Vector3>();
	private List<Vector3> _vertices = new List<Vector3>();

	private ARPlaneMeshVisualizer _planeMeshVisualizer = null;
	private Material _planeMaterial = null;
	private ARPlane _plane = null;

#endregion

#region Methods

	private void X_ModeChange(GameManager.InputMode newMode)
	{
		if(newMode != GameManager.InputMode.Spawn)
		{
			GetComponent<MeshRenderer>().material = _shadowMaterial;
		}
	}

	private void X_BoundaryUpdate(ARPlaneBoundaryChangedEventArgs eventArgs)
	{
		if(GameManager.Instance.CurrentMode == GameManager.InputMode.Spawn)
			X_GenerateBoundaryUVs(_planeMeshVisualizer.mesh);
	}

	private void X_GenerateBoundaryUVs(Mesh mesh)
	{
		int vertexCount = mesh.vertexCount;

		// Reuse the list of UVs
		_planeUVs.Clear();
		if (_planeUVs.Capacity < vertexCount) { _planeUVs.Capacity = vertexCount; }

		mesh.GetVertices(_vertices);

		Vector3 centerInPlaneSpace = _vertices[_vertices.Count - 1];
		Vector3 uv = new Vector3(0, 0, 0);
		float shortestUVMapping = float.MaxValue;

		// Assume the last vertex is the center vertex.
		for (int i = 0; i < vertexCount - 1; i++)
		{
			float vertexDist = Vector3.Distance(_vertices[i], centerInPlaneSpace);

			// Remap the UV so that a UV of "1" marks the feathering boudary.
			// The ratio of featherBoundaryDistance/edgeDistance is the same as featherUV/edgeUV.
			// Rearrange to get the edge UV.
			float uvMapping = vertexDist / Mathf.Max(vertexDist - _featheringWidth, 0.001f);
			uv.x = uvMapping;

			// All the UV mappings will be different. In the shader we need to know the UV value we need to fade out by.
			// Choose the shortest UV to guarentee we fade out before the border.
			// This means the feathering widths will be slightly different, we again rely on a fairly uniform plane.
			if (shortestUVMapping > uvMapping) { shortestUVMapping = uvMapping; }

			_planeUVs.Add(uv);
		}

		_planeMaterial.SetFloat("_ShortestUVMapping", shortestUVMapping);

		// Add the center vertex UV
		uv.Set(0, 0, 0);
		_planeUVs.Add(uv);

		mesh.SetUVs(1, _planeUVs);
		mesh.UploadMeshData(false);
	}

#region Unity

	private void Awake()
	{
		#if !UNITY_EDITOR
		_plane = GetComponent<ARPlane>();
		_planeMaterial = GetComponent<MeshRenderer>().material;
		_planeMeshVisualizer = GetComponent<ARPlaneMeshVisualizer>();
#endif
	}

	private void OnEnable()
	{
#if !UNITY_EDITOR
		_plane.boundaryChanged += X_BoundaryUpdate;
#endif
		GameManager.Instance.ModeChanged += X_ModeChange;
	}

	private void OnDisable()
	{
#if !UNITY_EDITOR
		_plane.boundaryChanged -= X_BoundaryUpdate;
#endif
		GameManager.Instance.ModeChanged -= X_ModeChange;
	}

#endregion

#endregion

}
