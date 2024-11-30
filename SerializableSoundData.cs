using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

// Porting from https://github.com/gottyduke/Elin.Plugins/blob/master/CustomWhateverLoader/API/SerializableSoundData.cs

#pragma warning disable CS0649
#pragma warning disable CS0414
// ReSharper disable All 

namespace CustomTrackMod;

internal record SerializableSoundData : SerializableSoundDataV1;

internal record SerializableSoundDataV1
{
    [JsonConverter(typeof(StringEnumConverter))]
    public SoundData.Type type = SoundData.Type.Default;
    
    public int loop = 0;
    public float minInterval = 0f;
    
    public float chance = 1f;
    public float delay = 0f;
    public float startAt = 0f;
    public bool fadeAtStart = false;
    public float fadeLength = 0f;
    
    public float volume = 0.5f;
    public bool volumeAsMtp = false;
    
    public bool allowMultiple = true;
    public bool skipIfPlaying = false;
    public bool important = false;
    public bool alwaysPlay = false;
    public bool noSameSound = false;
    
    public float pitch = 1f;
    public float randomPitch = 0f;
    public float reverbMix = 1f;
    public float spatial = 0f;

    public SerializableBGMData bgmDataOptional = new();
    
    public record SerializableBGMData
    {
        public bool day = false;
        public bool night = false;
        
        public float fadeIn = 0.1f;
        public float fadeOut = 0.5f;
        
        public float failDuration = 0.7f;
        public float failPitch = 0.12f;
        public float pitchDuration = 0.01f;
        
        public List<BGMData.Part> parts = [new()];
    }
}
// ReSharper restore All 