using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class GGScript : MonoBehaviour {

    public UILabel Name;

    public UITexture Photo;
    public Texture DefaultPhoto;

    public GameObject Btn_LogIn;
    public GameObject Btn_LogOut;

    void Start()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    public void LogIn()
    {
        Social.localUser.Authenticate(success =>
        {
            if (success)
            {
                Name.text = "名字:" + Social.localUser.userName;
                //Photo.mainTexture = Social.localUser.image;
                Btn_LogIn.SetActive(false);
                Btn_LogOut.SetActive(true);
                Debug.Log("Google is log in");
            }
            else
            {
                Name.text = "名字:";
                //Photo.mainTexture = DefaultPhoto;
                Btn_LogIn.SetActive(true);
                Btn_LogOut.SetActive(false);
                Debug.Log("Google is log out");
            }
        });
    }

    public void LogOut()
    {
        ((PlayGamesPlatform)Social.Active).SignOut();
        Name.text = "名字:";
        //Photo.mainTexture = DefaultPhoto;

        Btn_LogIn.SetActive(true);
        Btn_LogOut.SetActive(false);

        Debug.Log("Google is log out");
    }
}
