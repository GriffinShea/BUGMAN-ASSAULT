using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {
	public bool DEMO_MODE = true;


	//resource prefabs
	public GameObject MetalSupply;
	public GameObject GasSupply;
	//building prefabs
	public GameObject Stockpile;
	public GameObject Factory;
	public GameObject Turret;
	public GameObject Barracks;
	//unit prefabs
	public GameObject Builder;
	public GameObject Collector;
	public GameObject Tank;
	//enemy prefabs
	public GameObject Hive;
	public GameObject Warrior;
	public GameObject Scout;



	//heightmap properties
	public Texture2D heightmap;
	private Vector2 worldScaler;

	//containers for GameObjects
	private List<GameObject> resources = new List<GameObject>();
	private List<GameObject> enemies = new List<GameObject>();
	private List<GameObject> playerBuildings = new List<GameObject>();
	private List<GameObject> playerUnits = new List<GameObject>();

	//other references
	public Camera camera;
	public GameObject generalCursor;
	public GameObject textBox;



	public float GetHeightAtPos(float x, float z) {
		return (1 - heightmap.GetPixel(
			Mathf.FloorToInt(x / worldScaler.x) + heightmap.width / 2,
			Mathf.FloorToInt(z / worldScaler.y) + heightmap.height / 2
		).r) * transform.lossyScale.y;
	}

	public GameObject FindClosestPlayerObject(Vector3 pos, float range) {
		float bestDist = range;
		GameObject closest = null;
		float dist;

		for (int i = 0; i < playerBuildings.Count; i++) {
			dist = (Help.GetGameObject2DPos(playerBuildings[i]) - new Vector2(pos.x, pos.z)).magnitude;
			if (!playerBuildings[i].GetComponent<Health>().ShouldDie() && dist < bestDist) {
				closest = playerBuildings[i];
				bestDist = dist;
			}
		}

		for (int i = 0; i < playerUnits.Count; i++) {
			dist = (Help.GetGameObject2DPos(playerUnits[i]) - new Vector2(pos.x, pos.z)).magnitude;
			if (!playerUnits[i].GetComponent<Health>().ShouldDie() && dist < bestDist) {
				closest = playerUnits[i];
				bestDist = dist;
			}
		}
		
		return closest;
	}

	public GameObject FindClosestEnemy(Vector3 pos, float range) {
		float bestDist = range;
		GameObject closest = null;
		float dist;

		for (int i = 0; i < enemies.Count; i++) {
			dist = (Help.GetGameObject2DPos(enemies[i]) - new Vector2(pos.x, pos.z)).magnitude;
			if (!enemies[i].GetComponent<Health>().ShouldDie() && dist < bestDist) {
				closest = enemies[i];
				bestDist = dist;
			}
		}

		return closest;
	}

	public Vector3 GetStockpileLocation() {
		return playerBuildings[0].transform.position;
	}

	public int GetManpowerUsed() {
		int sum = 0;
		for (int i = 0; i < playerUnits.Count; i++) {
			if (playerUnits[i].GetComponent<Collector>() != null)
				sum += 2;
			else if (playerUnits[i].GetComponent<Builder>() != null)
				sum += 6;
			else if (playerUnits[i].GetComponent<Tank>() != null)
				sum += 4;
		}
		return sum;
	}



	private void Start() {
		//calculate worldScaler
		worldScaler = new Vector2(
			transform.lossyScale.x * GetComponent<MeshFilter>().mesh.bounds.size.x / heightmap.width,
			transform.lossyScale.z * GetComponent<MeshFilter>().mesh.bounds.size.z / heightmap.height
		);

		//set general cursor
		General.world = gameObject;
		General.camera = camera;
		General.cursor = generalCursor;

		//create a hive (this spawns ten bugmen)
		CreateEnemy(Hive, 50, -30);

		if (DEMO_MODE) {
			camera.transform.position = new Vector3(-75, 30, -25);
			//create stockpile and some other buildings
			CreatePlayerBuilding(Stockpile, -60, 0);
			CreatePlayerBuilding(Factory, -50, 10);
			CreatePlayerBuilding(Turret, -60, -20);
			CreatePlayerBuilding(Turret, -35, -5);
			CreatePlayerBuilding(Barracks, -60, 12.5f);
			CreatePlayerBuilding(Barracks, -60, 15);
			CreatePlayerBuilding(Barracks, -60, 17.5f);

			//create player units
			CreatePlayerUnit(Builder, -60f, -5f, new Vector2(-60f, -10f));
			CreatePlayerUnit(Collector, -57.5f, -2.5f, new Vector2(-52.5f, -7.5f));
			CreatePlayerUnit(Collector, -62.5f, -2.5f, new Vector2(-67.5f, -7.5f));
			CreatePlayerUnit(Tank, -45f, -5f, new Vector2(-45.1f, -5f));
			CreatePlayerUnit(Tank, -45f, 0f, new Vector2(-45.1f, 0f));
			CreatePlayerUnit(Tank, -50f, -5f, new Vector2(-50.1f, -5f));
			CreatePlayerUnit(Tank, -50f, 0f, new Vector2(-50.1f, 0f));
		} else {
			camera.transform.position = new Vector3(-70, 60, -0);
			CreatePlayerBuilding(Stockpile, -30, 50);
			CreatePlayerBuilding(Barracks, -20, 50);
			CreatePlayerUnit(Builder, -30, 40, new Vector2(-30, 40));
		}

		//add some supplies (but not under water
		for (int i = 0; i < 30; i++) {
			float x = Random.Range(-100, 100);
			float z = Random.Range(-100, 100);
			while (GetHeightAtPos(x, z) < 0.1) {
				x = Random.Range(-100, 100);
				z = Random.Range(-100, 100);
			}
			CreateSupply(MetalSupply, x, z);
		}
		for (int i = 0; i < 10; i++) {
			float x = Random.Range(-100, 100);
			float z = Random.Range(-100, 100);
			while (GetHeightAtPos(x, z) < 0.1) {
				x = Random.Range(-100, 100);
				z = Random.Range(-100, 100);
			}
			CreateSupply(GasSupply, x, z);
		}


		//stockpile needs a second setup to calculate initial manpower statistics
		playerBuildings[0].GetComponent<Stockpile>().SecondSetup(gameObject);

		return;
	}

	private void LateUpdate() {
		//check for and resolve General inputs
		General.Update();

		//check for escape key or if the stockpile is destroyed to end the game
		if (Input.GetKeyUp(KeyCode.Escape) || playerBuildings[0].GetComponent<Health>().ShouldDie()) {
			Debug.Log("STOCKPILE HAS BEEN DESTROYED! YOU LOSE!!!");
			Application.Quit();
		}

		if (enemies[0].GetComponent<Health>().ShouldDie()) {
			Debug.Log("HIVE HAS BEEN DESTROYED! YOU WIN!!!");
			Application.Quit();
		}

		//cull enemies
		for (int i = enemies.Count - 1; i >= 0; i--) {
			if (enemies[i].GetComponent<Health>().ShouldDespawn()) {
				GameObject enemy = enemies[i];
				enemies.RemoveAt(i);
				Destroy(enemy);
			}
		}

		//update UI
		string message;
		message = playerBuildings[0].GetComponent<Stockpile>().GetResourceString();
		camera.GetComponent<CameraControls>().SetResourceText(message);

		message = "Stockpile HP: " + playerBuildings[0].GetComponent<Health>().GetCurrHealth();
		message += "/" + playerBuildings[0].GetComponent<Health>().GetMaxHealth();
		message += " | Hive HP: " + enemies[0].GetComponent<Health>().GetCurrHealth();
		message += "/" + enemies[0].GetComponent<Health>().GetMaxHealth();
		camera.GetComponent<CameraControls>().SetScoreboardText(message);
		
		//message = "Metal: " + playerBuildings[0].GetComponent<Stockpile>().GetMetalCount();
		//camera.GetComponent<CameraControls>().SetSelectionText(message);

		//message = "Metal: " + playerBuildings[0].GetComponent<Stockpile>().GetMetalCount();
		//camera.GetComponent<CameraControls>().SetOrdersText(message);

		return;
	}

	private void moveObjectToTerrainHeight(GameObject go) {
		float moveup = GetHeightAtPos(go.transform.position.x, go.transform.position.z);
		moveup -= go.transform.position.y;
		moveup += Help.GetGameObjectHeight(go) / 2;
		go.transform.Translate(Vector3.up * moveup);
		return;
	}



	public GameObject CreateEnemy(GameObject prefab, float x, float z) {
		//instantiate and setup the enemy then add to the list
		GameObject enemy = InstantiateAtPos(prefab, x, z);
		enemies.Add(enemy);
		if (prefab == Hive) {
			enemy.GetComponent<Hive>().SetUp(gameObject);
		} else if (prefab == Warrior) {
			enemy.GetComponent<Warrior>().SetUp(gameObject, enemies[0]);
		} else if (prefab == Scout) {
			enemy.GetComponent<Scout>().SetUp(gameObject, enemies[0]);
		}
		return enemy;
	}

	public void CreatePlayerUnit(GameObject prefab, float x, float z, Vector2 initialDest) {
		CreatePlayerUnit(prefab, x, z, new Vector3(initialDest.x, GetHeightAtPos(initialDest.x, initialDest.y), initialDest.y));
		return;
	}

	public void CreatePlayerUnit(GameObject prefab, float x, float z, Vector3 initialDest) {
		//instantiate and setup the unit then add to the list
		GameObject unit = InstantiateAtPos(prefab, x, z);
		if (prefab == Builder) {
			unit.GetComponent<Builder>().SetUp(gameObject, playerBuildings[0]);
			unit.GetComponent<MoveOrder>().SetDestination(initialDest, 4);
		} else if (prefab == Collector) {
			unit.GetComponent<Collector>().SetUp(gameObject, playerBuildings[0]);
			unit.GetComponent<MoveOrder>().SetDestination(initialDest, 4);
		} else if (prefab == Tank) {
			unit.GetComponent<Tank>().SetUp(gameObject, playerBuildings[0]);
			unit.GetComponent<MoveOrder>().SetDestination(initialDest, 4);
		}
		playerUnits.Add(unit);
		return;
	}

	public void CreatePlayerBuilding(GameObject prefab, float x, float z) {
		//instantiate and setup the building then add to the list
		GameObject building = InstantiateAtPos(prefab, x, z);
		if (prefab == Stockpile) {
			building.GetComponent<Stockpile>().SetUp();
		} else if (prefab == Turret) {
			building.GetComponent<Turret>().SetUp(gameObject);
		} else if (prefab == Factory) {
			building.GetComponent<Factory>().SetUp(gameObject, playerBuildings[0]);
		} else if (prefab == Barracks) {
			building.GetComponent<Barracks>().SetUp(playerBuildings[0]);
		}
		playerBuildings.Add(building);
		return;
	}

	private void CreateSupply(GameObject prefab, float x, float z) {
		//instantiate and setup the building then add to the list
		GameObject supply = InstantiateAtPos(prefab, x, z);
		if (prefab == MetalSupply)
			supply.GetComponent<MetalSupply>().SetUp();
		else if (prefab == GasSupply)
			supply.GetComponent<GasSupply>().SetUp(playerBuildings[0]);
		resources.Add(supply);
		return;
	}

	public GameObject InstantiateAtPos(GameObject prefab, float x, float z) {
		//instantiate at the position, ontop of the heightmap surface
		GameObject go = (GameObject) Instantiate(
			prefab,
			new Vector3(x, prefab.transform.position.y + GetHeightAtPos(x, z), z),
			Quaternion.identity
		);
		return go;
	}

	/*
	 * here is the code I used to create the mesh from the heightmap
	 
using UnityEditor;
...
	 void CreateMeshCollider() {
		//source: https://answers.unity.com/questions/1033085/heightmap-to-mesh.html

		Vector3 vert;
		List<Vector3> verts = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<int> tris = new List<int>();

		//Bottom left section of the map, other sections are similar
		int meshSize = 64;
		for (int i = 0; i < meshSize; i++) {
			for (int j = 0; j < meshSize; j++) {
				//Add each new vertex in the plane
				vert = new Vector3(
					((float) i / (meshSize - 1) - 0.5f) * GetComponent<MeshFilter>().mesh.bounds.size.x,
					0,
					((float) j / (meshSize - 1) - 0.5f) * GetComponent<MeshFilter>().mesh.bounds.size.z
				);
				vert.y = GetHeightAtPos(
					-vert.x * transform.lossyScale.x,
					-vert.z * transform.lossyScale.z
				) / transform.lossyScale.y;
				verts.Add(vert);
				uvs.Add(new Vector2(((float) -i / (meshSize - 1)), ((float) -j / (meshSize - 1))));
				//Skip if a new square on the plane hasn't been formed
				if (i == 0 || j == 0) continue;
				//Adds the index of the three vertices in order to make up each of the two tris
				tris.Add(meshSize * i + j); //Top right
				tris.Add(meshSize * i + j - 1); //Bottom right
				tris.Add(meshSize * (i - 1) + j - 1); //Bottom left - First triangle
				tris.Add(meshSize * (i - 1) + j - 1); //Bottom left 
				tris.Add(meshSize * (i - 1) + j); //Top left
				tris.Add(meshSize * i + j); //Top right - Second triangle
			}
		}

		Mesh procMesh = new Mesh();
		procMesh.vertices = verts.ToArray(); //Assign verts, uvs, and tris to the mesh
		procMesh.uv = uvs.ToArray();
		procMesh.triangles = tris.ToArray();
		procMesh.RecalculateNormals(); //Determines which way the triangles are facing
		GetComponent<MeshFilter>().mesh = procMesh; //Assign Mesh object to MeshFilter
		GetComponent<MeshCollider>().sharedMesh = procMesh; //Assign Mesh object to MeshCollider

		AssetDatabase.CreateAsset(procMesh, "Assets/heightmap_mesh.asset");
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		return;
	}
	*/
}
