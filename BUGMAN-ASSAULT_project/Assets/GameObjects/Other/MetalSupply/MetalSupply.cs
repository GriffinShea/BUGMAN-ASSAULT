using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalSupply : MonoBehaviour {
	public string SUPPLY_TYPE = "metal";
	public int INITIAL_SUPPLY = 100;

	private int supply;

	public string GetSupplyType() { return SUPPLY_TYPE; }

	public void SetUp() {
		supply = INITIAL_SUPPLY;
		GetComponent<Targetable>().SetUp("metalsupply");
		SetScale();
		return;
	}

	public Resources PickupResources(int capacity) {
		//give as much resource as capacity can take or until depleated
		Resources res;
		res.gas = 0;
		res.manpower = 0;
		res.metal = Mathf.Min(capacity, supply);
		supply -= res.metal;
		res.empty = supply == 0;
		SetScale();
		return res;
	}

	private void SetScale() {
		float scale = (float) 2*supply / INITIAL_SUPPLY + 0.1f;
		transform.localScale = new Vector3(scale, scale, scale);
		return;
	}
}
