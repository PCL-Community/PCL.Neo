namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

public record FeatureCunstomKv
{
    public Dictionary<string, bool> FeatureCunstomValue { get; } = new();

    public bool IsDemoUser
    {
        get
        {
            return FeatureCunstomValue.TryGetValue("is_demo_user", out bool value) && value;
        }
        set
        {
            FeatureCunstomValue["is_demo_user"] = value;
        }
    }

    public bool HasCustomResolution
    {
        get
        {
            return FeatureCunstomValue.TryGetValue("has_custom_resolution", out bool value) && value;
        }
        set
        {
            FeatureCunstomValue["has_custom_resolution"] = value;
        }
    }

    public bool HasQuickPlaysSupport
    {
        get
        {
            return FeatureCunstomValue.TryGetValue("has_quick_plays_support", out bool value) && value;
        }
        set
        {
            FeatureCunstomValue["has_quick_plays_support"] = value;
        }
    }


    public bool IsQuickPlaySingleplayer
    {
        get
        {
            return FeatureCunstomValue.TryGetValue("is_quick_play_singleplayer", out bool value) && value;
        }
        set
        {
            FeatureCunstomValue["is_quick_play_singleplayer"] = value;
        }
    }

    public bool IsQuickPlayMultiplayer
    {
        get
        {
            return FeatureCunstomValue.TryGetValue("is_quick_play_multiplayer", out bool value) && value;
        }
        set
        {
            FeatureCunstomValue["is_quick_play_multiplayer"] = value;
        }
    }


    public bool IsQuickPlayRealms
    {
        get
        {
            return FeatureCunstomValue.TryGetValue("is_quick_play_realms", out bool value) && value;
        }
        set
        {
            FeatureCunstomValue["is_quick_play_realms"] = value;
        }
    }
}