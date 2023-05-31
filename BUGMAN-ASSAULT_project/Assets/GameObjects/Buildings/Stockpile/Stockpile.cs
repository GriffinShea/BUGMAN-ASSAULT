using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stockpile : MonoBehaviour {
	public static int STOCKPILE_HEALTH = 100;
	public static int INITIAL_METAL = 100;
	public static int INITIAL_GAS = 25;

	private int metalCount;
	private int gasCount;
	private int manpowerTotal;
	private int manpowerUsed;

	public int GetMetalCount() { return metalCount; }

	public void SetUp() {
		metalCount = INITIAL_METAL;
		gasCount = INITIAL_GAS;
		gameObject.GetComponent<Health>().SetUp(STOCKPILE_HEALTH);
		return;
	}

	public void SecondSetup(GameObject world) {
		manpowerUsed = world.GetComponent<World>().GetManpowerUsed();
		return;
	}

	public void DeliverResources(Resources resources) {
		metalCount += resources.metal;
		gasCount += resources.gas;
		manpowerUsed -= resources.manpower;
		return;
	}

	public bool RemoveResources(Resources resources) {
		if (
			resources.metal > metalCount
			|| resources.gas > gasCount
			|| resources.manpower > manpowerTotal - manpowerUsed
		) return false;

		metalCount -= resources.metal;
		gasCount -= resources.gas;
		manpowerUsed += resources.manpower;

		return true;
	}

	public void AddManpowerTotal(int manpower) {
		manpowerTotal += manpower;
		return;
	}

	public void ReduceManpowerTotal(int manpower) {
		manpowerTotal -= manpower;
		return;
	}

	public string GetResourceString() {
		string message;
		message = "Metal: " + metalCount;
		message += " | Gas: " + gasCount;
		message += " | Manpower: " + manpowerUsed + "/" + manpowerTotal + " used";
		return message;
	}

	private void OnMouseOver() {
		if (Input.GetMouseButtonUp(0)) {
			General.Select(gameObject, "stockpile", "[DELETE] --> surrender");
		}
		return;
	}
}
