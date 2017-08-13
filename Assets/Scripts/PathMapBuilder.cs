﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Assets.Scripts;
using UnityEngine;

[ExecuteInEditMode]
public class PathMapBuilder : MonoBehaviour {


    public class Connection
    {
        public GSDSplineC Spline { get; set; }
        public List<GSDSplineN> Nodes { get; set; }
        public List<IntersectionPoint> StartEndPoint { get; set; }
        private float startF = -5, endF = -5;
        

        public Connection()
        {
            Nodes = new List<GSDSplineN>();
            StartEndPoint = new List<IntersectionPoint>();
        }

        public float GetLength()
        {
            return Mathf.Abs(Nodes[0].tDist - Nodes[1].tDist);
        }

        public float GetStartF()
        {
            if (startF > -1) return startF;
            if (StartEndPoint[0].Node1 == Nodes[0] || StartEndPoint[0].Node2 == Nodes[0]) startF = Nodes[0].tDist;
            else
            {
                startF = Nodes[Nodes.Count - 1].tDist;
            }
            return startF;
        }

        public float GetEndF()
        {
            if (endF > -1) return startF;
            if (StartEndPoint[1].Node1 == Nodes[0] || StartEndPoint[1].Node2 == Nodes[0]) endF = Nodes[0].tDist;
            else
            {
                endF = Nodes[Nodes.Count - 1].tDist;
            }
            return endF;
        }
        
    }

    public class IntersectionPoint
    {
        public GSDSplineN Node1 { get; set; }
        public GSDSplineN Node2 { get; set; }
        public List<Connection> Roads { get; set; }
        public bool bIsSpawner { get; set; }
        public DijkstraTable DijkstraTable { get; set; }
        public GSDRoadIntersection IntGSD { get; set; }

        public IntersectionPoint()
        {
            Roads = new List<Connection>();
        }
    }

    //private List<Vector3> Intersections = new List<Vector3>();
    private List<IntersectionPoint> Intersections = new List<IntersectionPoint>();
    private List<Connection> Connections = new List<Connection>();

    public GameObject RoadNetwork;

    public bool Show = false;
    public float DrawDetail = 0.1f;

    public bool AutoGenerate = true;

	// Use this for initialization
	void Start ()
	{
	    if(AutoGenerate) Generate();
        Debug.Log("Intersections: " + Intersections.Count);
	    //Debug.Log("Intersections: " + Intersections.Count);
        
     //   //Remove Intersection duplicates
     //   HashSet<Vector3> hash = new HashSet<Vector3>();
	    //foreach (var pos in Intersections)
	    //{
	    //    hash.Add(pos);
	    //}
	    //Intersections = new List<Vector3>(hash);
     //   Debug.Log("Intersections no dups: " + Intersections.Count);
        Debug.Log("Connections: " + Connections.Count);
	}

    void Update()
    {
        if (Show)
            Draw();
    }

    private void Draw()
    {
        Color c = Color.green;
        //Connections
        Vector3 firstPoint, secondPoint, firstPointAlt, secondPointAlt;
        foreach (var con in Connections)
        {
            float detail = DrawDetail;
            float prevDetail = 0.0f;
            while (detail < 1)
            {
                firstPoint = con.Spline.GetSplineValue(prevDetail);
                secondPoint = con.Spline.GetSplineValue(detail);
                firstPoint.y = secondPoint.y = 5;
                Debug.DrawLine(firstPoint, secondPoint, c,Time.deltaTime);
                prevDetail = detail;
                detail += DrawDetail;
            }
            firstPoint = con.Spline.GetSplineValue(1 - DrawDetail);
            secondPoint = con.Spline.GetSplineValue(0.999999f);
            firstPoint.y = secondPoint.y = 5;
            Debug.DrawLine(firstPoint, secondPoint, c, Time.deltaTime);
        }

        //Intersections
        foreach (var i in Intersections)
        {
            float detail = DrawDetail;
            float prevDetail = 0.0f;
            Vector3 control = i.IntGSD.Node1.pos;

            firstPoint = i.IntGSD.Node1.GSDSpline.GetSplineValue(i.IntGSD.Node1.tTime - DrawDetail*2);
            firstPointAlt = i.IntGSD.Node1.GSDSpline.GetSplineValue(i.IntGSD.Node1.tTime + DrawDetail * 2);
            secondPoint = i.IntGSD.Node2.GSDSpline.GetSplineValue(i.IntGSD.Node2.tTime - DrawDetail*2);
            secondPointAlt = i.IntGSD.Node2.GSDSpline.GetSplineValue(i.IntGSD.Node2.tTime + DrawDetail * 2);
            while (detail < 1)
            {
                Debug.DrawLine(CalculateSplinePoint(prevDetail, firstPoint, secondPoint, control), CalculateSplinePoint(detail, firstPoint, secondPoint, control), c, Time.deltaTime);
                Debug.DrawLine(CalculateSplinePoint(prevDetail, firstPointAlt, secondPoint, control), CalculateSplinePoint(detail, firstPointAlt, secondPoint, control), c, Time.deltaTime);
                Debug.DrawLine(CalculateSplinePoint(prevDetail, firstPoint, secondPointAlt, control), CalculateSplinePoint(detail, firstPoint, secondPointAlt, control), c, Time.deltaTime);
                Debug.DrawLine(CalculateSplinePoint(prevDetail, firstPointAlt, secondPointAlt, control), CalculateSplinePoint(detail, firstPointAlt, secondPointAlt, control), c, Time.deltaTime);
                prevDetail = detail;
                detail += DrawDetail;
            }
        }
    }

    public void Generate()
    {
        foreach (Transform child in RoadNetwork.transform)
        {
            var tempCon = new Connection();
            //Ignore intersections
            if (!child.GetComponent<GSDRoad>())
            {
                foreach (Transform inter in child.transform)
                {
                    var i = new IntersectionPoint();
                    i.IntGSD = inter.GetComponent<GSDRoadIntersection>();
                    i.Node1 = i.IntGSD.Node1;
                    i.Node2 = i.IntGSD.Node2;
                    Intersections.Add(i);
                }
                continue;
            }
            var road = child.GetChild(0);
            var spline = road.gameObject.GetComponent<GSDSplineC>();
            foreach (var node in spline.mNodes)
            {
                if (node.bIsEndPoint)
                {
                    tempCon.Nodes.Add(node);
                }
                if (node.bIsIntersection)
                {
                    tempCon.Nodes.Add(node);
                    //Intersections.Add(node.pos);
                }
                if (tempCon.Nodes.Count > 1)
                {
                    tempCon.Spline = spline;
                    Connections.Add(tempCon);
                    tempCon = new Connection();
                    if (!node.bIsEndPoint) tempCon.Nodes.Add(node);
                }
            }
        }

        //Link Intersections and connections
        foreach (var connection in Connections)
        {
            foreach (var intersection in Intersections)
            {
                if (intersection.Node1 == connection.Nodes[0]
                    || intersection.Node1 == connection.Nodes[1]
                    || intersection.Node2 == connection.Nodes[0]
                    || intersection.Node2 == connection.Nodes[1])
                {
                    connection.StartEndPoint.Add(intersection);
                    intersection.Roads.Add(connection);
                }
            }
        }

        foreach (var i in Intersections)
        {
            RoadManager.AddIntersection(i);
        }
        foreach (var c in Connections)
        {
            RoadManager.AddRoad(c);
        }
        RoadManager.CalculateDijkstraTables();
    }

    private Vector3 CalculateSplinePoint(float t, Vector3 startpoint, Vector3 endpoint, Vector3 control)
    {
        Vector3 r = (Mathf.Pow((1 - t), 3) * startpoint)
               + (3 * Mathf.Pow((1 - t), 2) * t * control)
               + (3 * (1 - t) * Mathf.Pow(t, 2) * control)
               + (Mathf.Pow(t, 3) * endpoint);
        r.y = 5;
        return r;
    }
}
