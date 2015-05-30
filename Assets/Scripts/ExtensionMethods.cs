using UnityEngine;
using System.Collections;

namespace UnityEngine.TransformExtensions {
	public static class ExtensionMethods {

		/// <summary>
		/// Performs a deep search for the provided name.
		/// </summary>
		/// <param name="trm">The transform to search through</param>
		/// <param name="name">The name of the transform object to search for.</param>
		/// <returns></returns>
		public static Transform FindChildRecursive(this Transform trm, string name) {

			Transform child = null;

			// Loop through top level 
			foreach (Transform t in trm) {
				if (t.name == name) {
					child = t;
					return child;
				} else if (t.childCount > 0) {
					child = t.FindChildRecursive(name);
					if (child) {
						return child;
					}
				}
			}
			return child;
		}
	}
}
