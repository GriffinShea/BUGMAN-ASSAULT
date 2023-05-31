using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasSupply : MonoBehaviour {
    public string SUPPLY_TYPE = "gas";
    public float PRODUCTION_TIME = 1f;

    private GameObject stockpile;

    private bool harvested;
    private float productionTimer;
    private Resources res;

    public bool isHarvested() { return harvested; }

    public void SetUp(GameObject s) {
        stockpile = s;
        GetComponent<Targetable>().SetUp("gassupply");
        harvested = false;
        productionTimer = 0;
        res.metal = 0;
        res.manpower = 0;
        res.gas = 1;
        res.empty = false;
        return;
	}

    void Update() {
        if (harvested) {
            productionTimer += Time.deltaTime;

            if (productionTimer > PRODUCTION_TIME) {
                productionTimer = 0;
                stockpile.GetComponent<Stockpile>().DeliverResources(res);
			}
        }
        return;
    }

    public void BuildHarvester() {
        harvested = true;
        transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        transform.GetChild(1).GetComponent<MeshRenderer>().enabled = true;
        DisableScaffold();
        return;
    }

    public void EnableBlueScaffold() {
        transform.GetChild(2).GetComponent<MeshRenderer>().enabled = true;
        return;
    }

    public void EnableGreenScaffold() {
        transform.GetChild(3).GetComponent<MeshRenderer>().enabled = true;
        return;
    }

    public void DisableScaffold() {
        transform.GetChild(2).GetComponent<MeshRenderer>().enabled = false;
        transform.GetChild(3).GetComponent<MeshRenderer>().enabled = false;
        return;
    }
}
