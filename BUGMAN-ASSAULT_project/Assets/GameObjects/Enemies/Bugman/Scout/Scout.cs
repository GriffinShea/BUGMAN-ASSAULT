using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Scout : MonoBehaviour {
	public int SCOUT_HEALTH = 10;
	public float SCOUT_RANGE = 30f;
	public float SCOUT_TURN_SPEED = 1f;
	public float SCOUT_WALK_SPEED = 0.5f;
	public float SCOUT_RUN_SPEED = 2f;

	private GameObject world;
	private GameObject hive;

	private string state;
	private Vector3 target;

	public void SetUp(GameObject w, GameObject h) {
		world = w;
		hive = h;

		GetComponent<Health>().SetUp(SCOUT_HEALTH, Kill);
		GetComponent<Targetable>().SetUp("enemy");

		GetComponent<Animator>().SetBool("InCombat", false);
		GetComponent<NavMeshAgent>().speed = 1f;
		GetComponent<NavMeshAgent>().stoppingDistance = 10f;

		SetupExplore();
		return;
	}

	public void Kill() {
		SetupDead();
		return;
	}

	private void Update() {
		if (state != "dead") {
			if (state == "explore") {
				float angle = Random.Range(-SCOUT_TURN_SPEED, SCOUT_TURN_SPEED);
				transform.rotation *= Quaternion.AngleAxis(angle, transform.up);

				GameObject targetObject = world.GetComponent<World>().FindClosestPlayerObject(transform.position, SCOUT_RANGE);
				if (targetObject != null) {
					target = targetObject.transform.position;
					SetupReport();
				}

			}
			
			else if (state == "report") {
				if (GetComponent<NavMeshAgent>().remainingDistance <= GetComponent<NavMeshAgent>().stoppingDistance) {
					hive.GetComponent<Hive>().Report(target);
					SetupExplore();
				}
			}
		}
		return;
	}

	private void SetupExplore() {
		state = "explore";
		GetComponent<Animator>().SetFloat("Speed", SCOUT_WALK_SPEED);
		GetComponent<NavMeshAgent>().isStopped = true;
		return;
	}

	private void SetupReport() {
		state = "report";
		GetComponent<Animator>().SetFloat("Speed", SCOUT_RUN_SPEED);
		GetComponent<NavMeshAgent>().SetDestination(hive.transform.position);
		GetComponent<NavMeshAgent>().isStopped = false;
		return;
	}

	private void SetupDead() {
		state = "dead";
		GetComponent<Animator>().SetTrigger("Die");
		GetComponent<NavMeshAgent>().isStopped = true;
		return;
	}
}
