using SDMonoUI.UI.Elements;
using Simple_Platformer.Game.MainGame.Components;

namespace Simple_Platformer.Game.MainGame.GameObjectSystem
{
    public class Flag : GameObject
    {
        public Flag()
        {
            AddComponent(new Texture2DComponent()
            {
                textureName = "flag",
                rectangle = new RectangleF(10, 10)
            });


        }
        public override void Update(float dt)
        {
            base.Update(dt);
        }

    }
     
}