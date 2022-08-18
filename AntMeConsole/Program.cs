using AntMe.PlayerManagement;
using AntMe.Plugin.Simulation;
using AntMe.SharedComponents.Plugin;
using AntMe.SharedComponents.States;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntMeConsole
{
    class Program
    {
        static void Main(string[] parameter)
        {
            string folder = parameter.FirstOrDefault(param => param.ToUpper().StartsWith("/FOLDER"));
            if (folder == null)
                throw new Exception("No /folder provided");

            var path = folder.Substring(8).Trim();
            var files = new List<string>();
            foreach (String file in Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly))
            {
                var full = Path.GetFullPath(file);
                PlayerStore.Instance.RegisterFile(full);
                files.Add(full.ToLower());
            }
            files = files.OrderBy(i=>i).ToList();

            Console.Write("|  ");

            foreach (String fileX in files)
                Console.Write("| {0} ", PlayerStore.Instance.KnownPlayer.FirstOrDefault(i => i.File == fileX).ColonyName);

            Console.Write("|\n");

            Console.Write("| :--- ");

            foreach (String fileX in files)
                Console.Write(" | :--- ");

            Console.Write("|\n");

            float[,] scores = new float[files.Count, files.Count];
            int[,] points = new int[files.Count, files.Count];
            var rounds = 5;
            Random random = new Random();
            for (var y = 0; y < files.Count; y++)
            {
                Console.Write("| {0} ", PlayerStore.Instance.KnownPlayer.FirstOrDefault(i => i.File == files[y]).ColonyName);
                for (var x = 0; x < files.Count; x++) 
                {
                    if (x == y)
                        Console.Write("| X ");
                    else if (x < y)
                        Console.Write("| {0}-{1} ", scores[x, y], (float)rounds - scores[x, y]);
                    else
                    {
                        for (var r = 0; r < rounds; r++)
                        {
                            FreeGamePlugin plugin = new FreeGamePlugin(false);
                            plugin.StartupParameter(new string[] { "/file=" + files[y], "/file=" + files[x] });
                            plugin.Start();

                            SimulationState current = null;
                            while (plugin.State == PluginState.Running)
                            {
                                SimulationState simulationState = new SimulationState();
                                plugin.CreateState(ref simulationState);
                                current = simulationState;
                            }
                            var pointsX = current.ColonyStates[0].Points;
                            var pointsY = current.ColonyStates[1].Points;
                            points[x, y] += pointsX;
                            points[y, x] += pointsY;
                            if (pointsX == pointsY)
                                scores[x, y] += 0.5f;
                            else if (pointsX > pointsY)
                                scores[x, y] += 1;
                        }
                        scores[y, x] = (float)rounds - scores[x, y];
                        Console.Write("| {0}-{1} ", scores[x, y], (float)rounds - scores[x, y]);

                    }
                }
                Console.Write("|\n");
            }

            Console.Write("| Wins ");
            for(var x = 0;x <files.Count;x++){
                float score = 0;
                for (var y = 0; y < files.Count; y++)
                    score += scores[x, y];
                Console.Write("| {0}/{1} ", score, (files.Count - 1)* rounds);
            }

            Console.Write("|\n");

            Console.Write("| Avarage Points ");
            for (var x = 0; x < files.Count; x++)
            {
                int p = 0;
                for (var y = 0; y < files.Count; y++)
                    p += points[x, y];
                Console.Write("| {0} ", files.Count > 1 ? p / (files.Count - 1) / rounds : 0);
            }

            Console.Write("|\n");
        }
    }
}
