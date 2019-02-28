using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstrumentsManager : MonoBehaviour
{
    public static InstrumentsManager Instance; // Синглтон
    [HideInInspector] public Transform thisTransform;
    public float distanceBetweenInstruments;
    public GameObject prefabInstrument;

    public Instrument[] instruments;// список инструментов 
    private bool instrumentPrepared = false;
    private Instrument preparedInstrument;


    public bool InstrumentPrepared
    {
        get
        {
            return instrumentPrepared;
        }
    }

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров Tasks!");
        }

        Instance = this;
        thisTransform = transform;
    }

    void Start()
    {
        CreateInstruments();
    }

    //создание коллекции целей
    private void CreateInstruments()
    {
        //смещение по y
        float startingYPoint = thisTransform.position.y - ((GridBlocks.Instance.blockSize + distanceBetweenInstruments) * (instruments.Length - 1)) * 0.5f;

        for (int i = 0; i < instruments.Length; i++)
        {
            GameObject elementGameObject = Instantiate(prefabInstrument, new Vector3(thisTransform.position.x, startingYPoint + (i * (GridBlocks.Instance.blockSize + distanceBetweenInstruments)), thisTransform.position.z), Quaternion.identity, this.thisTransform);
            Image image = elementGameObject.GetComponent(typeof(Image)) as Image;
            image.sprite = SpriteBank.SetShape(instruments[i].elementsType);
            instruments[i].Image = image;
            instruments[i].Text = image.GetComponentInChildren<Text>();
            instruments[i].AddAction(elementGameObject);            
        }
    }

    public void PreparInstrument(Instrument instrument)
    {
        //добавить проверку что нет актиного инструмента
        instrumentPrepared = true;
        preparedInstrument = instrument;
    }

    public void ActivateInstrument(Block block)
    {
        if (instrumentPrepared)
        {
            block.Hit();
            instrumentPrepared = false;
            preparedInstrument.SubQuantity();
        }
    }
}
