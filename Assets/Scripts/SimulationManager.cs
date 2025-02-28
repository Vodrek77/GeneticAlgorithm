using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;

public class SimulationManager : MonoBehaviour
{
    //GameObjects
    public GameObject geneticAlgorithm;

    //Variables
    private int index = 0;
    public int simulation = 0;
    public int simType = 1;
    public int simNum = 1;
    public int simCount = 1;
    private int snapshot = 0;

    //Arrays
    private int[][] hiddenSizes;
    private float[][][][] fullData;

    //Lists

    void Awake()
    {
        QualitySettings.vSyncCount = 0;

        hiddenSizes = new int[30][];

        for (int i = 1; i <= 5; i++)
        {
            for (int j = 3; j <= 8; j++)
            {
                hiddenSizes[index] = new int[i];

                for (int k = 0; k < i; k++)
                {
                    hiddenSizes[index][k] = j;
                }

                index++;
            }
        }

        fullData = new float[5][][][];
        for(int j = 0; j < fullData.Length; j++)
        {
            fullData[j] = new float[10][][];
            for(int k = 0; k < fullData[j].Length; k++)
            {
                fullData[j][k] = new float[25][];
                for(int l = 0; l < fullData[j][k].Length; l++)
                {
                    fullData[j][k][l] = new float[100];
                }
            }
        }

        simulation = LoadSimIndex();

        StartNewSim();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartNewSim()
    {
        if (simCount == 10)
        {
            if (simNum == 5)
            {
                if (simType == 6)
                {
                    simulation++;
                    simType = 1;

                    if(simulation > 30)
                    {
                        Application.Quit();
                    }
                }
                else
                {
                    simType++;
                }
                simNum = 1;
            }
            else
            {
                simNum++;
            }
            simCount = 0;
        }

        geneticAlgorithm.GetComponent<GeneticAlgorithm>().StartNewSimulation(hiddenSizes[simulation], simType, simNum);

        simCount++;
    }

    public void ResetSim()
    {
        geneticAlgorithm.GetComponent<GeneticAlgorithm>().StartNewSimulation(hiddenSizes[simulation], simType, simNum);
    }

    public void SaveData()
    {
        SimData saveData = new SimData
        {
            simulationType = simType,
            simulationData = Serialize4DArray(fullData)
        };

        string codeNN = "";

        for(int i = 0; i < geneticAlgorithm.GetComponent<GeneticAlgorithm>().currentHL.Length; i++)
        {
            codeNN += (geneticAlgorithm.GetComponent<GeneticAlgorithm>().currentHL[i] + "-");
        }
        codeNN += "sim";

        string pathNN = "C:/mygame/simulation/" + codeNN;

        if (!Directory.Exists(pathNN))
        {
            Directory.CreateDirectory(pathNN);
        }

        string finalPath = pathNN + "/" + simType + "-type";

        string jsonData = JsonUtility.ToJson(saveData, true);

        //LINUX
        File.WriteAllText(finalPath + ".json", jsonData);

        //WINDOWS
        //File.WriteAllText(finalPath + ".json", jsonData);

        SimIndex indexData = new SimIndex
        {
            index = new int[]{simulation, simType, simNum, simCount}
        };

        string simulationData = JsonUtility.ToJson(indexData, true);

        //LINUX
        File.WriteAllText("/mygame/simulation/simIndex.json", simulationData);

        //WINDOWS
        //File.WriteAllText("C:/mygame/simulation/simIndex.json", simulationData);

        Debug.Log("SIMULATION " + simulation + " INFORMATION SAVED");
    }

    public void UpdateData(List<Bird> input)
    {
        input.Sort((a, b) => b.fitness.CompareTo(a.fitness));

        for (int i = 0; i < 100; i++)
        {
            fullData[simNum-1][simCount-1][snapshot][i] = input[i].fitness;
        }
        snapshot++;
        if(snapshot == 25)
        {
            snapshot = 0;
        }
    }


    public int LoadSimIndex()
    {
        //LINUX
        string filePath = "/mygame/simulation/simIndex.json";

        //WINDOWS
        //string filePath = "C:/mygame/simulation/simIndex.json";

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);

            SimIndex data = JsonUtility.FromJson<SimIndex>(json);

            simType = data.index[1];
            simNum = data.index[2];
            simCount = data.index[3];

            return data.index[0];
        }

        simType = 1;
        simNum = 1;
        simCount = 0;

        return 0;
    }

    float[] FlattenJagged3DArray(float[][][] jaggedArray)
    {
        // Calculate the total number of elements in the jagged array
        int totalSize = 0;
        foreach (float[][] level1 in jaggedArray)
        {
            foreach (float[] level2 in level1)
            {
                totalSize += level2.Length;
            }
        }

        // Create a flat 1D array
        float[] flatArray = new float[totalSize];
        int index = 0;

        // Flatten the jagged array into the 1D array
        foreach (float[][] level1 in jaggedArray)
        {
            foreach (float[] level2 in level1)
            {
                foreach (float element in level2)
                {
                    flatArray[index] = element;
                    index++;
                }
            }
        }

        return flatArray;
    }
    public Level1 Serialize4DArray(float[][][][] originalArray)
    {
        // Initialize the nested structure with the same dimensions as the original array
        Level1 l1 = new Level1
        {
            level1 = new Level2[originalArray.Length] // First dimension
        };

        // Populate the nested structure with data from the original jagged array
        for (int i = 0; i < originalArray.Length; i++) // First dimension
        {
            l1.level1[i] = new Level2
            {
                level2 = new Level3[originalArray[i].Length] // Second dimension
            };

            for (int j = 0; j < originalArray[i].Length; j++) // Second dimension
            {
                l1.level1[i].level2[j] = new Level3
                {
                    level3 = new Level4[originalArray[i][j].Length] // Third dimension
                };

                for (int k = 0; k < originalArray[i][j].Length; k++) // Third dimension
                {
                    l1.level1[i].level2[j].level3[k] = new Level4
                    {
                        level4 = new float[originalArray[i][j][k].Length] // Third dimension
                    };

                    for(int l = 0; l < originalArray[i][j][k].Length; l++)
                    {
                        // Transfer values from the original jagged array to the nested structure
                        l1.level1[i].level2[j].level3[k].level4[l] = originalArray[i][j][k][l];
                    }
                }
            }
        }

        return l1;
    }

    public Level2 Serialize3DArray(float[][][] originalArray)
    {
        // Initialize the nested structure with the same dimensions as the original array
        Level2 l2 = new Level2
        {
            level2 = new Level3[originalArray.Length] // First dimension
        };

        // Populate the nested structure with data from the original jagged array
        for (int i = 0; i < originalArray.Length; i++) // First dimension
        {
            l2.level2[i] = new Level3
            {
                level3 = new Level4[originalArray[i].Length] // Second dimension
            };

            for (int j = 0; j < originalArray[i].Length; j++) // Second dimension
            {
                l2.level2[i].level3[j] = new Level4
                {
                    level4 = new float[originalArray[i][j].Length] // Third dimension
                };

                for (int k = 0; k < originalArray[i][j].Length; k++) // Third dimension
                {
                    // Transfer values from the original jagged array to the nested structure
                    l2.level2[i].level3[j].level4[k] = originalArray[i][j][k];
                }
            }
        }

        return l2;
    }

    public Level3 Serialize2DArray(float[][] originalArray)
    {
        // Initialize the nested structure with the same dimensions as the original array
        Level3 l3 = new Level3
        {
            level3 = new Level4[originalArray.Length] // First dimension
        };

        // Populate the nested structure with data from the original jagged array
        for (int i = 0; i < originalArray.Length; i++) // First dimension
        {
            l3.level3[i] = new Level4
            {
                level4 = originalArray[i] // Second dimension
            };
            for (int j = 0; j < originalArray[i].Length; j++) // Third dimension
            {
                // Transfer values from the original jagged array to the nested structure
                l3.level3[i].level4[j] = originalArray[i][j];
            }
        }

        return l3;
    }

    float[] FlattenJagged2DArray(float[][] jaggedArray)
    {
        // Calculate the total number of elements in the jagged array
        int totalSize = 0;
        foreach (float[] level1 in jaggedArray)
        {
            totalSize += level1.Length;
        }

        // Create a flat 1D array
        float[] flatArray = new float[totalSize];
        int index = 0;

        // Flatten the jagged array into the 1D array
        foreach (float[] level1 in jaggedArray)
        {
            foreach (float element in level1)
            {
                flatArray[index] = element;
                index++;
            }
        }

        return flatArray;
    }

    [System.Serializable]
    public class SimIndex
    {
        public int[] index;
    }

    [System.Serializable]
    public class SimData
    {
        public int simulationType;
        public Level1 simulationData;
    }

    [System.Serializable]
    public class Level4
    {
        public float[] level4;
    }

    [System.Serializable]
    public class Level3
    {
        public Level4[] level3;
    }

    [System.Serializable]
    public class Level2
    {
        public Level3[] level2;
    }

    [System.Serializable]
    public class Level1
    {
        public Level2[] level1;
    }
}
