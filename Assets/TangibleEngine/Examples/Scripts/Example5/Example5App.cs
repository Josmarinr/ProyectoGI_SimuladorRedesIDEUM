using System.Collections.Generic;
using Shared;
using TE;
using UnityEngine;

namespace Example5
{

  /// <summary>
  // This example is based off of Example 1, but uses two tangible placement zones, and spans over two 4k touch displays as a 7680 x 2160 resolution application.
  // Using Touchscript, we can use multiple monitors and Tangible Engine at the same time.

  //The SimulatorView prefab is included in the scene since the canvas scaler reference resolution needs to be set to 7680 x 2160 in order to work properly. 

  //Use the following command line arguments in a build to show the application properly:
  //TangibleEngineDemos.exe -screen-width 7860 -screen-height 2160 -popupwindow -screen-fullscreen 0
  /// </summary>
  public class Example5App : MonoBehaviour
  {

    /// <summary>
    /// Reference to the camera responsible for displaying the UI. The camera is also used for performing raycasts into the scene.
    /// </summary>
    public Camera Camera;

    public GameObject tanPrefab;
    public Canvas can;

    void Start()
    {
      //Subscribe to the relevant tangible engine events
      TangibleEngine.OnTangibleAdded += TangibleEngine_OnTangibleAdded;
      
    }

    void OnDestroy()
    {
      //Unsubscribe from the relevant tangible engine events
      TangibleEngine.OnTangibleAdded -= TangibleEngine_OnTangibleAdded;
      
    }

    private void TangibleEngine_OnTangibleAdded(Tangible t)
    {
      TangibleHolder holder = Instantiate(tanPrefab).GetComponent<TangibleHolder>();
      holder.transform.SetParent(can.transform);
      holder.SetUp(t);
    }

    void Update()
    {
      
      if (Input.GetKeyDown(KeyCode.Escape))
      {
        Quit();
      }
    }

    public void Quit() {
      Application.Quit();
    }


  }
}