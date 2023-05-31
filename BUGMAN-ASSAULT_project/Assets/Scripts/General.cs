using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//REVISIT: i should probably find a way to handle most of this in each objects script

public static class General {	//i.e. PlayerController
	public static GameObject world;
	public static Camera camera;
	public static GameObject cursor;

	private static GameObject selection;
	private static string selectType;

	public static void Update() {
		//delete self destructs on stockpile
		if (selectType == "stockpile") {
			if (Input.GetKeyUp(KeyCode.Delete)) {
				selection.GetComponent<Health>().SelfDestruct();
			}
		}
		//button controls for factory to add and remove build orders
		else if (selectType == "factory") {
			if (Input.GetKeyUp(KeyCode.Alpha1)) {
				selection.GetComponent<Factory>().AddBuildOrder("builder");
			} else if (Input.GetKeyUp(KeyCode.Alpha2)) {
				selection.GetComponent<Factory>().AddBuildOrder("collector");
			} else if (Input.GetKeyUp(KeyCode.Alpha3)) {
				selection.GetComponent<Factory>().AddBuildOrder("tank");
			} else if (Input.GetKeyUp(KeyCode.X)) {
				selection.GetComponent<Factory>().PopBuildOrder();
			}
		}
		//x pops a build order on builder
		else if (selectType == "builder") {
			if (Input.GetKeyUp(KeyCode.X)) {
				selection.GetComponent<Builder>().PopBuildOrder();
			}
		}

		//check for mouse intersection with map
		Ray ray = camera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit) && hit.collider == world.GetComponent<MeshCollider>()) {
			//set cursor location
			cursor.transform.SetPositionAndRotation(hit.point, Quaternion.identity);
		}

		//right click to target location on map
		if (Input.GetMouseButtonUp(1)) {

			//builder move and build orders
			if (selectType == "builder") {
				if (Input.GetKey(KeyCode.Alpha0))
					selection.GetComponent<Builder>().AddBuildOrder("factory", cursor.transform.position);
				else if (Input.GetKey(KeyCode.Alpha9))
					selection.GetComponent<Builder>().AddBuildOrder("turret", cursor.transform.position);
				else if (Input.GetKey(KeyCode.Alpha8))
					selection.GetComponent<Builder>().AddBuildOrder("barracks", cursor.transform.position);
				else if (selection.GetComponent<Orderable>().ClearOrder())
					selection.GetComponent<MoveOrder>().SetDestination(cursor.transform.position, 1);
			}
			//tank move order
			else if (selectType == "tank")
				selection.GetComponent<MoveOrder>().SetDestination(cursor.transform.position, 1);
			//collector move order
			else if (selectType == "collector" && selection.GetComponent<Orderable>().ClearOrder()) {
				selection.GetComponent<MoveOrder>().SetDestination(cursor.transform.position, 1);
				Deselect();
			}
			//factory set flag
			else if (selectType == "factory") {
				selection.GetComponent<Factory>().SetFlag(cursor.transform.position);
				Debug.Log(cursor.transform.position);
				Deselect();
			}
		}

		//left click to deslect
		else if (Input.GetMouseButtonDown(0)) {
			Deselect();
		}

		return;
	}

	public static void Select(GameObject s, string t, string o) {
		//select gameobject and enable cursor if needed
		selection = s;
		selectType = t;
		if (
			selectType == "collector" ||
			selectType == "builder" ||
			selectType == "tank" ||
			selectType == "bugman" ||
			selectType == "factory"
		) {
			cursor.GetComponent<MeshRenderer>().enabled = true;
		}
		
		string message = selectType + " HP: " + selection.GetComponent<Health>().GetCurrHealth();
		message += "/" + selection.GetComponent<Health>().GetMaxHealth();
		camera.GetComponent<CameraControls>().SetSelectionText(message);
		camera.GetComponent<CameraControls>().SetOrdersText(o);

		return;
	}

	public static void Deselect() {
		//remove selection and disable cursor
		selection = null;
		selectType = "";
		cursor.GetComponent<MeshRenderer>().enabled = false;
		camera.GetComponent<CameraControls>().SetSelectionText("Use left click [M0] to select a unit or building.");
		camera.GetComponent<CameraControls>().SetOrdersText("...");
		return;
	}

	public static void Target(GameObject target, string targetType) {
		if (selectType == "collector") {
			if (targetType == "metalsupply")
				selection.GetComponent<Collector>().SetTargetSupply(target);
		}

		else if (selectType == "builder") {
			if (targetType == "gassupply")
				selection.GetComponent<Builder>().AddHarvestOrder(target);
		}
		
		else if (selectType == "tank" || selectType == "turret") {
			if (targetType == "enemy")
				selection.GetComponent<Tank>().SetTarget(target);
		}

		Deselect();
		return;
	}
}
