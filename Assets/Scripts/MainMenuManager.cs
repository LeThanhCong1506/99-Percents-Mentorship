using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Objects")]
    private GameObject _loadingBarObject;
    private GameObject[] _objectToHide ;
    private Image _loadingBar;

    [Header("Scene to Load")]
    [SerializeField] private SceneField _persisstentGameplay;
    [SerializeField] private SceneField _levelScene; 

    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();
    private void Awake()
    {
        //_loadingBarObject.SetActive(false);
    }

    public void StartGame()
    {
        //HideMenu();

        _loadingBarObject.SetActive(true);

        _scenesToLoad.Add(SceneManager.LoadSceneAsync(_persisstentGameplay));
        _scenesToLoad.Add(SceneManager.LoadSceneAsync(_levelScene,LoadSceneMode.Additive));

        StartCoroutine(ProgressLoadingBar());
    }

    //private void HideMenu()
    //{
    //   for(int i = 0; i < _objectToHide.Length; i++)
    //    {
    //        _objectToHide[i].SetActive(false);
    //    }
    //}

    private IEnumerator ProgressLoadingBar()
    {
        float loadProgress = 0f;
        for(int i = 0; i < _scenesToLoad.Count; i++)
        {
            while (!_scenesToLoad[i].isDone)
            {
                loadProgress += _scenesToLoad[i].progress ;
                //_loadingBar.fillAmount = loadProgress / _scenesToLoad.Count;
                yield return null;
            }
        }
    }

}
