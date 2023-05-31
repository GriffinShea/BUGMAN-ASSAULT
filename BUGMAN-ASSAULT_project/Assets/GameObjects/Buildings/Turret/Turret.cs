using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public int TURRET_HEALTH = 10;
    public int TURRET_ATTACK_POWER = 1;
    public float TURRET_ATTACK_RANGE = 20;
    public float TURRET_ATTACK_TIME = 1;

    private GameObject world;
    private GameObject targetEnemy;

    private float attackTimer;

    public void SetUp(GameObject w) {
        world = w;
        attackTimer = TURRET_ATTACK_TIME;
        GetComponent<Health>().SetUp(TURRET_HEALTH);
        return;
    }

    private void Update() {
        if (GetComponent<Health>().ShouldDie()) return;

        if (targetEnemy == null) {
            targetEnemy = world.GetComponent<World>().FindClosestEnemy(transform.position, TURRET_ATTACK_RANGE);
        } else if (
            targetEnemy.GetComponent<Health>().ShouldDie() ||
            (targetEnemy.transform.position - transform.position).magnitude > TURRET_ATTACK_RANGE
        ) targetEnemy = null;

        attackTimer += Time.deltaTime;
        if (attackTimer > 0.25)
            transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().enabled = false;

        if (targetEnemy != null) {
            Vector3 dir = targetEnemy.transform.position - transform.position;
            dir = new Vector3(dir.x, 0, dir.z);
            transform.rotation = Quaternion.LookRotation(dir.normalized);
            
            if (attackTimer > TURRET_ATTACK_TIME) {
                attackTimer = 0;
                targetEnemy.GetComponent<Health>().TakeDamage(TURRET_ATTACK_POWER);
                transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            }
        }


        return;
    }

    public void SetTarget(GameObject t) {
        targetEnemy = t;
        return;
    }

    private void OnMouseOver() {
        if (Input.GetMouseButtonUp(0)) {
            General.Select(gameObject, "turret", "[M1] --> target");
        }
        return;
    }
}
