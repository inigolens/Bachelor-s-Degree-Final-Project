using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SolarSystemManager : MonoBehaviour
{
    public static SolarSystemManager instance;
    public GameObject[] planets;
    float sunRadius;
    float maxDistance = 850;
    float minScale = 10;
    [SerializeField]
    private GameObject sun;
    public GameObject father;
    public Shader atmosphereShader;
    public int lastplanetttouched;
    [SerializeField] Material moonMaterial;

    public int seed;

    public Transform shipTransform;

    public List<List<int>> detectedVoxelIDs = new List<List<int>>();

    void Start()
    {
        //Random.seed = 9999999;
        if(ReadWrite.Instance !=null)if (ReadWrite.Instance.ReadIntFromFile() != null) Random.seed = ReadWrite.Instance.ReadIntFromFile();
        print(Random.seed);
        seed = (int) Random.Range(-9999, 9999);
        sunRadius = sun.transform.localScale.x/1.2f;
        GenerateSolarSystem();



    }

   
    void Awake()
    {
        if(instance != null && instance != this) {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this; 
        }
        DontDestroyOnLoad(this.gameObject);  // Esto asegura que el objeto y todos sus hijos persistan entre escenas
    }
    //void LateUpdate()
    //{
    //    // Llamando a la lógica después de que Unity haya procesado las transformaciones
    //    AdjustChildScales();
    //}

    public async void GenerateSolarSystem()
    {
        int planetnum = Random.Range(1, 6);
        for (int f = 0; f < planetnum; f++){
            detectedVoxelIDs.Add(new List<int>());
        }
        planets = new GameObject[planetnum];
        float planetposition = sunRadius;

        for (int i = 0; i < planetnum; i++)
        {
            GameObject planet = new GameObject("Planet" + i);
            planet.gameObject.layer = 7;
            planet.transform.parent = father.transform;

            GameObject terrain = new GameObject("Terrain");

            // Ajustar la escala local de terrain antes de agregar el componente Planet
            terrain.transform.localScale = Vector3.one;

            terrain.AddComponent<Planet>();
            terrain.transform.parent = planet.transform;

            terrain.transform.localPosition = Vector3.zero;
            float planetScale = Random.Range(minScale * 4, sunRadius / 1.5f);
            terrain.transform.localScale = new Vector3(planetScale, planetScale, planetScale);
            GameObject atmosphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            atmosphere.transform.localScale = new Vector3(1.5f * planetScale, 1.5f * planetScale, 1.5f * planetScale);
            atmosphere.transform.parent = terrain.transform;
            atmosphere.gameObject.name = "atmosphere";
            atmosphere.GetComponent<Renderer>().material = new Material(atmosphereShader);
            atmosphere.gameObject.layer = 7;
            
            int numSatellites = 0;
            float lastSatPosition = planetScale;

            if (planetScale / 4 > minScale)
            {
                numSatellites = Random.Range(0, 2);

                if (numSatellites != 0)
                {
                    GameObject satelites = new GameObject("Satelites");
                    satelites.transform.parent = planet.transform;

                    for (int j = 0; j < numSatellites; j++)
                    {
                        GameObject satelite = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        satelite.transform.parent = satelites.transform;
                        satelite.GetComponent<MeshRenderer>().material = moonMaterial;
                        float sateliteScale = Random.Range(minScale, planetScale / 3f);

                        lastSatPosition = Random.Range(lastSatPosition + sateliteScale, lastSatPosition + planetScale + sateliteScale);
                        satelite.transform.localPosition = new Vector3(lastSatPosition, 0, 0);
                        satelite.transform.localScale = new Vector3(sateliteScale, sateliteScale, sateliteScale);

                        DualRotation satrt = satelite.AddComponent<DualRotation>();
                        satrt.rotationSpeed = Random.Range(10, 40);
                        satrt.selfRotationSpeed = Random.Range(5, 30);
                        lastSatPosition += sateliteScale / 2;
                    }
                }
            }

            planetposition = Random.Range(planetposition + planetScale / 2 + lastSatPosition, planetposition + planetScale / 2 + lastSatPosition);
            planet.transform.localPosition = new Vector3(planetposition, 0, 0);

            DualRotation rt = planet.AddComponent<DualRotation>();
            rt.rotationSpeed = Random.Range(0.1f, 0.6f);
            rt.selfRotationSpeed = 0;

            planets[i] = planet;

            if (planetScale / 2 < lastSatPosition)
            {
                planetposition += lastSatPosition;
            }
            else
            {
                planetposition += planetScale / 2;
            }
        }
        StartCoroutine(AdjustChildScales());
    }
    
    IEnumerator AdjustChildScales()
    {
        yield return null;
        for (int i = 0; i < planets.Length; i++)
        {
            GameObject terrain = planets[i].transform.Find("Terrain").gameObject;
            Color atmospherecolor = terrain.GetComponent<Planet>().colorSettings.oceanColor.Evaluate(0.25f);
           
            for (int j = 0; j < terrain.transform.childCount; j++)
            {
                
                Transform hijo = terrain.transform.GetChild(j);
                if (hijo.gameObject.name.Equals("mesh"))
                {
                    // Obtener la escala global del objeto padre
                    Vector3 parentScale = terrain.transform.lossyScale;

                    // Establecer la posición y escala locales del hijo
                    hijo.localPosition = Vector3.zero;

                    // Ajustar la escala local del hijo en relación con la escala global del objeto padre
                    Vector3 localScale = new Vector3(1, 1, 1);
                    hijo.localScale = localScale;
                }else if (hijo.gameObject.name.Equals("atmosphere"))
                {
                    hijo.GetComponent<Renderer>().material.SetColor("_Color", atmospherecolor);
                }
                

                // Imprimir información de depuración
              
            }
        }
        
    }

    public void DeactiveorActiveChildren(bool activate)
    {
        DeactivateAllChildrenRecursivelyRecusive(transform, activate);
    }

    void DeactivateAllChildrenRecursivelyRecusive(Transform parent, bool activate)
    {
        foreach (Transform child in parent)
        {
            // Desactivar el hijo actual
            child.gameObject.SetActive(activate);

            // Llamar recursivamente a la función para desactivar los subhijos
            DeactivateAllChildrenRecursivelyRecusive(child, activate);
        }
    }

    public void MoveShipAwayFromPlanet(float distance)
    {
        // Calcular la dirección desde el planeta hacia la nave
        Vector3 directionFromPlanetToShip = shipTransform.position - planets[lastplanetttouched].transform.position;

        // Normalizar la dirección para obtener un vector unitario
        Vector3 normalizedDirection = directionFromPlanetToShip.normalized;

        // Hacer que la nave mire en la dirección opuesta al planeta
        shipTransform.rotation = Quaternion.LookRotation(-normalizedDirection, Vector3.up);

        // Mover la nave a la distancia especificada en la dirección opuesta
        shipTransform.position = planets[lastplanetttouched].transform.position + normalizedDirection * distance;
        shipTransform.forward = -shipTransform.up;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Comprueba si se presiona la tecla Escape
        {
            DestroyAndLoadMenu(); // Llama a la función para destruir el objeto y cargar la escena del menú
        }
    }
    public void DestroyAndLoadMenu()
    {
        Destroy(gameObject); // Destruye este objeto.
        SceneManager.LoadScene("Scenes/MenudeInicio"); // Cambia a la escena del menú de inicio.
    }
}
