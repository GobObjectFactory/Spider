﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider
{
    public class GameHelper : BaseGame, IGame
    {
        protected IGame game;

        public GameHelper(IGame game)
        {
            this.game = game;
        }

        #region IGameSettings Members

        public Variation Variation { get { return game.Variation; } set { game.Variation = value; } }
        public int Seed { get { return game.Seed; } set { game.Seed = value; } }
        public double[] Coefficients { get { return game.Coefficients; } set { game.Coefficients = value; } }
        public bool Diagnostics { get { return game.Diagnostics; } set { game.Diagnostics = value; } }
        public bool Interactive { get { return game.Interactive; } set { game.Interactive = value; } }

        public bool TraceMoves { get { return game.TraceMoves; } set { game.TraceMoves = value; } }
        public bool ComplexMoves { get { return game.ComplexMoves; } set { game.ComplexMoves = value; } }

        #endregion

        #region IGame Members

        public Pile Shuffled { get { return game.Shuffled; } }
        public Tableau Tableau { get { return game.Tableau; } }
        public Tableau FindTableau { get { return game.FindTableau; } }
        public int NumberOfPiles { get { return game.NumberOfPiles; } }
        public int NumberOfSuits { get { return game.NumberOfSuits; } }

        public MoveList UncoveringMoves { get { return game.UncoveringMoves; } }
        public MoveList SupplementaryMoves { get { return game.SupplementaryMoves; } }
        public MoveList SupplementaryList { get { return game.SupplementaryList; } }
        public MoveList Candidates { get { return game.Candidates; } }
        public PileList OneRunPiles { get { return game.OneRunPiles; } }

        public void FindMoves(Tableau tableau)
        {
            game.FindMoves(tableau);
        }

        public void ProcessCandidate(Move move)
        {
            game.ProcessCandidate(move);
        }

        public void ProcessMove(Move move)
        {
            game.ProcessMove(move);
        }

        public int AddSupplementary()
        {
            return game.AddSupplementary();
        }

        public int FindHolding(IGetCard map, HoldingStack holdingStack, bool inclusive, Pile fromPile, int from, int fromStart, int fromEnd, int to, int maxExtraSuits)
        {
            return game.FindHolding(map, holdingStack, inclusive, fromPile, from, fromStart, fromEnd, to, maxExtraSuits);
        }

        public void PrintGame()
        {
            game.PrintGame();
        }

        public static void PrintGame(Game other)
        {
            Game.PrintGame(other);
        }

        public void PrintGames()
        {
            game.PrintGames();
        }

        public void PrintMoves()
        {
            game.PrintMoves();
        }

        public void PrintMoves(MoveList moves)
        {
            game.PrintMoves(moves);
        }

        public void PrintCandidates()
        {
            game.PrintCandidates();
        }

        public void PrintViableCandidates()
        {
            game.PrintViableCandidates();
        }

        public void PrintMove(Move move)
        {
            game.PrintMove(move);
        }

        #endregion
    }
}
