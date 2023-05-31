using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour
{
	private string targetName;

	public void SetUp(string t) {
		targetName = t;
		return;
	}

	private void OnMouseOver() {
		if (Input.GetMouseButtonUp(1)) {
			General.Target(gameObject, targetName);
		}
		return;
	}
}
