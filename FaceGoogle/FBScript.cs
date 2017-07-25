using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;

public class FBScript : MonoBehaviour {

    public UILabel Name;
    public UITexture Photo;
    public Texture DefaultPhoto;

    public GameObject Btn_LogIn;
    public GameObject Btn_LogOut;

    List<string> perm = new List<string>() {"public_profile","email","user_friends"};

    void Awake()
    {
        FB.Init(SetInit, OnHideUnity);
    }

    void SetInit()
    {
        if (FB.IsLoggedIn) Debug.Log("FB is log in");
        else Debug.Log("FB is log out");

        LogInOrOut(FB.IsLoggedIn);
    }

    void OnHideUnity(bool isGameShow)
    {
        if (!isGameShow) Time.timeScale = 0;
        else Time.timeScale = 1;
    }

    public void LogIn()
    {
        FB.LogInWithReadPermissions(perm, authCallBack);
    }
    
    public void LogOut()
    {
        FB.LogOut();
        LogInOrOut(FB.IsLoggedIn);

    }

    void authCallBack(IResult result)
    {
        if (result.Error != null) Debug.Log(result.Error);
        else
        {
            if (FB.IsLoggedIn) Debug.Log("FB is log in");
            else Debug.Log("FB is not log in");

            LogInOrOut(FB.IsLoggedIn);
        }
    }

    void LogInOrOut(bool isIn)
    {
        if (isIn)
        {
            Btn_LogIn.SetActive(false);
            Btn_LogOut.SetActive(true);

            FB.API("/me", HttpMethod.GET, DisplayUserInfo);
            FB.API("/me/picture?type=square&height=128&width=128", HttpMethod.GET, DisplayPhoto);
        }
        else
        {
            Btn_LogIn.SetActive(true);
            Btn_LogOut.SetActive(false);

            Name.text = "名字:";
            Photo.mainTexture = DefaultPhoto;
        }
    }

    //名字
    void DisplayUserInfo(IResult result)
    {
        if (result.Error == null)
        {
            Name.text = "名字:" + result.ResultDictionary["name"];
        }
        else Debug.Log(result.Error);
    }

    //頭像
    void DisplayPhoto(IGraphResult result)
    {
        if (result.Error == null)
        {
            Photo.mainTexture = result.Texture;
        }
        else Debug.Log(result.Error);
    }
}
