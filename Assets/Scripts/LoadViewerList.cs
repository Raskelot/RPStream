using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.IO;
using UnityEngine.UI;
using System.Net;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LoadViewerList : MonoBehaviour
{
    private bool isCoroutineSent = false;
    public JsonObject jData = new JsonObject();
    private int errorCount = 0;

    public void LoadViewer(string channelName)
    {
        if (!isCoroutineSent)
        {
            isCoroutineSent = true;
            string url = "http://tmi.twitch.tv/group/user/" + channelName + "/chatters";
            WWW www = new WWW(url);
            StartCoroutine(WaitForRequest(www));
        }
    }

    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;

        if (www.error == null)
        {
            string jsonString = www.text;
            jData = JsonObject.CreateFromJSON(jsonString);
            GameData.viewer.Clear();
            for (int i = 0; i < GameData.character.Count; i++)
            {
                GameData.character[i].isOnline = false;
            }

            for (int i = 0; i < jData.chatters.viewers.Length; i++)
            {
                GameData.viewer.Add(jData.chatters.viewers[i]);
                for (int k = 0; k < GameData.character.Count; k++)
                {
                    if (jData.chatters.viewers[i] == GameData.character[k].username)
                        GameData.character[k].isOnline = true;
                }
            }

            for (int i = 0; i < jData.chatters.moderators.Length; i++)
            {
                GameData.viewer.Add(jData.chatters.moderators[i]);
                for (int k = 0; k < GameData.character.Count; k++)
                {
                    if (jData.chatters.moderators[i] == GameData.character[k].username)
                        GameData.character[k].isOnline = true;
                }
            }

            for (int i = 0; i < jData.chatters.vips.Length; i++)
            {
                GameData.viewer.Add(jData.chatters.vips[i]);
                for (int k = 0; k < GameData.character.Count; k++)
                {
                    if (jData.chatters.vips[i] == GameData.character[k].username)
                        GameData.character[k].isOnline = true;
                }
            }

            for (int i = 0; i < jData.chatters.broadcaster.Length; i++)
            {
                GameData.viewer.Add(jData.chatters.broadcaster[i]);
                for (int k = 0; k < GameData.character.Count; k++)
                {
                    if (jData.chatters.broadcaster[i] == GameData.character[k].username)
                        GameData.character[k].isOnline = true;
                }
            }

            for (int i = 0; i < jData.chatters.staff.Length; i++)
            {
                GameData.viewer.Add(jData.chatters.staff[i]);
                for (int k = 0; k < GameData.character.Count; k++)
                {
                    if (jData.chatters.staff[i] == GameData.character[k].username)
                        GameData.character[k].isOnline = true;
                }
            }

            for (int i = 0; i < jData.chatters.admins.Length; i++)
            {
                GameData.viewer.Add(jData.chatters.admins[i]);
                for (int k = 0; k < GameData.character.Count; k++)
                {
                    if (jData.chatters.admins[i] == GameData.character[k].username)
                        GameData.character[k].isOnline = true;
                }
            }
            errorCount = 0;
            Debug.Log(jsonString);
        }
        else
        {
            errorCount++;
            if (errorCount >= 3)
                Debug.Log("Error Viewer List loading more than 3 times.");
        }
        isCoroutineSent = false;
    }
}
