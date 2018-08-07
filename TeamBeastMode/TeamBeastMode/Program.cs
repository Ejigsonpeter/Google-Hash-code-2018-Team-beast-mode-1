using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeamBeastMode
{
    class Program
    {
        
       private static readonly string[] ReadFile =
        {
           @"c:\hashcode\a_example.in",
            @"c:\hashcode\b_should_be_easy.in",
            @"c:\hashcode\c_no_hurry.in",
            @"c:\hashcode\d_metropolis.in",
            @"c:\hashcode\e_high_bonus.in"
        };

        static void Main(string[] args)
        {
            int totalScore = 0;
            foreach (string fileName in ReadFile)
            {
                System.Console.Write("Processing: {0}", fileName);
                Solver solver = new SolverByCarTime();
                Solver solver2 = new SolverByCar();
                solver.Load(fileName);
                solver2.Load(fileName);

                System.Console.Write(", Max Possible: {0}", solver.CalcMaxPossibleScore());

                long startTicks = DateTime.Now.Ticks;
                solver.Solve();
                solver2.Solve();
                System.Console.Write(", Run time: {0}", new TimeSpan(DateTime.Now.Ticks - startTicks));

                Solver bestSolver;
                if (solver.CalculateScore() > solver2.CalculateScore())
                    bestSolver = solver;
                else
                    bestSolver = solver2;

                int score = bestSolver.CalculateScore();
                bestSolver.WriteOutput(fileName + ".out");
                System.Console.Write(", Score: {0}", score);
                totalScore += score;

                System.Console.WriteLine();
            }

            System.Console.WriteLine("Total Score: {0}", totalScore);
        }
    }
}
