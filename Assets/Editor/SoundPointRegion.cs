using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using UnityEditor;

public class SoundPointRegion : MonoBehaviour, ISpline {

	public bool debug = true;
	[SerializeField]
	public Transform Target;
	public AudioClip clip;

	private Transform[] Points;
	private GameObject AudioGameObject;
	private AudioSource Source;

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
		//Points = gameObject.GetComponentsInChildren<Transform>();

		Source = gameObject.GetComponentInChildren<AudioSource>();
		Source.clip = clip;
		Source.Play();
	}

	// Update is called once per frame
	void Update () {
		//Transform t = ClosestPoint(Target);
		SetAudioPosition(FindClosest(Target.position));
	}

	private Transform ClosestPoint (Transform t) {
		if (Points.Length == 0) return null;

		Transform closest = Points[0];
		float maxDist = Vector3.Distance(closest.position, t.position);
		for (int i = 1; i < Points.Length; i++) {
			float curDist = Vector3.Distance(Points[i].position, t.position);
			if (curDist < maxDist) {
				closest = Points[i];
				maxDist = curDist;
			}
		}

		return closest;
	}

	private void SetAudioPosition (Vector3 t) {
		Source.transform.position = t;
	}

	private void renderPoints () {
		if (Points != null) {
			for (int i = 0; i < Points.Length; i++) {
				Gizmos.DrawSphere(Points[i].position, .5f);
			}
		}
	}

	private void OnDrawGizmos () {
		renderPoints();
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
		ResetIndex();
		if (index >= points.Count)
			points.Add(position);
		else
			points.Insert(index, position);
	}


	public void RemoveControlPoint (int index) {
		ResetIndex();
		points.RemoveAt(index);
	}


	SplineIndex uniformIndex;
	SplineIndex Index {
		get {
			if (uniformIndex == null) uniformIndex = new SplineIndex(this);
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
			Vector3.forward * 3,
			Vector3.forward * 6,
			Vector3.forward * 9,
			Vector3.forward * 12
		};
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
}

public class SplineIndex {
	public Vector3[] linearPoints;
	SoundPointRegion spline;


	public int ControlPointCount;


	public SplineIndex (SoundPointRegion spline) {
		this.spline = spline;
		ReIndex();
		ControlPointCount = spline.ControlPointCount;
	}


	public void ReIndex () {
		var searchStepSize = 0.00001f;
		var length = spline.GetLength(searchStepSize);
		var indexSize = Mathf.FloorToInt(length * 2);
		var _linearPoints = new List<Vector3>(indexSize);
		var t = 0f;


		var linearDistanceStep = length / 1024;
		var linearDistanceStep2 = Mathf.Pow(linearDistanceStep, 2);


		var start = spline.GetNonUniformPoint(0);
		_linearPoints.Add(start);
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
		return SoundPointRegion.Interpolate(a, b, c, d, u);
	}
}

[CustomEditor(typeof(SoundPointRegion))]
public class SplineComponentEditor : Editor {
	int hotIndex = -1;
	int removeIndex = -1;
	public GameObject go;

	public override void OnInspectorGUI () {
		EditorGUILayout.HelpBox("Hold Shift and click to append and insert curve points. Backspace to delete points.", MessageType.Info);
		var spline = target as SoundPointRegion;
		GUILayout.BeginHorizontal();
		var closed = GUILayout.Toggle(spline.closed, "Closed", "button");
		if (spline.closed != closed) {
			spline.closed = closed;
			spline.ResetIndex();
		}
		if (GUILayout.Button("Flatten Y Axis")) {
			Undo.RecordObject(target, "Flatten Y Axis");
			//TODO: Flatten(spline.points);
			spline.ResetIndex();
		}
		if (GUILayout.Button("Center around Origin")) {
			Undo.RecordObject(target, "Center around Origin");
			//TODO: CenterAroundOrigin(spline.points);
			spline.ResetIndex();
		}
		GUILayout.EndHorizontal();

		DrawDefaultInspector();
		//EditorGUI.ObjectField(new Rect(0, 0, 100, 100), new UnityEditor.SerializedObject(spline).FindProperty("Target"));
	}

	void OnSceneGUI () {
		var spline = target as SoundPointRegion;


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
		}

		removeIndex = -1;
		serializedObject.ApplyModifiedProperties();


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
		var spline = target as SoundPointRegion;
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
		var spline = target as SoundPointRegion;
		var plane = new Plane(spline.transform.up, spline.transform.position);
		var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		float center;
		if (plane.Raycast(ray, out center)) {
			var hit = ray.origin + ray.direction * center;
			var discSize = HandleUtility.GetHandleSize(hit);
			Handles.DrawWireDisc(hit, spline.transform.up, discSize);
			var p = SearchForClosestPoint(Event.current.mousePosition);


			if ((hit - spline.GetNonUniformPoint(0)).sqrMagnitude < 25) p = 0;
			if ((hit - spline.GetNonUniformPoint(1)).sqrMagnitude < 25) p = 1;


			var sp = spline.GetNonUniformPoint(p);


			var extend = Mathf.Approximately(p, 0) || Mathf.Approximately(p, 1);


			Handles.color = extend ? Color.red : Color.white;
			Handles.DrawLine(hit, sp);
			Handles.color = Color.white;


			var i = 1 + Mathf.FloorToInt(p * (spline.points.Count - 3));


			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.shift) {
				if (extend) {
					if (i == spline.points.Count - 2) i++;
					points.InsertArrayElementAtIndex(i);
					points.GetArrayElementAtIndex(i).vector3Value = spline.transform.InverseTransformPoint(hit);
					hotIndex = i;
				} else {
					i++;
					points.InsertArrayElementAtIndex(i);
					points.GetArrayElementAtIndex(i).vector3Value = spline.transform.InverseTransformPoint(sp);
					hotIndex = i;
				}
				serializedObject.ApplyModifiedProperties();
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
		var spline = target as SoundPointRegion;
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
	static void DrawGizmosLoRes (SoundPointRegion spline, GizmoType gizmoType) {
		Gizmos.color = Color.white;
		DrawGizmo(spline, 64);
	}

	[DrawGizmo(GizmoType.Selected)]
	static void DrawGizmosHiRes (SoundPointRegion spline, GizmoType gizmoType) {
		Gizmos.color = Color.white;
		DrawGizmo(spline, 1024);
	}

	static void DrawGizmo (SoundPointRegion spline, int stepCount) {
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


