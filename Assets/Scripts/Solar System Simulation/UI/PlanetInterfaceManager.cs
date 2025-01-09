using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlanetInterfaceManager : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject uiPlanetComponent;
    int planetnum;
    bool activated = false;
    [SerializeField] List<GameObject> planetUIList = new List<GameObject>();
    [SerializeField] TextMeshProUGUI percentageText;
    [SerializeField] GameObject perUI;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (activated)
            {
                foreach (GameObject go in planetUIList)
                {
                    go.SetActive(false);
                }
                perUI.SetActive(false);
                activated = false;
            }
            else
            {
                crearUI();
                activated = true;
            }
            
        }
    }
    private void crearUI()
    {
        int doneMaterials = 0, totalMaterials = 0;
        for (int i = 0; i < SolarSystemManager.instance.planets.Length; i++)
        {
            GameObject ins = planetUIList[i];
            ins.SetActive(true);
            PlanetInterfaceComponent insPlanetUI = ins.GetComponent<PlanetInterfaceComponent>();
            insPlanetUI.changePlanetName(i + 1);
            insPlanetUI.changeNumOfMaterials(SolarSystemManager.instance.detectedVoxelIDs[i].Count, SolarSystemManager.instance.planets[i].GetComponentInChildren<Planet>().colorSettings.biomeColourSettings.biomes.Length * 2);
            doneMaterials += SolarSystemManager.instance.detectedVoxelIDs[i].Count;
            totalMaterials += SolarSystemManager.instance.planets[i].GetComponentInChildren<Planet>().colorSettings.biomeColourSettings.biomes.Length * 2;
        }
        perUI.SetActive(true);
        print("" + doneMaterials + "/" + totalMaterials + " = " + (float)doneMaterials / (float)totalMaterials * 100);
        percentageText.text = ""+(int) ((float) doneMaterials / (float)totalMaterials *100)+"%";
    }
}
