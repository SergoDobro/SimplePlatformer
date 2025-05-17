using Simple_Platformer.Game.MainGame.GameObjectSystem;
using System;

namespace SDLibTemplate_v11.Game.MainGame
{
    public class AIModel
    {
        LevelData levelData;
        public void Act(Player player)
        {
            var choice = Random.Shared.NextSingle();
            if (choice<0.5)
            {
                player.ButtonUpPressed();

            }
            else if (choice<0.75)
            {
                player.ButtonRightPressed();

            }
            else 
            {
                player.ButtonLeftPressed();

            }
        }
    }
}