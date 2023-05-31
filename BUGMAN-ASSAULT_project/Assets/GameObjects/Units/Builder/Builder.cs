using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour
{
	public GameObject Factory;
	public GameObject FactoryScaffold;
	public int FACTORY_COST = 75;
	public float FACTORY_BUILD_TIME = 12f;
	
	public GameObject Turret;
	public GameObject TurretScaffold;
	public int TURRET_COST = 10;
	public float TURRET_BUILD_TIME = 5f;

	public GameObject Barracks;
	public GameObject BarracksScaffold;
	public int BARRACKS_COST = 20;
	public float BARRACKS_BUILD_TIME = 5f;

	public int HARVESTER_COST = 40;
	public float HARVESTER_BUILD_TIME = 10f;

	public Material ScaffoldGreen;
	public Material ScaffoldBlue;

	public int BUILDER_HEALTH = 10;
	public float BUILDER_SPEED = 5;

	private struct Order {
		public string orderType;
		public Resources res;
		public Vector3 location;
		public GameObject scaffold;
	}

	private GameObject world;
	private GameObject stockpile;
	private List<Order> buildOrders;
	private float buildTimer;

	//REVISIT:
	//ORDERS TO BUILD HARVESTERS ON GAS SUPPLIES ARE HANDLED DIFFERENTLY FROM OTHER ORDERS
	//THIS IS VERY JANK

	public void SetUp(GameObject w, GameObject s) {
		world = w;
		stockpile = s;
		buildOrders = new List<Order>();
		buildTimer = 0;
		GetComponent<Health>().SetUp(BUILDER_HEALTH, Kill);
		GetComponent<Orderable>().SetUp(
			"builder",
			"[M1 + 8] --> barracks (20)\n[M1 + 9] --> turret (10)\n[M1 + 0] --> factory (75)\n[M1] on spout --> gas harvester (40)\n[M1] on terrain --> move to",
			ClearOrder
		);
		GetComponent<MoveOrder>().SetUp(BUILDER_SPEED);
		return;
	}

	public void Kill() {
		GetComponent<Health>().DefaultKill();
		Resources res;
		res.empty = false;
		res.metal = 0;
		res.gas = 0;
		res.manpower = 6;
		stockpile.GetComponent<Stockpile>().DeliverResources(res);
		ClearOrder();
		return;
	}

	public bool ClearOrder() {
		while (buildOrders.Count > 0) PopBuildOrder();
		return true;
	}

	public void AddBuildOrder(string orderType, Vector3 location) {
		//attempt to request enough metal based on type
		Order order;
		order.orderType = orderType;
		order.location = location;
		order.scaffold = null;

		order.res.empty = false;
		order.res.gas = 0;
		order.res.manpower = 0;
		order.res.metal = 10000000;

		bool success = false;
		if (order.orderType == "factory") {
			order.res.metal = FACTORY_COST;
			order.scaffold = world.GetComponent<World>().InstantiateAtPos(FactoryScaffold, order.location.x, order.location.z);
		} else if (order.orderType == "turret") {
			order.res.metal = TURRET_COST;
			order.scaffold = world.GetComponent<World>().InstantiateAtPos(TurretScaffold, order.location.x, order.location.z);
		} else if (order.orderType == "barracks") {
			order.res.metal = BARRACKS_COST;
			order.scaffold = world.GetComponent<World>().InstantiateAtPos(BarracksScaffold, order.location.x, order.location.z);
		}
		success = stockpile.GetComponent<Stockpile>().RemoveResources(order.res);

		//if successful, add order
		if (success) {
			buildOrders.Add(order);
			order.scaffold.GetComponent<MeshRenderer>().material = ScaffoldGreen;

			//if this is the first buildOrder, set a new destination immediately
			if (buildOrders.Count == 1) {
				buildTimer = 0;
				GetComponent<MoveOrder>().SetDestination(buildOrders[0].location, 4);
			}
		}
		else Destroy(order.scaffold);


		return;
	}

	public void AddHarvestOrder(GameObject gasSupply) {
		//attempt to request enough metal
		Resources res;
		res.metal = HARVESTER_COST;
		res.gas = 0;
		res.manpower = 0;
		res.empty = false;
		bool success = stockpile.GetComponent<Stockpile>().RemoveResources(res);

		//if successful, add order
		if (success) {
			Order order;
			order.orderType = "harvester";
			order.res = res;
			order.scaffold = gasSupply;
			order.location = order.scaffold.transform.position;
			order.scaffold.GetComponent<GasSupply>().EnableGreenScaffold();
			buildOrders.Add(order);

			//if this is the first buildOrder, set a new destination immediately
			if (buildOrders.Count == 1) {
				buildTimer = 0;
				GetComponent<MoveOrder>().SetDestination(buildOrders[0].location, 4);
			}
		}
	}

	public void PopBuildOrder() {
		//remove the order from the queue and return the resources to the stockpile
		if (buildOrders.Count > 0) {
			string orderType = buildOrders[buildOrders.Count - 1].orderType;
			if (orderType != "harvester") Destroy(buildOrders[buildOrders.Count - 1].scaffold);
			else buildOrders[buildOrders.Count - 1].scaffold.GetComponent<GasSupply>().DisableScaffold();
			stockpile.GetComponent<Stockpile>().DeliverResources(buildOrders[buildOrders.Count - 1].res);
			buildOrders.RemoveAt(buildOrders.Count - 1);

			if (buildOrders.Count == 0)
				GetComponent<MoveOrder>().SetDestination(transform.position, 1);
		}
		return;
	}

	void Update() {
		if (!GetComponent<Health>().ShouldDie()) {
			//if orderable component has nothing to do, and there are orders in the queue
			if (GetComponent<Orderable>().DoOrder()) {
				if (buildOrders.Count > 0) {
					//add to build timer, make scaffold of object currently being built blue
					buildTimer += Time.deltaTime;

					//different procedure for harvester order and other buildings
					if (buildOrders[0].orderType == "harvester") {
						//make sure scaffold is blue
						buildOrders[0].scaffold.GetComponent<GasSupply>().EnableBlueScaffold();

						//if timer is up then build harvester and pop from orders and set next destination if necessary
						if (buildTimer > HARVESTER_BUILD_TIME) {
							buildOrders[0].scaffold.GetComponent<GasSupply>().BuildHarvester();
							buildOrders.RemoveAt(0);
							if (buildOrders.Count > 0) {
								buildTimer = 0;
								GetComponent<MoveOrder>().SetDestination(buildOrders[0].location, 4);
							}
						}
					} else {
						//make sure scaffold is blue
						buildOrders[0].scaffold.GetComponent<MeshRenderer>().material = ScaffoldBlue;

						//set prefab if timer up
						GameObject prefab = null;
						if (buildOrders[0].orderType == "factory" && buildTimer > FACTORY_BUILD_TIME)
							prefab = Factory;
						else if (buildOrders[0].orderType == "turret" && buildTimer > TURRET_BUILD_TIME)
							prefab = Turret;
						else if (buildOrders[0].orderType == "barracks" && buildTimer > BARRACKS_BUILD_TIME)
							prefab = Barracks;

						//if prefab is set create a new building
						if (prefab != null) {
							world.GetComponent<World>().CreatePlayerBuilding(
								prefab,
								buildOrders[0].location.x,
								buildOrders[0].location.z
							);
							Destroy(buildOrders[0].scaffold);

							//begin moving to next build order location
							buildOrders.RemoveAt(0);
							if (buildOrders.Count > 0) {
								buildTimer = 0;
								GetComponent<MoveOrder>().SetDestination(buildOrders[0].location, 4);
							}
						}
					}

				}
			}
		} else { }
		return;
	}
}
