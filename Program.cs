﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    class Program
    {
        static void Main(string[] args)
        {
            bool evaluate = false;
            Player player = new Player();
            int i = 0;
            while (i < args.Length)
            {
                string arg = args[i];
                if (arg == "--threads")
                {
                    player.Threads = int.Parse(args[i + 1]);
                    i += 2;
                    continue;
                }
                if (arg == "--games")
                {
                    player.Games = int.Parse(args[i + 1]);
                    i += 2;
                    continue;
                }
                if (arg == "--seed")
                {
                    player.Seed = int.Parse(args[i + 1]);
                    i += 2;
                    continue;
                }
                if (arg == "--suits")
                {
                    player.Suits = int.Parse(args[i + 1]);
                    i += 2;
                    continue;
                }
                if (arg == "--coefficient")
                {
                    player.Coefficient = int.Parse(args[i + 1]);
                    i += 2;
                    evaluate = true;
                    continue;
                }
                if (arg == "--trace")
                {
                    player.TraceStartFinish = true;
                    player.TraceDeals = true;
                    player.TraceMoves = true;
                    i++;
                    continue;
                }
                if (arg == "--simple")
                {
                    player.SimpleMoves = true;
                    i++;
                    continue;
                }
                if (arg == "--diagnostics")
                {
                    player.Diagnostics = true;
                    i++;
                    continue;
                }
                if (arg == "--show_results")
                {
                    player.ShowResults = true;
                    i++;
                    continue;
                }
                if (arg.Substring(0, 2) == "--")
                {
                    Console.WriteLine("invalid argument: " + arg);
                    return;
                }
                break;
            }
            if (i != args.Length)
            {
                Console.WriteLine("extra argument: " + args[i]);
                return;
            }
            if (evaluate)
            {
                player.EvaluateCoefficient();
            }
            else
            {
                player.Play();
            }
        }
    }
}
