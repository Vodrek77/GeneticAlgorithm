using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    //Instance Variables
    private int _inputSize;
    private int _outputSize;

    //Arrays
    public int[] hiddenSizes;
    public float[][] biases;
    public float[][][] weights;

    //Initalizes the NN architecture as called
    public void Initialize(int inputSize, int[] hiddenSizes, int outputSize)
    {
        //Sets architechture
        _inputSize = inputSize;
        _outputSize = outputSize;
        this.hiddenSizes = hiddenSizes;

        //Initializes the first set of weights and biases for the system
        InitializeWeightsAndBiases();
    }

    //Fills in all the variables based on inputted values
    private void InitializeWeightsAndBiases()
    {
        //Declares the amount of layers
        int layers = hiddenSizes.Length + 1;

        //Creates the first layer of the weights and biases arrays
        weights = new float[layers][][];
        biases = new float[layers][];

        int previousSize = _inputSize;

        //Fills in the remaining dimensions of the weights and biases arrays to fufill the architecture 
        for (int i = 0; i < layers; i++)
        {
            //Determines whether or not this layer should be another hidden layer or an output layer
            int currentSize = (i == hiddenSizes.Length) ? _outputSize : hiddenSizes[i];
            weights[i] = new float[previousSize][];
            biases[i] = new float[currentSize];

            //Initializes weights
            for (int j = 0; j < previousSize; j++)
            {
                //Sets starting weights to a random value
                weights[i][j] = new float[currentSize];
                for (int k = 0; k < currentSize; k++)
                {
                    weights[i][j][k] = Random.Range(-1f, 1f);
                }
            }

            //Initializes biases
            for (int j = 0; j < currentSize; j++)
            {
                //Sets starting biases to a random value
                biases[i][j] = Random.Range(-1f, 1f);
            }

            //Uses the current size as the previous size for the next layer
            previousSize = currentSize;
        }
    }

    //Sends data through the network and determine whether or not outputs should activate based on values
    public float[] FeedForward(float[] inputs)
    {
        float[] outputs = inputs;

        //Feeds each new value into the next layer, all through to the output layer
        for (int i = 0; i < hiddenSizes.Length + 1; i++)
        {
            outputs = Activate(MatrixMultiply(outputs, weights[i], biases[i]));
        }

        return outputs;
    }

    //Does matrix multiplication on the weights and biases to determine the values to feed forward
    public float[] MatrixMultiply(float[] inputs, float[][] weights, float[] biases)
    {
        //Determines how many ouputs there are
        int outputLength = weights[0].Length;
        float[] outputs = new float[outputLength];

        //For each output, determine the value to be sent
        for (int i = 0; i < outputLength; i++)
        {
            outputs[i] = biases[i];
            for (int j = 0; j < inputs.Length; j++)
            {
                outputs[i] += inputs[j] * weights[j][i];
            }
        }

        return outputs;
    }

    //Determines whether or not the output neuron fires or not (jump/not jump)
    public float[] Activate(float[] inputs)
    {
        float[] outputs = new float[inputs.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            //Uses the Tanh function to scale the inputted values and determine an output
            outputs[i] = (float) System.Math.Tanh(inputs[i]);
        }
        return outputs;
    }
}