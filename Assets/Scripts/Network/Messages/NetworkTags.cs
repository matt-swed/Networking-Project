/// <summary>
/// Classes that stores all tags
/// </summary>
public static class NetworkTags
{
    /// <summary>
    /// Tags used in game
    /// </summary>
    public struct InGame
    {
        ///////////////////////////
        // Common
        public const ushort SPAWN_OBJECT = 1001;
        ///////////////////////////
        // Message denoting the position and velocity of a player
        public const ushort REP_SYNC_POS = 3001;
    }
}

