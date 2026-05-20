using UnityEngine;
using TE;
using System.Collections.Generic;
using System.Collections;

namespace Example5
{
  public class OrigamiVisual : MonoBehaviour
  {

    public float offset;

    //public int _tangibleId;

    Mesh mesh;

    // mesh variables
    Vector3[] verts;
    int[] tris;
    Vector3[] norms;
    Color32[] colors;

    // procedural generation variables
    int num_sections = 10;
    int tris_per_section = 10;
    Vector3[] growth_axes;
    int vert_count { get { return num_sections * tris_per_section * 3; } }

    string tangible_name = "tangible_0";
    Gradient activeGradient
    {
      get
      {
        Gradient grad;
        if (gradients.TryGetValue(tangible_name, out grad))
        {
          return grad;
        }
        return default_gradient;
      }
    }

    Dictionary<string, Gradient> gradients = new Dictionary<string, Gradient>();
    Gradient default_gradient;

    public void Configure(Tangible tangible)
    {
      //_tangibleId = tangible.Id;
      tangible_name = tangible.PatternName;
      SynchronizeWithTangible(tangible);
      //TangibleEngine.Subscribe(this);
    }

    void Start()
    {
      default_gradient = buildGradient("#222222", "#444444", "#888888", "#aaaaaa");
      gradients["120_50"] = buildGradient("#0edfff", "#73efdc", "#e778ff", "#5c8aff");
      gradients["120_60"] = buildGradient("#a7ffcb", "#efd959", "#efd959", "#d7ff5c");
      gradients["120_70"] = buildGradient("#ff521a", "#efd959", "#ff0f00", "#ffb440");

      mesh = GetComponent<MeshFilter>().mesh;
      mesh.Clear();

      verts = new Vector3[vert_count];
      tris = new int[vert_count];
      norms = new Vector3[vert_count];
      colors = new Color32[vert_count];

      growth_axes = new Vector3[tris_per_section * 3];

      for (int i = 0; i < tris_per_section; i++)
      {
        Vector3 dir = new Vector3(Mathf.Sign(Random.Range(-1.0f, 1.0f)) * Random.Range(1.0f, 8.0f), Mathf.Sign(Random.Range(-1.0f, 1.0f)) * Random.Range(1.0f, 8.0f), Random.Range(-3.0f, 3.0f));
        dir.Normalize();
        Vector3 dir2 = new Vector3(0, 1.0f, 1.0f);
        dir2.Normalize();

        growth_axes[i * 3 + 0] = dir;
        growth_axes[i * 3 + 1] = Quaternion.AngleAxis(Random.Range(40, 140), Vector3.Cross(dir, dir2)) * dir;
        growth_axes[i * 3 + 2] = Quaternion.AngleAxis(Random.Range(220, 320), Vector3.Cross(dir, dir2)) * dir;

        for (int j = 0; j < num_sections; j++)
        {
          int n = i * 3 + j * tris_per_section * 3;

          for (int k = 0; k < 3; k++)
          {
            tris[n + k] = n + k; ;
            norms[n + k] = Vector3.Cross(growth_axes[i * 3], growth_axes[i * 3 + 1]);
            colors[n + k] = new Color32(255, 255, 255, 255);
          }
        }
      }
      mesh.vertices = verts;
      mesh.triangles = tris;
      mesh.normals = norms;
      mesh.colors32 = colors;
    }

    float baseAmount = 2.0f;

    void Update()
    {
      float magnitude = Mathf.Abs(offset / 30.0f) + baseAmount;
      Gradient gradient = activeGradient;
      for (int i = 0; i < tris_per_section; i++)
      {
        float increment = Mathf.Max(magnitude - i, 0);
        increment = (float)i * (1.0f - Mathf.Exp(-increment / 3));

        for (int j = 0; j < num_sections; j++)
        {
          int n = i * 3 + j * tris_per_section * 3;

          float frac = ((float)j / (float)(num_sections));

          Vector3 center = new Vector3(3, 0, 0);

          Quaternion local_rot = Quaternion.AngleAxis(frac * 360.0f, new Vector3(0, 0, -1));
          Quaternion rot = Quaternion.AngleAxis(-offset, new Vector3(0, 0, -1));

          Vector3 norm = Vector3.Cross(verts[n + 2] - verts[n + 1], verts[n] - verts[n + 1]);
          if (norm.z > 0)
          {
            norm *= -1;
          }
          Color32 c = getColor(gradient, Vector3.Dot(new Vector3(norm.x, 0, norm.z), new Vector3(1, 0, 0)));

          for (int k = 0; k < 2; k++)
          {
            verts[n + k] = rot * (local_rot * (center + growth_axes[i * 3 + k] * increment));
            norms[n + k] = norm;
            colors[n + k] = c;
          }
        }
      }

      mesh.vertices = verts;
      mesh.normals = norms;
      mesh.colors32 = colors;
    }

    // UTILITY FUNCTIONS
    Gradient buildGradient(string c1, string c2, string c3, string c4)
    {
      Gradient grad = new Gradient();
      GradientColorKey[] gck = new GradientColorKey[4];
      ColorUtility.TryParseHtmlString(c1, out gck[0].color);
      gck[0].time = 0.0f;
      ColorUtility.TryParseHtmlString(c2, out gck[1].color);
      gck[1].time = 0.5f;
      ColorUtility.TryParseHtmlString(c3, out gck[2].color);
      gck[2].time = 1.0f;
      ColorUtility.TryParseHtmlString(c4, out gck[3].color);
      gck[3].time = 1.5f;
      GradientAlphaKey[] gak = new GradientAlphaKey[2];
      gak[0].alpha = 1.0f;
      gak[0].time = 0.0f;
      gak[1].alpha = 1.0f;
      gak[1].alpha = 2.0f;
      grad.SetKeys(gck, gak);
      return grad;
    }

    Color32 getColor(Gradient gradient, float frac)
    {
      float f = Mathf.Clamp(frac, -1, 1) * 2;
      Color c = gradient.Evaluate(f);

      return new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), (byte)(c.a * 255));
    }

    bool _dying = false;

    public void TangibleRemove()
    {
      if (!_dying)
      {
        _dying = true;
        StartCoroutine(KillMe());
      }
    }

    public void TangibleSynch(Tangible t)
    {
      if (!_dying)
      {
        SynchronizeWithTangible(t);
      }
    }

    void SynchronizeWithTangible(Tangible t)
    {
      offset = t.R * Mathf.Rad2Deg;
      Ray ray = Camera.main.ScreenPointToRay(new Vector3(t.Pos.x, t.Pos.y, 0));
      Debug.DrawRay(ray.origin, ray.direction * 16, Color.yellow);
      Vector3 point = ray.GetPoint(transform.position.z - Camera.main.transform.position.z);
      transform.position = new Vector3(point.x, point.y, -2.5f);
    }

    IEnumerator KillMe()
    {
      float t = 0.0f;
      while (t < 1.0f)
      {
        t += Time.deltaTime;
        offset *= 0.9f;
        baseAmount *= 0.9f;
        yield return null;
      }
      Destroy(gameObject);
    }
  }
}