using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseMessagingManager : MonoBehaviour
{
    public static FirebaseMessagingManager Instance; // Синглтон

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
        if (Application.isPlaying)
        {
            DontDestroyOnLoadManager.DontDestroyOnLoad(gameObject); //Set as do not destroy            
        }
    }

    public void Start()
    {
        //Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        //Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
    }

    //public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    //{
    //    //UnityEngine.Debug.Log("Received Registration Token: " + token.Token);
    //}

    //public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    //{
    //    //UnityEngine.Debug.Log("Received a new message from: " + e.Message.From);
    //    //string dataTextStr = "No Data Text!";
    //    //e.Message.Data.TryGetValue("dataString", out dataTextStr);
    //    SupportFunctions.CreateInformationPanel(e.Message.Notification.Body);        
    //}
}
