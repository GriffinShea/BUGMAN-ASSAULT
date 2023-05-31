using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : MonoBehaviour {
	public GameObject Builder;
	public GameObject Collector;
	public GameObject Tank;
	public int FACTORY_HEALTH = 40;

	private GameObject world;

	private GameObject stockpile;
	private List<string> buildOrders;
	private float orderTimer;
	private Vector3 flag;

	public void SetUp(GameObject w, GameObject s) {
		world = w;
		stockpile = s;
		buildOrders = new List<string>();
		orderTimer = 0;
		SetFlag(transform.position + transform.forward * -2 * transform.lossyScale.x * GetComponent<MeshFilter>().mesh.bounds.size.x);
		GetComponent<Health>().SetUp(FACTORY_HEALTH);
		return;
	}

	public void SetFlag(Vector3 f) {
		flag = f;
		transform.GetChild(0).transform.position = flag;
		return;
	}

	public void AddBuildOrder(string orderType) {
		//if successful request of materials, add order
		Resources res = GetResources(orderType);
		Debug.Log(res.metal);
		Debug.Log(res.gas);
		Debug.Log(res.manpower);
		if (stockpile.GetComponent<Stockpile>().RemoveResources(res))
			buildOrders.Add(orderType);
		return;
	}

	public void PopBuildOrder() {
		//remove the order from the queue and refund resources
		string orderType = buildOrders[buildOrders.Count - 1];
		buildOrders.RemoveAt(buildOrders.Count - 1);
		Resources res = GetResources(orderType);
		stockpile.GetComponent<Stockpile>().DeliverResources(res);
		return;
	}

	Resources GetResources(string orderType) {
		Resources res;
		res.empty = false;
		res.metal = 0;
		res.gas = 0;
		res.manpower = 0;
		if (orderType == "builder") {
			res.metal = 20;
			res.gas = 20;
			res.manpower = 6;
		} else if (orderType == "collector") {
			res.metal = 10;
			res.gas = 10;
			res.manpower = 2;
		} else if (orderType == "tank") {
			res.metal = 25;
			res.gas = 40;
			res.manpower = 4;
		}
		return res;
	}

	void Update() {
		//if there is a build order, increment the timer and then check whether to produce the unit
		if (buildOrders.Count > 0) {
			orderTimer += Time.deltaTime;
			if (buildOrders[0] == "builder") {
				if (orderTimer > 5) {
					world.GetComponent<World>().CreatePlayerUnit(
						Builder,
						gameObject.transform.position.x,
						gameObject.transform.position.z,
						flag
					);
					buildOrders.RemoveAt(0);
					orderTimer = 0;
				}
			} else if (buildOrders[0] == "collector") {
				if (orderTimer > 1) {
					world.GetComponent<World>().CreatePlayerUnit(
						Collector,
						gameObject.transform.position.x,
						gameObject.transform.position.z,
						flag
					);
					buildOrders.RemoveAt(0);
					orderTimer = 0;
				}
			} else if (buildOrders[0] == "tank") {
				if (orderTimer > 2.5) {
					world.GetComponent<World>().CreatePlayerUnit(
						Tank,
						gameObject.transform.position.x,
						gameObject.transform.position.z,
						flag
					);
					buildOrders.RemoveAt(0);
					orderTimer = 0;
				}
			}
		}
		return;
	}
	
	void OnMouseOver() {
		if (Input.GetMouseButtonUp(0)) {
			General.Select(
				gameObject,
				"factory",
				"[1] --> builder (20|20|6)\n[2] --> collector (10|10|2)\n[3] --> tank (25|40|4)\n[M1] on terrain --> set flag"
			);
		}
		return;
	}
}
