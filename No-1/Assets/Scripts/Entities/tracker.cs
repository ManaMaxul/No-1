using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tracker : MonoBehaviour
{
    public GameObject player;
    private Material mat;
    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetVector("_Player_Pos", player.transform.position);
    }
}
