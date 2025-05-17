namespace SDLibTemplate_v11.Game.MainGame
{
    public interface IRegistable
    {
        public void Register();
        public void GameObjectDestroyed();
        public void ResetGlobal(); //to reset global instance holder, if any
    }
}