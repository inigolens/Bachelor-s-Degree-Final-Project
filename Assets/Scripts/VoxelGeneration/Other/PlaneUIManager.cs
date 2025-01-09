using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlaneUIManager : MonoBehaviour
{
    public GameObject planet;
    public GameObject uiObject; // El objeto UI a animar
    bool activated = false;
    private Vector3 originalScale; // Escala original del objeto
    [SerializeField] Compass compass;
    [SerializeField] TextMeshProUGUI shipDirectionText;
    public GameObject activableInterface;
    private bool activatedAnimatedUi;
    [SerializeField] GameObject firstPersonUI;
    // Start is called before the first frame update
    void Start()
    {
        PlanetInterfaceComponent insPlanetUI = planet.GetComponent<PlanetInterfaceComponent>();
        if (SolarSystemManager.instance == null) return;
        insPlanetUI.changePlanetName(SolarSystemManager.instance.lastplanetttouched + 1);
        print(SolarSystemManager.instance.detectedVoxelIDs[SolarSystemManager.instance.lastplanetttouched].Count);
        insPlanetUI.changeNumOfMaterials(SolarSystemManager.instance.detectedVoxelIDs[SolarSystemManager.instance.lastplanetttouched].Count, SolarSystemManager.instance.planets[SolarSystemManager.instance.lastplanetttouched].GetComponentInChildren<Planet>().colorSettings.biomeColourSettings.biomes.Length * 2);
        if (uiObject != null)
        {
            originalScale = uiObject.transform.localScale; // Guarda la escala original
        }
    }

    // Update is called once per frame
    public void UpdatePlanetUI()
    {
        if (activated)
        {
            PlanetInterfaceComponent insPlanetUI = planet.GetComponent<PlanetInterfaceComponent>();
            if (SolarSystemManager.instance == null) return;
            insPlanetUI.changePlanetName(SolarSystemManager.instance.lastplanetttouched + 1);
            insPlanetUI.changeNumOfMaterials(SolarSystemManager.instance.detectedVoxelIDs[SolarSystemManager.instance.lastplanetttouched].Count, WorldManager.Instance.WorldColors.Length - 1);
            
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            activated = !activated;
            activableInterface.SetActive(activated);
            if(activated) UpdatePlanetUI();
        }
        if (activated)
        {
            shipDirectionText.text = compass.GetCompassDirection();
        }
    }

    public void startFoundMaterialAnimation()
    {
        if(!activatedAnimatedUi) StartCoroutine(AnimateUI());
    }

    private IEnumerator AnimateUI()
    {
        activatedAnimatedUi = true;
        uiObject.SetActive(true); // Activa el objeto UI
        uiObject.transform.localScale = Vector3.zero; // Establece el tamaño inicial a 0

        float duration = 0.5f; // Duración total de la animación (medio segundo)
        float elapsedTime = 0f;

        // Fase de crecimiento
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            uiObject.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        // Asegurarse de que alcanza la escala final
        uiObject.transform.localScale = originalScale;
        yield return new WaitForSeconds(1f);
        elapsedTime = 0f;

        // Fase de encogimiento
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            uiObject.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        // Asegurarse de que alcanza la escala final
        uiObject.transform.localScale = Vector3.zero;

        uiObject.SetActive(false); // Desactiva el objeto UI
        activatedAnimatedUi=false;
    }

    public void changeFirstPerson(bool active)
    {
        firstPersonUI.SetActive(active);
    }
}
