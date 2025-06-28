namespace PCL.Neo.Core.Models.Minecraft.Game.Data.Arguments;

public record CustomFeatures
{
    public Dictionary<string, bool> FeatureCustomValue { get; } = new();

    public bool IsDemoUser
    {
        get
        {
            return FeatureCustomValue.TryGetValue("is_demo_user", out bool value) && value;
        }
        set
        {
            FeatureCustomValue["is_demo_user"] = value;
        }
    }

    public bool HasCustomResolution
    {
        get
        {
            return FeatureCustomValue.TryGetValue("has_custom_resolution", out bool value) && value;
        }
        set
        {
            FeatureCustomValue["has_custom_resolution"] = value;
        }
    }

    public bool HasQuickPlaysSupport
    {
        get
        {
            return FeatureCustomValue.TryGetValue("has_quick_plays_support", out bool value) && value;
        }
        set
        {
            FeatureCustomValue["has_quick_plays_support"] = value;
        }
    }


    public bool IsQuickPlaySingleplayer
    {
        get
        {
            return FeatureCustomValue.TryGetValue("is_quick_play_singleplayer", out bool value) && value;
        }
        set
        {
            FeatureCustomValue["is_quick_play_singleplayer"] = value;
        }
    }

    public bool IsQuickPlayMultiplayer
    {
        get
        {
            return FeatureCustomValue.TryGetValue("is_quick_play_multiplayer", out bool value) && value;
        }
        set
        {
            FeatureCustomValue["is_quick_play_multiplayer"] = value;
        }
    }


    public bool IsQuickPlayRealms
    {
        get
        {
            return FeatureCustomValue.TryGetValue("is_quick_play_realms", out bool value) && value;
        }
        set
        {
            FeatureCustomValue["is_quick_play_realms"] = value;
        }
    }
}