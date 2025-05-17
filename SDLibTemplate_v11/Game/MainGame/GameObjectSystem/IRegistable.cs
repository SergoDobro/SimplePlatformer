namespace Simple_Platformer.Game.MainGame.GameObjectSystem
{
    public interface IRegistable
    {
        public void Register();
        public void GameObjectDestroyed();
        public void ResetGlobal(); //to reset global instance holder, if any
    }
}