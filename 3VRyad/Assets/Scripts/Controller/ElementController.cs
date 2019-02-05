using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]// требуем наличие у объекта EventTrigger

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
        if (!ThisElement.LockedForMove)
        {
            MasterController.Instance.DragLocalObject(transform);
            ThisElement.drag = true;
        }
    }

    public void EndDrag(PointerEventData data)//прекращаем перетаскивание
    {
        if (!ThisElement.LockedForMove)
        {
            MasterController.Instance.DropLocalObject();
            ThisElement.drag = false;
        }
    }

    public void DoubleClick(PointerEventData data)//обработка двойного клика
    {
        if (ThisElement.Activated && !Grid.Instance.blockedForMove)
        {
            if (Time.time > timeFirstClick + timeBetweenClicks)
            {
                timeFirstClick = Time.time;
            }
            else
            {
                Debug.Log("Double click");
                Block block = Grid.Instance.GetBlock(ThisElement);
                Grid.Instance.Move(block);
            }
        }
        
    }
}
