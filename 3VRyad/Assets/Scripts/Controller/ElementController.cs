using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]// требуем наличие у объекта EventTrigger

//система управления элементом игроком
public class ElementController : MonoBehaviour {

    public Element ThisElement { protected get; set; }
    private float timeFirstClick = 0;
    private readonly float timeBetweenClicks = 0.5f;

    void Start()
    {  
        EventTrigger trigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entryBeginDrag = new EventTrigger.Entry();
        entryBeginDrag.eventID = EventTriggerType.BeginDrag;
        entryBeginDrag.callback.AddListener((data) => { Drag((PointerEventData)data); });
        trigger.triggers.Add(entryBeginDrag);
        EventTrigger.Entry entryEndDrag = new EventTrigger.Entry();
        entryEndDrag.eventID = EventTriggerType.EndDrag;
        entryEndDrag.callback.AddListener((data) => { EndDrag((PointerEventData)data); });
        trigger.triggers.Add(entryEndDrag);
        EventTrigger.Entry entryPointerClick = new EventTrigger.Entry();
        entryPointerClick.eventID = EventTriggerType.PointerClick;
        entryPointerClick.callback.AddListener((data) => { DoubleClick((PointerEventData)data); });
        trigger.triggers.Add(entryPointerClick);
    }

    public void Drag(PointerEventData data)//начало перетаскивания
    {
        if (!ThisElement.LockedForMove && !ThisElement.Destroyed)
        {
            //MasterController.Instance.DragElement(transform);
            ThisElement.drag = true;
        }
    }

    public void EndDrag(PointerEventData data)//прекращаем перетаскивание
    {
        if (!ThisElement.LockedForMove && !ThisElement.Destroyed)
        {
            MasterController.Instance.DropElement();
            ThisElement.drag = false;
        }
    }

    public void DoubleClick(PointerEventData data)//обработка двойного клика
    {
        //если есть актиный инструмент
        if (InstrumentPanel.Instance.InstrumentPrepared)
        {
            //Block block = GridBlocks.Instance.GetBlock(ThisElement);
            //InstrumentsManager.Instance.ActivateInstrument(block);
        }
        else
        {
            if (ThisElement.Activated && !ThisElement.LockedForMove && !ThisElement.Destroyed)
            {
                if (Time.time > timeFirstClick + timeBetweenClicks)
                {
                    timeFirstClick = Time.time;
                }
                else
                {
                    //Debug.Log("Double click");
                    Block block = GridBlocks.Instance.GetBlock(ThisElement);
                    GridBlocks.Instance.Move(block);
                }
            }
        }
             
    }
}
