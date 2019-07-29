using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Start")
        {
            StartCoroutine(Preload());
            DontDestroyOnLoadManager.DontDestroyOnLoad(gameObject); //Set as do not destroy 
        }
        else
        {
            Destroy(gameObject);
        }        
    }

    private IEnumerator Preload() {

        Transform ImageLoadTransform = transform.Find("ImageLoad");
        Image imageLoad = ImageLoadTransform.GetComponent<Image>();
        Text textLoad = ImageLoadTransform.Find("TextLoad").GetComponent<Text>();

        //ожидаем прогрузки кадра
        yield return new WaitForEndOfFrame();
        textLoad.text = "Предварительная загрузка всех ресуров...";
        imageLoad.fillAmount = 0;
        Debug.Log("Предварительная загрузка всех ресуров: " + Time.realtimeSinceStartup);
        Resources.LoadAll("");

        yield return new WaitForEndOfFrame();
        textLoad.text = "Загрузка звуков...";
        imageLoad.fillAmount = 0.20f;
        Debug.Log("Загрузка звуков: " + Time.realtimeSinceStartup);
        SoundBank.Preload();

        yield return new WaitForEndOfFrame();
        textLoad.text = "Загрузка картинок...";
        imageLoad.fillAmount = 0.40f;
        Debug.Log("Загрузка картинок: " + Time.realtimeSinceStartup);
        SpriteBank.Preload();

        //yield return new WaitForEndOfFrame();
        //textLoad.text = "Загрузка эффектов...";
        //imageLoad.fillAmount = 0.60f;
        //Debug.Log("Загрузка эффектов: " + Time.realtimeSinceStartup);
        //ParticleSystemManager.Instance.Preload();
        //gameObject.AddComponent<ParticleSystemManager>().Preload();

        yield return new WaitForEndOfFrame();
        textLoad.text = "Загрузка сохранений...";
        imageLoad.fillAmount = 0.80f;
        Debug.Log("Загрузка сохранений: " + Time.realtimeSinceStartup);
        JsonSaveAndLoad.LoadSave();

        yield return new WaitForEndOfFrame();
        textLoad.text = "Определение времени...";
        imageLoad.fillAmount = 0.90f;
        Debug.Log("Определение времени: " + Time.realtimeSinceStartup);
        CheckTime.Realtime();
        textLoad.text = "Загружаем основную сцену...";
        imageLoad.fillAmount = 1;

        yield return new WaitForSeconds(0.3f);
        //DontDestroyOnLoadManager.DestroyAll();

        //загружаем уровень
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainMenu");
            ////создаем изображение для отображения загрузки уровня
            //GameObject imageMainLoadGO = Instantiate(PrefabBank.ImageMainLoad, transform);
            //Image imageLoadScene = imageMainLoadGO.transform.Find("ImageLoad").GetComponent<Image>();
            //ожидаем загрузки уровня
            float progress = 0;
            while (!asyncLoad.isDone)
            {
                progress = asyncLoad.progress / 0.9f;
                //imageLoadScene.fillAmount = progress;
                yield return new WaitForEndOfFrame();
            }
            //Destroy(imageMainLoadGO);
        Destroy(gameObject);
    }
}
