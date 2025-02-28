using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    //Script References
    public NeuralNetwork neuralNetwork;

    //Transform Component (used for position)
    private Transform _nearestPipe;

    //RigidBody2D Component (used for physics interactions)
    private Rigidbody2D _rb;

    //Fitness Variables
    public float fitness;

    //State Variables
    public bool isAlive;
    private bool _isRunning;

    //NN Architechture Variables
    private int _input = 5;
    private int _output = 1;

    //Runs as soon as application is started or Agent is instantiated
    public void Awake()
    {
        //Sets the Agent as inactive until ready
        isAlive = false;
        gameObject.SetActive(false);
        _isRunning = false;

        //Gains reference to the RigidBody2D component
        _rb = GetComponent<Rigidbody2D>();
    }

    /*
    Called 50 times a second (at base time scale)
    Control System for Agent actions and behavior
    */
    private void FixedUpdate()
    {
        //Determines if the Agent is active
        if (_isRunning)
        {
            //If Agent leaves bounds, treat it as dead
            if (transform.position.y > 6f || transform.position.y < -6f)
            {
                isAlive = false;
                gameObject.SetActive(false);
                _isRunning = false;
            }
            else
            {
                //If within bounds, add the survived time to fitness and supply new inputs
                if (isAlive)
                {
                    fitness += Time.deltaTime;

                    //If the location of the next obstacle is known, update data
                    if (_nearestPipe != null)
                    {
                        /*
                        The Agent recieves the following:
                            -Agent Y Position
                            -Agent Y Velocity
                            -X-Axis Distance to Obstacle
                            -Y-Axis Distance to Obstacle
                            -Scale of Objective Area
                        */
                        float[] inputs = new float[_input];
                        inputs[0] = transform.position.y;
                        inputs[1] = _rb.linearVelocity.y;
                        inputs[2] = _nearestPipe.position.x - transform.position.x;
                        inputs[3] = _nearestPipe.position.y - transform.position.y;
                        inputs[4] = _nearestPipe.transform.localScale.y;

                        //Feeds the input values into the Neural Network
                        float[] output = neuralNetwork.FeedForward(inputs);

                        //Determines if the Agent should take an action or not
                        if (output[0] >= 0f)
                        {
                            Jump();
                        }
                    }
                }
            }
        }
    }

    //Creates a new NN with specified dimensions
    public void setNNSize(int[] hL)
    {
        neuralNetwork = gameObject.AddComponent<NeuralNetwork>();
        neuralNetwork.Initialize(_input, hL, _output);
    }

    //Jump function, allows the Agent to take action
    private void Jump()
    {
        _rb.linearVelocity = Vector2.up * 8f;
    }

    //References a position of the nearest pipe to use for inputs
    public void SetNearestPipe(Transform pipe)
    {
        _nearestPipe = pipe;
    }

    //If the Agent collides, treat it as dead
    private void OnCollisionEnter2D(Collision2D collision)
    {
        isAlive = false;
        gameObject.SetActive(false);
        _isRunning = false;
    }

    //Resets the Agent to a default state
    public void resetGeneration()
    {
        fitness = 0f;
        isAlive = true;
        transform.position = new Vector2(-7f, Random.Range(-3f, 3f));
        gameObject.SetActive(true);

        _isRunning = true;
    }
}
