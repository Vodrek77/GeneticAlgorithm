using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JetBrains.Annotations;

public class GeneticAlgorithm : MonoBehaviour
{
    //References to GameObjects and Prefabs
    public GameObject birdPrefab;
    public GameObject pipeContainer;
    public GameObject simManager;
    private GameObject _pipeMgr;
    private GameObject _birdContainer;

    //Transform Component (used for position)
    public Transform spawnPoint;

    //Size of the Population
    private int _populationSize = 100;

    //Mutation Variables
    private float _mutationRate;
    private float _mutationMagnitude;

    //Generation Count and Evolution Variables
    public int generation = 0;
    private bool _isEvolving;

    //Time Scale and Control Variables
    public float timeScale = 50.0f;
    private bool _resetting = false;

    //Simulation Variables
    private int _simType;
    private int _simNum;

    //Lists
    public List<Bird> birds;

    //Arrays
    public int[] currentHL;

    //Runs as soon as application is started or script is instantiated
    private void Awake()
    {
        //Sets timeScale to the base time scale
        Time.timeScale = timeScale;
    }

    /*
    Called 50 times a second (at base time scale)
    Control System for Agents and Evolution
    */
    private void FixedUpdate()
    {
        //Starts a new Generation if conditions are met
        if (AllBirdsDead() && !_isEvolving && !_resetting)
        {
            _isEvolving = true;
            StartNewGeneration();
        }
        else
        {
            //If all Agents are not dead, relays information to them
            for (int i = 0; i < birds.Count; i++)
            {
                birds[i].SetNearestPipe(_pipeMgr.GetComponent<PipeManager>().GetNearestPipe());
            }
        }
    }

    //Initalizes the first population of a simulation
    private void InitPopulation()
    {
        //Creates a new List
        birds = new List<Bird>();

        //Creates Bird objects (Agents) to populate the list, as well as adding Neural Networks to them
        for (int i = 0; i < _populationSize; i++)
        {
            GameObject birdObj = Instantiate(birdPrefab, new Vector3(-7f, Random.Range(-3f, 3f), 0f), Quaternion.identity, _birdContainer.transform);
            birds.Add(birdObj.GetComponent<Bird>());
            birds[i].GetComponent<Bird>().setNNSize(currentHL);
        }

        //Ensures all birds are reset when the first generation begins
        for (int j = 0; j < birds.Count; j++)
        {
            birds[j].resetGeneration();
        }
    }


    //Checks if all the birds are dead
    public bool AllBirdsDead()
    {
        //Loops through all Agents
        for (int i = 0; i < birds.Count; i++)
        {
            //Checks if each Agent is alive, if one is, returns false
            if (birds[i].isAlive)
            {
                _isEvolving = false;
                return false;
            }
        }
        return true;
    }

    /*
    Handles Evolution, including:
        -Saving Data (When a Simulation is Complete)
        -Finding Parents (Selection Method)
        -Creating new Agents from Parent Objects
            -Instantiation
            -Crossover
            -Mutation
        -Removes old, unused Agents
    */
    private void Evolve()
    {
        //Creates new Lists
        List<Bird> newBirds = new List<Bird>();
        List<Bird> parents = new List<Bird>();

        //Sorts Agents based on Fitness
        birds.Sort((a, b) => b.fitness.CompareTo(a.fitness));

        Debug.Log("GENERATION " + generation + " COMPLETE - MAX FITNESS: " + birds[0].fitness);

        //Ensures the generation ran as intended, if not, restart
        if (birds[0].fitness == 0)
        {
            for(int i = 0; i < birds.Count; i++)
            {
                birds[i].resetGeneration();
            }
            return;
        }

        //References the SimulationManager to save Data if needed
        SimulationManager simScript = simManager.GetComponent<SimulationManager>();
        if(generation%20 == 0)
        {
            simScript.UpdateData(birds);
        }
        if (generation > 499)
        {
            simScript.SaveData();
            simScript.StartNewSim();
        }

        //Determines selection method to be used to find parents
        if (_simType == 1)
        {
            parents = TruncationSelection(birds, _simNum);
        }
        else if (_simType == 2)
        {
            parents = RouletteWheelSelection(birds, _simNum);
        }
        else if(_simType == 3)
        {
            parents = RankSelection(birds, _simNum);
        }
        else if(_simType == 4)
        {
            parents = TournamentSelection(birds, _simNum);
        }
        else if(_simType == 5)
        {
            parents = SUSSelection(birds, _simNum);
        }
        else if(_simType == 6)
        {
            //Elitism Selection
            birds.Sort((a, b) => b.fitness.CompareTo(a.fitness));
            parents = birds;
            for (int i = 0; i < 2 * _simNum; i++)
            {
                newBirds.Add(parents[i]);
            }
        }

        // Crossover and mutate to create new Agents
        while (newBirds.Count < _populationSize)
        {
            //Choose 2 random parents out of the List
            int parent1Index = Random.Range(0, parents.Count);
            int parent2Index = Random.Range(0, parents.Count);

            //If Parents are the same, chooses a different one
            while(parent1Index == parent2Index)
            {
                parent2Index = Random.Range(0, parents.Count);
            }

            //References to Agent Objects
            Bird parent1 = parents[parent1Index];
            Bird parent2 = parents[parent2Index];

            //Instantiation, Crossover, and Mutation to create a new Agent
            Bird child = Instantiate(birdPrefab, spawnPoint.position, Quaternion.identity, _birdContainer.transform).GetComponent<Bird>();
            child.GetComponent<Bird>().setNNSize(currentHL);
            child.neuralNetwork = Crossover(parent1, parent2, child.neuralNetwork);
            Mutate(child.neuralNetwork);
            newBirds.Add(child);
        }

        //Ensures all Agents that are not used are destroyed
        foreach (Bird bird in birds)
        {
            if (!newBirds.Contains(bird))
            {
                Destroy(bird.gameObject);
            }
        }
        birds = new List<Bird>(newBirds);
        foreach (Bird parent in parents)
        {
            if (!birds.Contains(parent))
            {
                Destroy(parent.gameObject);
            }
        }

        //Starts the next generation with new Agents
        for (int k = 0; k < birds.Count; k++)
        {
            birds[k].gameObject.SetActive(true);
            birds[k].resetGeneration();
        }

        //Increments generation count
        generation++;
    }

    /*
    Truncation Selection:
        -Keeps the TOP x% of Population
        -Parameters are:
            -100% Population
            -80% Population
            -60% Population
            -40% Population
            -20% Population
        -Choose a random parent from available Agents
        -Repeat for each parent
        -Return parents
    */
    private List<Bird> TruncationSelection(List<Bird> birds, int coefficient)
    {
        List<Bird> parents = new List<Bird>();

        //Sort for highest fitness
        birds.Sort((a, b) => b.fitness.CompareTo(a.fitness));
        for (int i = 0; i < 20 * (6 - _simNum); i++)
        {
            parents.Add(birds[i]);
        }

        return parents;
    }

    /*
    Roulette Wheel Selection:
        -Scales each Agent's fitness and adds them up
        -Parameters are:
            -Fitness ^ 0.5
            -Fitness ^ 0.75
            -Fitness ^ 1 (Natural)
            -Fitness ^ 1.25
            -Fitness ^ 1.5
        -Lower Scaling = Lower Value of Fitness
        -Higher Scaling = Higher Value of Fitness
        -Chooses a random number 0 to total fitness
        -Chooses Agents at random and adds fitness to a variable,
            if an Agent's fitness goes above, it becomes a parent
        -Repeat for each parent
        -Return parents
    */
    private List<Bird> RouletteWheelSelection(List<Bird> birds, int coefficient)
    {
        //Creates a new List
        List<Bird> parents = new List<Bird>();

        //Decides scaling factor
        float scalingFactor = .5f + (.25f * (coefficient - 1));
        float totalFitness = 0f;

        //Finds total fitness of all Agents (scaled)
        foreach (Bird agent in birds)
        {
            totalFitness += Mathf.Pow(agent.fitness, scalingFactor);
        }

        //Creates a list of parents
        for(int i = 0; i < _populationSize/2; i++)
        {
            //Generate a random value for selection
            float randomValue = Random.Range(0f, totalFitness);

            //Select based on the random value
            float cumulativeFitness = 0f;
            foreach (Bird agent in birds)
            {
                cumulativeFitness += Mathf.Pow(agent.fitness, scalingFactor);

                if (cumulativeFitness >= randomValue)
                {
                    parents.Add(agent);
                }
            }
        }

        return parents;
    }

    /*
    Stochastic Universal Sampling (SUS) Selection:
        -Adds up each Agent's fitness, selects based on pointers
        -Parameters are:
            -1 Pointer
            -2 Pointer
            -3 Pointer
            -4 Pointer
            -5 Pointer
        -Finds distance of starting pointer (0 to totalFitness/pointers)
        -Chooses a random Agent and adds to the fitness count
        -If an Agent passes a pointer for the first time, add them as a parent
        -Repeat for all parents
        -Return parents
    */
    private List<Bird> SUSSelection(List<Bird> birds, int coefficient)
    {
        //Creates a new List
        List<Bird> parents = new List<Bird>();

        //Adds up fitness values
        float totalFitness = 0f;
        foreach (Bird agent in birds)
        {
            totalFitness += (agent.fitness);
        }

        //Calculate distance between pointers
        float pointerSpacing = totalFitness/coefficient;

        //Finds parents
        while(parents.Count < _populationSize/2)
        {
            //Finds placing of first pointer
            float startPointer = Random.Range(0f, pointerSpacing);

            //Sets up iteration variables
            float cumulativeFitness = 0f;
            int currentSelection = 0;

            //Loop over the population and place the pointers
            for (int i = 0; i < birds.Count; i++)
            {
                cumulativeFitness += birds[i].fitness;

                //Select parents when cumulative fitness passes the current pointer
                while (cumulativeFitness > startPointer + currentSelection * pointerSpacing && currentSelection < coefficient)
                {
                    parents.Add(birds[Mathf.Clamp(i - 1, 0, birds.Count - 1)]);
                    currentSelection++;
                }
            }
        }

        return parents;
    }

    /*
    Rank Selection:
        -Sorts all Agents by fitness (scaled) and assigns a probability
            based on their fitness
        -Parameters are:
           -Fitness ^ 0.5
           -Fitness ^ 0.75
           -Fitness ^ 1 (Natural)
           -Fitness ^ 1.25
           -Fitness ^ 1.5
        -Finds the total of ranks
        -Creates probabilities for each rank (higher rank = higher probability)
        -Choose a random agent, add their probability up
        -If an Agent passes the probability limit, add them as a parent
        -Repeat for all parents
        -Return parents
    */
    private List<Bird> RankSelection(List<Bird> birds, int coefficient)
    {
        //Creates new Lists
        List<Bird> parents = new List<Bird>();
        List<float> rankProbabilities = new List<float>();

        //Decides scaling factor
        float scalingFactor = .5f + (.25f * (coefficient - 1));
        float totalSum = 0f;
        
        //Sorts Agents based on fitness
        birds.Sort((a, b) => b.fitness.CompareTo(a.fitness));

        //Creates a variable to track total rank
        float totalRank = 0f;

        //Adds all rankings to the varible, as well as probabilities to a List
        for (int i = 0; i < _populationSize; i++)
        {
            totalRank += (_populationSize - i);
            rankProbabilities.Add(Mathf.Pow(_populationSize - i, scalingFactor));
        }

        //Creates a list of probabilities for each Agent based on rank
        for (int i = 0; i < _populationSize; i++)
        {
            rankProbabilities[i] /= totalRank;
        }

        //Finds the total probability
        for (int i = 0; i < rankProbabilities.Count - 1; i++)
        {
            totalSum += rankProbabilities[i];
        }

        //Ensures the algorithm will always lead to selection
        rankProbabilities[rankProbabilities.Count - 1] = 1f - totalSum;

        //Creates a list of parents
        for (int i = 0; i < _populationSize/2; i++)
        {
            //Determines probabilities for the next parent
            float randomValue = Random.value;
            float cumulativeProbability = 0f;

            //Adds probabilities up until it passes threshold
            for (int j = 0; j < _populationSize; j++)
            {
                cumulativeProbability += rankProbabilities[j];

                //Adds Agent as a parent
                if (cumulativeProbability >= randomValue)
                {
                    parents.Add(birds[j]);
                    break;
                }
            }
        }

        return parents;
    }

    /*
    Tournament Selection:
        -Chooses x random Agents
        -Parameters are:
           -2 ^ 1 Agents
           -2 ^ 2 Agents
           -2 ^ 3 Agents
           -2 ^ 4 Agents
           -2 ^ 5 Agents
        -Out of the chosen Agents, finds the one with highest fitness
        -Adds highest fitness Agent as a parent
        -Repeat for all parents
        -Return parents
    */
    private List<Bird> TournamentSelection(List<Bird> birds, int coefficient)
    {
        //Creates new Lists
        List<Bird> parents = new List<Bird>();
        List<Bird> tournament = new List<Bird>();

        //Chooses random Agents
        for (int i = 0; i < Mathf.Pow(2, coefficient); i++)
        {
            tournament.Add(birds[Random.Range(0, birds.Count)]);
        }

        //Creates a list of parents
        for (int i = 0; i < _populationSize/2; i++)
        {
            //Determines the best Agent, add as a parent
            Bird fittest = tournament[0];
            foreach (Bird agent in tournament)
            {
                if (agent.fitness > fittest.fitness)
                {
                    fittest = agent;
                }
            }

            parents.Add(fittest);
        }

        return parents;
    }

    //Crosses over neural networks to provide changes to weights and biases based on parents
    private NeuralNetwork Crossover(Bird parent1, Bird parent2, NeuralNetwork child)
    {
        //For every weight, choose what the new Agent's weights for that neuron will be
        for (int i = 0; i < child.weights.Length; i++)
        {
            for (int j = 0; j < child.weights[i].Length; j++)
            {
                for (int k = 0; k < child.weights[i][j].Length; k++)
                {
                    //Weighted selection (based on fitness) for which parent's weight gets inheirited
                    child.weights[i][j][k] = Random.Range(0f, parent1.fitness + parent2.fitness) >= parent1.fitness ? parent2.neuralNetwork.weights[i][j][k] : parent1.neuralNetwork.weights[i][j][k];
                }
            }
        }

        //For every bias, choose what the new Agent's biases for that neuron will be
        for (int a = 0; a < child.biases.Length; a++)
        {
            for(int b = 0; b < child.biases[a].Length; b ++)
            {
                //Weighted selection (based on fitness) for which parent's bias gets inheirited
                child.biases[a][b] = Random.Range(0f, parent1.fitness + parent2.fitness) >= parent1.fitness ? parent2.neuralNetwork.biases[a][b] : parent1.neuralNetwork.biases[a][b];
            }
        }

        //Returns a child Agent with a new neural network with combined weights and biases from parents
        return child;
    }

    //Mutates the neural networks to introduce diversity and different solutions into the system
    private void Mutate(NeuralNetwork network)
    {
        /*
        Determines mutation rate and magnitude for this mutation
        These scale based on how many generations have passed, decreasing over time to retain solutions while searching for small optimizations to these networks
        */
        _mutationRate = 20 * (Mathf.Log(generation + 1, 10) / Mathf.Pow(generation, .5f));
        _mutationMagnitude = 3 * (Mathf.Log(generation + 1, 10)/generation);

        //For each weight, check if it will be mutated, if so, mutate the weight based on variables
        for (int i = 0; i < network.weights.Length; i++)
        {
            for (int j = 0; j < network.weights[i].Length; j++)
            {
                for (int k = 0; k < network.weights[i][j].Length; k++)
                {
                    if(Random.Range(0f, 100f) <= _mutationRate)
                    {
                        network.weights[i][j][k] = network.weights[i][j][k] + Random.Range(-_mutationMagnitude, _mutationMagnitude);
                    }
                }
            }
        }

        //For each bias, check if it will be mutated, if so, mutate the weight based on variables
        for (int a = 0; a < network.biases.Length; a++)
        {
            for(int b = 0; b < network.biases[a].Length; b++)
            {
                if(Random.Range(0f, 100f) <= _mutationRate)
                {
                    network.biases[a][b] = network.biases[a][b] + Random.Range(-_mutationMagnitude, _mutationMagnitude);
                }
            }
        }
    }


    //Starts a new generation of Agents and resets the simulation environment
    private void StartNewGeneration()
    {
        //Destroys and creates a new object to control obstacles
        DestroyImmediate(transform.GetChild(1).transform.gameObject, false);
        _pipeMgr = Instantiate(pipeContainer, gameObject.transform);

        //Evolves and starts the next generation of Agents
        Evolve();
    }

    //Starts a new simulation based on a new NN architecture, selection method, and parameters 
    public void StartNewSimulation(int[] hiddenLayers, int simT, int simN)
    {
        //Changes to variables
        _resetting = true;

        _simType = simT;
        _simNum = simN;
        generation = 0;

        currentHL = hiddenLayers;

        //Destroys all other previous Agents
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        //Creates a new container for Agents
        _birdContainer = new GameObject("BirdContainer");
        _birdContainer.transform.parent = gameObject.transform;

        //Instantiates an obstacle spawner
        _pipeMgr = Instantiate(pipeContainer, gameObject.transform);

        //Initializes the first generation
        InitPopulation();

        _resetting = false;
    }
}