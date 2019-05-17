using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    private const float DefaultAspectRatio = 2f; // iPhone 5 landscape ratio
    private const float DefaultOrthographicSize = 5f;
    private Camera _camera;
    // Start is called before the first frame update
    void Start()
    {
        //находим все канвасы и ставим себя в качестве основной камеры
        Canvas[] findeObjects = FindObjectsOfType(typeof(Canvas)) as Canvas[];
        _camera = this.transform.GetComponent<Camera>();        

        foreach (Canvas item in findeObjects)
        {
            item.worldCamera = _camera;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //_camera.orthographicSize = DefaultOrthographicSize;

        //_camera.projectionMatrix = Matrix4x4.Ortho(
        //-DefaultOrthographicSize * DefaultAspectRatio,
        //DefaultOrthographicSize * DefaultAspectRatio,
        //-DefaultOrthographicSize, DefaultOrthographicSize,
        //_camera.nearClipPlane, _camera.farClipPlane);

        float ratio = (float)Screen.height / Screen.width;
        if (ratio > 0.5625f)
        {
            float ortSize = 5.3f / 0.5625f * ratio;
            Camera.main.orthographicSize = ortSize;
        }
        else
        {
            Camera.main.orthographicSize = 5.3f;
        }
        


    }
}
