using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Warrior : MonoBehaviour {
	public int WARRIOR_HEALTH = 20;
	public float WARRIOR_AGGRO_RANGE = 35f;
	public float WARRIOR_ATTACK_RANGE = 5f;
	public float WARRIOR_TURN_SPEED = 1.75f;
	public float WARRIOR_WALK_SPEED = 0.5f;
	public float WARRIOR_RUN_SPEED = 2f;

	private GameObject world;
	private GameObject hive;

	private string state;
	private float wanderTimer;
	private float attackTimer;
	private Vector3 destination;
	private GameObject target;

	public void SetUp(GameObject w, GameObject h) {
		world = w;
		hive = h;

		GetComponent<Health>().SetUp(WARRIOR_HEALTH, Kill);
		GetComponent<Targetable>().SetUp("enemy");

		GetComponent<NavMeshAgent>().speed = 1f;
		GetComponent<NavMeshAgent>().stoppingDistance = WARRIOR_AGGRO_RANGE / 2f;

		SetupWander();
		return;
	}

	public void Kill() {
		SetupDead();
		return;
	}

	private void Update() {
		if (state != "dead") {
			if (state == "attack") {
				if (target.GetComponent<Health>().ShouldDie()) {
					target = world.GetComponent<World>().FindClosestPlayerObject(transform.position, WARRIOR_AGGRO_RANGE);
					if (target == null) {
						SetupCharge();
					} else {
						SetupAttack();
					}
				} else {
					attackTimer += Time.deltaTime;
					if (attackTimer > 1f && (transform.position - target.transform.position).magnitude < WARRIOR_ATTACK_RANGE) {
						attackTimer = 0;
						GetComponent<Animator>().SetTrigger("Attack");
						target.GetComponent<Health>().TakeDamage(1);
					}

					Vector3 dir = target.transform.position - transform.position;
					dir = new Vector3(dir.x, 0, dir.z);
					transform.rotation = Quaternion.LookRotation(dir.normalized);
				}
			} else {
				if (state == "wander") {
					wanderTimer -= Time.deltaTime;
					if (wanderTimer <= 0) SetupIdle();

					float angle = Random.Range(-WARRIOR_TURN_SPEED, WARRIOR_TURN_SPEED);
					transform.rotation *= Quaternion.AngleAxis(angle, transform.up);
				} else if (state == "charge") {
					if (GetComponent<NavMeshAgent>().remainingDistance <= GetComponent<NavMeshAgent>().stoppingDistance) {
						SetupWander();
					}
				}

				//check for a target in range, if there is one, change state to attack it
				target = world.GetComponent<World>().FindClosestPlayerObject(transform.position, WARRIOR_AGGRO_RANGE);
				if (target != null) SetupAttack();
			}
		}

		return;
	}

	private void SetupWander() {
		state = "wander";
		wanderTimer = Random.Range(5f, 10f);
		GetComponent<Animator>().SetBool("InCombat", false);
		GetComponent<Animator>().SetFloat("Speed", WARRIOR_WALK_SPEED);
		GetComponent<NavMeshAgent>().isStopped = true;
		return;
	}

	private void SetupIdle() {
		state = "idle";
		GetComponent<Animator>().SetBool("InCombat", false);
		GetComponent<Animator>().SetFloat("Speed", 0f);
		GetComponent<NavMeshAgent>().isStopped = true;
		return;
	}

	private void SetupCharge() {
		state = "charge";
		GetComponent<Animator>().SetBool("InCombat", false);
		GetComponent<Animator>().SetFloat("Speed", WARRIOR_RUN_SPEED);
		GetComponent<NavMeshAgent>().SetDestination(destination);
		GetComponent<NavMeshAgent>().isStopped = false;
		return;
	}

	private void SetupAttack() {
		state = "attack";
		GetComponent<Animator>().SetBool("InCombat", true);
		GetComponent<Animator>().SetFloat("Speed", WARRIOR_RUN_SPEED);
		GetComponent<NavMeshAgent>().isStopped = true;
		return;
	}

	private void SetupDead() {
		state = "dead";
		GetComponent<Animator>().SetTrigger("Die");
		GetComponent<NavMeshAgent>().isStopped = true;
		return;
	}

	public void SetDestination(Vector3 dest) {
		if (state == "idle") {
			destination = dest;
			SetupCharge();
		}
		return;
	}
}
