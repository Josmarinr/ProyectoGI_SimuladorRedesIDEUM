using UnityEngine;
using UnityEngine.SceneManagement;

namespace Examples {
  public class ExamplesController : MonoBehaviour {

    private int? _currentlyLoadedScene;

    void Start() {
      LoadScene(1);
    }

    /// <summary>
    /// Unloads the current scene and loads a new one based on the provided index if the index references a different scene than what is currently loaded.
    /// </summary>
    /// <param name="index"></param>
    public void LoadScene(int index) {
      if (index < 1 || index > 3) return;
      if (_currentlyLoadedScene == index) return;
      if (_currentlyLoadedScene.HasValue) {
        SceneManager.UnloadSceneAsync(_currentlyLoadedScene.Value);
      }
      _currentlyLoadedScene = index;
      SceneManager.LoadScene(index, LoadSceneMode.Additive);
    }
  }
}