using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TE;

namespace Example5
{
  public class TangibleHolder : MonoBehaviour
  {
    Tangible tan;
    public GameObject origamiPrefab;
    OrigamiVisual visual;
    void Start()
    {
      //Subscribe to the relevant tangible engine events

      TangibleEngine.OnTangibleRemoved += TangibleEngine_OnTangibleRemoved;
      TangibleEngine.OnTangibleUpdated += TangibleEngine_OnTangibleUpdated;
    }

    void OnDestroy()
    {
      //Unsubscribe from the relevant tangible engine events

      TangibleEngine.OnTangibleRemoved -= TangibleEngine_OnTangibleRemoved;
      TangibleEngine.OnTangibleUpdated -= TangibleEngine_OnTangibleUpdated;
    }

    private void TangibleEngine_OnTangibleRemoved(Tangible t)
    {
      if (t.Id == tan.Id)
      {
        visual.TangibleRemove();
        Destroy(this.gameObject);
      }
    }

    private void TangibleEngine_OnTangibleUpdated(Tangible t)
    {
      if (t.Id == tan.Id)
      {
        visual.TangibleSynch(t);
      }
    }

    public void SetUp(Tangible t)
    {
      tan = t;
      visual = Instantiate(origamiPrefab).GetComponent<OrigamiVisual>();
      visual.Configure(t);
    }


  }

}
