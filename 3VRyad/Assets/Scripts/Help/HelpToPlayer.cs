using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class HelpToPlayer
{
    private static List<Type> enumTypes = new List<Type>();
    private static List<Hint> hintsList = new List<Hint>();
    private static HintStatus[] hintsStatus = null;
    private static Hint activeHint = null;
    //private static bool showHints = JsonSaveAndLoad.LoadSave().SettingsSave.showHints;//показывать подсказки

    private static bool deletedByClickingOnCanvas = false;
    private static float timeCreateHints;
    private static float delayTime = 0;

    //возвращает истину если подсказка активна
    public static bool HelpActive()
    {
        if (activeHint != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //перезагрузка сохранений
    public static void ReloadSave()
    {
        DellGameHelp();
        hintsStatus = null;
        hintsList = new List<Hint>();
        //LoadShowHintsStatus();
        CreateHintStatusList();
    }

    //здесь указываем enum для подсказок
    private static void CreateHintStatusList()
    {
        if (hintsStatus == null)
        {
            //список enum которые используются для подсказок
            enumTypes = new List<Type>();
            enumTypes.Add(typeof(BlockTypeEnum));
            enumTypes.Add(typeof(ElementsTypeEnum));
            enumTypes.Add(typeof(BlockingElementsTypeEnum));
            enumTypes.Add(typeof(BehindElementsTypeEnum));
            enumTypes.Add(typeof(HelpEnum));

            //создаем массив статусов подсказок
            int count = 0;
            foreach (Type item in enumTypes)
            {
                count += Enum.GetNames(item).Length;
            }
            hintsStatus = new HintStatus[count];

            //загружаем сохранения
            List<HelpSave> helpSaves = JsonSaveAndLoad.LoadSave().helpSave;
            int i = 0;
            foreach (Type enumType in enumTypes)
            {
                foreach (var curEnum in Enum.GetValues(enumType))
                {
                    HelpSave helpSave = helpSaves.Find(item => item.elementsTypeEnum == curEnum.ToString());
                    //ищем в сохранениях
                    if (helpSave != null)
                    {
                        hintsStatus[i] = new HintStatus(curEnum.ToString(), helpSave.status);
                    }
                    else
                    {
                        hintsStatus[i] = new HintStatus(curEnum.ToString(), false);
                    }

                    i++;
                }
            }
        }
    }

    public static void ClearHintList()
    {
        hintsList.Clear();//очищаем список подсказок
    }

    public static void AddHint(BlockTypeEnum typeEnum)
    {
        if (SettingsController.ShowHints)
        {
            AddHint(typeof(BlockTypeEnum), typeEnum.ToString(), (int)typeEnum, false);
        }
    }

    public static void AddHint(BlockingElementsTypeEnum elementsTypeEnum)
    {
        if (SettingsController.ShowHints)
        {
            AddHint(typeof(BlockingElementsTypeEnum), elementsTypeEnum.ToString(), (int)elementsTypeEnum, false);
        }
    }

    public static void AddHint(BehindElementsTypeEnum elementsTypeEnum)
    {
        if (SettingsController.ShowHints)
        {
            AddHint(typeof(BehindElementsTypeEnum), elementsTypeEnum.ToString(), (int)elementsTypeEnum, false);
        }
    }

    public static void AddHint(ElementsTypeEnum elementsTypeEnum)
    {
        if (SettingsController.ShowHints)
        {
            AddHint(typeof(ElementsTypeEnum), elementsTypeEnum.ToString(), (int)elementsTypeEnum, false);
        }
    }

    public static void AddHint(HelpEnum helpEnum)
    {
        if (SettingsController.ShowHints)
        {
            AddHint(typeof(HelpEnum), helpEnum.ToString(), (int)helpEnum, true);
        }
    }

    private static void AddHint(Type enumType, string help, int number, bool toTop)
    {
        //если не показывали то добавляем
        if (!ShowedHint(enumType, number))
        {
            //если она уже добавлена в массив, то пропускаем
            Hint hint = hintsList.Find(item => item.help == help);
            if (hint == null)
            {
                //определяем позицию по длинам энумов
                int count = GetEnumPosition(enumType);

                if (toTop)
                {
                    hintsList.Insert(0, new Hint(help, count + number));
                }
                else
                {
                    hintsList.Add(new Hint(help, count + number));
                }
            }
        }
    }

    //проверяет показывали ли мы подсказку в указаном енумо под описанным номером
    private static bool ShowedHint(Type enumType, int number)
    {
        //проверяем показывали ли мы такую подсказку игроку
        CreateHintStatusList();

        //определяем позицию в по длинам энумов
        int count = GetEnumPosition(enumType);

        return hintsStatus[count + number].status;
    }

    private static int GetEnumPosition(Type enumType)
    {
        //определяем позицию в по длинам энумов
        int count = 0;
        foreach (Type item in enumTypes)
        {
            if (item != enumType)
            {
                count += Enum.GetNames(item).Length;
            }
            else
            {
                break;
            }
        }
        return count;
    }

    public static bool CreateNextGameHelp()
    {
        bool created = false;
        if (hintsList.Count > 0 && activeHint == null)
        {
            //!!!добавить все блоки в список для запрета обработки

            List<Hint> hintForDell = new List<Hint>();

            //перебираем подскази до тех пор пока не создадим хоть одну
            foreach (Hint hint in hintsList)
            {
                //перепроверяем не пометили ли эту подсказку как показанную
                if (hintsStatus[hint.numberHelp].status)
                {
                    //добавляем для удаления
                    hintForDell.Add(hint);
                    continue;
                }

                activeHint = hint;
                activeHint.canvasHelpToPlayer = UnityEngine.Object.Instantiate(PrefabBank.CanvasHelpToPlayer);

                //находим нужную подсказку
                if (activeHint.help == BlockTypeEnum.StandardBlock.ToString())
                {
                    //для стандартного блока не показываем подсказку
                    created = false;
                    //помечаем как показанную
                    hintsStatus[activeHint.numberHelp].status = true;
                    //добавляем для удаления
                    hintForDell.Add(hint);
                }
                else if (activeHint.help == BlockTypeEnum.Sliding.ToString())
                {
                    created = CreateBlockHelp(BlockTypeEnum.Sliding);
                }
                else if (activeHint.help == ElementsTypeEnum.StandardElement.ToString())
                {
                    created = CreateStandardElementHelp(3);
                }
                else if (activeHint.help == ElementsTypeEnum.CrushableWall.ToString())
                {
                    created = CreateCrushableWallHelp((ElementsTypeEnum)Enum.Parse(typeof(ElementsTypeEnum), activeHint.help));
                }                
                else if (activeHint.help == ElementsTypeEnum.SmallFlask.ToString() || activeHint.help == ElementsTypeEnum.MediumFlask.ToString() || activeHint.help == ElementsTypeEnum.BigFlask.ToString())
                {
                    created = CreateFlaskHelp((ElementsTypeEnum)Enum.Parse(typeof(ElementsTypeEnum), activeHint.help));
                }
                else if (activeHint.help == ElementsTypeEnum.ImmortalWall.ToString())
                {
                    created = CreateImortalWallHelp();
                }
                else if (activeHint.help == ElementsTypeEnum.Drop.ToString())
                {
                    created = CreateDropElementHelp();
                }
                else if (activeHint.help == ElementsTypeEnum.SeedBarrel.ToString())
                {
                    created = CreateSeedBarelHelp((ElementsTypeEnum)Enum.Parse(typeof(ElementsTypeEnum), activeHint.help));
                }
                else if (activeHint.help == BlockingElementsTypeEnum.Liana.ToString())
                {
                    created = CreateLianaHelp((BlockingElementsTypeEnum)Enum.Parse(typeof(BlockingElementsTypeEnum), activeHint.help));
                }
                else if (activeHint.help == BehindElementsTypeEnum.Grass.ToString() || activeHint.help == BehindElementsTypeEnum.Dirt.ToString())
                {
                    created = CreateGrassHelp((BehindElementsTypeEnum)Enum.Parse(typeof(BehindElementsTypeEnum), activeHint.help));
                }
                else if (activeHint.help == HelpEnum.Gnome.ToString())
                {
                    created = InterfaceHelp("Gnome");
                }
                else if (activeHint.help == HelpEnum.Tasks.ToString())
                {
                    created = InterfaceHelp("PanelCollectsElement");
                }
                else if (activeHint.help == HelpEnum.Score.ToString())
                {
                    created = InterfaceHelp("ScorePanel");
                }
                else if (activeHint.help == HelpEnum.SuperBonus.ToString())
                {
                    created = InterfaceHelp("SuperBonus");
                }
                else if (activeHint.help == HelpEnum.Instruments.ToString())
                {
                    List<string> flashingItemsNames = new List<string>();
                    flashingItemsNames.Add("Instrument" + InstrumentsEnum.Shovel.ToString());
                    created = InterfaceHelp("PanelInstruments", flashingItemsNames);

                    if (created && ThingsManager.Instance != null)
                    {
                        //дарим один инструмент, если у играка их 0
                        Thing thing = ThingsManager.Instance.GetThing(InstrumentsEnum.Shovel);
                        if (thing != null && thing.Quantity == 0)
                        {
                            BundleShopV[] bundleShopV = new BundleShopV[1];
                            bundleShopV[0] = new BundleShopV(InstrumentsEnum.Shovel, 1);
                            ThingsManager.Instance.addinstruments(bundleShopV);
                        }
                    }                    
                }
                else if (activeHint.help == HelpEnum.Shop.ToString())
                {
                    created = InterfaceHelp("ButtonOpenShop");
                }
                else if (activeHint.help == HelpEnum.Gift.ToString())
                {
                    created = InterfaceHelp("Gnome");
                }
                else if (activeHint.help == HelpEnum.Lifes.ToString())
                {
                    created = InterfaceHelp("PanelLivesParent");
                }
                else if (activeHint.help == HelpEnum.DailyGift.ToString())
                {
                    created = InterfaceHelp("PanelDailyGift");
                }
                else if (activeHint.help == HelpEnum.OptionalLvl.ToString())
                {
                    created = InterfaceHelp("Gnome");
                }
                else if (activeHint.help == HelpEnum.Hoe.ToString())
                {
                    List<string> flashingItemsNames = new List<string>();
                    flashingItemsNames.Add("Instrument" + InstrumentsEnum.Hoe.ToString());
                    created = InterfaceHelp("PanelInstruments", flashingItemsNames);

                    if (created && ThingsManager.Instance != null)
                    {
                        //дарим один инструмент, если у играка их 0
                        Thing thing = ThingsManager.Instance.GetThing(InstrumentsEnum.Hoe);
                        if (thing != null && thing.Quantity == 0)
                        {
                            BundleShopV[] bundleShopV = new BundleShopV[1];
                            bundleShopV[0] = new BundleShopV(InstrumentsEnum.Hoe, 1);
                            ThingsManager.Instance.addinstruments(bundleShopV);
                        }
                    }
                }
                else if (activeHint.help == HelpEnum.Vortex.ToString())
                {
                    List<string> flashingItemsNames = new List<string>();
                    flashingItemsNames.Add("Instrument" + InstrumentsEnum.Vortex.ToString());
                    created = InterfaceHelp("PanelInstruments", flashingItemsNames);

                    if (created && ThingsManager.Instance != null)
                    {
                        //дарим один инструмент, если у играка их 0
                        Thing thing = ThingsManager.Instance.GetThing(InstrumentsEnum.Vortex);
                        if (thing != null && thing.Quantity == 0)
                        {
                            BundleShopV[] bundleShopV = new BundleShopV[1];
                            bundleShopV[0] = new BundleShopV(InstrumentsEnum.Vortex, 1);
                            ThingsManager.Instance.addinstruments(bundleShopV);
                        }
                    }
                }
                else if (activeHint.help == HelpEnum.Repainting.ToString())
                {
                    List<string> flashingItemsNames = new List<string>();
                    flashingItemsNames.Add("Instrument" + InstrumentsEnum.Repainting.ToString());
                    created = InterfaceHelp("PanelInstruments", flashingItemsNames);

                    if (created && ThingsManager.Instance != null)
                    {
                        //дарим один инструмент, если у играка их 0
                        Thing thing = ThingsManager.Instance.GetThing(InstrumentsEnum.Repainting);
                        if (thing != null && thing.Quantity == 0)
                        {
                            BundleShopV[] bundleShopV = new BundleShopV[1];
                            bundleShopV[0] = new BundleShopV(InstrumentsEnum.Repainting, 1);
                            ThingsManager.Instance.addinstruments(bundleShopV);
                        }
                    }
                }
                else if (activeHint.help == HelpEnum.Line4.ToString())
                {
                    created = CreateStandardElementHelp(4);
                }
                else if (activeHint.help == HelpEnum.Line5.ToString())
                {
                    created = CreateStandardElementHelp(5);
                }
                else if (activeHint.help == HelpEnum.Line6.ToString())
                {
                    created = CreateStandardElementHelp(6);
                }
                else
                {
                    //неудалось определить подсказку
                    Debug.Log("Неудалось определить подсказку!");
                    //добавляем для удаления
                    hintForDell.Add(hint);
                }

                //если удалось создать подсказку, выходим из цикла
                if (created)
                {
                    //создаем затемнение                  
                    Image imageHelpToPlayer = activeHint.canvasHelpToPlayer.GetComponentInChildren<Image>();
                    MainAnimator.Instance.AddElementForSmoothChangeColor(imageHelpToPlayer, new Color(imageHelpToPlayer.color.r, imageHelpToPlayer.color.g, imageHelpToPlayer.color.b, 0.9f), 2);
                    //устанавливаем камеру
                    activeHint.canvasHelpToPlayer.GetComponent<Canvas>().worldCamera = Camera.main;
                    //добавляем действие канвасу
                    Transform gOPanel = activeHint.canvasHelpToPlayer.transform.Find("Panel");
                    Button button = gOPanel.GetComponent<Button>();
                    button.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
                    button.onClick.AddListener(delegate { DeletedByClickingOnCanvas(); });
                    //добавляем действие кнопке закрыть
                    Transform buttonDellHelpGO = activeHint.canvasHelpToPlayer.transform.Find("ButtonDellHelp");
                    Button buttonDellHelp = buttonDellHelpGO.GetComponent<Button>();
                    buttonDellHelp.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
                    buttonDellHelp.onClick.AddListener(delegate { DeletedByClickingOnButton(); });
                    ////показываем кнопку постепенно
                    //Image imageDellHelp = buttonDellHelpGO.GetComponent<Image>();
                    //MainAnimator.Instance.AddElementForSmoothChangeColor(imageDellHelp, new Color(imageDellHelp.color.r, imageDellHelp.color.g, imageDellHelp.color.b, 1), 0.1f);

                    //показываем текст
                    CreateTextCloud();

                    //помечаем как показанную
                    hintsStatus[activeHint.numberHelp].status = true;
                    //добавляем для удаления
                    hintForDell.Add(hint);

                    MasterController.Instance.ForcedDropElement();//если игрок перетаскивает элемент, то бросаем его с возвратом на позицию
                    InstrumentPanel.Instance.DeactivateInstrument();
                    break;
                }
                else
                {
                    //неудалось создать подсказку
                    DellGameHelp();
                }
            }

            //удаляем пройденные подсказки
            foreach (Hint item in hintForDell)
            {
                hintsList.Remove(item);
            }
        }

        return created;
    }

    private static void CreateTextCloud()
    {
        //собераем все трансформы
        List<Transform> transformsList = new List<Transform>();
        List<RectTransform> rectTransformList = new List<RectTransform>();
        foreach (SpriteRenderSettings item in activeHint.spriteRendersSetingList)
        {
            transformsList.Add(item.spriteRenderer.transform);
        }
        foreach (ParentSettings item in activeHint.ParentSettingsList)
        {
            rectTransformList.Add(item.gameObjectTransform.GetComponent<RectTransform>());
        }

        if (transformsList.Count > 0 || rectTransformList.Count > 0)
        {
            Transform gOPanel = activeHint.canvasHelpToPlayer.transform.Find("Panel");
            Transform textCloud = gOPanel.transform.Find("TextCloud");
            RectTransform rectTransformGOPanel = gOPanel.GetComponent<RectTransform>();
            RectTransform rectTransformTextCloud = textCloud.GetComponent<RectTransform>();

            Vector3 newPosition = textCloud.position;
            float width = 1;
            //находим самый левый верхний объект
            foreach (Transform item in transformsList)
            {
                //обрабатываем все кроме элементов
                if (!item.GetComponent<BaseElement>())
                {
                    if (item.position.x < newPosition.x)
                    {
                        newPosition.x = item.position.x;
                        width = item.localScale.x; // ширина
                    }

                    if (item.position.y > newPosition.y)
                    {
                        newPosition.y = item.position.y;
                        width = item.localScale.x; // ширина
                    }
                }
            }
            textCloud.position = new Vector3(newPosition.x - width * 0.5f, newPosition.y, newPosition.z);

            Vector3 newLocalPosition = textCloud.localPosition;
            foreach (RectTransform item in rectTransformList)
            {
                if (item.localPosition.x < newLocalPosition.x)
                {
                    newLocalPosition.x = item.localPosition.x;
                    width = item.rect.width; // ширина
                }

                if (item.localPosition.y > newLocalPosition.y)
                {
                    newLocalPosition.y = item.localPosition.y;
                    width = item.rect.width; // ширина
                }
            }

            textCloud.localPosition = new Vector3(newLocalPosition.x - width * 0.5f, newLocalPosition.y, newLocalPosition.z);

            rectTransformTextCloud.anchoredPosition = new Vector3(rectTransformTextCloud.anchoredPosition.x - rectTransformTextCloud.rect.width * 0.5f, rectTransformTextCloud.anchoredPosition.y);

            //если выходит за пределы экрана слева переносим на право
            if (rectTransformTextCloud.anchoredPosition.x < rectTransformTextCloud.rect.width * 0.5f)
            {
                newPosition = textCloud.position;
                width = 1;
                //находим самый левый верхний объект
                foreach (Transform item in transformsList)
                {
                    //обрабатываем все кроме элементов
                    if (!item.GetComponent<BaseElement>())
                    {
                        if (item.position.x > newPosition.x)
                        {
                            newPosition.x = item.position.x;
                            width = item.localScale.x; // ширина
                        }

                        if (item.position.y > newPosition.y)
                        {
                            newPosition.y = item.position.y;
                            width = item.localScale.x; // ширина
                        }
                    }
                }
                textCloud.position = new Vector3(newPosition.x + width * 0.5f, newPosition.y, newPosition.z);

                newLocalPosition = textCloud.localPosition;
                foreach (RectTransform item in rectTransformList)
                {
                    if (item.localPosition.x > newLocalPosition.x)
                    {
                        newLocalPosition.x = item.localPosition.x;
                        width = item.rect.width; // ширина
                    }

                    if (item.localPosition.y > newLocalPosition.y)
                    {
                        newLocalPosition.y = item.localPosition.y;
                        width = item.rect.width; // ширина
                    }
                }
                textCloud.localPosition = new Vector3(newLocalPosition.x + width * 0.5f, newLocalPosition.y, newLocalPosition.z);
                rectTransformTextCloud.anchoredPosition = new Vector3(rectTransformTextCloud.anchoredPosition.x + rectTransformTextCloud.rect.width * 0.5f, rectTransformTextCloud.anchoredPosition.y);
            }

            //если слишков высоко, то смещаем вниз до пределов экрана
            if (-rectTransformTextCloud.anchoredPosition.y < rectTransformTextCloud.rect.height * 0.5f)
            {
                rectTransformTextCloud.anchoredPosition = new Vector3(rectTransformTextCloud.anchoredPosition.x, -rectTransformTextCloud.rect.height * 0.5f);
            }

            //если слишков низко, то смещаем вверх до пределов экрана
            if (rectTransformGOPanel.rect.height < -rectTransformTextCloud.anchoredPosition.y + rectTransformTextCloud.rect.height * 0.5f)
            {
                rectTransformTextCloud.anchoredPosition = new Vector3(rectTransformTextCloud.anchoredPosition.x, -rectTransformGOPanel.rect.height + rectTransformTextCloud.rect.height * 0.5f);
            }

            Text text = textCloud.GetComponentInChildren<Text>();

            if (activeHint.help == BlockTypeEnum.Sliding.ToString())
            {
                text.text = "Это льдинка, по ней всё скользит вниз!";
            }
            else if (activeHint.help == ElementsTypeEnum.StandardElement.ToString())
            {
                text.text = "Что бы убрать растения в нашем саду, их нужно собрать в комбинации из трех или более растений. Попробуй!";
            }
            else if (activeHint.help == ElementsTypeEnum.SmallFlask.ToString())
            {
                text.text = "Дарю тебе маленькое зелье за сбор линии из 4 растений. Нажми на него дважды и оно взорвет все ненужное и соберёт нужное!";
            }
            else if (activeHint.help == ElementsTypeEnum.MediumFlask.ToString())
            {
                text.text = "Это среднее зелье за сбор комбинации из 5 растений. Жми на него дважды!";
            }
            else if (activeHint.help == ElementsTypeEnum.BigFlask.ToString())
            {
                text.text = "Это большое зелье за сбор комбинации из 6 растений. Ты уже знаешь что с ним делать!";
            }
            else if (activeHint.help == ElementsTypeEnum.CrushableWall.ToString())
            {
                text.text = "Это куст. Его можно убрать, собрав комбинацию рядом или, взорвав зелье! Собери комбинацию.";
            }
            else if (activeHint.help == ElementsTypeEnum.ImmortalWall.ToString())
            {
                text.text = "Это каменная стена. Её невозможно разрушить, она очень крепкая!";
            }
            else if (activeHint.help == ElementsTypeEnum.Drop.ToString())
            {
                text.text = "Это хороший кирпич, из него можно построить новый сарай. Веди его жерез поле в самый низ!";
            }
            else if (activeHint.help == ElementsTypeEnum.SeedBarrel.ToString())
            {
                text.text = "Это бочка с семенами. Наполни её, собирая рядом растения, указанные на бочке!";
            }
            else if (activeHint.help == BlockingElementsTypeEnum.Liana.ToString())
            {
                text.text = "Эта лиана захватила весь наш сад! Её можно уничтожить тем же, чем уничтожается захваченное растение!";
            }            
            else if (activeHint.help == BehindElementsTypeEnum.Grass.ToString())
            {
                text.text = "Это сорняк. Он убирается вместе с растением, которое находится на нём!";
            }
            else if (activeHint.help == BehindElementsTypeEnum.Dirt.ToString())
            {
                text.text = "Уничтожь эту грязь! Потемневшая грязь каждый ход распространяется на соседнюю клетку с растением!";
            }
            else if (activeHint.help == HelpEnum.Gnome.ToString())
            {
                text.text = "Привет, я гномик Сеня! Мои магия и подсказки помогут тебе в этом интересном путишествии!";
            }
            else if (activeHint.help == HelpEnum.Lifes.ToString())
            {
                text.text = "Помни, что наших сил хватить лишь на несколько попыток собрать растения! Но в волшебном магазине можно прикупить зелье временного бессмертия!";
            }
            else if (activeHint.help == HelpEnum.DailyGift.ToString())
            {
                text.text = "Приходи в сад каждый день и забирай подарки от белочки Гифти! И где она их только берёт?";
            }
            else if (activeHint.help == HelpEnum.Gift.ToString())
            {
                text.text = "Набери три звезды при прохождении этого уровня и получи подарок!";
            }
            else if (activeHint.help == HelpEnum.OptionalLvl.ToString())
            {
                text.text = "Это необязательный для прохождения уровень! Если не знаешь как его пройти, переходи к следующему!";
            }
            else if (activeHint.help == HelpEnum.Tasks.ToString())
            {
                text.text = "Здесь ты видишь, что нужно собрать и сколько ходов для этого у тебя есть!";
            }
            else if (activeHint.help == HelpEnum.Score.ToString())
            {
                text.text = "Здесь ты видишь количество набранных очков. Чем больше очков ты наберешь, тем больше звезд получишь!";
            }
            else if (activeHint.help == HelpEnum.SuperBonus.ToString())
            {
                text.text = "Это котёл с волшебным зельем, который наполняется энергией собраных растений, а после помогает тебе, изливая волшебные капли на поле!";
            }
            else if (activeHint.help == HelpEnum.Instruments.ToString())
            {
                text.text = "Это твои инструменты-помощники. У тебя уже есть инструмент Лопата, она убирает одно растение!";
            }
            else if (activeHint.help == HelpEnum.Shop.ToString())
            {
                text.text = "Эта кнопка открывает волшебный магазин, в котором ты можешь найти всякие магические штучки!";
            }
            else if (activeHint.help == HelpEnum.Hoe.ToString())
            {
                text.text = "Теперь у тебя есть Газонокосилка! Она убирает две пересекающиеся линии. Заводи!";
            }
            else if (activeHint.help == HelpEnum.Vortex.ToString())
            {
                text.text = "Теперь у тебя есть Вихрь! Он перемешивает все растения на поле. Давай дунем! :))";
            }
            else if (activeHint.help == HelpEnum.Repainting.ToString())
            {
                text.text = "Теперь у тебя есть Перекраска! Она заменяет несколько растений на поле в растение, которое ты укажешь. Выбирай!";
            }
            else if (activeHint.help == HelpEnum.Line4.ToString())
            {
                text.text = "Попробуй собрать комбинацию из 4 растений!";
            }
            else if (activeHint.help == HelpEnum.Line5.ToString())
            {
                text.text = "А давай соберём комбинацию из 5 растений!";
            }
            else if (activeHint.help == HelpEnum.Line6.ToString())
            {
                text.text = "А что если собрать комбинацию из 6 растений?";
            }
            else
            {
                text.text = "Често говоря, я и сам не понимаю, что происходит :)";
            }
        }
        else
        {
            Debug.Log("Нет ни одного SpriteRenderSettings для обработки");
            return;
        }
    }

    //попытка удалить по клику по канвасу
    public static void DeletedByClickingOnCanvas()
    {
        if (deletedByClickingOnCanvas)
        {
            //если прошло больше времени чем указано
            if ((Time.time - delayTime) > timeCreateHints)
            {
                DellAndCreateHelp();
            }
        }
    }

    //удаление подсказки по нажатию на кнопку
    public static void DeletedByClickingOnButton()
    {
        DellAndCreateHelp();
    }

    //удаление подсказки с проверкой создающую новую подсказку или выполнением хода 
    private static void DellAndCreateHelp()
    {
        if (activeHint != null)
        {
            bool createNextGameHelpByClicking = activeHint.createNextGameHelpByClicking;
            if (DellGameHelp())
            {
                if (createNextGameHelpByClicking)
                {
                    if (!CreateNextGameHelp())
                    {
                        //выполняем ход
                        GridBlocks.Instance.Move();
                    }                    
                }
                else
                {
                    //выполняем ход
                    GridBlocks.Instance.Move();
                }
            }
        }
    }

    public static bool DellGameHelp()
    {
        //если есть подсказка для элементов
        if (activeHint != null)
        {

            //восстанавливаем значения сортировки спрайтов
            foreach (SpriteRenderSettings item in activeHint.spriteRendersSetingList)
            {
                if (item.spriteRenderer != null)
                {
                    item.spriteRenderer.sortingOrder = item.sortingOrder;
                    item.spriteRenderer.sortingLayerName = item.sortingLayerName;
                }
            }

            //восстанавливаем значения родителя
            foreach (ParentSettings item in activeHint.ParentSettingsList)
            {
                if (item.gameObjectTransform != null && item.parentTransform != null)
                {
                    item.gameObjectTransform.SetParent(item.parentTransform, false);
                }
            }

            //восстанавливаем значения блок контроллеров
            foreach (BlockControllerSettings item in activeHint.blockControllersSetingList)
            {
                if (item.blockController != null)
                {
                    item.blockController.handleDragging = item.handleDragging;
                    item.blockController.handleСlick = item.handleСlick;
                    item.blockController.permittedDirection = item.permittedDirection;
                }
            }

            //удаляем эффект мигания
            DellFromFlashing(activeHint);

            //удаляем затемнение
            UnityEngine.Object.Destroy(activeHint.canvasHelpToPlayer);

            deletedByClickingOnCanvas = false;
            JsonSaveAndLoad.RecordSave(hintsStatus);
            activeHint = null;
            return true;
        }
        else
        {
            return false;
        }
    }

    //разрешаем удалить подсказку после определенного времени
    private static void CanvasLiveTime(float time)
    {
        deletedByClickingOnCanvas = true;
        timeCreateHints = Time.time;
        delayTime = time;
    }

    //подсказки для блоков
    private static bool CreateBlockHelp(BlockTypeEnum BlockTypeEnum)
    {
        //получаем все блоки
        Block[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(Block)) as Block[];

        //если нашли хоть один элемент
        foreach (Block item in findeObjects)
        {
            //если блок нужного типа и имеет позицию в сетке
            if (item.Type == BlockTypeEnum && item.PositionInGrid != null)
            {
                //высвечиваем блок
                ChangeSorting(item.gameObject, activeHint);

                //добавляем эффект мерцания
                AddToFlashing(item.gameObject, activeHint);

                //таймаут для удаления подсказки
                CanvasLiveTime(1);
                return true;
            }
        }
        Debug.Log("Не нашли ни одного блока типа: " + BlockTypeEnum + "!");
        return false;
    }

    //подсказки для элементов

    //подсказка для стандартных элементов составляющие разную длинну
    private static bool CreateStandardElementHelp(int count)
    {
        if (count > 3)
        {
            //если линия больше 3, проверяем получим ли мы бонус за составление линии, если нет то выходим
            //и проверяем показывали ли мы для этого бонуса подсказку
            bool foundBonus = false;
            foreach (Bonus item in Bonuses.Instance.bonusesList)
            {
                if (item.Cost == count)
                {
                    //если для бонуса, который будет создан при сборе такой линии уже была показана подсказка, то не показываем
                    //и помечаем как показанную
                    if (ShowedHint(typeof(ElementsTypeEnum), (int)item.Type))
                    {
                        //помечаем как показанную
                        hintsStatus[activeHint.numberHelp].status = true;
                        ////удаляем
                        //hintsList.Remove(activeHint);
                        return false;
                    }
                    else//иначе продолжаем создавать подсказку
                    {
                        foundBonus = true;
                        break;
                    }
                }
            }

            if (!foundBonus)
            {
                return false;
            }
        }

        //получаем все доступные ходы
        List<ElementsForNextMove> elementsForNextMoveList = GridBlocks.Instance.CheckElementsForNextMove();
        //Если нет доступных ходов, то выходим
        if (elementsForNextMoveList.Count == 0)
        {
            return false;
        }

        ElementsForNextMove elementsForNextMove = null;
        foreach (ElementsForNextMove item in elementsForNextMoveList)
        {
            //если количество элементов соответствует длине линни которую мы хотим показать
            if (item.elementsList.Count == count)
            {
                //провряем, что бы все элементы были разблокированы
                foreach (Element elementItem in item.elementsList)
                {
                    if (elementItem.BlockingElement != null && !elementItem.BlockingElement.Destroyed)
                    {
                        continue;
                    }
                }
                elementsForNextMove = item;
                break;
            }
        }

        if (elementsForNextMove != null && HighlightSpecifiedMove(elementsForNextMove))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool CreateFlaskHelp(ElementsTypeEnum elementsTypeEnum)
    {
        //берем любую маленькую фласку
        //делаем подсветку фласки и соседних блококв
        //наоходим все объекты с нужным элементом
        ElementSmallFlask[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(ElementSmallFlask)) as ElementSmallFlask[];

        //если нашли хоть один элемент
        foreach (ElementSmallFlask item in findeObjects)
        {
            //берем блок с нашей флаской
            Block curBlock = GridBlocks.Instance.GetBlock(item.PositionInGrid);
            if (BlockCheck.ThisBlockWithElementCanMove(curBlock) && item.Type == elementsTypeEnum)
            {
                //высвечиваем блок
                ChangeSorting(curBlock.gameObject, activeHint);

                //добавляем эффект мерцания
                AddToFlashing(curBlock.gameObject, activeHint);

                //отключаем перетаскивание у фласки
                BlockController blockController = curBlock.GetComponent<BlockController>();
                activeHint.blockControllersSetingList.Add(new BlockControllerSettings(blockController, blockController.handleСlick, blockController.handleDragging, blockController.permittedDirection));
                blockController.handleDragging = false;

                //получаем соседние блоки
                Block[] blocks = GridBlocks.Instance.GetBlocksForHit(curBlock.PositionInGrid, item.GetComponent<ElementSmallFlask>().ExplosionRadius);

                //перебираем все блоки
                foreach (Block block in blocks)
                {
                    if (block != null)
                    {
                        ChangeSorting(block.gameObject, activeHint);
                        blockController = block.GetComponent<BlockController>();
                        //сохраняем настройки
                        activeHint.blockControllersSetingList.Add(new BlockControllerSettings(blockController, blockController.handleСlick, blockController.handleDragging, blockController.permittedDirection));
                        //деактивируем
                        blockController.handleDragging = false;
                        blockController.handleСlick = false;
                    }
                }
                return true;
            }
        }
        Debug.Log("Не нашли не одной маленькой фласки для создания подсказки!");
        return false;
    }

    private static bool CreateCrushableWallHelp(ElementsTypeEnum elementsTypeEnum)
    {
        //получаем возможные ходы
        List<ElementsForNextMove> elementsForNextMoveList = GridBlocks.Instance.CheckElementsForNextMove();
        //Если нет доступных ходов, то выходим
        if (elementsForNextMoveList.Count == 0)
        {
            return false;
        }

        //получаем все стены
        ElementWall[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(ElementWall)) as ElementWall[];

        //если нашли хоть один элемент
        foreach (ElementWall item in findeObjects)
        {
            //берем блок с нашим элементом
            Block curBlock = GridBlocks.Instance.GetBlock(item.PositionInGrid);

            if (item.Type == elementsTypeEnum && BlockCheck.ThisBlockWithElementWithoutBlockingElement(curBlock))
            {
                //получаем соседние блоки
                NeighboringBlocks blocks = GridBlocks.Instance.GetNeighboringBlocks(curBlock.PositionInGrid);
                //пытаемся найти ход где есть соседний блок в следующем ходе
                foreach (ElementsForNextMove curElementsForNextMove in elementsForNextMoveList)
                {
                    foreach (Element element in curElementsForNextMove.elementsList)
                    {
                        //если элемент не для передвижения и не заблокирвоан
                        if (element != curElementsForNextMove.elementForMove && element.BlockingElement == null || (element.BlockingElement != null && element.BlockingElement.Destroyed))
                        {
                            foreach (Block NeighboringBlock in blocks.allBlockField)
                            {
                                if (NeighboringBlock == GridBlocks.Instance.GetBlock(element.PositionInGrid))
                                {
                                    //высвечиваем блок
                                    ChangeSorting(curBlock.gameObject, activeHint);

                                    //добавляем эффект мерцания
                                    AddToFlashing(curBlock.gameObject, activeHint);

                                    //высвечиваем нужный ход
                                    return HighlightSpecifiedMove(curElementsForNextMove);
                                }
                            }
                        }
                    }
                }
                ////если не нашли соседний блок
                //return false;
            }
        }
        Debug.Log("Не нашли подходящую разрушаемую стену для подсказки!");
        return false;
    }

    private static bool CreateSeedBarelHelp(ElementsTypeEnum elementsTypeEnum)
    {
        //получаем возможные ходы
        List<ElementsForNextMove> elementsForNextMoveList = GridBlocks.Instance.CheckElementsForNextMove();
        //Если нет доступных ходов, то выходим
        if (elementsForNextMoveList.Count == 0)
        {
            return false;
        }

        //получаем все бочки
        SeedBarrelElement[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(SeedBarrelElement)) as SeedBarrelElement[];

        //если нашли хоть один элемент
        foreach (SeedBarrelElement item in findeObjects)
        {
            //берем блок с нашим элементом
            Block curBlock = GridBlocks.Instance.GetBlock(item.PositionInGrid);

            if (item.Type == elementsTypeEnum && BlockCheck.ThisBlockWithElementWithoutBlockingElement(curBlock))
            {
                //получаем соседние блоки
                NeighboringBlocks blocks = GridBlocks.Instance.GetNeighboringBlocks(curBlock.PositionInGrid);
                //пытаемся найти ход где есть соседний блок в следующем ходе
                foreach (ElementsForNextMove curElementsForNextMove in elementsForNextMoveList)
                {
                    //если в следующем ходе не та внешность элэмента, то ищем дальше
                    if (curElementsForNextMove.elementForMove.Shape != item.CollectShape)
                    {
                        continue;
                    }
                    foreach (Element element in curElementsForNextMove.elementsList)
                    {
                        //если элемент не для передвижения и не заблокирвоан
                        if (element != curElementsForNextMove.elementForMove && element.BlockingElement == null || (element.BlockingElement != null && element.BlockingElement.Destroyed))
                        {
                            foreach (Block NeighboringBlock in blocks.allBlockField)
                            {
                                if (NeighboringBlock == GridBlocks.Instance.GetBlock(element.PositionInGrid))
                                {
                                    //высвечиваем блок
                                    ChangeSorting(curBlock.gameObject, activeHint);

                                    //добавляем эффект мерцания
                                    AddToFlashing(item.gameObject, activeHint);

                                    //высвечиваем нужный ход
                                    return HighlightSpecifiedMove(curElementsForNextMove);
                                }
                            }
                        }
                    }
                }
                ////если не нашли соседний блок
                //return false;
            }
        }
        Debug.Log("Не нашли подходящую бочку для подсказки!");
        return false;
    }

    private static bool CreateLianaHelp(BlockingElementsTypeEnum elementsTypeEnum)
    {
        //получаем возможные ходы
        List<ElementsForNextMove> elementsForNextMoveList = GridBlocks.Instance.CheckElementsForNextMove();
        //Если нет доступных ходов, то выходим
        if (elementsForNextMoveList.Count == 0)
        {
            return false;
        }

        foreach (ElementsForNextMove curElementsForNextMove in elementsForNextMoveList)
        {
            foreach (Element element in curElementsForNextMove.elementsList)
            {
                //если элемент не для передвижения и заблокирвоан нужным типом элемента
                if (element != curElementsForNextMove.elementForMove && element.BlockingElement != null && !element.BlockingElement.Destroyed && element.BlockingElement.Type == elementsTypeEnum)
                {
                    //подсвечиваем элемент
                    AddToFlashing(element.BlockingElement.gameObject, activeHint);

                    //высвечиваем нужный ход
                    return HighlightSpecifiedMove(curElementsForNextMove);
                }
            }
        }
        Debug.Log("Не нашли подходящий блокирующий элемент!");
        return false;
    }

    private static bool CreateGrassHelp(BehindElementsTypeEnum elementsTypeEnum)
    {
        //получаем возможные ходы
        List<ElementsForNextMove> elementsForNextMoveList = GridBlocks.Instance.CheckElementsForNextMove();
        //Если нет доступных ходов, то выходим
        if (elementsForNextMoveList.Count == 0)
        {
            return false;
        }

        foreach (ElementsForNextMove curElementsForNextMove in elementsForNextMoveList)
        {
            bool found = false;
            foreach (Element element in curElementsForNextMove.elementsList)
            {
                //если элемент не для передвижения и имеет позади нужны элемент и не заблокирован
                if (element != curElementsForNextMove.elementForMove && element.BlockingElement == null || (element.BlockingElement != null && element.BlockingElement.Destroyed))
                {
                    Block block = GridBlocks.Instance.GetBlock(element.PositionInGrid);
                    if (block != null && block.BehindElement != null && !block.BehindElement.Destroyed && block.BehindElement.Type == elementsTypeEnum)
                    {
                        //подсвечиваем элемент
                        AddToFlashing(block.BehindElement.gameObject, activeHint);
                        //указываем, что нашли нужный ход                        
                        found = true;
                    }
                }
            }

            if (found)
            {
                //проверяем так же блок назначения
                if (curElementsForNextMove.targetBlock != null && curElementsForNextMove.targetBlock.BehindElement != null && !curElementsForNextMove.targetBlock.BehindElement.Destroyed && curElementsForNextMove.targetBlock.BehindElement.Type == elementsTypeEnum)
                {
                        //подсвечиваем элемент
                    AddToFlashing(curElementsForNextMove.targetBlock.BehindElement.gameObject, activeHint);
                }
                //высвечиваем нужный ход
                return HighlightSpecifiedMove(curElementsForNextMove);
            }
        }
        Debug.Log("Не нашли подходящий элемент на заднем фоне для подсказки!");
        return false;
    }

    private static bool CreateImortalWallHelp()
    {
        //получаем все стены
        ElementWall[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(ElementWall)) as ElementWall[];

        //если нашли хоть один элемент
        foreach (ElementWall item in findeObjects)
        {
            //берем блок с нашим элементом
            Block curBlock = GridBlocks.Instance.GetBlock(item.PositionInGrid);

            if (item.Type == ElementsTypeEnum.ImmortalWall && !item.Destroyed && curBlock != null)
            {
                //высвечиваем блок
                ChangeSorting(curBlock.gameObject, activeHint);

                //добавляем эффект мерцания
                AddToFlashing(curBlock.gameObject, activeHint);

                //таймаут для удаления подсказки
                CanvasLiveTime(1);
                return true;
            }
        }
        Debug.Log("Не нашли ни одной бесмертной стены!");
        return false;
    }

    private static bool CreateDropElementHelp()
    {
        //получаем все элементы
        Element[] findeObjects = UnityEngine.Object.FindObjectsOfType(typeof(Element)) as Element[];

        //если нашли хоть один элемент
        foreach (Element item in findeObjects)
        {
            //берем блок с нашим элементом
            Block curBlock = GridBlocks.Instance.GetBlock(item.PositionInGrid);

            if (item.Type == ElementsTypeEnum.Drop && !item.Destroyed && curBlock != null)
            {
                //высвечиваем блок
                ChangeSorting(curBlock.gameObject, activeHint);

                //добавляем эффект мерцания
                AddToFlashing(curBlock.gameObject, activeHint);

                //таймаут для удаления подсказки
                CanvasLiveTime(1);
                return true;
            }
        }
        Debug.Log("Не нашли ни одного сбрасываемого элемента!");
        return false;
    }
    
    //подсказки для интерфейса
    //goName - название основного элемента
    //flashingItemsNames - элементы которые будут мигать
    private static bool InterfaceHelp(string goName, List<string> flashingItemsNames = null)
    {
        //находим гнома
        GameObject go = GameObject.Find(goName);

        if (go != null)
        {
            //замена родителя на наш канвас
            ChangeParent(go, activeHint);
            //запуск анимации для мигающих элементов
            if (flashingItemsNames != null)
            {
                foreach (string item in flashingItemsNames)
                {
                    GameObject itemGO = GameObject.Find(item);
                    if (itemGO != null)
                    {
                        AddToFlashing(itemGO, activeHint);
                    }
                }
            }
            //таймаут для удаления подсказки
            activeHint.createNextGameHelpByClicking = true;
            CanvasLiveTime(2);
            return true;
        }
        else
        {
            Debug.Log("Не нашли " + goName + " для создания подсказки!");
            return false;
        }
    }

    //вспомогательные
    //добавление для мерцания
    private static void AddToFlashing(GameObject gameObject, Hint hint)
    {
        float speed = 20f;
        float alfa = 0.5f;
        foreach (SpriteRenderer childrenSpriteRenderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if (childrenSpriteRenderer != null)
            {
                hint.FlashingSpriteRendersList.Add(childrenSpriteRenderer);

                MainAnimator.Instance.AddElementForSmoothChangeColor(childrenSpriteRenderer, new Color(childrenSpriteRenderer.color.r, childrenSpriteRenderer.color.g, childrenSpriteRenderer.color.b, alfa), speed, true);
            }
        }

        foreach (Image childrenImage in gameObject.GetComponentsInChildren<Image>())
        {
            if (childrenImage != null)
            {
                hint.FlashingImageList.Add(childrenImage);

                MainAnimator.Instance.AddElementForSmoothChangeColor(childrenImage, new Color(childrenImage.color.r, childrenImage.color.g, childrenImage.color.b, alfa), speed, true);
            }
        }
    }

    private static void DellFromFlashing(Hint hint)
    {
        foreach (SpriteRenderer item in hint.FlashingSpriteRendersList)
        {
            MainAnimator.Instance.DellElementForSmoothChangeColor(item);
        }

        foreach (Image item in hint.FlashingImageList)
        {
            MainAnimator.Instance.DellElementForSmoothChangeColor(item);
        }
    }

    private static void ChangeSorting(GameObject gameObject, Hint hint)
    {
        foreach (SpriteRenderer childrenSpriteRenderer in gameObject.GetComponentsInChildren<SpriteRenderer>())
        {
            if (childrenSpriteRenderer != null)
            {
                hint.spriteRendersSetingList.Add(new SpriteRenderSettings(childrenSpriteRenderer, childrenSpriteRenderer.sortingLayerName, childrenSpriteRenderer.sortingOrder));

                childrenSpriteRenderer.sortingOrder = childrenSpriteRenderer.sortingLayerID + childrenSpriteRenderer.sortingOrder;
                childrenSpriteRenderer.sortingLayerName = "Help";
                //Debug.Log(childrenSpriteRenderer.sortingOrder);
            }
        }
    }

    private static void ChangeParent(GameObject gameObject, Hint hint)
    {
        if (gameObject.transform != null)
        {
            hint.ParentSettingsList.Add(new ParentSettings(gameObject.transform, gameObject.transform.parent));

            gameObject.transform.SetParent(hint.canvasHelpToPlayer.transform.Find("Panel"), false);
        }
    }

    private static bool HighlightSpecifiedMove(ElementsForNextMove elementsForNextMove)
    {
        MainAnimator.Instance.ElementsForNextMove = elementsForNextMove;
        List<Block> blocks = new List<Block>();

        //получаем все блоки
        foreach (Element item in elementsForNextMove.elementsList)
        {
            blocks.Add(GridBlocks.Instance.GetBlock(item.PositionInGrid));
        }

        //записываем данные для блока в который будет смещен элемент
        ChangeSorting(elementsForNextMove.targetBlock.gameObject, activeHint);

        //записываем разрешенное направление для движения элемента
        BlockController blockController = elementsForNextMove.targetBlock.GetComponent<BlockController>();
        activeHint.blockControllersSetingList.Add(new BlockControllerSettings(blockController, blockController.handleСlick, blockController.handleDragging, blockController.permittedDirection));
        blockController.permittedDirection = elementsForNextMove.oppositeDirectionForMove;

        //перебираем все блоки
        foreach (Block block in blocks)
        {
            if (block != null && !GridBlocks.Instance.BlockInProcessing(block))
            {
                //block.Blocked = true;
                ChangeSorting(block.gameObject, activeHint);
                blockController = block.GetComponent<BlockController>();
                //сохраняем настройки
                activeHint.blockControllersSetingList.Add(new BlockControllerSettings(blockController, blockController.handleСlick, blockController.handleDragging, blockController.permittedDirection));
                //деактивируем все элементы управления кроме нужного блока
                if (block != elementsForNextMove.blockElementForMove)
                {
                    blockController.handleDragging = false;
                    blockController.handleСlick = false;
                }
                else
                {
                    //записываем разрешенное направление для движения элемента                        
                    blockController.permittedDirection = elementsForNextMove.directionForMove;
                }
            }
            else
            {
                Debug.Log("Не удалось высветить следующий ход");
                return false;
            }
        }
        return true;
    }
}

public class Hint
{
    public string help;
    public int numberHelp;
    public bool createNextGameHelpByClicking = false;
    public List<BlockControllerSettings> blockControllersSetingList = new List<BlockControllerSettings>();
    public List<SpriteRenderSettings> spriteRendersSetingList = new List<SpriteRenderSettings>();
    public List<ParentSettings> ParentSettingsList = new List<ParentSettings>();
    public List<SpriteRenderer> FlashingSpriteRendersList = new List<SpriteRenderer>();
    public List<Image> FlashingImageList = new List<Image>();
    public GameObject canvasHelpToPlayer;

    public Hint(string help, int numberHelp)
    {
        this.help = help;
        this.numberHelp = numberHelp;
    }
}

public class HintStatus
{
    public string help;
    public bool status = false;

    public HintStatus(string help, bool status)
    {
        this.help = help;
        this.status = status;
    }
}

public class SpriteRenderSettings
{
    public SpriteRenderer spriteRenderer;
    public string sortingLayerName;
    public int sortingOrder;

    public SpriteRenderSettings(SpriteRenderer spriteRenderer, string sortingLayerName, int sortingOrder)
    {
        this.spriteRenderer = spriteRenderer;
        this.sortingLayerName = sortingLayerName;
        this.sortingOrder = sortingOrder;
    }
}

public class BlockControllerSettings
{
    public BlockController blockController;
    public bool handleСlick;
    public bool handleDragging;
    public DirectionEnum permittedDirection;

    public BlockControllerSettings(BlockController blockController, bool handleСlick, bool handleDragging, DirectionEnum permittedDirection)
    {
        this.blockController = blockController;
        this.handleСlick = handleСlick;
        this.handleDragging = handleDragging;
        this.permittedDirection = permittedDirection;
    }
}

public class ParentSettings
{
    public Transform gameObjectTransform;
    public Transform parentTransform;

    public ParentSettings(Transform gameObjectTransform, Transform parentTransform)
    {
        this.gameObjectTransform = gameObjectTransform;
        this.parentTransform = parentTransform;
    }
}
