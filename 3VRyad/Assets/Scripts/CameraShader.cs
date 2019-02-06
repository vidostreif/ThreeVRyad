using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraShader : MonoBehaviour
{
    public Material material;
    //public GameObject explosionEffect;
    public static CameraShader Instance; // Синглтон

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров CameraShader!");
        }

        Instance = this;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }
}
