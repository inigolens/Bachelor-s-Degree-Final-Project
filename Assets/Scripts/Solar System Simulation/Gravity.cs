using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] GameObject sun;
    static float Gvalue = 9.8f;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        AddGravityForce(sun.GetComponent<Rigidbody>(), this.GetComponent<Rigidbody>());
    }

    public static void AddGravityForce(Rigidbody atractor, Rigidbody target)
    {
        float massProduct = (atractor.mass*target.mass)* Gvalue;
        Vector3 difference = atractor.position - target.position;
        float distance = difference.magnitude;

        float unScaledforceMagnitude = massProduct / Mathf.Pow(distance, 2);
        float forceMagnitude = Gvalue * unScaledforceMagnitude;

        Vector3 forceDirection = difference.normalized;

        Vector3 forceVector = forceDirection * forceMagnitude;
        target.AddForce(forceVector);


    }
}
