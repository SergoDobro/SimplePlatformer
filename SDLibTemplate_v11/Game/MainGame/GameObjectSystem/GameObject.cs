using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.ComponentModel.Design;
using SDMonoLibUtilits.Scenes;
using SDLibTemplate_v11.Game.MainGame;
using Simple_Platformer.Game.MainGame.Components;
using GameComponent = SDLibTemplate_v11.Game.MainGame.GameComponent;

namespace Simple_Platformer.Game.MainGame.GameObjectSystem
{
    public class GameObject : Scene
    {

        public Vector2 Position { get; set; }
        public float Rotation { get; set; }

        public Rigidbody RigidBody { get; set; }
        public List<GameComponent> gameComponents { get; set; } = new List<GameComponent>();
        public GameComponent GetComponent<T>()
        {
            foreach (var component in gameComponents)
            {
                if (component is T)
                {
                    return component;
                }
            }
            return null;
        }
        public GameObject AddComponent(GameComponent gameComponent)
        {
            gameComponents.Add(gameComponent);
            if (gameComponent is Rigidbody)
            {
                RigidBody = gameComponent as Rigidbody;
            }
            if (gameComponent is IRegistable)
            {

                (gameComponent as IRegistable).Register();
            }
            //if (gameComponent is IGlobalRegisterable<gameComponent.GetType()>)
            //{

            //    (gameComponent as IGlobalRegisterable).Register();
            //}
            
            gameComponent.gameObject = this;
            return this;
        }
        public void Destroy()
        {
            foreach (var item in gameComponents)
            {
                if (item is IRegistable)
                {
                    (item as IRegistable).GameObjectDestroyed();
                }
            }
            if (myGameObjectUpdater is not null)
                myGameObjectUpdater.DestroyGameObject(this);

        }
        // TODO: Add a 'Category' and 'Id' property here if we need to map objects to dictionaries
        // [JsonIgnore] // Uncomment if adding these later
        // public string Category { get; set; } = "Misc";
        //public string Id { get; set; } = Guid.NewGuid().ToString();

        public GameObjectUpdater myGameObjectUpdater = null;
        public GameObject Instantiate(GameObject gameObject, string gameObjectUpdater_name = "main")
        {
            if (GameObjectUpdater.gameObjectUpdater.ContainsKey(gameObjectUpdater_name))
                GameObjectUpdater.gameObjectUpdater[gameObjectUpdater_name].InstantiateGameObject(gameObject);
            else
                GameObjectUpdater.gameObjectUpdater.Add(gameObjectUpdater_name, new GameObjectUpdater());


            myGameObjectUpdater = GameObjectUpdater.gameObjectUpdater[gameObjectUpdater_name];
            return gameObject;
        }
    }
    public class GameObjectUpdater : ComplexScene
    {
        public static Dictionary<string, GameObjectUpdater> gameObjectUpdater = new Dictionary<string, GameObjectUpdater>();

        public void InstantiateGameObject(GameObject gameObject)
        {
            scenes.Add(gameObject);
        }
        public void DestroyGameObject(GameObject gameObject)
        {
            if (scenes.Contains(gameObject))
                scenes.Remove(gameObject);
        }


    }
}