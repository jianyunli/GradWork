﻿using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum MODE
{
    CREATE,
    EDIT
}

public enum NODE
{
    SPAWNER,
    INTERSECTION,
    CONNECTION
}

public class ModeSelect : MonoBehaviour
{
    public GameObject SpawnerPrefab;
    public GameObject IntersectionPrefab;
    public GameObject ConnectionNodePrefab;
    public GameObject RoadDrawPrefab;
    private GameObject roadDrawPrefab;

    private LineRenderer lineRenderer;

    private GameObject selectedObject = null;

    public Canvas EditBoxesParent;

    public Button BtnToggleMode;
    public Button BtnSpawner;
    public Button BtnIntersection;
    public Button BtnConnection;

    private MODE mode = MODE.CREATE;
    public MODE Mode { get { return mode; } }

    private NODE node = NODE.SPAWNER;

    private ArrayList Connection = new ArrayList();

    // Use this for initialization
    void Start () {
        BtnToggleMode.GetComponentInChildren<Text>().text = "Edit";
    }
	
	// Update is called once per frame
	void Update () {
	    if (mode == MODE.CREATE)
	    {
	        if (Input.GetMouseButtonUp(0))
	        {
	            var screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
	            RaycastHit hitInfo;
                int layerCast = 1 << 8;
                if (Physics.Raycast(screenRay, out hitInfo, layerCast))
	            {
	                switch (node)
	                {
	                    case NODE.SPAWNER:
	                        //Create new spawner and add to main nodes list
	                        GameObject s = Instantiate(SpawnerPrefab, hitInfo.point, Quaternion.identity) as GameObject;
	                        MainManager.Main.AddNode(s);
	                        break;
	                    case NODE.INTERSECTION:
	                        //Create new intersection and add to main nodes list
	                        GameObject i =
	                            Instantiate(IntersectionPrefab, hitInfo.point, Quaternion.identity) as GameObject;
	                        MainManager.Main.AddNode(i);
	                        break;
	                    case NODE.CONNECTION:
	                        //Create new connection node and add to local Connectionlist
	                        GameObject c =
	                            Instantiate(ConnectionNodePrefab, hitInfo.point, Quaternion.identity) as GameObject;
	                        Connection.Add(c);
	                        break;
	                    default:
	                        throw new ArgumentOutOfRangeException();
	                }
	            }
	        }
	        if (Input.GetMouseButtonUp(1) && node == NODE.CONNECTION)
	        {
                AddRoad();
	            GameObject[] delete = GameObject.FindGameObjectsWithTag("RoadDraw");
	            int deleteCount = delete.Length;
	            for (int i = deleteCount - 1; i >= 0; i--)
	            {
	                Destroy(delete[i]);
	            }
	        }
	    }
        else if (mode == MODE.EDIT)
	    {
	        if (Input.GetMouseButtonUp(0))
	        {
                var screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
	            int layerCast = 1 << 8;
	            layerCast = ~layerCast;
	            if (Physics.Raycast(screenRay, out hitInfo, layerCast))
	            {
	                if (hitInfo.collider.gameObject.GetComponent<IntersectionNode>() != null)
	                {
	                    CloseAllUIBoxes();
                        OpenUIBoxPos(0);
	                }
	                else if (hitInfo.collider.gameObject.GetComponent<SpawnerNode>() != null)
	                {
	                    CloseAllUIBoxes();
                        OpenUIBoxPos(1);
	                }
	                else if(hitInfo.collider.gameObject.GetComponent<Connection>() != null)
	                    //dunno about connections yet
	                {
	                    CloseAllUIBoxes();
                        OpenUIBoxPos(2);
	                }
	            }
	        }
	    }

	}

    private void newLine()
    {
        roadDrawPrefab = GameObject.Instantiate(RoadDrawPrefab) as GameObject;
        lineRenderer = roadDrawPrefab.GetComponent<LineRenderer>();
        lineRenderer.numPositions = 0;
    }

    private void drawRoad()
    {
        
    }

    private void AddRoad()
    {
        ////Show connection in editor
        GetComponent<LineRenderer>().numPositions = MainManager.Main.GetConnectionCount() + Connection.Count;
        for (int j = 0; j < Connection.Count; j++)
        {
            GetComponent<LineRenderer>().SetPosition(j,(Connection[j] as GameObject).transform.position);
            //Gizmos.color = Color.white;
            //Gizmos.DrawLine((Connection[j - 1] as GameObject).transform.position, (Connection[j] as GameObject).transform.position);
        }
        //Add local connectionlist to main connectionlist and clear local
        Connection con = new Connection();
        con.Add(Connection);
        MainManager.Main.AddConnection(con);
        Connection.Clear();
    }

    private void CloseAllUIBoxes()
    {
        foreach (Transform uibox in EditBoxesParent.transform)
        {
            uibox.gameObject.SetActive(false);
        }
    }

    private void OpenUIBoxPos(int index)
    {
        var child = EditBoxesParent.transform.GetChild(index);
        child.position = Input.mousePosition;
        child.gameObject.SetActive(true);
    }

    public void BtnClick()
    {
        if (mode == MODE.CREATE)
        {
            if(node == NODE.CONNECTION && Connection.Count > 1) AddRoad();
            BtnToggleMode.GetComponentInChildren<Text>().text = "Create";
            mode = MODE.EDIT;
            BtnIntersection.enabled = false;
            BtnConnection.enabled = false;
            BtnSpawner.enabled = false;
        }
        else
        {
            BtnToggleMode.GetComponentInChildren<Text>().text = "Edit";
            mode = MODE.CREATE;
            BtnIntersection.enabled = true;
            BtnConnection.enabled = true;
            BtnSpawner.enabled = false;
        }
    }

    public void BtnSpawnerClick()
    {
        if (node == NODE.CONNECTION && Connection.Count > 1) AddRoad();
        BtnIntersection.enabled = true;
        BtnConnection.enabled = true;
        BtnSpawner.enabled = false;
        node = NODE.SPAWNER;
    }

    public void BtnIntersectionClick()
    {
        if (node == NODE.CONNECTION && Connection.Count > 1) AddRoad();
        BtnIntersection.enabled = false;
        BtnConnection.enabled = true;
        BtnSpawner.enabled = true;
        node = NODE.INTERSECTION;
    }

    public void BtnConnectionClick()
    {
        BtnIntersection.enabled = true;
        BtnConnection.enabled = false;
        BtnSpawner.enabled = true;
        node = NODE.CONNECTION;
    }
}
