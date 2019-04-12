using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //находим все канвасы и ставим себя в качестве основной камеры
        Canvas[] findeObjects = FindObjectsOfType(typeof(Canvas)) as Canvas[];
        Camera camera = this.transform.GetComponent<Camera>();

        foreach (Canvas item in findeObjects)
        {
            item.worldCamera = camera;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
