using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlanetInterfaceComponent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI planetName, numOfMaterials;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changePlanetName(int num)
    {
        planetName.text = "Planet " + num;
    }

    public void changeNumOfMaterials(int actualnum, int totalnum)
    {
        numOfMaterials.text = actualnum + "/" + totalnum;
    }
}
