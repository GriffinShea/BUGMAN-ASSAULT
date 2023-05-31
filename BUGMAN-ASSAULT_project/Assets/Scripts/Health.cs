using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public Material Destroyed;

    private int maxHealth;
    private int currHealth;

    private float despawnTimer;

    public System.Action Kill;

    public int GetCurrHealth() { return currHealth; }
    public int GetMaxHealth() { return maxHealth; }

    public bool ShouldDie() { return currHealth <= 0; }

    public void SetUp(int mh, System.Action K) {
        maxHealth = mh;
        currHealth = maxHealth;
        Kill = K;
        return;
	}

    public void SetUp(int mh) {
        maxHealth = mh;
        currHealth = maxHealth;
        Kill = DefaultKill;
        return;
    }

    public void DefaultKill() {
        GetComponent<MeshRenderer>().material = Destroyed;
        return;
	}

    void Update() {
        if (ShouldDie()) despawnTimer += Time.deltaTime;
	}

    public bool ShouldDespawn() {
        if (despawnTimer > 5) return true;
        else return false;
	}

    public void TakeDamage(int damage) {
        currHealth -= damage;
        if (currHealth <= 0) Kill();
        return;
	}

    public void SelfDestruct() {
        currHealth = 0;
        return;
	}

}
