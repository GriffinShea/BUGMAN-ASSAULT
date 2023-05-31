using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour {
	public int COLLECTOR_CAPACITY = 10;
	public int COLLECTOR_HEALTH = 5;
	public float COLLECTOR_SPEED = 10;

	private GameObject world;
	private GameObject stockpile;
	private GameObject targetSupply;
	private Resources res;

	public bool IsEmpty() { return res.metal == 0; }

	public void SetUp(GameObject w, GameObject s) {
		world = w;
		stockpile = s;
		GetComponent<Health>().SetUp(COLLECTOR_HEALTH, Kill);
		GetComponent<Orderable>().SetUp(
			"collector",
			"[M1] on metal --> collect\n[M1] on terrain --> move to",
			ClearOrder
		);
		GetComponent<MoveOrder>().SetUp(COLLECTOR_SPEED);
		res.metal = 0;
		res.gas = 0;
		res.manpower = 0;
		res.empty = false;
		return;
	}

	public void Kill() {
		GetComponent<Health>().DefaultKill();
		Resources res;
		res.empty = false;
		res.metal = 0;
		res.gas = 0;
		res.manpower = 2;
		stockpile.GetComponent<Stockpile>().DeliverResources(res);
		return;
	}

	public bool ClearOrder() {
		if (IsEmpty()) {
			targetSupply = null;
			return true;
		} else return false;
	}

	public void SetTargetSupply(GameObject target) {
		if (ClearOrder()) {
			targetSupply = target;
			GetComponent<MoveOrder>().SetDestination(targetSupply.transform.position, 3);
		}
		return;
	}

	void Update() {
		if (!GetComponent<Health>().ShouldDie()) {
			//if move order destination is reached and there is a supply
			if (GetComponent<Orderable>().DoOrder()) {
				if (targetSupply != null) {
					if (IsEmpty()) {
						//if the collector is empty it means it just reached the supply so pick up resource and go to stockpile
						res = targetSupply.GetComponent<MetalSupply>().PickupResources(COLLECTOR_CAPACITY);
						GetComponent<MoveOrder>().SetDestination(stockpile.transform.position, 5);
					} else {
						//if collector is not empty it means it just reached the stockpile so deliver resource then go
						//back to supply if it is not empty
						stockpile.GetComponent<Stockpile>().DeliverResources(res);
						res.metal = 0;
						if (res.empty) targetSupply = null;
						else GetComponent<MoveOrder>().SetDestination(targetSupply.transform.position, 3);
					}
				}
			}
		}
		return;
	}
}
