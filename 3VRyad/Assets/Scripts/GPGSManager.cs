using System;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;

public static class GPGSManager
{
    //public const string DEFAULT_SAVE_NAME = "Save";

    //private static ISavedGameClient savedGameClient;
    //private static ISavedGameMetadata currentMetadata;

    //private static DateTime startDateTime;

    //private static CloudSavesUI savesUI;

    //public static bool IsAuthenticated
    //{
    //    get
    //    {
    //        if (PlayGamesPlatform.Instance != null) return PlayGamesPlatform.Instance.IsAuthenticated();
    //        return false;
    //    }
    //}
    //public static bool SavesUIOpened { get; private set; }

    //public static void Initialize(bool debug)
    //{
    //        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
    //                    .EnableSavedGames()
    //                    .Build();
    //        PlayGamesPlatform.InitializeInstance(config);
    //        PlayGamesPlatform.DebugLogEnabled = debug;
    //        PlayGamesPlatform.Activate();

    //        startDateTime = DateTime.Now;
    //}
    //public static void Initialize(bool debug, CloudSavesUI savesUI)
    //{
    //    GPGSManager.savesUI = savesUI;
    //    Initialize(debug);
    //}

    //public static void Auth(Action<bool> onAuth)
    //{
    //    Social.localUser.Authenticate((success) =>
    //    {
    //        if (success) savedGameClient = PlayGamesPlatform.Instance.SavedGame;
    //        onAuth(success);
    //    });
    //}

    //public static void ShowSavesUI(Action<SavedGameRequestStatus, byte[]> onDataRead, Action onDataCreate)
    //{
    //    if (!IsAuthenticated)
    //    {
    //        onDataRead(SavedGameRequestStatus.AuthenticationError, null);
    //        return;
    //    }
    //    SavesUIOpened = true;
    //    savedGameClient.ShowSelectSavedGameUI("Select saved game",
    //        savesUI.MaxDisplayCount,
    //        savesUI.AllowCreate,
    //        savesUI.AllowDelete,
    //        (status, metadata) =>
    //        {
    //            if (status == SelectUIStatus.SavedGameSelected && metadata != null)
    //            {
    //                if (string.IsNullOrEmpty(metadata.Filename)) onDataCreate(); //либо любое другое действие при создании сохранения
    //                else ReadSaveData(metadata.Filename, onDataRead); //либо любое другое действие при чтении сохранения
    //            }
    //            SavesUIOpened = false;
    //        });
    //}

    //private static void OpenSaveData(string fileName, Action<SavedGameRequestStatus, ISavedGameMetadata> onDataOpen)
    //{
    //    if (!IsAuthenticated)
    //    {
    //        onDataOpen(SavedGameRequestStatus.AuthenticationError, null);
    //        return;
    //    }
    //    savedGameClient.OpenWithAutomaticConflictResolution(fileName,
    //        DataSource.ReadCacheOrNetwork,
    //        ConflictResolutionStrategy.UseLongestPlaytime,
    //        onDataOpen);
    //}

    //public static void ReadSaveData(string fileName, Action<SavedGameRequestStatus, byte[]> onDataRead)
    //{
    //    if (!IsAuthenticated)
    //    {
    //        onDataRead(SavedGameRequestStatus.AuthenticationError, null);
    //        return;
    //    }
    //    OpenSaveData(fileName, (status, metadata) =>
    //    {
    //        if (status == SavedGameRequestStatus.Success)
    //        {
    //            savedGameClient.ReadBinaryData(metadata, onDataRead);
    //            currentMetadata = metadata;
    //        }
    //    });
    //}

    //public static void WriteSaveData(byte[] data)
    //{
    //    if (!IsAuthenticated || data == null || data.Length == 0) {
    //        Debug.Log("Ошибка сохранения в google." + " IsAuthenticated: " + IsAuthenticated.ToString() + " data.Length: " + data.Length);
    //        return;
    //    }
            
    //    TimeSpan currentSpan = DateTime.Now - startDateTime;
    //    Action onDataWrite = () =>
    //    {
    //        TimeSpan totalPlayTime = currentMetadata.TotalTimePlayed + currentSpan;
    //        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder()
    //        .WithUpdatedDescription("Saved game at " + DateTime.Now)
    //        .WithUpdatedPlayedTime(totalPlayTime);
    //        SavedGameMetadataUpdate updatedMetadata = builder.Build();
    //        savedGameClient.CommitUpdate(currentMetadata,
    //            updatedMetadata,
    //            data,
    //            (status, metadata) => currentMetadata = metadata);
    //        startDateTime = DateTime.Now;
    //        Debug.Log("Сохранения в google.");
    //    };
    //    if (currentMetadata == null)
    //    {
    //        OpenSaveData(DEFAULT_SAVE_NAME, (status, metadata) =>
    //        {
    //            Debug.Log("Cloud data write status: " + status.ToString());
    //            if (status == SavedGameRequestStatus.Success)
    //            {
    //                currentMetadata = metadata;
    //                onDataWrite();
    //            }
    //        });
    //        return;
    //    }
    //    onDataWrite();
    //}

    //public static void GetSavesList(Action<SavedGameRequestStatus, List<ISavedGameMetadata>> onReceiveList)
    //{
    //    if (!IsAuthenticated)
    //    {
    //        onReceiveList(SavedGameRequestStatus.AuthenticationError, null);
    //        return;
    //    }
    //    savedGameClient.FetchAllSavedGames(DataSource.ReadNetworkOnly, onReceiveList);
    //}

    //public struct CloudSavesUI
    //{
    //    public uint MaxDisplayCount { get; private set; }
    //    public bool AllowCreate { get; private set; }
    //    public bool AllowDelete { get; private set; }

    //    public CloudSavesUI(uint maxDisplayCount, bool allowCreate, bool allowDelete)
    //    {
    //        MaxDisplayCount = maxDisplayCount;
    //        AllowCreate = allowCreate;
    //        AllowDelete = allowDelete;
    //    }
    //}

}
