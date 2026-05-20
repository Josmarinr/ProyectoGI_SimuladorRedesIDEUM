
using System.Collections.Generic;
using Shared;
using TE;
using UnityEngine;

namespace Example4 {

  /// <summary>
  // This example is based off of Example 1, but uses two tangible placement zones, and spans over two 4k touch displays as a 7680 x 2160 resolution application.
  // Using Touchscript, we can use multiple monitors and Tangible Engine at the same time.

  //The SimulatorView prefab is included in the scene since the canvas scaler reference resolution needs to be set to 7680 x 2160 in order to work properly. 

  //Use the following command line arguments in a build to show the application properly:
  //TangibleEngineDemos.exe -screen-width 7860 -screen-height 2160 -popupwindow -screen-fullscreen 0
  /// </summary>
  public class Example4App : MonoBehaviour {

    /// <summary>
    /// Reference to the camera responsible for displaying the UI. The camera is also used for performing raycasts into the scene.
    /// </summary>
    public Camera Camera;

    /// <summary>
    /// Reference to the collider used as the tangible 'hotspot'. Raycasts from the camera are checked to see if a tangible's center point intersects
    /// with this collider.
    /// </summary>
    public Collider2D TangibleHotspotCollider, TangibleHotspotCollider2;
    public TangibleZone zone1, zone2;

   

    void Start() {
      //Subscribe to the relevant tangible engine events
      TangibleEngine.OnTangibleAdded += TangibleEngine_OnTangibleAdded;
      TangibleEngine.OnTangibleRemoved += TangibleEngine_OnTangibleRemoved;
      TangibleEngine.OnTangibleUpdated += TangibleEngine_OnTangibleUpdated;
    }

    void OnDestroy() {
      //Unsubscribe from the relevant tangible engine events
      TangibleEngine.OnTangibleAdded -= TangibleEngine_OnTangibleAdded;
      TangibleEngine.OnTangibleRemoved -= TangibleEngine_OnTangibleRemoved;
      TangibleEngine.OnTangibleUpdated -= TangibleEngine_OnTangibleUpdated;
    }

    private void TangibleEngine_OnTangibleAdded(Tangible t) {
        TrySetActiveTangible(t);
    }

    private void TangibleEngine_OnTangibleRemoved(Tangible t) {
      if (zone1._activeTangibleId == t.Id)
      {
        zone1.HideInfo();
      }
      else if (zone2._activeTangibleId == t.Id)
      {
        zone2.HideInfo();
      }
    }

    private void TangibleEngine_OnTangibleUpdated(Tangible t) {
       TrySetActiveTangible(t);
    }

    /// <summary>
    /// Check to see if the provided Tangible instance meets the criteria for becoming the active tangible. If the provided tangible does meet the
    /// criteria, show the UI and set the UI's content to match the content associated with the pattern-id of this tangible instance.
    /// </summary>
    /// <param name="t">The tangible to check against the criteria</param>
    private void TrySetActiveTangible(Tangible t) {
      //check to see if the tangible is within the hotspot. If it is not, return early.
      if (!IsWithinHotspot(t)) 
      {
        if (zone1._activeTangibleId == t.Id)
        {
          zone1.HideInfo();
        }
        else if(zone2._activeTangibleId == t.Id)
        {
          zone2.HideInfo();
        }
      }
    }


    /// <summary>
    /// Checks to see if the provided tangible intersects with the hotspot's collider.
    /// </summary>
    /// <param name="t">The tangible instance to check</param>
    /// <returns>true if the provided tangible's position intersects with the hotspot's collider. False otherwise.</returns>
    private bool IsWithinHotspot(Tangible t) {
      // Get the world position of the tangible (which is in screen space by default)
      var worldPoint = Camera.ScreenToWorldPoint(t.Pos);
      // Try to see if the world position of the tangible overlaps with a 2D collider.
      var hitCollider = Physics2D.OverlapPoint(worldPoint);

      if(hitCollider == TangibleHotspotCollider)
      {
        zone1.ShowInfo(t);
        return true;
      }
      else if(hitCollider == TangibleHotspotCollider2)
      {
        zone2.ShowInfo(t);
        return true;
      }

      return false;
    }

  }
}
