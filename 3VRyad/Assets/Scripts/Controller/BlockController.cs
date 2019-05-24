using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]// требуем наличие у объекта EventTrigger

public class BlockController : MonoBehaviour
{
    public Block thisBlock;
    public DirectionEnum permittedDirection = DirectionEnum.All; //разрешенное направление для смещения
    public bool handleСlick = true;//обрабатывать клик
    public bool handleDragging = true;//обрабатывать перетаскивание
    public PointerEventData pointerEventData = null;
    private Element dragElement = null;//перетаскиваемый элемент
    private float timeFirstClick = 0;
    private readonly float timeBetweenClicks = 0.5f;//максимальное время для срабатывания двойного клика

    void Start()
    {
        thisBlock = GetComponent<Block>();
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
        if (InstrumentPanel.Instance.InstrumentPrepared)
        {
            SceneSettings.Instance.HideSetings();
            //SpriteRenderer spriteRenderer = this.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
            //spriteRenderer.color = new Color(spriteRenderer.color.r + 0.5f, spriteRenderer.color.g + 0.5f, spriteRenderer.color.b + 0.5f, spriteRenderer.color.a);
        }
    }

    public void PointerClick(PointerEventData data)//обработка двойного клика
    {
        if (handleСlick)
        {
            SceneSettings.Instance.HideSetings();
            //если есть активный инструмент
            if (InstrumentPanel.Instance.InstrumentPrepared)
            {
                InstrumentPanel.Instance.ActivateInstrument(thisBlock);
            }
            else
            {
                if (thisBlock.Element != null && thisBlock.Element.Activated && !thisBlock.Element.LockedForMove && !thisBlock.Element.Destroyed)
                {
                    if (Time.time > timeFirstClick + timeBetweenClicks)
                    {
                        timeFirstClick = Time.time;
                    }
                    else
                    {
                        //Debug.Log("Double click");
                        //Block block = GridBlocks.Instance.GetBlock(ThisBlock.Element);
                        GridBlocks.Instance.Move(thisBlock);
                    }
                }
            }
        }        
    }

    public void Drag(PointerEventData data)//начало перетаскивания
    {
        if (handleDragging)
        {
            SceneSettings.Instance.HideSetings();
            if (!InstrumentPanel.Instance.InstrumentPrepared)
            {
                if (thisBlock.Element != null && !thisBlock.Element.LockedForMove && !thisBlock.Element.Destroyed)
                {
                    MasterController.Instance.DragElement(this);
                    dragElement = thisBlock.Element;
                    dragElement.drag = true;
                    pointerEventData = data;
                }
            }
        }        
    }

    public void EndDrag(PointerEventData data)//прекращаем перетаскивание
    {
        if (!InstrumentPanel.Instance.InstrumentPrepared)
        {
            if (thisBlock.Element != null && !thisBlock.Element.LockedForMove && !thisBlock.Element.Destroyed)
            {
                MasterController.Instance.DropElement();
                if (dragElement != null)
                {
                    dragElement.drag = false;
                    pointerEventData = null;
                }
            }
        }
    }
}
