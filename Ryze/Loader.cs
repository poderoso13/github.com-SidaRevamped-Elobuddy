using System;

namespace Loader
{
    class Loader
    {
        static void Main(string[] args)
        {
            EloBuddy.SDK.Events.Loading.OnLoadingComplete += Initialize;
        }
        static void Initialize(EventArgs args)
        {
            if (EloBuddy.Player.Instance.ChampionName == "Ryze")
            {
                Ryze.RyzeLoading();
            }
        }
    }
}
