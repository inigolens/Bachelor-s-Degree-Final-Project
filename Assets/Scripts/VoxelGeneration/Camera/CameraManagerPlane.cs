using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraManagerPlane : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("Colisión de la camara");
        SolarSystemManager.instance.DeactiveorActiveChildren(true);
        SceneManager.LoadScene("Scenes/5_SolarSistemGenerator");
    }
}
