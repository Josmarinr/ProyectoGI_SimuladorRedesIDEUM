using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;

namespace TE.Comm {
  public class Msg {

    public enum DecodeResult {
      Decoded,
      InvalidType,
      MalformedData
    }

    public enum MsgTypes : int {
      None = 0,
      Init = 1,
      Patterns = 2,
      Update = 3,
      Error = 4
    }

    public Msg() { }
    
    public Msg(MsgTypes type) {
      Type = type;
    }

    public virtual DecodeResult TryDeserialize(byte[] raw, out Msg msg) {
      msg = null;
      try {
        string rawString = Encoding.UTF8.GetString(raw);
        msg = JsonConvert.DeserializeObject<Msg>(rawString);

        int messageType = (int)msg.Type;
        if(messageType > 4) {
          return DecodeResult.InvalidType;
        }
        else {
          return DecodeResult.Decoded;
        }
      }
      catch(JsonException e) {
        Trace.WriteLine("ERROR " + e);
        return DecodeResult.MalformedData;
      }
      catch(ArgumentNullException e) {
        Trace.WriteLine("ERROR " + e);
        return DecodeResult.MalformedData;
      }
    }

    public virtual string Serialize() {
      try {
        string jsonString = JsonConvert.SerializeObject(this);
        return jsonString;
      } catch(JsonException e) {
        Trace.WriteLine(e);
        return null;
      }
    }

    [JsonProperty("TYPE")]
    public MsgTypes Type;

    [JsonProperty("PATTERNS")]
    public Pattern[] Patterns=null;

    [JsonProperty("TANGIBLES")]
    public Tangible[] Tangibles=null;

    [JsonProperty("POINTERS")]
    public Pointer[] Pointers=null;

    [JsonProperty("STATUS")]
    public bool Status = true;

    [JsonProperty("DEBUG_TEXT")]
    public string DebugText;

    [JsonProperty("ID")]
    public uint Id;

  }
}