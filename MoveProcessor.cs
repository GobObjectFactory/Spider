﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spider
{
    public class MoveProcessor : GameHelper
    {
        public MoveProcessor(Game game)
            : base(game)
        {
        }

        public void ProcessMove(Move move)
        {
            if (RecordComplex)
            {
                AddMove(move);
            }

            if (ComplexMoves)
            {
                MakeMove(move);
            }
            else
            {
                ConvertToSimpleMoves(move);
            }
        }

        public void ConvertToSimpleMoves(Move move)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("CTSM: {0}", move);
            }

            // First move to the holding piles.
            Stack<Move> moveStack = new Stack<Move>();
            for (int holdingNext = move.HoldingNext; holdingNext != -1; holdingNext = HoldingList[holdingNext].Next)
            {
                HoldingInfo holding = HoldingList[holdingNext];
                int undoFromRow = Tableau[holding.To].Count;
                MakeMoveUsingEmptyPiles(holding.From, holding.FromRow, holding.To);
                moveStack.Push(new Move(holding.To, undoFromRow, holding.From == move.From ? move.To : move.From));
            }
            if (move.Type == MoveType.CompositeSinglePile)
            {
                // Composite single pile move.
                MakeCompositeSinglePileMove(move.Next);
            }
            else if (move.Type == MoveType.Swap)
            {
                // Swap move.
                SwapUsingEmptyPiles(move.From, move.FromRow, move.To, move.ToRow);
            }
            else
            {
                // Ordinary move.
                MakeMoveUsingEmptyPiles(move.From, move.FromRow, move.To);
            }

            // Lastly move from the holding piles, if we still can.
            while (moveStack.Count > 0)
            {
                TryToMakeMoveUsingEmptyPiles(moveStack.Pop());
            }
        }

        public void MakeMove(Move move)
        {
            if (move.Next != -1)
            {
                for (int next = move.Next; next != -1; next = SupplementaryList[next].Next)
                {
                    Move subMove = SupplementaryList[next];
                    MakeSingleMove(subMove);
                }
                return;
            }
            MakeSingleMove(move);
        }

        private void SwapUsingEmptyPiles(int from, int fromRow, int to, int toRow)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("SWUEP: {0}/{1} -> {2}/{3}", from, fromRow, to, toRow);
            }
            int emptyPiles = FindEmptyPiles();
            int fromSuits = Tableau.CountSuits(from, fromRow);
            int toSuits = Tableau.CountSuits(to, toRow);
            if (fromSuits == 0 && toSuits == 0)
            {
                return;
            }
            if (fromSuits + toSuits - 1 > ExtraSuits(emptyPiles))
            {
                throw new InvalidMoveException("insufficient empty piles");
            }
            Stack<Move> moveStack = new Stack<Move>();
            for (int n = emptyPiles; n > 0 && fromSuits + toSuits > 1; n--)
            {
                if (fromSuits >= toSuits)
                {
                    int moveSuits = toSuits != 0 ? fromSuits : fromSuits - 1;
                    fromSuits -= MoveOffUsingEmptyPiles(from, fromRow, to, moveSuits, n, moveStack);
                }
                else
                {
                    int moveSuits = fromSuits != 0 ? toSuits : toSuits - 1;
                    toSuits -= MoveOffUsingEmptyPiles(to, toRow, from, moveSuits, n, moveStack);
                }
            }
            if (fromSuits + toSuits != 1 || fromSuits * toSuits != 0)
            {
                throw new Exception("bug: left over swap runs");
            }
            if (fromSuits == 1)
            {
                MakeSimpleMove(from, fromRow, to);
            }
            else
            {
                MakeSimpleMove(to, toRow, from);
            }
            while (moveStack.Count != 0)
            {
                Move move = moveStack.Pop();
                MakeSimpleMove(move.From, move.FromRow, move.To);
            }
        }

        private void UnloadToEmptyPiles(int from, int lastFromRow, int to, Stack<Move> moveStack)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("ULTEP: {0}/{1} -> {2}", from, lastFromRow, to);
            }
            int emptyPiles = FindEmptyPiles();
            int suits = Tableau.CountSuits(from, lastFromRow);
            if (suits > ExtraSuits(emptyPiles))
            {
                throw new InvalidMoveException("insufficient empty piles");
            }
            int totalSuits = Tableau.CountSuits(from, lastFromRow);
            int remainingSuits = totalSuits;
            int fromRow = Tableau[from].Count;
            for (int n = 0; n < emptyPiles; n++)
            {
                int m = Math.Min(emptyPiles, n + remainingSuits);
                for (int i = m - 1; i >= n; i--)
                {
                    int runLength = Tableau.GetRunUp(from, fromRow);
                    fromRow -= runLength;
                    fromRow = Math.Max(fromRow, lastFromRow);
                    MakeSimpleMove(from, -runLength, EmptyPiles[i]);
                    moveStack.Push(new Move(EmptyPiles[i], -runLength, to));
                    remainingSuits--;
                }
                for (int i = n + 1; i < m; i++)
                {
                    int runLength = Tableau[EmptyPiles[i]].Count;
                    MakeSimpleMove(EmptyPiles[i], -runLength, EmptyPiles[n]);
                    moveStack.Push(new Move(EmptyPiles[n], -runLength, EmptyPiles[i]));
                }
                if (remainingSuits == 0)
                {
                    break;
                }
            }
        }

        private int MoveOffUsingEmptyPiles(int from, int lastFromRow, int to, int remainingSuits, int n, Stack<Move> moveStack)
        {
            int suits = Math.Min(remainingSuits, n);
            if (Diagnostics)
            {
                Utils.WriteLine("MOUEP: {0} -> {1}: {2}", from, to, suits);
            }
            for (int i = n - suits; i < n; i++)
            {
                // Move as much as possible but not too much.
                Pile fromPile = Tableau[from];
                int fromRow = fromPile.Count - Tableau.GetRunUp(from, fromPile.Count);
                if (fromRow < lastFromRow)
                {
                    fromRow = lastFromRow;
                }
                int runLength = fromPile.Count - fromRow;
                MakeSimpleMove(from, -runLength, EmptyPiles[i]);
                moveStack.Push(new Move(EmptyPiles[i], -runLength, to));
            }
            for (int i = n - 2; i >= n - suits; i--)
            {
                int runLength = Tableau[EmptyPiles[i]].Count;
                MakeSimpleMove(EmptyPiles[i], -runLength, EmptyPiles[n - 1]);
                moveStack.Push(new Move(EmptyPiles[n - 1], -runLength, EmptyPiles[i]));
            }
            return suits;
        }

        private void MakeCompositeSinglePileMove(int first)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("MCSPM");
            }
            bool aborted = false;
            int offloadPile = -1;
            Stack<Move> moveStack = new Stack<Move>();
            for (int next = first; next != -1; next = SupplementaryList[next].Next)
            {
                int emptyPiles = FindEmptyPiles();
                Move move = Tableau.Normalize(SupplementaryList[next]);
                if (move.Type == MoveType.Unload)
                {
                    offloadPile = move.To;
                    UnloadToEmptyPiles(move.From, move.FromRow, -1, moveStack);
                }
                else if (move.Type == MoveType.Reload)
                {
                    if (Diagnostics)
                    {
                        Utils.WriteLine("RL:");
                    }
                    while (moveStack.Count != 0)
                    {
                        Move subMove = moveStack.Pop();
                        int to = subMove.To != -1 ? subMove.To : move.To;
                        MakeSimpleMove(subMove.From, subMove.FromRow, to);
                    }
                    offloadPile = -1;

                }
                else if (move.Flags.UndoHolding())
                {
                    TryToMakeMoveUsingEmptyPiles(move);
                }
                else
                {
                    if (!TryToMakeMoveUsingEmptyPiles(move))
                    {
                        // Things got messed up due to a discard.  There might
                        // be another pile with the same target.
                        bool foundAlternative = false;
                        Pile fromPile = Tableau[move.From];
                        if (move.From >= 0 && move.From < fromPile.Count)
                        {
                            Card fromCard = fromPile[move.FromRow];
                            for (int to = 0; to < NumberOfPiles; to++)
                            {
                                if (to == move.From)
                                {
                                    continue;
                                }
                                Pile toPile = Tableau[to];
                                if (toPile.Count == 0)
                                {
                                    continue;
                                }
                                if (!fromCard.IsSourceFor(toPile[toPile.Count - 1]))
                                {
                                    continue;
                                }
                                if (TryToMakeMoveUsingEmptyPiles(new Move(move.From, move.FromRow, to)))
                                {
                                    foundAlternative = true;
                                }
                                break;
                            }
                        }
                        if (!foundAlternative)
                        {
                            // This move is hopelessly messed up.
                            aborted = true;
                            break;
                        }
                    }
                }
            }
            if (!aborted && moveStack.Count != 0)
            {
                throw new Exception("missing reload move");
            }
        }

        private bool TryToMakeMoveUsingEmptyPiles(Move move)
        {
            if (Diagnostics)
            {
                Utils.WriteLine("TTMMUEP: {0}/{1} -> {2}", move.From, move.FromRow, move.To);
            }
            if (SimpleMoveIsValid(move))
            {
                if (SafeMakeMoveUsingEmptyPiles(move.From, move.FromRow, move.To) == null)
                {
                    return true;
                }
            }
            if (Diagnostics)
            {
                Utils.WriteLine("*** failed to make move ***");
            }
            return false;
        }

        private bool SimpleMoveIsValid(Move move)
        {
            return Tableau.MoveIsValid(move);
        }

        private void MakeMovesUsingEmptyPiles(int first)
        {
            for (int next = first; next != -1; next = SupplementaryList[next].Next)
            {
                Move move = SupplementaryList[next];
                MakeMoveUsingEmptyPiles(move.From, move.FromRow, move.To);
            }
        }

        private void MakeMoveUsingEmptyPiles(int from, int lastFromRow, int to)
        {
            string error = SafeMakeMoveUsingEmptyPiles(from, lastFromRow, to);
            if (error != null)
            {
                throw new InvalidMoveException(error);
            }
        }

        private string SafeMakeMoveUsingEmptyPiles(int from, int lastFromRow, int to)
        {
            if (lastFromRow < 0)
            {
                lastFromRow += Tableau[from].Count;
            }
            if (Diagnostics)
            {
                Utils.WriteLine("MMUEP: {0}/{1} -> {2}", from, lastFromRow, to);
            }
            int toRow = Tableau[to].Count;
            int extraSuits = Tableau.CountSuits(from, lastFromRow) - 1;
            if (extraSuits < 0)
            {
                return "not a single run";
            }
            if (extraSuits == 0)
            {
                MakeSimpleMove(from, lastFromRow, to);
                return null;
            }
            int emptyPiles = FindEmptyPiles();
            PileList usableEmptyPiles = new PileList(EmptyPiles);
            if (toRow == 0)
            {
                usableEmptyPiles.Remove(to);
                emptyPiles--;
            }
            int maxExtraSuits = ExtraSuits(emptyPiles);
            if (extraSuits > maxExtraSuits)
            {
                return "insufficient empty piles";
            }
            int suits = 0;
            int fromRow = Tableau[from].Count;
            Stack<Move> moveStack = new Stack<Move>();
            for (int n = emptyPiles; n > 0; n--)
            {
                for (int i = 0; i < n; i++)
                {
                    int runLength = Tableau.GetRunUp(from, fromRow);
                    fromRow -= runLength;
                    MakeSimpleMove(from, -runLength, usableEmptyPiles[i]);
                    moveStack.Push(new Move(usableEmptyPiles[i], -runLength, to));
                    suits++;
                    if (suits == extraSuits)
                    {
                        break;
                    }
                }
                if (suits == extraSuits)
                {
                    break;
                }
                for (int i = n - 2; i >= 0; i--)
                {
                    int runLength = Tableau[usableEmptyPiles[i]].Count;
                    MakeSimpleMove(usableEmptyPiles[i], -runLength, usableEmptyPiles[n - 1]);
                    moveStack.Push(new Move(usableEmptyPiles[n - 1], -runLength, usableEmptyPiles[i]));
                }
            }
            MakeSimpleMove(from, lastFromRow, to);
            while (moveStack.Count != 0)
            {
                Move move = moveStack.Pop();
                MakeSimpleMove(move.From, move.FromRow, move.To);
            }
            return null;
        }

        private void MakeSimpleMove(int from, int fromRow, int to)
        {
            if (fromRow < 0)
            {
                fromRow += Tableau[from].Count;
            }
            if (Diagnostics)
            {
                Utils.WriteLine("    MSM: {0}/{1} -> {2}", from, fromRow, to);
            }
            Debug.Assert(Tableau[from].Count != 0);
            Debug.Assert(fromRow < Tableau[from].Count);
            Debug.Assert(Tableau.CountSuits(from, fromRow) == 1);
            Debug.Assert(Tableau[to].Count == 0 || Tableau[from][fromRow].IsSourceFor(Tableau[to][Tableau[to].Count - 1]));
            MakeMove(new Move(from, fromRow, to, Tableau[to].Count));
        }

        private void MakeSingleMove(Move move)
        {
            // Record the move.
            if (!RecordComplex)
            {
                AddMove(move);
            }

            // Make the move.
            Tableau.Move(move);
        }

        public void AddMove(Move move)
        {
            move.Score = 0;
            if (TraceMoves)
            {
                Utils.WriteLine("Move {0}: {1}", Moves.Count, move);
            }
            Moves.Add(move);
        }

    }
}
