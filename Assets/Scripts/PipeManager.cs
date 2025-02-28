using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager : MonoBehaviour
{
    //GameObjects
    public GameObject pipes;

    //Variables
    public int pipesCreated = 0;

    //Runs when the object is instantiated
    void Awake()
    {
        //Creates the first set of pipes
        CreatePipes();

        float timeVal = Random.Range(1.5f, 2.5f);

        //Instantiates a set of Pipes every few seconds after the first
        InvokeRepeating("CreatePipes", timeVal, timeVal);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreatePipes()
    {
        Instantiate(pipes, new Vector2(10.0f, Random.Range(-3.0f, 3.0f)), transform.rotation, this.transform).GetComponent<PipeController>().SetHeight(pipesCreated);
        pipesCreated++;

        if(pipesCreated > 60)
        {
            pipesCreated = 60;
        }
    }

    public Transform GetNearestPipe()
    {
        int closestIndex = 0;

        if(transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).transform.position.x < transform.GetChild(closestIndex).transform.position.x)
                {
                    closestIndex = i;
                }
            }

            return transform.GetChild(closestIndex).transform;
        }

        return transform;
    }
}
