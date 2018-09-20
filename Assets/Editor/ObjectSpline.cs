using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using UnityEditor;

public class ObjectSpline : MonoBehaviour, ISpline {

	public float Tiling = 2;
	public bool debug = true;

	public GameObject CopyObject;

	public List<GameObject> GameObjects = new List<GameObject>();

	public bool closed = false;
	public List<Vector3> points = new List<Vector3>();
	public float? length;

	public int ControlPointCount {
		get {
			return points.Count;
		}
		set { }
	}

	internal static Vector3 Interpolate (Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u) {
		return (
			0.5f *
			(
				(-a + 3f * b - 3f * c + d) *
				(u * u * u) +
				(2f * a - 5f * b + 4f * c - d) *
				(u * u) +
				(-a + c) *
				u + 2f * b
			)
		);
	}

	void OnSceneGui () {

	}

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {
	}

	public Vector3 GetNonUniformPoint (float t) {
		switch (points.Count) {
			case 0:
				return Vector3.zero;
			case 1:
				return transform.TransformPoint(points[0]);
			case 2:
				return transform.TransformPoint(Vector3.Lerp(points[0], points[1], t));
			case 3:
				return transform.TransformPoint(points[1]);
			default:
				return Hermite(t);
		}
	}

	public void InsertControlPoint (int index, Vector3 position) {
		Vector3 back = GetBackward(0);
		Vector3 front = GetForward(1);


		ResetIndex();
		if (index >= points.Count)
			points.Add(position);
		else
			points.Insert(index, position);

		Debug.Log("Adding index " + index);

		if (index == 1 && points.Count > 1) {
			Debug.Log("Appended front");
			points[0] = position;
		}

		if (index == points.Count - 2 && points.Count > 1) {
			Debug.Log("Appended back");
			points[points.Count - 1] = position;
		}
	}


	public void RemoveControlPoint (int index) {
		ResetIndex();
		points.RemoveAt(index);
	}


	ObjectSplineIndex uniformIndex;
	ObjectSplineIndex Index {
		get {
			if (uniformIndex == null) uniformIndex = new ObjectSplineIndex(this);
			return uniformIndex;
		}
	}

	public void ResetIndex () {
		uniformIndex = null;
		length = null;
	}

	public Vector3 Hermite (float t) {
		var count = points.Count - (closed ? 0 : 3);
		var i = Mathf.Min(Mathf.FloorToInt(t * (float)count), count - 1);
		var u = t * (float)count - (float)i;
		var a = GetPointByIndex(i);
		var b = GetPointByIndex(i + 1);
		var c = GetPointByIndex(i + 2);
		var d = GetPointByIndex(i + 3);
		return transform.TransformPoint(Interpolate(a, b, c, d, u));
	}

	public Vector3 GetPointByIndex (int i) {
		if (i < 0) i += points.Count;
		return points[i % points.Count];
	}

	public Vector3 GetControlPoint (int index) {
		return points[index];
	}

	void Reset () {
		points = new List<Vector3>() {
			transform.forward,
			transform.forward * 3,
			transform.forward * 5,
			transform.forward* 7
		};

		GameObjects = new List<GameObject>();
	}

	void OnValidate () {
		if (uniformIndex != null) uniformIndex.ReIndex();
	}

	public void SetControlPoint (int index, Vector3 position) {
		ResetIndex();
		points[index] = position;
	}

	public Vector3 GetRight (float t) {
		var A = GetPoint(t - 0.001f);
		var B = GetPoint(t + 0.001f);
		var delta = (B - A);
		return new Vector3(-delta.z, 0, delta.x).normalized;
	}


	public Vector3 GetForward (float t) {
		var A = GetPoint(t - 0.001f);
		var B = GetPoint(t + 0.001f);
		return (B - A).normalized;
	}


	public Vector3 GetUp (float t) {
		var A = GetPoint(t - 0.001f);
		var B = GetPoint(t + 0.001f);
		var delta = (B - A).normalized;
		return Vector3.Cross(delta, GetRight(t));
	}


	public Vector3 GetPoint (float t) {
		return Index.GetPoint(t);
	}


	public Vector3 GetLeft (float t) {
		return -GetRight(t);
	}


	public Vector3 GetDown (float t) {
		return -GetUp(t);
	}


	public Vector3 GetBackward (float t) {
		return -GetForward(t);
	}

	public float getT(int i) {
		return Index.GetT(i);
	}
	 
	public float GetLength (float step = 0.001f) {
		var D = 0f;
		var A = GetNonUniformPoint(0);
		for (var t = 0f; t < 1f; t += step) {
			var B = GetNonUniformPoint(t);
			var delta = (B - A);
			D += delta.magnitude;
			A = B;
		}
		return D;
	}

	public Vector3 GetDistance (float distance) {
		if (length == null) length = GetLength();
		return uniformIndex.GetPoint(distance / length.Value);
	}

	public Vector3 FindClosest (Vector3 worldPoint) {
		var smallestDelta = float.MaxValue;
		var step = 1f / 1024;
		var closestPoint = Vector3.zero;
		for (var i = 0; i <= 1024; i++) {
			var p = GetPoint(i * step);
			var delta = (worldPoint - p).sqrMagnitude;
			if (delta < smallestDelta) {
				closestPoint = p;
				smallestDelta = delta;
			}
		}
		return closestPoint;
	}

	public float FindClosestT (Vector3 worldPoint) {
		float t = 0;
		var smallestDelta = float.MaxValue;
		var step = 1f / 1024;
		var closestPoint = Vector3.zero;
		for (var i = 0; i <= 1024; i++) {
			var p = GetPoint(i * step);
			var delta = (worldPoint - p).sqrMagnitude;
			if (delta < smallestDelta) {
				closestPoint = p;
				smallestDelta = delta;
				t = i * step;
			}
		}
		return t;
	}

	//public void UpdateGameObjects () {

	//	if (points.Count < 4 || Index == null) return;

	//	int diff = points.Count - GameObjects.Count;
	//	if (diff < 0) {
	//		for (int i = 0; i < -diff; i++) {
	//			GameObjects.RemoveAt(GameObjects.Count - 1);
	//		}
	//	} else if (diff > 0) {
	//		for (int i = 0; i < diff; i++) {
	//			GameObjects.Add(GameObject.Instantiate(CopyObject));
	//		}
	//	}

	//	for (int i = 0; i < points.Count; i++) {
	//		GameObjects[i].transform.position = points[i];
	//		GameObjects[i].transform.rotation.SetLookRotation(GetForward(FindClosestT(points[i])));
	//	}

	//}
}

public class ObjectSplineIndex {
	public Vector3[] linearPoints;
	public float[] indexT;
	ObjectSpline spline;

	public int ControlPointCount;


	public ObjectSplineIndex (ObjectSpline spline) {
		this.spline = spline;
		ReIndex();
		ControlPointCount = spline.ControlPointCount;
	}


	public void ReIndex () {
		var searchStepSize = 0.00001f;
		var length = spline.GetLength(searchStepSize);
		var indexSize = Mathf.FloorToInt(length * 2);
		var _linearPoints = new List<Vector3>(indexSize);
		var _indexT = new List<float>(indexSize);
		var t = 0f;


		var linearDistanceStep = length / 1024;
		var linearDistanceStep2 = Mathf.Pow(linearDistanceStep, 2);


		var start = spline.GetNonUniformPoint(0);
		_linearPoints.Add(start);
		_indexT.Add(0);
		while (t <= 1f) {
			var current = spline.GetNonUniformPoint(t);
			while ((current - start).sqrMagnitude <= linearDistanceStep2) {
				t += searchStepSize;
				current = spline.GetNonUniformPoint(t);
			}
			start = current;
			_linearPoints.Add(current);
		}
		linearPoints = _linearPoints.ToArray();
		indexT = _indexT.ToArray();
	}


	public Vector3 GetPoint (float t) {
		var sections = linearPoints.Length - (spline.closed ? 0 : 3);
		var i = Mathf.Min(Mathf.FloorToInt(t * (float)sections), sections - 1);
		var count = linearPoints.Length;
		if (i < 0) i += count;
		var u = t * (float)sections - (float)i;
		var a = linearPoints[(i + 0) % count];
		var b = linearPoints[(i + 1) % count];
		var c = linearPoints[(i + 2) % count];
		var d = linearPoints[(i + 3) % count];
		return ObjectSpline.Interpolate(a, b, c, d, u);
	}

	public float GetT(int i) {

		return indexT[i];
	}
}

[CustomEditor(typeof(ObjectSpline))]
class ObjectSplineEditor : Editor {
	int hotIndex = -1;
	int removeIndex = -1;
	public GameObject go;

	public override void OnInspectorGUI () {
		EditorGUILayout.HelpBox("Hold Shift and click to append and insert curve points. Backspace to delete points.", MessageType.Info);
		var spline = target as ObjectSpline;
		GUILayout.BeginHorizontal();
		var closed = GUILayout.Toggle(spline.closed, "Closed", "button");
		if (spline.closed != closed) {
			spline.closed = closed;
			spline.ResetIndex();
		}
		if (GUILayout.Button("Flatten Y Axis")) {
			Undo.RecordObject(target, "Flatten Y Axis");
			Flatten(spline.points);
			spline.ResetIndex();
		}
		if (GUILayout.Button("Center around Origin")) {
			Undo.RecordObject(target, "Center around Origin");
			CenterAroundOrigin(spline.points);
			spline.ResetIndex();
		}
		GUILayout.EndHorizontal();

		DrawDefaultInspector();
		//EditorGUI.ObjectField(new Rect(0, 0, 100, 100), new UnityEditor.SerializedObject(spline).FindProperty("Target"));
	}

	void OnSceneGUI () {
		var spline = target as ObjectSpline;


		var e = Event.current;
		GUIUtility.GetControlID(FocusType.Passive);


		var mousePos = (Vector2)Event.current.mousePosition;
		var view = SceneView.currentDrawingSceneView.camera.ScreenToViewportPoint(Event.current.mousePosition);
		var mouseIsOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
		if (mouseIsOutside) return;

		var points = serializedObject.FindProperty("points");
		if (Event.current.shift) {
			if (spline.closed)
				ShowClosestPointOnClosedSpline(points);
			else
				ShowClosestPointOnOpenSpline(points);
		}

		for (int i = 0; i < spline.points.Count; i++) {
			var prop = points.GetArrayElementAtIndex(i);
			var point = prop.vector3Value;
			var wp = spline.transform.TransformPoint(point);


			if (hotIndex == i) {
				var newWp = Handles.PositionHandle(wp, Tools.pivotRotation == PivotRotation.Global ? Quaternion.identity : spline.transform.rotation);
				var delta = spline.transform.InverseTransformDirection(newWp - wp);
				if (delta.sqrMagnitude > 0) {
					prop.vector3Value = point + delta;
					spline.ResetIndex();
					UpdateGameObjects();
				}
				HandleCommands(wp);
			}

			Handles.color = i == 0 | i == spline.points.Count - 1 ? Color.red : Color.white;
			var buttonSize = HandleUtility.GetHandleSize(wp) * 0.1f;
			if (Handles.Button(wp, Quaternion.identity, buttonSize, buttonSize, Handles.SphereHandleCap))
				hotIndex = i;

			var v = SceneView.currentDrawingSceneView.camera.transform.InverseTransformPoint(wp);
			var labelIsOutside = v.z < 0;
			if (!labelIsOutside) Handles.Label(wp, i.ToString());

		}

		if (removeIndex >= 0 && points.arraySize > 4) {
			points.DeleteArrayElementAtIndex(removeIndex);
			spline.ResetIndex();
			UpdateGameObjects();
		}

		removeIndex = -1;
		serializedObject.ApplyModifiedProperties();


	}

	public void UpdateGameObjects () {

		var GameObjects = serializedObject.FindProperty("GameObjects");
		var spline = target as ObjectSpline;
		var points = serializedObject.FindProperty("points");
		var CopyObject = serializedObject.FindProperty("CopyObject");

		float tiling = serializedObject.FindProperty("Tiling").floatValue;

		float splineLength = spline.GetLength();
		int count = (int) (splineLength / tiling);
		float increment = (float) 1/(splineLength/tiling);

		//if (points.arraySize < 4) return;

		int diff = count - GameObjects.arraySize;
		if (diff < 0 ) {
			for (int i = 0; i < -diff; i++) {
				GameObject.Destroy(GameObjects.GetArrayElementAtIndex(GameObjects.arraySize - 1).objectReferenceValue as GameObject);
				GameObjects.DeleteArrayElementAtIndex(GameObjects.arraySize - 1);
			}
		} else if (diff > 0) {
			for (int i = 0; i < diff; i++) {
				GameObjects.InsertArrayElementAtIndex(GameObjects.arraySize);
				GameObjects.GetArrayElementAtIndex(GameObjects.arraySize-1).objectReferenceValue = GameObject.Instantiate(CopyObject.objectReferenceValue);
			}
		}

		float t = 0.001f;
		for (int i = 0; i < count; i++) {
			Vector3 newPos = spline.GetPoint(t);
			Vector3 pointFwd = spline.GetLeft(t);
			Vector3 pointUp = spline.GetDown(t);
			GameObject go = (GameObjects.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
			if (go != null) {
				go.transform.parent = spline.transform;
				go.transform.position = newPos;
				go.transform.LookAt(pointFwd + newPos, pointUp);
				Debug.Log(pointFwd);
			} else {
				//GameObject.Destroy(GameObjects.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
				GameObjects.DeleteArrayElementAtIndex(i);
			}
			t += increment;
		}
	}

	void HandleCommands (Vector3 wp) {
		if (Event.current.type == EventType.ExecuteCommand) {
			if (Event.current.commandName == "FrameSelected") {
				SceneView.currentDrawingSceneView.Frame(new Bounds(wp, Vector3.one * 10), false);
				Event.current.Use();
			}
		}
		if (Event.current.type == EventType.KeyDown) {
			if (Event.current.keyCode == KeyCode.Backspace) {
				removeIndex = hotIndex;
				Event.current.Use();
			}
		}
	}

	void ShowClosestPointOnClosedSpline (SerializedProperty points) {
		var spline = target as ObjectSpline;
		var plane = new Plane(spline.transform.up, spline.transform.position);
		var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		float center;
		if (plane.Raycast(ray, out center)) {
			var hit = ray.origin + ray.direction * center;
			Handles.DrawWireDisc(hit, spline.transform.up, 5);
			var p = SearchForClosestPoint(Event.current.mousePosition);
			var sp = spline.GetNonUniformPoint(p);
			Handles.DrawLine(hit, sp);


			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift) {
				var i = (Mathf.FloorToInt(p * spline.points.Count) + 2) % spline.points.Count;
				points.InsertArrayElementAtIndex(i);
				points.GetArrayElementAtIndex(i).vector3Value = spline.transform.InverseTransformPoint(sp);
				serializedObject.ApplyModifiedProperties();
				hotIndex = i;
			}
		}
	}


	void ShowClosestPointOnOpenSpline (SerializedProperty points) {
		var spline = target as ObjectSpline;
		//var plane = new Plane(spline.transform.up, spline.transform.position);
		var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		RaycastHit hit;
		float center = 100;
		if (Physics.Raycast(ray, out hit, 100)) {
			//var hit = ray.origin + ray.direction * center;
			var discSize = HandleUtility.GetHandleSize(hit.point);
			Handles.DrawWireDisc(hit.point, spline.transform.up, discSize);
			var p = SearchForClosestPoint(Event.current.mousePosition);


			if ((hit.point - spline.GetNonUniformPoint(0)).sqrMagnitude < 25) p = 0;
			if ((hit.point - spline.GetNonUniformPoint(1)).sqrMagnitude < 25) p = 1;


			var sp = spline.GetNonUniformPoint(p);


			if (false) {
				p = Mathf.Round(p);
			}

			var extend = Mathf.Approximately(p, 0) || Mathf.Approximately(p, 1);
			var extendL = Mathf.Approximately(p, 0);

			Handles.color = extend ? Color.red : Color.white;
			Handles.DrawLine(hit.point, sp);
			Handles.color = Color.white;


			var i = 1 + Mathf.FloorToInt(p * (spline.points.Count - 3));


			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift) {
				if (extend) {
					if (i == spline.points.Count - 2) {
						i++;
					}
					points.InsertArrayElementAtIndex(i);
					points.GetArrayElementAtIndex(i).vector3Value = spline.transform.InverseTransformPoint(hit.point);

					if (extendL) {
						points.GetArrayElementAtIndex(i - 1).vector3Value = points.GetArrayElementAtIndex(i).vector3Value + spline.GetBackward(0);
					} else {
						points.GetArrayElementAtIndex(i + 1).vector3Value = points.GetArrayElementAtIndex(i).vector3Value + spline.GetForward(1);
					}

					hotIndex = i;
				} else {
					i++;
					points.InsertArrayElementAtIndex(i);
					points.GetArrayElementAtIndex(i).vector3Value = spline.transform.InverseTransformPoint(sp);
					hotIndex = i;
				}
				serializedObject.ApplyModifiedProperties();
				UpdateGameObjects();
			}
		}
	}

	void Flatten (List<Vector3> points) {
		for (int i = 0; i < points.Count; i++) {
			points[i] = Vector3.Scale(points[i], new Vector3(1, 0, 1));
		}
	}


	void CenterAroundOrigin (List<Vector3> points) {
		var center = Vector3.zero;
		for (int i = 0; i < points.Count; i++) {
			center += points[i];
		}
		center /= points.Count;
		for (int i = 0; i < points.Count; i++) {
			points[i] -= center;
		}
	}


	float SearchForClosestPoint (Vector2 screenPoint, float A = 0f, float B = 1f, float steps = 1000) {
		var spline = target as ObjectSpline;
		var smallestDelta = float.MaxValue;
		var step = (B - A) / steps;
		var closestI = A;
		for (var i = 0; i <= steps; i++) {
			var p = spline.GetNonUniformPoint(i * step);
			var gp = HandleUtility.WorldToGUIPoint(p);
			var delta = (screenPoint - gp).sqrMagnitude;
			if (delta < smallestDelta) {
				closestI = i;
				smallestDelta = delta;
			}
		}
		return closestI * step;
	}


	[DrawGizmo(GizmoType.NonSelected)]
	static void DrawGizmosLoRes (ObjectSpline spline, GizmoType gizmoType) {
		Gizmos.color = Color.white;
		DrawGizmo(spline, 64);
	}

	[DrawGizmo(GizmoType.Selected)]
	static void DrawGizmosHiRes (ObjectSpline spline, GizmoType gizmoType) {
		Gizmos.color = Color.white;
		DrawGizmo(spline, 1024);
	}

	static void DrawGizmo (ObjectSpline spline, int stepCount) {
		if (spline.points.Count > 0) {
			var P = 0f;
			var start = spline.GetNonUniformPoint(0);
			var step = 1f / stepCount;
			do {
				P += step;
				var here = spline.GetNonUniformPoint(P);
				Gizmos.DrawLine(start, here);
				start = here;
			} while (P + step <= 1);
		}
	}
}


