using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Assets.Scripts;

namespace Assets.Editors {
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
			int count = (int)(splineLength / tiling);
			float increment = (float)1 / (splineLength / tiling);

			//if (points.arraySize < 4) return;

			int diff = count - GameObjects.arraySize;
			if (diff < 0) {
				for (int i = 0; i < -diff; i++) {
					GameObject.Destroy(GameObjects.GetArrayElementAtIndex(GameObjects.arraySize - 1).objectReferenceValue as GameObject);
					GameObjects.DeleteArrayElementAtIndex(GameObjects.arraySize - 1);
				}
			} else if (diff > 0) {
				for (int i = 0; i < diff; i++) {
					GameObjects.InsertArrayElementAtIndex(GameObjects.arraySize);
					GameObjects.GetArrayElementAtIndex(GameObjects.arraySize - 1).objectReferenceValue = GameObject.Instantiate(CopyObject.objectReferenceValue);
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
}
