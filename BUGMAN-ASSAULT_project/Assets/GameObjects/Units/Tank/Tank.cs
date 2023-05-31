using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public int TANK_HEALTH = 20;
	public int TANK_ATTACK_POWER = 3;
	public float TANK_SPEED = 10;
	public float TANK_AGGRO_RANGE = 40;
	public float TANK_ATTACK_RANGE = 30;
	public float TANK_ATTACK_TIME = 2;

	private GameObject world;
	private GameObject stockpile;
	private GameObject targetEnemy;
	private float attackTimer;
	private string state;

	//REVISIT: is this the best way to do this??? also it kind of makes a mess in World
	public void SetUp(GameObject w, GameObject s) {
		world = w;
		stockpile = s;
		targetEnemy = null;
		attackTimer = TANK_ATTACK_TIME;
		GetComponent<Health>().SetUp(TANK_HEALTH, Kill);
		GetComponent<Orderable>().SetUp(
			"tank",
			"[M1] on enemy --> attack\n[M1] on terrain --> move to",
			ClearOrder
		);
		GetComponent<MoveOrder>().SetUp(TANK_SPEED);
		SetupIdle();
		return;
	}

	public void Kill() {
		GetComponent<Health>().DefaultKill();
		Resources res;
		res.empty = false;
		res.metal = 0;
		res.gas = 0;
		res.manpower = 4;
		stockpile.GetComponent<Stockpile>().DeliverResources(res);
		return;
	}

	public bool ClearOrder() {
		targetEnemy = null;
		return true;
	}

	private void Update() {
		//if dead do nothing, else
		if (state != "dead") {
			//if this unit should die, go to dead state
			if (GetComponent<Health>().ShouldDie()) SetupDead();

			//incease attack timer and disable the muzzleflash 0.25 seconds after an attack
			attackTimer += Time.deltaTime;
			if (attackTimer > 0.25)
				transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().enabled = false;

			//allow the orderable component (only has movable order) to handle procedure until nothing it has reached destination
			bool moving = !GetComponent<Orderable>().DoOrder();
			
			//if current target is dead, set targetEnemy to null and enter idle state
			if (targetEnemy != null && targetEnemy.GetComponent<Health>().ShouldDie()) {
				targetEnemy = null;
				SetupIdle();
			}

			//in idle state chase enemy if it enters aggro range
			if (state == "idle") {
				targetEnemy = world.GetComponent<World>().FindClosestEnemy(transform.position, TANK_AGGRO_RANGE);
				if (targetEnemy != null) SetupChasing();
			}
			
			//in pursuing state, use navmesh to pathfind to target until within aggro range, then chase
			else if (state == "pursuing") {
				Debug.DrawLine(transform.position, targetEnemy.transform.position, Color.green, 0, true);
				SetupPursuing();
				if (!moving)
					if ((transform.position - targetEnemy.transform.position).magnitude < TANK_AGGRO_RANGE)
						SetupChasing();
			}

			//in chasing state, steer towards target and attack if within attack range, if enemy moves out of aggro range go to idle
			else if (state == "chasing") {
				Debug.DrawLine(transform.position, targetEnemy.transform.position, Color.yellow, 0, true);
				Vector3 dir = targetEnemy.transform.position - transform.position;
				dir = new Vector3(dir.x, 0, dir.z);
				transform.rotation = Quaternion.LookRotation(dir.normalized);
				GetComponent<Rigidbody>().AddForce(transform.forward * TANK_SPEED * 20f * Time.deltaTime, ForceMode.Impulse);

				float distance = (transform.position - targetEnemy.transform.position).magnitude;

				if (distance > TANK_AGGRO_RANGE + 5) SetupIdle();
				else if (
					distance < TANK_ATTACK_RANGE
					&& attackTimer > TANK_ATTACK_TIME
				) {
					attackTimer = 0;
					targetEnemy.GetComponent<Health>().TakeDamage(TANK_ATTACK_POWER);
					transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().enabled = true;
					Debug.DrawLine(transform.position, targetEnemy.transform.position, Color.red, 0.25f, true);
				}
			}
		}

		return;
	}

	private void SetupIdle() {
		state = "idle";
		GetComponent<MoveOrder>().StopMove();
		return;
	}

	private void SetupChasing() {
		state = "chasing";
		return;
	}

	private void SetupPursuing() {
		state = "pursuing";
		GetComponent<MoveOrder>().SetDestination(targetEnemy.transform.position, TANK_AGGRO_RANGE - 5);
		return;
	}

	private void SetupDead() {
		state = "dead";
		GetComponent<MoveOrder>().StopMove();
		return;
	}

	public void SetTarget(GameObject t) {
		targetEnemy = t;
		SetupPursuing();
		return;
	}
}
