using ClassikNet;
using Simple_Platformer.Game.MainGame.AI;
using Simple_Platformer.Game.MainGame.GameObjectSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SDLib_Experiments.Game.MainGame
{
    public class Selector
    {

        public List<Chamber> chamberList;
        public void Init(object data, int count = 200, int[] netType = null)
        {
            chamberList = new List<Chamber>();

            for (int i = 0; i < count; i++)
            {
                chamberList.Add(new Chamber());
                chamberList.Last().classicNet = new PlatformerBrain();
                if (netType is null)
                    chamberList.Last().classicNet.Init(new int[] { 11, 5, 5, 5, 4 });
                else
                    chamberList.Last().classicNet.Init(netType);
                CreateChamberSpecificData(data, i, chamberList.Last());
            }
            foreach (var item in chamberList)
                item.Init();

        }
        public virtual void CreateChamberSpecificData(object generaldata, int chamberId, Chamber chamber)
        {
        }
        public CancellationToken? cts;
        bool debug_one_thread = true;
        public void Tick()
        {
            if (debug_one_thread)
            {
                for (int i = 0; i < chamberList.Count; i++)
                {
                    try
                    {
                        if (i == 25)
                        {

                        }
                        chamberList[i].Tick();

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(i);
                    }
                }
                return;
            }
            Task[] ths = new Task[chamberList.Count];
            for (int i = 0; i < chamberList.Count; i++)
            {
                // Capture the current value of 'i' in a loop-local variable 'j'
                int j = i; // Fix: Create a local copy for each iteration
                if (cts is null)
                {

                    ths[j] = new Task(() =>
                    {
                        chamberList[j].Tick(); // Now 'j' is stable per task
                    });
                    ths[j].Start();
                }
                else
                {

                    ths[j] = new Task(() =>
                    {
                        chamberList[j].Tick(); // Now 'j' is stable per task
                    }, cts.Value);
                    if (!ths[j].IsCompleted)
                        ths[j].Start();
                }
            }
            foreach (var item in ths)
                item.Wait();
        }
        protected int original_count;
        public float portion_of_unkillable;
        public float deathEnchancer;
        protected int number_of_unkillable;
        protected int number_of_killed;
        public virtual void UpdateParametrs(List<Chamber> sorted_chambers)
        {

            original_count = (int)(sorted_chambers.Count);
            portion_of_unkillable = 0.00f;
            number_of_unkillable = (int)(sorted_chambers.Count * portion_of_unkillable);
            number_of_killed = (int)(sorted_chambers.Count * (1 - portion_of_unkillable));
            deathEnchancer = 10;

        }
        public virtual void Select()
        {
            var resList = chamberList.OrderByDescending(x => x.Evaluate()).ToList();
            
            //count stats
            BestInGenerationScore = resList[0].score;
            AverageInGenerationScore = resList.Sum(x => x.score) / (float)resList.Count;

            //params
            UpdateParametrs(resList); 

            List<Player> freePlayers = new List<Player>(); 
            for (int i = (int)(resList.Count * portion_of_unkillable); i < resList.Count; i++)
            {
                if (deathEnchancer * i / (float)resList.Count > Random.Shared.NextDouble())
                {
                    freePlayers.Add(resList[i].player);
                    resList.RemoveAt(i);
                    i--;
                }
            }
            int newBorns = chamberList.Count - resList.Count;
            if (newBorns > 0)
            {
                //resList.Add(resList[0].CloneChamber());
                //for (int i = 0; i < newBorns/2 && newBorns>0; i+=2)
                //{
                //    resList.Add(Chamber.CrossingOverChamber(ClassicNet.Crossingover(resList[0 + i].classicNet, resList[1 + i].classicNet)));
                //    newBorns--;
                //}
            }
            for (int i = 0; i < newBorns; i++)
            {
                if (i >= resList.Count)
                {
                    newBorns -= resList.Count;
                    i = 0;
                }
                resList.Add(resList[i].CloneChamber());
                resList.Last().player = freePlayers[i];

                //freePlayers.Add(resList[i].player);
            }
            chamberList.Clear();
            chamberList = resList;
            foreach (var item in chamberList)
            {
                item.Init();
            }

        }
        public float BestInGenerationScore;
        public float AverageInGenerationScore;
        public Chamber BestChamber()
        {
            return chamberList.OrderByDescending(x => x.Evaluate()).First();
        }

    }
}
