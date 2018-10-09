
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
	public class GameEventHandler : MonoBehaviour {

		private Dictionary<string, Action> EventListners = new Dictionary<string, Action>();

		private Queue<GameEvent> EventQueue = new Queue<GameEvent>();

		// Use this for initialization
		void Start () {

		}

		// Update is called once per frame
		void Update () {
			if (EventQueue.Count > 0) {
				GameEvent e = EventQueue.Dequeue();
				if (e != null) {
					TryExecute(e);
				}
			}
		}

		public bool AddEventListner (string msg, Action a) {
			try {
				EventListners.Add(msg, a);
			} catch (ArgumentException e) {
				Debug.Log("Event Listener already exists for this key. Please try a unique msg.");
				return false;
			}
			return true;
		}

		public void SendEvent (GameEvent e) {
			EventQueue.Enqueue(e);
		}

		public void TryExecute (GameEvent e) {
			// TODO: 
			// Valildate 
			Action a;
			if (EventListners.TryGetValue(e.Msg, out a) && a != null) {
				a();
			}
		}
	}
}
