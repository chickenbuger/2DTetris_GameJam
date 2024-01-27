using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawntetris : MonoBehaviour
{
    public GameObject[] Tetris;

    private void Start()
    {
        NewTetris();
    }

    public void NewTetris()
    {
        Instantiate(Tetris[Random.Range(0, Tetris.Length)], transform.position, Quaternion.identity);
    }
}
