using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

[ExecuteInEditMode]
public class MeshCombiner : EditorWindow
{
	private static string patch = "Assets/SceneMeshCombiner/Mesh";

	[MenuItem ("Window/Scene Mesh Combiner Tools")]
	public static void  ShowWindow ()
	{
		EditorWindow.GetWindow (typeof(MeshCombiner));
	}

	private static int MaxVertex = 50000;
	private Thread thread;
	private bool isConverting;
	private bool replaceafterconvert = false;
	private bool setStatic = true;
	private bool generateUv2 = false;
	private bool disableRootafterCombine;
	private bool onlyStatic = true;


	void OnGUI ()
	{
		titleContent.text = "Scene Combiner";
		GUILayout.Space (50);

		if (GUILayout.Button ("How to use", GUILayout.Height (20.0f))) {
			Application.OpenURL ("http://www.hardworkerstudio.com/SceneMeshCombiner/manual.pdf");
		}
		GUILayout.Space (10);
		MaxVertex = EditorGUILayout.IntField ("Vertices limit per mesh", MaxVertex);

		GUILayout.Space (10);
		onlyStatic = EditorGUILayout.Toggle ("Combine only static", onlyStatic);
		if (GUILayout.Button ("Auto combine static object by materials type", GUILayout.Height (30.0f))) {
			AutoCombineAndGroupUpByMaterial ();
		}

		if (GUILayout.Button ("Combine selected by materials type", GUILayout.Height (30.0f))) {
			CombineAndGroupUpByMaterial (Selection.gameObjects);
		}

		if (GUILayout.Button ("Combine selected by mesh type", GUILayout.Height (30.0f))) {
			CombineAndGroupUp (Selection.gameObjects);
		}
		GUILayout.Space (10);
		setStatic = EditorGUILayout.Toggle ("Static object", setStatic);
		generateUv2 = EditorGUILayout.Toggle ("Auto Generate UV2", generateUv2);
		EditorGUILayout.LabelField ("Vertices limit per mesh must under 60000");
		GUILayout.Space (10);
		if (GUILayout.Button ("Combine selected in one mesh", GUILayout.Height (30.0f))) {
			
			JustCombineAndGroupUp (Selection.gameObjects);
		}

		if (GUILayout.Button ("Generate UV2", GUILayout.Height (30.0f))) {
			GenerateUV2 (Selection.gameObjects);
		}

		if (onGenerateOBJ) {
			EditorGUI.ProgressBar (new Rect (0, 10, position.width, 20), progress, progressstatus);
		} else {
			if (GUILayout.Button ("Convert selected to .obj", GUILayout.Height (30.0f))) {
				ThreadSetup ();
				thread = new Thread (HeavyJob);
				thread.Start ();
			}
			GUILayout.Space (10);
			replaceafterconvert = EditorGUILayout.Toggle ("Replace after convert", replaceafterconvert);
			GUILayout.Space (10);
			EditorGUILayout.LabelField (patch);
		}
		if (GUILayout.Button ("Open folder", GUILayout.Height (30.0f))) {
			ShowExplorer (patch + "/");

		}

		GUILayout.Space (30);
		EditorGUILayout.LabelField ("Selected properties");
		EditorGUILayout.IntField ("Mesh count ", selectedmeshCount);
		EditorGUILayout.IntField ("Mesh type count ", selectedmeshtype);
		EditorGUILayout.IntField ("Vertices count ", selectedverticescount);
		EditorGUILayout.IntField ("Material type count ", selectedmaterialcount);


	}

	public void ShowExplorer (string itemPath)
	{
		itemPath = itemPath.Replace (@"/", @"\"); 
		System.Diagnostics.Process.Start ("explorer.exe", itemPath);
	}

	void HeavyJob ()
	{
		ConvertMeshToFile (threadIndex);
	}

	private bool isRefreshed;

	void Update ()
	{
		MeshProperty (Selection.gameObjects);
		if (!isRefreshed) {
			AssetDatabase.Refresh ();
			if (generateUv2) {
				foreach (string f in assetsToReplace) {
					ModelImporter modelImporter = AssetImporter.GetAtPath (f) as ModelImporter;
					if (modelImporter) {
						modelImporter.generateSecondaryUV = true;
					}
				}
			}

			isRefreshed = true;

		}
		if (replacing) {
			for (int i = 0; i < assetsToReplace.Count; i++) {
				if (threadmeshlist [i]) {
					GameObject prefab = new GameObject ();
					Mesh mes = (Mesh)AssetDatabase.LoadAssetAtPath (assetsToReplace [i], typeof(Mesh));
					if (generateUv2) {
						Unwrapping.GenerateSecondaryUVSet (mes);
					}
					prefab.AddComponent<MeshFilter> ();
					prefab.GetComponent<MeshFilter> ().mesh = mes;
					prefab.AddComponent<MeshRenderer> ();
					prefab.GetComponent<MeshRenderer> ().materials = convertThreads [i].mats;
					prefab.transform.position = threadmeshlist [i].transform.position;
					prefab.name = "Re_" + threadmeshlist [i].name;
					prefab.transform.localScale = new Vector3 (-prefab.transform.localScale.x, prefab.transform.localScale.y, prefab.transform.localScale.z);
					prefab.isStatic = setStatic;
					threadmeshlist [i].gameObject.SetActive (false);
				}
			}
			replacing = false;
		}
	}

	void GenerateUV2 (GameObject[] target)
	{
		for (int i = 0; i < target.Length; i++) {
			MeshFilter[] meshFilter = target [i].GetComponentsInChildren<MeshFilter> ();

			for (int m = 0; m < meshFilter.Length; m++) {
				Unwrapping.GenerateSecondaryUVSet (meshFilter [m].sharedMesh);
			}
		}
		Debug.Log ("UV2 Generated");
	}

	void JustCombineAndGroupUp (GameObject[] target)
	{
		disableRootafterCombine = false;
		List<MeshFilter> meshlist = new List<MeshFilter> ();

		for (int i = 0; i < target.Length; i++) {
			MeshFilter[] meshFilter = target [i].GetComponentsInChildren<MeshFilter> ();

			for (int m = 0; m < meshFilter.Length; m++) {
				meshlist.Add (meshFilter [m]);
			}
		}
		CombineGroup (meshlist.ToArray (), null);
	}

	void AutoCombineAndGroupUpByMaterial ()
	{
		disableRootafterCombine = true;
		GameObject[] targetget = (GameObject[])GameObject.FindObjectsOfType (typeof(GameObject));
		List<GameObject> target = new List<GameObject> ();
		for (int i = 0; i < targetget.Length; i++) {
			if ((!onlyStatic || targetget [i].isStatic) && targetget [i].GetComponent<MeshFilter> ()) {
				target.Add (targetget [i].gameObject);
			}
		}
		CombineAndGroupUpByMaterial (target.ToArray ());
	}

	void CombineAndGroupUpByMaterial (GameObject[] target)
	{
		disableRootafterCombine = true;
		FindMaterialGroup (target);
		Debug.Log ("Grouped count " + materialTypeGroup.Count);
		GameObject root = new GameObject ();
		root.name = "Object group";
		foreach (MeshGroupInstance meshgroup in materialTypeGroup) {
			CombineGroup (meshgroup.meshlist.ToArray (), root.transform);
		}
	}

	void CombineAndGroupUp (GameObject[] target)
	{
		disableRootafterCombine = true;
		FindMeshGroup (target);
		Debug.Log ("Grouped count " + meshTypeGroup.Count);
		GameObject root = new GameObject ();
		root.name = "Object group";
		foreach (MeshGroupInstance meshgroup in meshTypeGroup) {
			CombineGroup (meshgroup.meshlist.ToArray (), root.transform);
		}
	}

	List<MeshGroupInstance> meshTypeGroup = new List<MeshGroupInstance> ();
	int selectedmeshCount;
	int selectedmeshtype;
	int selectedmaterialcount;
	int selectedverticescount;

	void MeshProperty (GameObject[] target)
	{
		if (target.Length <= 0)
			return;
		
		List<MeshFilter> meshlist = new List<MeshFilter> ();
		selectedverticescount = 0;
		for (int i = 0; i < target.Length; i++) {
			MeshFilter[] meshFilter = target [i].GetComponentsInChildren<MeshFilter> ();
			if (meshFilter.Length > 0) {
				for (int m = 0; m < meshFilter.Length; m++) {
					meshlist.Add (meshFilter [m]);
					if(meshFilter [m].sharedMesh)
					selectedverticescount += meshFilter [m].sharedMesh.vertexCount;
				}
			}
		}
		selectedmeshCount = meshlist.Count;
		List<MeshGroupInstance> meshTypeGroupD = new List<MeshGroupInstance> ();
		foreach (MeshFilter mes in meshlist) {
			foreach (MeshGroupInstance mesh in meshTypeGroupD) {
				if (mesh.mesh != null) {
					if (mesh.mesh.sharedMesh == mes.sharedMesh) {
						// add same mesh to the existing list
						mesh.meshlist.Add (mes);
						return;
					}
				}
			}
			// add new mesh to the list
			MeshGroupInstance newmesh = new MeshGroupInstance ();
			newmesh.meshlist = new List<MeshFilter> ();
			newmesh.meshlist.Add (mes);
			newmesh.mesh = mes;
			meshTypeGroup.Add (newmesh);
		}
		selectedmeshtype = meshTypeGroupD.Count;
	}

	void FindMeshGroup (GameObject[] target)
	{ 
		List<MeshFilter> meshlist = new List<MeshFilter> ();

		selectedverticescount = 0;
		for (int i = 0; i < target.Length; i++) {
			MeshFilter[] meshFilter = target [i].GetComponentsInChildren<MeshFilter> ();
			if (meshFilter.Length > 0) {
				for (int m = 0; m < meshFilter.Length; m++) {
					meshlist.Add (meshFilter [m]);
					if(meshFilter [m].sharedMesh)
					selectedverticescount += meshFilter [m].sharedMesh.vertexCount;
				}
			}
		}
		selectedmeshCount = meshlist.Count;
		//Debug.Log ("Scene mesh count " + meshlist.Count);

		meshTypeGroup = new List<MeshGroupInstance> ();
		foreach (MeshFilter mes in meshlist) {
			AddMeshToGroup (mes);
		}
		selectedmeshtype = meshTypeGroup.Count;
		//Debug.Log ("Scene difference meshes count " + meshTypeGroup.Count);
	}

	List<MeshGroupInstance> materialTypeGroup = new List<MeshGroupInstance> ();

	void FindMaterialGroup (GameObject[] target)
	{ 
		
		List<MeshFilter> meshlist = new List<MeshFilter> ();
		selectedverticescount = 0;

		for (int i = 0; i < target.Length; i++) {
			MeshFilter[] meshFilter = target [i].GetComponentsInChildren<MeshFilter> ();
			if (meshFilter.Length > 0) {
				for (int m = 0; m < meshFilter.Length; m++) {
					meshlist.Add (meshFilter [m]);
					if(meshFilter [m].sharedMesh)
					selectedverticescount += meshFilter [m].sharedMesh.vertexCount;
				}
			}
		}
		selectedmeshCount = meshlist.Count;
		//Debug.Log ("Scene mesh count " + meshlist.Count);

		materialTypeGroup = new List<MeshGroupInstance> ();
		foreach (MeshFilter mes in meshlist) {
			AddMaterialToGroup (mes, mes.GetComponent<Renderer> ());
		}
		selectedmaterialcount = materialTypeGroup.Count;
		//Debug.Log ("Scene difference material count " + materialTypeGroup.Count);
	}

	bool compareMaterial (Material[] mat1, Material[] mat2)
	{
		for (int i = 0; i < mat1.Length; i++) {
			if (mat1.Length == mat2.Length) {
				if (mat1 [i] != mat2 [i]) {
					return false;
				}
			} else {
				return false;
			}
		}
		return true;
	}

	void AddMaterialToGroup (MeshFilter mesh, Renderer render)
	{
		foreach (MeshGroupInstance mes in materialTypeGroup) {
			if (mes.Mat.Length > 0) {
				//Debug.Log ("mat " + mes.Mat + " vs " + render.sharedMaterials);
				if (compareMaterial (mes.Mat, render.sharedMaterials)) {
					// add same mesh to the existing list
					mes.meshlist.Add (mesh);
					return;
				}
			}
		}
		// add new mesh to the list

		MeshGroupInstance newmesh = new MeshGroupInstance ();
		newmesh.meshlist = new List<MeshFilter> ();
		newmesh.meshlist.Add (mesh);
		newmesh.mesh = mesh;
		newmesh.Mat = render.sharedMaterials;
		materialTypeGroup.Add (newmesh);

	}

	void AddMeshToGroup (MeshFilter mesh)
	{
		foreach (MeshGroupInstance mes in meshTypeGroup) {
			if (mes.mesh != null) {
				if (mes.mesh.sharedMesh == mesh.sharedMesh) {
					// add same mesh to the existing list
					mes.meshlist.Add (mesh);
					return;
				}
			}
		}
		// add new mesh to the list
		MeshGroupInstance newmesh = new MeshGroupInstance ();
		newmesh.meshlist = new List<MeshFilter> ();
		newmesh.meshlist.Add (mesh);
		newmesh.mesh = mesh;
		meshTypeGroup.Add (newmesh);

	}

	List<LimitedMeshInstance> reGroupByVertexLimit = new List<LimitedMeshInstance> ();

	void GroupUpLimitedVertex (MeshFilter mesh)
	{
		for (int i = 0; i < reGroupByVertexLimit.Count; i++) {
			if (!reGroupByVertexLimit [i].limited && mesh.sharedMesh) {
				if (reGroupByVertexLimit [i].VertexCount + mesh.sharedMesh.vertexCount < MaxVertex) {
					reGroupByVertexLimit [i].Meshlist.Add (mesh);
					reGroupByVertexLimit [i].VertexCount += mesh.sharedMesh.vertexCount;
					return;
				} else {
					reGroupByVertexLimit [i].limited = true;
					//Debug.Log ("** reach a limit of vertex " + reGroupByVertexLimit [i].VertexCount);
				}
			}
		}
		//Debug.Log ("** create new mesh group");
		LimitedMeshInstance newLimMesh = new LimitedMeshInstance ();
		newLimMesh.Meshlist = new List<MeshFilter> ();
		newLimMesh.Meshlist.Add (mesh);
		if(mesh.sharedMesh)
		newLimMesh.VertexCount += mesh.sharedMesh.vertexCount;
		newLimMesh.limited = false;
		reGroupByVertexLimit.Add (newLimMesh);
	}


	void CombineGroup (MeshFilter[] meshlist, Transform root)
	{
		reGroupByVertexLimit = new List<LimitedMeshInstance> ();

		for (int i = 0; i < meshlist.Length; i++) {
			GroupUpLimitedVertex (meshlist [i]);
		}
		int count = 0;
		//Debug.Log (":: New group created : count " + reGroupByVertexLimit.Count);
		foreach (LimitedMeshInstance lim in reGroupByVertexLimit) {
			//Debug.Log (":: Group " + count + " vertex count " + lim.VertexCount);

			MeshFilter[] reGroupedMeshList = lim.Meshlist.ToArray ();
			if (reGroupedMeshList [0].sharedMesh) {
				for (int s = 0; s < reGroupedMeshList [0].sharedMesh.subMeshCount; s++) {
					Combine (reGroupedMeshList, s, root);
				}
			}
			count++;
		}

	}

	void CombineGroup (GameObject[] target, Transform root)
	{
		// Must group only same mesh
		List<MeshFilter> meshlist = new List<MeshFilter> ();

		for (int i = 0; i < target.Length; i++) {
			MeshFilter[] meshFilter = target [i].GetComponentsInChildren<MeshFilter> ();
			if (meshFilter.Length > 0) {
				for (int m = 0; m < meshFilter.Length; m++) {
					meshlist.Add (meshFilter [m]);
				}
			}
		}

		for (int s = 0; s < meshlist [0].sharedMesh.subMeshCount; s++) {
			
			Combine (meshlist.ToArray (), s, root);
		}

	}

	void Combine (MeshFilter[] meshFilters, int submesh, Transform root)
	{
		GameObject target = new GameObject ();


		if (target.GetComponent<MeshFilter> () == null) {
			target.AddComponent<MeshFilter> ();
		}
		if (target.GetComponent<MeshRenderer> () == null) {
			target.AddComponent<MeshRenderer> ();
		}

		Mesh finalMesh = new Mesh ();
		Matrix4x4 ourMatrix = target.transform.localToWorldMatrix;
		//Material material = null;
		Material[] materials = null;

		CombineInstance[] combiners = new CombineInstance[meshFilters.Length];
		
		for (int i = 0; i < meshFilters.Length; i++) {
			
			if (meshFilters [i].transform == target.transform)
				continue;

			combiners [i].subMeshIndex = submesh;
			combiners [i].mesh = meshFilters [i].sharedMesh;
			combiners [i].transform = meshFilters [i].transform.localToWorldMatrix;
			materials = new Material[1];
			materials [0] = meshFilters [i].gameObject.GetComponent<MeshRenderer> ().sharedMaterials [submesh];
			if (disableRootafterCombine) {
				meshFilters [i].transform.root.gameObject.SetActive (false);
			} else {
				meshFilters [i].transform.gameObject.SetActive (false);
			}

		}

		finalMesh.CombineMeshes (combiners);
		if (generateUv2) {
			Unwrapping.GenerateSecondaryUVSet (finalMesh);
		}

		target.gameObject.isStatic = setStatic;
		target.GetComponent<MeshFilter> ().sharedMesh = finalMesh;
		target.GetComponent<MeshRenderer> ().materials = materials;
		target.name = "Group_" + materials [0].name + "_Sub_mesh" + submesh;
		target.transform.parent = root;
		if (target.GetComponent<MeshCollider> ()) {
			target.GetComponent<MeshCollider> ().sharedMesh = finalMesh;
		}
	}

	void ResetAll ()
	{
		replacing = false;
		onGenerateOBJ = false;
		isRefreshed = true;
		thread.Join ();
	}

	void ConvertMeshToFile (int index)
	{

		meshfillter = convertThreads [index].meshfillter;
		mesh = convertThreads [index].mesh;
		mfname = convertThreads [index].mfname;
		mats = convertThreads [index].mats;
		subMeshCount = convertThreads [index].subMeshCount;
		vertices = convertThreads [index].vertices;
		normals = convertThreads [index].normals;
		uv = convertThreads [index].uv;
		uv2 = convertThreads [index].uv2;
		colors = convertThreads [index].colors;
		matsname = convertThreads [index].matsname;
		triangles = convertThreads [index].triangles;

		int suffix = 0;
		string fullname = mfname + ".obj";
		while (File.Exists (patch + "/" + fullname)) {
			fullname = mfname + "_" + suffix + ".obj";
			suffix++;
		}
		onGenerateOBJ = true;
		MeshToFile (meshfillter, mesh, mats, mfname, patch + "/" + fullname, true);
	}

	void ThreadSetup ()
	{
		assetsToReplace = new List<string> ();
		GameObject[] selectedlist = Selection.gameObjects;
		threadmeshlist = new List<MeshFilter> ();
		for (int i = 0; i < selectedlist.Length; i++) {
			MeshFilter[] meshFilter = selectedlist [i].GetComponentsInChildren<MeshFilter> ();
			if (meshFilter.Length > 0) {
				for (int m = 0; m < meshFilter.Length; m++) {
					threadmeshlist.Add (meshFilter [m]);
				}
			}
		}
		progressstatus = "Converting..";
		threadIndex = 0;
		replacing = false;
		threadCount = threadmeshlist.Count;
		convertThreads = new ThreadInstance[threadCount];
		for (int i = 0; i < convertThreads.Length; i++) {
			convertThreads [i] = new ThreadInstance ();
			convertThreads [i].meshfillter = threadmeshlist [i];
			convertThreads [i].mesh = convertThreads [i].meshfillter.sharedMesh;
			convertThreads [i].mfname = convertThreads [i].meshfillter.name;
			convertThreads [i].mats = convertThreads [i].meshfillter.GetComponent<Renderer> ().sharedMaterials;
			convertThreads [i].subMeshCount = convertThreads [i].mesh.subMeshCount;
			convertThreads [i].vertices = convertThreads [i].mesh.vertices;
			convertThreads [i].normals = convertThreads [i].mesh.normals;
			convertThreads [i].uv = convertThreads [i].mesh.uv;
			convertThreads [i].uv2 = convertThreads [i].mesh.uv2;
			convertThreads [i].colors = convertThreads [i].mesh.colors;
			convertThreads [i].matsname = new string[convertThreads [i].mesh.subMeshCount];

			for (int v = 0; v < convertThreads [i].mats.Length; v++) {
				convertThreads [i].matsname [v] = convertThreads [i].mats [v].name;
			}
			convertThreads [i].triangles = new TriangleData[convertThreads [i].subMeshCount];
			for (int t = 0; t < convertThreads [i].triangles.Length; t++) {
				convertThreads [i].triangles [t].triangles = convertThreads [i].mesh.GetTriangles (t);
			}
		}

	}

	private List<MeshFilter> threadmeshlist;
	private ThreadInstance[] convertThreads;
	private int threadCount;
	private int threadIndex;
	private MeshFilter meshfillter;
	private Mesh mesh;
	private Material[] mats;
	private string[] matsname;
	private string mfname;
	private int subMeshCount;
	private Vector3[] vertices;
	private Vector3[] normals;
	private Vector2[] uv;
	private Vector2[] uv2;
	private Color[] colors;
	private TriangleData[] triangles;

	private List<string> assetsToReplace = new List<string> ();
	private string progressstatus;
	private bool onGenerateOBJ;
	private float progress = 0;
	private bool replacing = false;

	public string MeshToString (MeshFilter mf, Mesh m, Material[] mats, string mfname)
	{

		StringBuilder sb = new StringBuilder ();

		sb.Append ("g ").Append (mfname).Append ("\n");
		foreach (Vector3 v in vertices) {
			sb.Append (string.Format ("v {0} {1} {2}\n", v.x, v.y, v.z));
		}
		sb.Append ("\n");
		foreach (Vector3 v in normals) {
			sb.Append (string.Format ("vn {0} {1} {2}\n", v.x, v.y, v.z));
		}
		sb.Append ("\n");
		foreach (Vector2 v in uv) {
			sb.Append (string.Format ("vt {0} {1}\n", v.x, v.y));
		}
		sb.Append ("\n");
		foreach (Vector2 v in uv2) {
			sb.Append (string.Format ("vt1 {0} {1}\n", v.x, v.y));
		}

		sb.Append ("\n");
		foreach (Vector2 v in uv2) {
			sb.Append (string.Format ("vt2 {0} {1}\n", v.x, v.y));
		}
		sb.Append ("\n");
		foreach (Color c in colors) {
			sb.Append (string.Format ("vc {0} {1} {2} {3}\n", c.r, c.g, c.b, c.a));
		}
		for (int material = 0; material < subMeshCount; material++) {
			sb.Append ("\n");
			sb.Append ("usemtl ").Append (matsname [material]).Append ("\n");
			sb.Append ("usemap ").Append (matsname [material]).Append ("\n");

			int[] triangle = triangles [material].triangles;
			for (int i = 0; i < triangle.Length; i += 3) {
				sb.Append (string.Format ("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
					triangle [i] + 1, triangle [i + 1] + 1, triangle [i + 2] + 1));
			}
		}


		return sb.ToString ();
	}

	public void MeshToFile (MeshFilter mf, Mesh mesh, Material[] mat, string mfname, string filename, bool append)
	{
		try {
			using (StreamWriter sw = new StreamWriter (filename, append)) {
				progressstatus = "Convert " + mfname;
				Debug.Log(mfname+".obj complate");
				sw.WriteLine (MeshToString (mf, mesh, mat, mfname));
				assetsToReplace.Add (filename);

				if (threadIndex < threadCount-1) {
					threadIndex += 1;
					progress = threadIndex / threadCount;

					if(threadIndex >= threadCount - 1)
						Debug.Log("Prepare to import");
					
					ConvertMeshToFile (threadIndex);

				} else {
					Debug.Log("Imported");
					progress = 1;
					progressstatus = "Importing...";
					isRefreshed = false;
					onGenerateOBJ = false;
					replacing = replaceafterconvert;
				}
			}
		} catch (System.Exception) {
		}
	}
}

public class ThreadInstance
{
	public MeshFilter meshfillter;
	public Mesh mesh;
	public Material[] mats;
	public string[] matsname;
	public string mfname;
	public int subMeshCount;
	public Vector3[] vertices;
	public Vector3[] normals;
	public Vector2[] uv;
	public Vector2[] uv2;
	public Color[] colors;
	public TriangleData[] triangles;
	public string fullpatchofobj;
}

public struct TriangleData
{
	public int[] triangles;
}

public struct MeshGroupInstance
{
	public MeshFilter mesh;
	public List<MeshFilter> meshlist;
	public Material[] Mat;
}

public class LimitedMeshInstance
{
	public List<MeshFilter> Meshlist;
	public int VertexCount;
	public bool limited;
}
