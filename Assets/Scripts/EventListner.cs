using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
	public class EventListner {

		private Action ListenerAction = () => { };
		private string TargetMsg = "";

		public EventListner (string msg, Action a) {
			TargetMsg = msg;
			ListenerAction = a;
		}

		public void ValidateExecute (string msg, System.Object o) {
			// TODO: 
			// Valildate 
			// Execute ListenerAction	 
		}
	}
}
