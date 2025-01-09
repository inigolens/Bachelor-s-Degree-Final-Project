using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReadWrite : MonoBehaviour
{
    public static ReadWrite Instance { get; private set; }

    private string seedFilePath;
    private string lastSeedsFilePath;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Esto asegura que el objeto no se destruya al cargar una nueva escena
            seedFilePath = Path.Combine(Application.persistentDataPath, "seed.json");
            lastSeedsFilePath = Path.Combine(Application.persistentDataPath, "lastseeds.json");
            Debug.Log(Application.persistentDataPath);
        }
        else
        {
            Destroy(gameObject);  // Destruye la instancia adicional que se haya creado
        }
    }

    public void WriteRandomIntToFile()
    {
        int randomInt = UnityEngine.Random.Range(0, int.MaxValue);  // Generar un número aleatorio
        string json = JsonUtility.ToJson(new RandomData(randomInt));

        // Escribir el número aleatorio en el archivo JSON
        File.WriteAllText(seedFilePath, json);
        Debug.Log("Random integer written to file: " + randomInt);
        UpdateLastSeedsList(randomInt);  // Actualizar la lista de últimas semillas
        SceneManager.LoadScene("Scenes/5_SolarSistemGenerator");
    }
    public void WriteIntToFile(int num)
    {
        string json = JsonUtility.ToJson(new RandomData(num));

        // Escribir el número aleatorio en el archivo JSON
        File.WriteAllText(seedFilePath, json);
        Debug.Log("Random integer written to file: " + num);
        UpdateLastSeedsList(num);  // Actualizar la lista de últimas semillas
        SceneManager.LoadScene("Scenes/5_SolarSistemGenerator");
    }

    public int ReadIntFromFile()
    {
        try
        {
            string json = File.ReadAllText(seedFilePath);
            RandomData data = JsonUtility.FromJson<RandomData>(json);
            return data.randomInt;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read from file: " + e.Message);
            return -1;  // Retornar un valor de error
        }
    }

    public void LoadSceneIfIntExists()
    {
        if (ReadIntFromFile() >= 0)
        {
            SceneManager.LoadScene("Scenes/5_SolarSistemGenerator");
        }
        else
        {
            Debug.LogError("No valid integer found in JSON, cannot change scene.");
        }
    }

    public List<int> getLastSeedList()
    {
        List<int> seeds = new List<int>();
        if (File.Exists(lastSeedsFilePath))
        {
            string json = File.ReadAllText(lastSeedsFilePath);
            if (!string.IsNullOrWhiteSpace(json)) // Asegúrate de que el archivo no esté vacío o contenga solo espacios en blanco
            {
                SeedsData data = JsonUtility.FromJson<SeedsData>(json);
                if (data != null && data.seeds != null) // Asegúrate de que la deserialización fue exitosa y que el objeto tiene una lista válida
                {
                    seeds = data.seeds;
                }
                else
                {
                    Debug.LogWarning("Last seeds file was not in a valid format or is corrupted.");
                }
            }
            else
            {
                Debug.LogWarning("Last seeds file is empty or contains only whitespace.");
            }
        }
        else
        {
            Debug.LogWarning("No last seeds file found.");
        }
        return seeds;
    }

    public void UpdateLastSeedsList(int newSeed)
    {
        List<int> seeds = new List<int>();
        if (File.Exists(lastSeedsFilePath))
        {
            try
            {
                string json = File.ReadAllText(lastSeedsFilePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    SeedsData data = JsonUtility.FromJson<SeedsData>(json);
                    if (data != null && data.seeds != null)
                    {
                        seeds = data.seeds;
                    }
                    else
                    {
                        Debug.LogWarning("Failed to parse seeds data, starting with an empty list.");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error reading last seeds file: " + e.Message);
            }
        }

        // Check if the new seed already exists and remove it to prevent duplicates
        if (seeds.Contains(newSeed))
        {
            seeds.Remove(newSeed);  // Elimina la semilla si ya existe
        }

        // Insert the new seed at the beginning of the list
        seeds.Insert(0, newSeed);  // Añade la nueva semilla al principio

        // Ensure that only the last 10 seeds are kept
        if (seeds.Count > 20)
        {
            seeds.RemoveRange(10, seeds.Count - 20);  // Mantiene solo las últimas 10 semillas
        }

        // Serialize the list back to JSON and write it to the file
        try
        {
            string newJson = JsonUtility.ToJson(new SeedsData { seeds = seeds });
            File.WriteAllText(lastSeedsFilePath, newJson);
        }
        catch (Exception e)
        {
            Debug.LogError("Error writing to last seeds file: " + e.Message);
        }
    }

    // Clase para almacenar datos en formato JSON
    [Serializable]
    private class RandomData
    {
        public int randomInt;

        public RandomData(int randInt)
        {
            randomInt = randInt;
        }
    }

    [Serializable]
    private class SeedsData
    {
        public List<int> seeds;
    }
}
