
using System.Linq;
using System.Threading;
using SDLibTemplate_v11.Game.Menu;
using SDMonoLibUtilits.Scenes.GUI;
using SDMonoUI.UI.Elements;

using var game = new SDMonoLibUtilits.RootScene();
game.Root_scene = new MainMenu();

new Thread(() =>
{
    Thread.Sleep(1000);
    //Write custom game interactions for automatic experiments
}).Start();

game.Run();

