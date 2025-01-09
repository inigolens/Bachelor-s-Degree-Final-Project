using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsableTile : MonoBehaviour
{
    public string name;
    // 0 Arriba, 1 Abajo, 2 Izquierda, 3 Derecha, 4 Adelante, 5 Atras
    public string[] facetype = new string[6];
    public Mesh mesh;

    public bool containsMesh()
    {
        return mesh != null;
    }
}
