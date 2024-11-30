using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CustomTrackMod;

internal record SerializablePrivateData
{
    public bool forceMaxLevel = false;
    public Dictionary<string, string> instrumentMap = new();
}