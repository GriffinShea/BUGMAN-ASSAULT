using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Orderable))]
[RequireComponent(typeof(NavMeshAgent))]

public class MoveOrder : MonoBehaviour
{
	public void SetUp(float s) {
		GetComponent<NavMeshAgent>().speed = s;
		return;
	}

	public void SetDestination(Vector3 dest, float stopDist) {
		if (!GetComponent<Health>().ShouldDie()) {
			GetComponent<NavMeshAgent>().SetDestination(dest);
			GetComponent<NavMeshAgent>().stoppingDistance = stopDist;
			GetComponent<NavMeshAgent>().isStopped = false;
			GetComponent<Orderable>().SetOrderState("move");
		}
		return;
	}

	public bool MoveUpdate() {
		if (
			GetComponent<NavMeshAgent>().remainingDistance <= GetComponent<NavMeshAgent>().stoppingDistance
			|| GetComponent<NavMeshAgent>().isStopped == true
		) {
			return true;
		} else {
			return false;
		}
	}

	public void StopMove() {
		GetComponent<NavMeshAgent>().isStopped = true;
		return;
	}
}
