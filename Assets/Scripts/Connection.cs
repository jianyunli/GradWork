﻿using UnityEngine;
using System.Collections;

public class Connection : MonoBehaviour
{

    public Node StartNode;
    public Node EndNode;

    private ArrayList nodes = new ArrayList();
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Add(ArrayList l)
    {
        nodes = l;//work on this later
    }

    public void Add(Node n)
    {
        nodes.Add(n);
    }

    public Node GetStartNode()
    {
        return StartNode;
    }

    public Node GetEndNode()
    {
        return EndNode;
    }
}
