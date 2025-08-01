using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
    [SerializeField] private SceneField[] _sceneToLoad;
    [SerializeField] private SceneField[] _sceneToUnLoad;

    private GameObject _player;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        if(_player == null)
        {
            Debug.LogError("Player GameObject not found. Make sure it has the 'Player' tag assigned.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == _player)
        {
            LoadScene();
            UnloadScene();
        }
    }

    private void LoadScene()
    {
        for(int i = 0; i < _sceneToLoad.Length; i++)
        {
            bool isSceneLoaded = false;
            for(int j=0;j<SceneManager.sceneCount;j++)
            {
                Scene loadScene =SceneManager.GetSceneAt(j);
                if(loadScene.name == _sceneToLoad[i].SceneName)
                {
                    isSceneLoaded = true;
                    break;
                }
            }

            if (!isSceneLoaded)
            {
                SceneManager.LoadSceneAsync(_sceneToLoad[i], LoadSceneMode.Additive);
            }
        }
    }

    private void UnloadScene()
    {
      for(int i =0; i < _sceneToUnLoad.Length; i++)
        {
           
            for(int j=0;j<SceneManager.sceneCount;j++)
            {
                Scene unloadScene = SceneManager.GetSceneAt(j);
                if(unloadScene.name == _sceneToUnLoad[i].SceneName)
                {
                    SceneManager.UnloadSceneAsync(_sceneToUnLoad[i]);
                }
            }
        }
    }
}
