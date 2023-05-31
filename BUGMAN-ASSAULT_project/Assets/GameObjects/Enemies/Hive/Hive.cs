using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hive : MonoBehaviour {
	public GameObject Warrior;
	public GameObject Scout;
	public int HIVE_HEALTH = 100;

	private GameObject world;

	private float worldTimer;
	private float buildTimer;
	private int buildCounter;

	private List<GameObject> warriors = new List<GameObject>();
	private List<GameObject> scouts = new List<GameObject>();

	public void SetUp(GameObject w) {
		world = w;
		buildTimer = 0;
		buildCounter = -1;
		
		GetComponent<Health>().SetUp(HIVE_HEALTH, Kill);

		GetComponent<Targetable>().SetUp("enemy");
		return;
	}

	public void Kill() {
		for (int i = 0; i < warriors.Count; i++) {
			warriors[0].GetComponent<Health>().SelfDestruct();
		}

		for (int i = 0; i < scouts.Count; i++) {
			scouts[0].GetComponent<Health>().SelfDestruct();
		}

		GetComponent<Health>().DefaultKill();

		return;
	}

	private void Update() {
		worldTimer += Time.deltaTime;
		buildTimer += Time.deltaTime;

		//spawn a new enemy every 5 seconds, every fifth enemy is a scout
		if (buildTimer > 5) {
			buildTimer = 0;
			buildCounter++;
			GameObject prefab = Warrior;
			if (buildCounter == 0) prefab = Scout;
			else if (buildCounter == 4) buildCounter = -1;

			//over time, enemies will spawn facing closer towards the direction of the stockpile (takes 5 minutes)
			float angle = Random.Range(-1f, 1f) * 2 * Mathf.PI;
			Vector3 toStockpile3D = world.GetComponent<World>().GetStockpileLocation() - transform.position;
			Vector2 toStockpile2D = new Vector2(toStockpile3D.x, toStockpile3D.z);
			float angleToStockpile = -Mathf.Acos(Vector2.Dot(toStockpile2D.normalized, (new Vector2(0f, 1f)).normalized));
			float timeFactor = Mathf.Min(worldTimer / 300.0f, 1);
			angle = angle * (1 - timeFactor) + angleToStockpile * timeFactor;

			GameObject enemy = world.GetComponent<World>().CreateEnemy(
				prefab,
				transform.position.x + Mathf.Sin(angle),
				transform.position.z + Mathf.Cos(angle)
			);
			enemy.transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up);

			if (prefab == Warrior) warriors.Add(enemy);
			else scouts.Add(enemy);
		}
		
		//remove dead units from lists
		for (int i = warriors.Count - 1; i >= 0; i--) {
			if (warriors[i].GetComponent<Health>().ShouldDie()) {
				warriors.RemoveAt(i);
			}
		}
		for (int i = scouts.Count - 1; i >= 0; i--) {
			if (scouts[i].GetComponent<Health>().ShouldDie()) {
				scouts.RemoveAt(i);
			}
		}

		return;
	}

	public void Report(Vector3 dest) {
		for (int i = 0; i < warriors.Count; i++) {
			warriors[i].GetComponent<Warrior>().SetDestination(dest);
		}
		return;
	}
}
