using GameLogic;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.ComponentModel.Design;
using SDMonoLibUtilits.Scenes;

namespace SDLibTemplate_v11.Game.MainGame
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
        }
        // TODO: Add a 'Category' and 'Id' property here if we need to map objects to dictionaries
        // [JsonIgnore] // Uncomment if adding these later
        // public string Category { get; set; } = "Misc";
        //public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}