using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class Shop : MonoBehaviour, IStoreListener
{
    public static Shop Instance; // Синглтон
    private int coins = 0; //валюта магазина
    private int exchangeRate = 3; //курс обмена звезд на коины
    private Text textCoins;
    private GameObject panelShop;

    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;
    private ProductV currentProduct;

    [Tooltip("Список товаров для доступные для покупки")]
    public ProductV[] PRODUCTS;

    public int Coins
    {
        get
        {
            return coins;
        }
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
        DontDestroyOnLoad(gameObject);
        InitializePurchasing();
    }

    // Start is called before the first frame update
    void Start()
    {
        Save save = JsonSaveAndLoad.LoadSave();
        coins = save.shopSave.coins;

        Transform gOTextCoins = transform.Find("TextCoins");
        textCoins = gOTextCoins.GetComponent(typeof(Text)) as Text;
        UpdateTextCoins();

        //Shop.OnPurchaseConsumable += Shop_OnPurchaseConsumable;
        //Shop.OnPurchaseNonConsumable += Shop_OnPurchaseNonConsumable;
    }

    public void ExchangeStarsForCoins(Level level, int stars) {

        if (level.Stars < stars )
        {
            coins += (stars - level.Stars) * exchangeRate;
            JsonSaveAndLoad.RecordSave(this);
            UpdateTextCoins();
        }
    }

    //обнолвление текста
    private void UpdateTextCoins()
    {
        textCoins.text = coins.ToString();
    }

    //создание меню магазина
    public void CreateShopButtons()
    {
        if (panelShop != null)
        {
            Destroy(panelShop);
        }
        panelShop = Instantiate(PrefabBank.Instance.shopPanelPrefab, transform);
        Transform contentTransform = panelShop.transform.Find("Viewport/Content");

        //витрина
        foreach (ProductV product in PRODUCTS)
        {
            GameObject bottonGO = Instantiate(PrefabBank.Instance.shopButtonPrefab, contentTransform);
            Transform textNameTransform = bottonGO.transform.Find("TextName");
            textNameTransform.GetComponentInChildren<Text>().text = product.id;
            Transform textPriceTransform = bottonGO.transform.Find("TextPrice");
            textPriceTransform.GetComponentInChildren<Text>().text = product.priceCoins.ToString();
            Transform imageTransform = bottonGO.transform.Find("Image");
            imageTransform.GetComponentInChildren<Image>().sprite = product.image;

            product.AddAction(bottonGO);
        }

        //Добавляем кнопке выход действие
        Transform buttonExitTransform = panelShop.transform.Find("ButtonExit");
        Button ButtonE = buttonExitTransform.GetComponent(typeof(Button)) as Button;
        ButtonE.onClick.AddListener(delegate { DestroyPanelShop(); });

    }

    public void DestroyPanelShop()
    {
        Destroy(panelShop);
    }

    //магазин за реальные деньги
    ///// Событие, которое запускается при удачной покупке многоразового товара.
    //public static event OnSuccessConsumable OnPurchaseConsumable;
    ///// Событие, которое запускается при удачной покупке не многоразового товара.
    //public static event OnSuccessNonConsumable OnPurchaseNonConsumable;
    ///// Событие, которое запускается при неудачной покупке какого-либо товара.
    //public static event OnFailedPurchase PurchaseFailed;

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
        //foreach (string s in NC_PRODUCTS) builder.AddProduct(s, ProductType.NonConsumable);
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
        if (product.priceCoins <= coins && product.priceCoins != 0)
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
        }
        else if(product.priceCoins == 0)
        { 
            //покупка за реальные деньги
            currentProduct = product;
            BuyProductForRealMoney(product);
        }
        else
        {
            Debug.Log("Недалось распознать тип покупки: " + product.id);
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
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
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
                result = InstrumentsManager.Instance.addinstruments(product.compositionBundle);
            }

            //если небыло никаких ошибок и в купленном бандле есть монеты - добавляем монеты
            if (result)
            {
                coins += product.coins;                
            }

            //если успешная покупка, то отнимаем стоимось покупки в монетах (если она есть)
            if (result)
            {
                Debug.Log("успешная покупка " + product.id);
                coins -= product.priceCoins;
            }
            else
            {
                Debug.Log("неудачная покупка " + product.id);
            }

            UpdateTextCoins();
            currentProduct = null;
            return result;
        }
        else
        {
            Debug.Log("Была произведена успешная покупка, но целевой бандел не найден!! " + product.id);
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

        return false;
    }


    
    //public delegate void OnSuccessConsumable(PurchaseEventArgs args);
    //protected virtual void OnSuccessC(PurchaseEventArgs args)
    //{
    //    if (OnPurchaseConsumable != null) OnPurchaseConsumable(args);
    //    Debug.Log(currentProduct.id + " Buyed!");
    //}
    //public delegate void OnSuccessNonConsumable(PurchaseEventArgs args);
    //protected virtual void OnSuccessNC(PurchaseEventArgs args)
    //{
    //    if (OnPurchaseNonConsumable != null) OnPurchaseNonConsumable(args);
    //    Debug.Log(currentProduct.id + " Buyed!");
    //}
    //public delegate void OnFailedPurchase(Product product, PurchaseFailureReason failureReason);
    //protected virtual void OnFailedP(Product product, PurchaseFailureReason failureReason)
    //{
    //    if (PurchaseFailed != null) PurchaseFailed(product, failureReason);
    //    Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    //}

    //вызывается когда покупка не удалась и сообщение с ошибкой пишется в консоль
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        //OnFailedP(product, failureReason);
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }
}

[System.Serializable]
public class ProductV {
    public string id;
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
}