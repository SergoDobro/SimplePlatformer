using ClassikNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Platformer.Game.MainGame.AI
{
    internal class PlatformerBrain : ClassikNet.ClassicNet
    {
        public override bool UseFastDefiner { get; } = true;
        public override string[] DefineOutput_Fast()
        {
            return new string[] {
                    "out_left",
                    "out_up",
                    "out_right",
                    //"learnRate",
                    //"memCell1",
                    //"memCell2",
            };
        }
        public override string[] DefineInput_Fast()
        {
            return new string[]
                {
                    "positionX",
                    "positionY",
                    "distToTopX",
                    "distToTopY",
                    "distToBotX",
                    "distToBotY",
                    "distToTopTopX",
                    "distToTopTopY",
                    "velX",
                    "velY",
                    "CanJump",
                    //"memCell",
                    //"memCell1",
                    //"memCell2",
                };
        }

        public override ClassicNet GetCloningInstance()
        {
            return new PlatformerBrain();

        }

    }
}
