﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;

public class SpawnerNode : Nodes {

    public List<int> Connections = new List<int>();
    //List of spawnables
    public GameObject CarPrefab;
    public GameObject TruckPrefab;
    public GameObject BikePrefab;
    public GameObject JeepPrefab;

    public bool SpawnCar = true;
    public bool SpawnJeep = false;
    public bool SpawnBike = false;
    public bool SpawnTruck = false;

    public int CarSpawnPerc;
    public int JeepSpawnPerc;
    public int BikeSpawnPerc;
    public int TruckSpawnPerc;

    public float GeneralSpawnRate;

    private float _timeCounter = 0;


    // Use this for initialization
    void Start () {
	    MainManager.Main.AddNode(this);
	}
	
	// Update is called once per frame
	void Update ()
	{
	    _timeCounter += Time.deltaTime + Random.Range(0,Time.deltaTime);
	    int totalPerc = 0;
	    if(SpawnCar)totalPerc += CarSpawnPerc;
        if (SpawnJeep) totalPerc += JeepSpawnPerc;
        if (SpawnBike) totalPerc +=BikeSpawnPerc;
        if (SpawnTruck) totalPerc += TruckSpawnPerc;
	    if (GeneralSpawnRate <= _timeCounter)
	    {
	        _timeCounter = 0;

	    }
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.transform.parent.GetType() == typeof(Vehicle))
        {
            if (col.transform.parent.gameObject.GetComponent<UnitBehaviorTree>().GetTargetNode() == this)
            {
                Destroy(col.transform.parent.gameObject);
            }
        }
        else if (col.transform.GetComponentInParent<Connection>() != null)
        {
            bool alreadyExists = false;
            foreach (var con in Connections)
            {
                if (con == col.gameObject.GetComponent<Connection>().Serial)
                {
                    alreadyExists = true;
                    continue;
                }
            }
            if (!alreadyExists)
            {
                Connections.Add(col.gameObject.transform.parent.GetComponent<Connection>().Serial);
                if (col.GetComponentInParent<Connection>().Val1 == null)
                    col.GetComponentInParent<Connection>().Val1 = this;
                else if (col.GetComponentInParent<Connection>().Val2 == null)
                    col.GetComponentInParent<Connection>().Val2 = this;
            }
        }
    }
}
