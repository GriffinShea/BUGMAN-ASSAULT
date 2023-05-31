using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Help {
	public static Vector2 GetGameObject2DPos(GameObject go) {
		return new Vector2(go.transform.position.x, go.transform.position.z);
	}
	
	public static float GetGameObjectHeight(GameObject go) {
		if (go.GetComponent<MeshFilter>() != null) {
			return go.transform.lossyScale.y * go.GetComponent<MeshFilter>().mesh.bounds.size.y;
		} else if (go.GetComponent<SkinnedMeshRenderer>()) {
			return go.transform.lossyScale.y * go.GetComponent<SkinnedMeshRenderer>().bounds.size.y;
		} else {
			return 0;
		}
	}
}

