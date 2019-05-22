﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class Shop : MonoBehaviour, IStoreListener
{
    public static Shop Instance; // Синглтон
    private bool updateCoins;
    private int coins = 0; //валюта магазина
    private int addCoins;
    private int exchangeRate = 3; //курс обмена звезд на коины
    
    private GameObject panelShop; //магазин
    private GameObject panelShopConfirmation; //панель подтверждения действия
    private Transform panelShopOnGame;// панель которая отображается в верхнем углу экрана
    private Transform buttonShopTransform;//кнопка магазина в углу экрана
    private Text textCoins;//текст монет в верхнем углу экрана

    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;
    private ProductV currentProduct;
    bool stateOfPurchase = false;

    [Tooltip("Список товаров для доступные для покупки")]
    public ProductV[] PRODUCTS;

    public int Coins
    {
        get
        {
            return coins + addCoins;
        }
    }

    public bool AddGiftCoins(Level level, int coins) {
        if (!level.GiftIssued)
        {
            this.addCoins += coins;
            //StartCoroutine(UpdateTextCoins(3));
            return true;
        }
        else
        {
            return false;
        }
    }

    //принудительно пересчитываем количество
    public void CountCoins()
    {
        coins += addCoins;
        addCoins = 0;
        textCoins.text = "" + coins;
    }

    public void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject); //Delete duplicate
            return;
        }
        else
        {
            Instance = this; //Make this object the only instance            
        }
        DontDestroyOnLoadManager.DontDestroyOnLoad(gameObject);
        InitializePurchasing();

        panelShopOnGame = transform.Find("PanelShopOnGame");
        buttonShopTransform = panelShopOnGame.Find("ButtonOpenShop");
        ChangeButtonAction(buttonShopTransform, CreateShop, "Магазин");
        Transform gOTextCoins = panelShopOnGame.Find("TextCoins");
        textCoins = gOTextCoins.GetComponent(typeof(Text)) as Text;        
    }

    // Start is called before the first frame update
    void Start()
    {
        Save save = JsonSaveAndLoad.LoadSave();
        coins = save.shopSave.coins;
        textCoins.text = "" + coins;
    }

    public void CoinFlew(int price) {

        if (price > addCoins)
        {
            price = addCoins;
        }
        coins += price;
        addCoins -= price;
        textCoins.text = "" + coins;
        if (price > 0)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Coin);
        }
    }

    private IEnumerator UpdateTextCoins()
    {
        if (!updateCoins)
        {
            textCoins.text = "" + coins;
            yield return new WaitForSeconds(0.1f);
            updateCoins = true;
            do
            {
                yield return new WaitForFixedUpdate();
                //if (addCoins > 50)
                //{
                //    double d = addCoins * 0.1f;
                //    int i = (int)Math.Truncate(d);
                //    coins += i;
                //    addCoins -= i;
                //    textCoins.text = "" + coins;
                //}
                //else if (addCoins > 0)
                //{
                //    coins += 1;
                //    addCoins -= 1;
                //    textCoins.text = "" + coins;
                //}
                if (addCoins < -50)
                {
                    double d = -addCoins * 0.1f;
                    int i = (int)Math.Truncate(d);
                    coins -= i;
                    addCoins += i;
                    textCoins.text = "" + coins;
                }
                else if (addCoins < 0)
                {
                    coins -= 1;
                    addCoins += 1;
                    textCoins.text = "" + coins;
                }
                                
            } while (addCoins != 0);
            textCoins.text = "" + coins;
            updateCoins = false;
        }       
    }

    public int ExchangeStarsForCoins(Level level, int stars) {
        if (level.Stars < stars )
        {
            int newCoins = (stars - level.Stars) * exchangeRate;
            addCoins += newCoins;
            //StartCoroutine(UpdateTextCoins(2));
            JsonSaveAndLoad.RecordSave(this);
            return newCoins;
        }
        else
        {
            return 0;
        }
    }

    //создание меню магазина
    public void CreateShop()
    {
        HelpToPlayer.DellGameHelp();
        if (panelShop != null)
        {
            Destroy(panelShop);
        }
        panelShop = Instantiate(PrefabBank.ShopPanelPrefab, transform);
        Transform contentTransform = panelShop.transform.Find("Viewport/Content");

        //принудительно пересчитываем количество вещей и монет
        ThingsManager.Instance.CountAllNumber();
        CountCoins();

        //витрина
        foreach (ProductV product in PRODUCTS)
        {
            GameObject bottonGO = Instantiate(PrefabBank.ShopButtonPrefab, contentTransform);
            Transform textNameTransform = bottonGO.transform.Find("TextName");
            textNameTransform.GetComponentInChildren<Text>().text = product.name;

            //выбираем как будем отоброжать цену - монеты или реальные деньги
            Transform textPriceCoinTransform = bottonGO.transform.Find("TextPriceCoin");
            Transform textPriceReMoneyTransform = bottonGO.transform.Find("TextPriceReMoney");
            if (product.priceCoins != 0)
            {
                textPriceCoinTransform.GetComponentInChildren<Text>().text = product.priceCoins.ToString();
                //скрываем другой текст
                Destroy(textPriceReMoneyTransform.gameObject);
            }
            else
            {
                textPriceReMoneyTransform.GetComponentInChildren<Text>().text = m_StoreController.products.WithID(product.id).metadata.localizedPriceString;
                //скрываем другой текст
                Destroy(textPriceCoinTransform.gameObject);
            }                         
            
            Transform imageTransform = bottonGO.transform.Find("Image");
            imageTransform.GetComponentInChildren<Image>().sprite = product.image;

            product.AddAction(bottonGO);
        }

        //Заменяем кнопке действие на Закрыть
        ChangeButtonAction(buttonShopTransform, DestroyPanelShop, "Закрыть");

        //увеличиваем панель в верхнем углу
        panelShopOnGame.localScale = panelShopOnGame.localScale * 2.0f;

        //Показать инструменты в верху экрана
        ThingsManager.Instance.CreateInstrumentsOnShop(panelShop.transform.Find("PanelShopInstruments"));
    }

    //создаем панель подтверждения покупки
    public IEnumerator CreateShopConfirmation(ProductV product)
    {        
        if (panelShop != null)
        {
            if (panelShopConfirmation != null)
            {
                Destroy(panelShopConfirmation);
            }
            panelShopConfirmation = Instantiate(PrefabBank.PanelShopConfirmation, panelShop.transform);
            panelShopConfirmation.transform.Find("TextConfirmation").GetComponent<Text>().text = "Вы успешно преобрели " + product.name;
            Transform buttonOkTransform = panelShopConfirmation.transform.Find("ButtonOk");
            ChangeButtonAction(buttonOkTransform, DestroyPanelShopConfirmation);
            Button buttonOk = buttonOkTransform.GetComponent<Button>();
            buttonOk.interactable = false;//отключаем кнопку до конца проигрования анимации
            
            //проигрываем анимацию 
            yield return StartCoroutine(ShopConfirmationAnimation(panelShop, panelShopConfirmation, product));

            stateOfPurchase = false;
            buttonOk.interactable = true;//включаем кнопку
        }
    }

    private IEnumerator ShopConfirmationAnimation(GameObject panelShop, GameObject panelShopConfirmation, ProductV product) {
        yield return new WaitForSeconds(0.1f);

        Transform panelShoppingListTransform = panelShopConfirmation.transform.Find("PanelShoppingList");
        //Transform PanelShopInstrumentsTransform = panelShop.transform.Find("PanelShopInstruments");

        //определяем длинну массива подарков
        int giftLength = 0;
        if (product.coins > 0)
        {
            giftLength++;
        }
        if (product.compositionBundle.Length > 0)
        {
            giftLength += product.compositionBundle.Length;
        }

            //показываем наши покупки
        if (giftLength > 0)
        {
            //смещение по x
            float startingXPoint = panelShoppingListTransform.position.x - ((1 + 0.5f) * (giftLength - 1)) * 0.5f;

            for (int i = 0; i < product.compositionBundle.Length; i++)
            {
                if (product.compositionBundle[i].count > 0)
                {
                    yield return StartCoroutine(CreateThingAnimation(new Vector3(startingXPoint + (i * (1 + 0.5f)), panelShoppingListTransform.position.y, panelShoppingListTransform.position.z), panelShoppingListTransform, product.compositionBundle[i].type, product.compositionBundle[i].count));              
                }
                yield return new WaitForSeconds(0.3f);
            }

            if (product.coins > 0)
            {
                yield return StartCoroutine(CreateCoinAnimation(new Vector3(startingXPoint + ((0 + product.compositionBundle.Length) * (1 + 0.5f)), panelShoppingListTransform.position.y, panelShoppingListTransform.position.z), panelShoppingListTransform, product.coins));
                 
            }            
        }
        yield return new WaitForSeconds(1.0f);
    }

    //анимация получения вещи
    public IEnumerator CreateThingAnimation(Vector3 startPosition, Transform transformParent, InstrumentsEnum instrumentsEnum, int getCount, Vector3 newPosition = new Vector3())
    {
        SoundManager.Instance.PlaySoundInternal(SoundsEnum.Ring_1);
        GameObject go = Instantiate(PrefabBank.PrefabButtonThing, startPosition, Quaternion.identity, transformParent);
        go.GetComponent<Image>().sprite = SpriteBank.SetShape(instrumentsEnum);
        go.GetComponentInChildren<Text>().text = "+" + getCount;

        //если требуется перемещаем на новую позицию
        if (newPosition != Vector3.zero)
        {
            MainAnimator.Instance.AddElementForSmoothMove(go.transform, newPosition, 1, SmoothEnum.InLineWithSlowdown, 0.05f, false, true);
            yield return new WaitForSeconds(0.3f);
        }

        //находим нашу вещ в менеджере вещей
        Thing thing = ThingsManager.Instance.GetThing(instrumentsEnum);

        if (thing != null)
        {
            //создаем новые покупки рядом с основной
            int count = getCount;
            do
            {
                count -= 1;
                //добавим рандома в месте создания монет
                float randomNumberX = UnityEngine.Random.Range(-15, 15) * 0.1f;
                float randomNumberY = UnityEngine.Random.Range(-15, 15) * 0.1f;

                //берем монетку и меняем у нее вид
                GameObject mminiThingGO = GameObject.Instantiate(Resources.Load("Prefabs/Canvas/GameCanvas/ImageCoin") as GameObject, go.transform.position, Quaternion.identity, transformParent);
                mminiThingGO.GetComponent<Image>().sprite = SpriteBank.SetShape(instrumentsEnum);

                //перемещаем на рандомную позицию
                MainAnimator.Instance.AddElementForSmoothMove(mminiThingGO.transform, new Vector3(go.transform.position.x + randomNumberX, go.transform.position.y + randomNumberY, go.transform.position.z), 1, SmoothEnum.InLineWithSlowdown, 0.05f, false, true);

                //перемещаем к панеле вещей в магазине или на игровую панель
                Vector3 positoinThingtoGo = new Vector3(0,0,0);
                if (thing.Go != null)
                {
                    positoinThingtoGo = thing.Go.transform.position;
                }
                else if (InstrumentPanel.Instance != null)
                {
                    ThingsButton thingButtonOnGame = InstrumentPanel.Instance.GetThingButton(instrumentsEnum);
                    if (thingButtonOnGame != null)
                    {
                        positoinThingtoGo = thingButtonOnGame.Button.transform.position;
                    }
                }

                MainAnimator.Instance.AddElementForSmoothMove(mminiThingGO.transform, positoinThingtoGo, 1, SmoothEnum.InLineWithOneSpeed, 0.85f, true, true, delegate { thing.ThingFlew(1); });

                yield return new WaitForEndOfFrame();
            } while (count > 0);
        }
    }

    //анимация получения монет
    public IEnumerator CreateCoinAnimation(Vector3 position, Transform transformParent, int getCoins, Vector3 newPosition = new Vector3()) {
      
        //показываем монету
        //Находим монету в магазине
        Transform shopImageCoinsTransform = panelShopOnGame.transform.Find("ImageCoins");

        //показываем монету среди подарков
        SoundManager.Instance.PlaySoundInternal(SoundsEnum.Ring_1);
        GameObject giftCoinGO = Instantiate(PrefabBank.PrefabButtonThing, position, Quaternion.identity, transformParent);
        giftCoinGO.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/coin") as Sprite;
        giftCoinGO.GetComponentInChildren<Text>().text = "+" + getCoins;

        //если требуется перемещаем на новую позицию
        if (newPosition != Vector3.zero)
        {
            MainAnimator.Instance.AddElementForSmoothMove(giftCoinGO.transform, newPosition, 1, SmoothEnum.InLineWithSlowdown, 0.05f, false, true);
            yield return new WaitForSeconds(0.3f);
        }

        int coins = getCoins;
        int exchangeRate = 5;
        //создаем монеты рядом с монетой
        do
        {
            //определяем какое количество будет передано в монете
            if (coins < exchangeRate)
            {
                exchangeRate = coins;
            }
            coins -= exchangeRate;

            //добавим рандома в месте создания монет
            float randomNumberX = UnityEngine.Random.Range(-15, 15) * 0.1f;
            float randomNumberY = UnityEngine.Random.Range(-15, 15) * 0.1f;

            GameObject coinGO = GameObject.Instantiate(Resources.Load("Prefabs/Canvas/GameCanvas/ImageCoin") as GameObject, giftCoinGO.transform.position, Quaternion.identity, transformParent);

            //перемещаем на рандомную позицию
            MainAnimator.Instance.AddElementForSmoothMove(coinGO.transform, new Vector3(giftCoinGO.transform.position.x + randomNumberX, giftCoinGO.transform.position.y + randomNumberY, giftCoinGO.transform.position.z), 1, SmoothEnum.InLineWithSlowdown, 0.05f, false, true);

            //перемещаем к монете в магазине
            MainAnimator.Instance.AddElementForSmoothMove(coinGO.transform, shopImageCoinsTransform.position, 1, SmoothEnum.InLineWithOneSpeed, 0.85f, true, true, delegate { Shop.Instance.CoinFlew(exchangeRate); });

            yield return new WaitForEndOfFrame();
        } while (coins > 0);
    }

    //создаем панель инфформации
    public void CreateShopInformation(string str)
    {
        if (panelShop != null)
        {
            if (panelShopConfirmation != null)
            {
                Destroy(panelShopConfirmation);
            }
            panelShopConfirmation = Instantiate(PrefabBank.PanelShopInformation, panelShop.transform);
            panelShopConfirmation.transform.Find("TextConfirmation").GetComponent<Text>().text = str;
            ChangeButtonAction(panelShopConfirmation.transform.Find("ButtonOk"), DestroyPanelShopConfirmation);
        }
    }

    public void DestroyPanelShop()
    {
        //если не в состоянии покупки
        if (!stateOfPurchase)
        {
            //уменьшаем панель в верхнем углу
            panelShopOnGame.localScale = panelShopOnGame.localScale / 2.0f;

            Destroy(panelShop);
            ChangeButtonAction(buttonShopTransform, CreateShop, "Магазин");

            //принудительно пересчитываем количество вещей и монет
            ThingsManager.Instance.CountAllNumber();
            CountCoins();
        }
    }

    public void DestroyPanelShopConfirmation()
    {
        Destroy(panelShopConfirmation);
    }

    private void ChangeButtonAction(Transform buttonTransform, Action action, string str) {
        Button ButtonE = buttonTransform.GetComponent(typeof(Button)) as Button;
        if (buttonTransform.GetComponentInChildren<Text>())
        {
            buttonTransform.GetComponentInChildren<Text>().text = str;
        }        
        ButtonE.onClick.RemoveAllListeners();
        ButtonE.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);        
        ButtonE.onClick.AddListener(delegate { action(); });
    }

    private void ChangeButtonAction(Transform buttonTransform, Action action)
    {
        Button ButtonE = buttonTransform.GetComponent(typeof(Button)) as Button;
        ButtonE.onClick.RemoveAllListeners();
        ButtonE.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
        ButtonE.onClick.AddListener(delegate { action(); });
    }

    //магазин за реальные деньги
    /// Проверить, куплен ли товар.
    /// <param name="id">Индекс товара в списке.</param>
    public static bool CheckBuyState(string id)
    {
        Product product = m_StoreController.products.WithID(id);
        if (product.hasReceipt) { return true; }
        else { return false; }
    }

    //Инизиализация IAP, добавляет предметы, которые доступны для продажи и позволяет обрабатывать необходимые события
    public void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        foreach (ProductV s in PRODUCTS) builder.AddProduct(s.id, s.productType);
        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    //вызывается когда приложение может подключиться к Unity IAP
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");

        m_StoreController = controller;
        m_StoreExtensionProvider = extensions;
    }
    //Вызывается когда приложению не удалось подключиться к Unity IAP
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }
    
    //купить товар
    public void Buy(ProductV product)
    {
        //если не уже в состоянии покупки
        if (!stateOfPurchase)
        {
            stateOfPurchase = true;
            if (product.priceCoins <= Coins && product.priceCoins != 0)
            {
                //если достаточно монет
                if (product.productType == ProductType.Consumable)
                {
                    Shop_OnPurchaseConsumable(product);
                }
                else if (product.productType == ProductType.NonConsumable)
                {
                    Shop_OnPurchaseNonConsumable(product);
                }
            }
            else if (product.priceCoins != 0)
            {
                //если недостаточно монет 
                //!!!предолжить купить монет
                CreateShopInformation("У вас недостаточно монет для покупки!");
                stateOfPurchase = false;
            }
            else if (product.priceCoins == 0)
            {
                //покупка за реальные деньги
                currentProduct = product;
                BuyProductForRealMoney(product);
            }
            else
            {
                Debug.Log("Неудалось распознать тип покупки: " + product.id);
                stateOfPurchase = false;
            }
        }        
    }

    //покупка продукта используя его идентификатор
    void BuyProductForRealMoney(ProductV productV)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productV.id);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                OnPurchaseFailed(product, PurchaseFailureReason.ProductUnavailable);
            }
        }
    }

    //вызывается когда покупка успешно совершена
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        bool result;
        if (PRODUCTS.Length > 0 && String.Equals(args.purchasedProduct.definition.id, currentProduct.id, StringComparison.Ordinal) && currentProduct.productType == ProductType.Consumable)
            result = Shop_OnPurchaseConsumable(currentProduct);
        else if (PRODUCTS.Length > 0 && String.Equals(args.purchasedProduct.definition.id, currentProduct.id, StringComparison.Ordinal) && currentProduct.productType == ProductType.NonConsumable)
            result = Shop_OnPurchaseNonConsumable(currentProduct);
        else
        {
            CreateShopInformation("Во время обработки вашей покупки, что-то пошло не так!");
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
            stateOfPurchase = false;
            result = false;
        }        

        if (result)
        {
            return PurchaseProcessingResult.Complete;
        }
        else
        {
            return PurchaseProcessingResult.Pending;
        }

    }
    
    //действия после удачной покупки в магазине
    private bool Shop_OnPurchaseConsumable(ProductV product)
    {
        if (product != null)
        {
            bool result = true;
            
            //если есть бандл инструментов
            if (product.compositionBundle.Length > 0)
            {
                result = ThingsManager.Instance.addinstruments(product.compositionBundle);
            }

            //если небыло никаких ошибок и в купленном бандле есть монеты - добавляем монеты
            if (result)
            {
                addCoins += product.coins;
                JsonSaveAndLoad.RecordSave(this);
            }

            //если успешная покупка, то отнимаем стоимось покупки в монетах (если она есть)
            if (result)
            {
                //выводим панель подтверждения
                StartCoroutine(CreateShopConfirmation(product));
                addCoins -= product.priceCoins;
                StartCoroutine(UpdateTextCoins());
                JsonSaveAndLoad.SetSaveToFile();
            }
            else
            {
                CreateShopInformation("Мы не смогли обработать вашу покупку!");
                stateOfPurchase = false;
            }

            product = null;            
            return result;
        }
        else
        {
            Debug.Log("Была произведена успешная покупка, но целевой бандел не найден!! " + product.id);
            CreateShopInformation("Мы не смогли обработать вашу покупку!");
            product = null;
            stateOfPurchase = false;
            return false;
        }        
    }

    private bool Shop_OnPurchaseNonConsumable(ProductV product)
    {

        //!!! пока не используем одноразовые покупки

        //if (currentProduct != null)
        //{
        //    Debug.Log("успешная покупка " + args.purchasedProduct.definition.id);
        //    InstrumentsManager.Instance.addinstruments(currentProduct.compositionBundle);
        //    currentProduct = null;
        //}
        //else
        //{
        //    Debug.Log("Была произведена успешная покупка, но целевой бандел не найден!! " + args.purchasedProduct.definition.id);
        //}
        stateOfPurchase = false;
        return false;
    }

    //вызывается когда покупка не удалась и сообщение с ошибкой пишется в консоль
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        //OnFailedP(product, failureReason);
        //выводим панель подтверждения
        CreateShopInformation("Во время обработки вашей покупки, что-то пошло не так!");
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        stateOfPurchase = false;
    }
}

[System.Serializable]
public class ProductV {
    public string id;
    public string name;
    public int priceCoins = 0;//цена в монетах
    public ProductType productType;//тип продука, многоразовый или одноразовый
    
    //что получаем за покупку
    public BundleShopV[] compositionBundle;//состав бандла для покупки
    public int coins = 0;//сколько получаем монет

    public Sprite image;

    private Button button;
    public Button Button
    {
        get
        {
            return button;
        }

        set
        {
            button = value;
        }
    }

    public void AddAction(GameObject elementGameObject)
    {
        //добавляем действие к кнопке
        Button = elementGameObject.GetComponent(typeof(Button)) as Button;
        Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(SoundManager.Instance.PlayClickButtonSound);
        Button.onClick.AddListener(delegate { Action(); });
    }

    //действие при нажатии
    public void Action()
    {
        Shop.Instance.Buy(this);
    }
}

[System.Serializable]
public class BundleShopV {
    public InstrumentsEnum type;//какой вид инструмента
    public int count;//количество

    public BundleShopV(InstrumentsEnum type, int count)
    {
        this.type = type;
        this.count = count;
    }
}