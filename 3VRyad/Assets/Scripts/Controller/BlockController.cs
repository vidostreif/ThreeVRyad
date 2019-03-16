using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]// требуем наличие у объекта EventTrigger

public class BlockController : MonoBehaviour
{
    public Block ThisBlock { protected get; set; }
    private Element dragElement = null;
    private float timeFirstClick = 0;
    private readonly float timeBetweenClicks = 0.5f;

    void Start()
    {
        ThisBlock = GetComponent<Block>();
        EventTrigger trigger = GetComponent<EventTrigger>();

        EventTrigger.Entry entryPointerEnter = new EventTrigger.Entry();
        entryPointerEnter.eventID = EventTriggerType.PointerEnter;
        entryPointerEnter.callback.AddListener((data) => { PointerEnter((PointerEventData)data); });
        trigger.triggers.Add(entryPointerEnter);

        EventTrigger.Entry entryPointerClick = new EventTrigger.Entry();
        entryPointerClick.eventID = EventTriggerType.PointerClick;
        entryPointerClick.callback.AddListener((data) => { PointerClick((PointerEventData)data); });
        trigger.triggers.Add(entryPointerClick);

        EventTrigger.Entry entryBeginDrag = new EventTrigger.Entry();
        entryBeginDrag.eventID = EventTriggerType.BeginDrag;
        entryBeginDrag.callback.AddListener((data) => { Drag((PointerEventData)data); });
        trigger.triggers.Add(entryBeginDrag);

        EventTrigger.Entry entryEndDrag = new EventTrigger.Entry();
        entryEndDrag.eventID = EventTriggerType.EndDrag;
        entryEndDrag.callback.AddListener((data) => { EndDrag((PointerEventData)data); });
        trigger.triggers.Add(entryEndDrag);
    }

    public void PointerEnter(PointerEventData data)//начало перетаскивания
    {
        if (InstrumentsManager.Instance.InstrumentPrepared)
        {
            //SpriteRenderer spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            //spriteRenderer.color = new Color(spriteRenderer.color.r + 0.5f, spriteRenderer.color.g + 0.5f, spriteRenderer.color.b + 0.5f, spriteRenderer.color.a);
        }
    }

    public void PointerClick(PointerEventData data)//обработка двойного клика
    {
        //если есть активный инструмент
        if (InstrumentsManager.Instance.InstrumentPrepared)
        {
            InstrumentsManager.Instance.ActivateInstrument(ThisBlock);
        }
        else
        {
            if (ThisBlock.Element != null && ThisBlock.Element.Activated && !ThisBlock.Element.LockedForMove && !ThisBlock.Element.Destroyed)
            {
                if (Time.time > timeFirstClick + timeBetweenClicks)
                {
                    timeFirstClick = Time.time;
                }
                else
                {
                    //Debug.Log("Double click");
                    //Block block = GridBlocks.Instance.GetBlock(ThisBlock.Element);
                    GridBlocks.Instance.Move(ThisBlock);
                }
            }
        }
    }

    public void Drag(PointerEventData data)//начало перетаскивания
    {
        if (!InstrumentsManager.Instance.InstrumentPrepared)
        {
            if (ThisBlock.Element != null && !ThisBlock.Element.LockedForMove && !ThisBlock.Element.Destroyed)
            {
                MasterController.Instance.DragLocalObject(ThisBlock.Element.thisTransform);
                dragElement = ThisBlock.Element;
                dragElement.drag = true;
            }
        }
    }

    public void EndDrag(PointerEventData data)//прекращаем перетаскивание
    {
        if (!InstrumentsManager.Instance.InstrumentPrepared)
        {
            if (ThisBlock.Element != null && !ThisBlock.Element.LockedForMove && !ThisBlock.Element.Destroyed)
            {
                MasterController.Instance.DropLocalObject();
                if (dragElement != null)
                {
                    dragElement.drag = false;
                }
            }
        }
    }
}
