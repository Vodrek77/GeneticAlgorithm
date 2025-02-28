using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeController : MonoBehaviour
{
    //Components
    public Transform scoreTrigger;
    public Transform top;
    public Transform bot;

    void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        //Moves the pipe to the left at a constant rate every frame
        transform.position = new Vector2(transform.position.x - .08f, transform.position.y);

        //Destroys the pipe when it goes off the screen
        if (transform.position.x <= -7.5f)
        {
            Destroy(gameObject);
        }
    }

    public void SetHeight(int pipeID)
    {
        //Sets the middle height based on how many previous pipes were created
        scoreTrigger.localScale = new Vector2(1, 2.5f - pipeID * .01f);

        //Changes position of the pipes to fit perfectly around the scoreTrigger object
        top.position = new Vector2(top.position.x, (5 + scoreTrigger.position.y) + (scoreTrigger.localScale.y / 2));
        bot.position = new Vector2(bot.position.x, (-5 + scoreTrigger.position.y) - (scoreTrigger.localScale.y / 2));
    }
}
