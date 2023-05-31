using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barracks : MonoBehaviour {
	public int BARRACKS_HEALTH = 10;
	public int BARRACKS_CAPACITY = 10;

	private GameObject stockpile;

	public void SetUp(GameObject s) {
		stockpile = s;
		gameObject.GetComponent<Health>().SetUp(BARRACKS_HEALTH, Kill);
		stockpile.GetComponent<Stockpile>().AddManpowerTotal(BARRACKS_CAPACITY);
		return;
	}

	public void Kill() {
		gameObject.GetComponent<Health>().DefaultKill();
		stockpile.GetComponent<Stockpile>().ReduceManpowerTotal(BARRACKS_CAPACITY);
		return;
	}
}
