﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Spider.Tests
{
    [TestClass]
    public class Tests
    {
        private Game game = null;

        [TestMethod]
        public void InstantiationTest()
        {
            game = new Game();
        }

        [TestMethod]
        public void SerializationTest()
        {
            string data1 = @"
                @2||KhTh3s5h9s-Ah-5hKsAs7sKs-Jh-7sKs8s8h-9hJh--6s3hQh-7s9s8h
                Jh-9s3hJh4s|7h6h5h4h3h2hAh-2s-7h6s5s4s-KhQsJsTs9s-2sAs-Th-Js
                Ts-2h-Kh-2s|9hTsAs9h3sQsJs5sTh4s8s3sQh9h8h2s5hQsAhTh3s4s5s2h
                8sAh7h6h6s4h4h8hQh5sQsTsAs7sKh2h6hKs8s4hQhJs6s3h6h7h@
            ";
            Game game1 = new Game(data1);
            Game game2 = new Game(game1.ToAsciiString());
            string data2 = game2.ToAsciiString();
            Assert.AreEqual(TrimAll(data2), TrimAll(data1));
        }

        [TestMethod]
        public void EmptyTest1()
        {
            // No cards: we win.
            string data = "@2||||@";
            game = new Game(data);
            Assert.IsTrue(game.MakeMove());
        }

        [TestMethod]
        public void EmptyTest2()
        {
            // No useful move: we lose.
            string data = "@2|||AS|@";
            game = new Game(data);
            Assert.IsFalse(game.MakeMove());
        }

        [TestMethod]
        public void UndoTest1()
        {
            // Undo an ordinary move.
            string data1 = "@2|||9s8h-9h-Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||9s-9h8h-Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckUndo(data1, data2, "Move");
        }

        [TestMethod]
        public void UndoTest2()
        {
            // Undo an ordinary move that turns over a card.
            string data1 = "@2||As|8h-9h-Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||As-9h8h-Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckUndo(data1, data2, "Move");
        }

        [TestMethod]
        public void UndoTest3()
        {
            // Undo an ordinary move that causes a discard.
            string data1 = "@2|||8h7h6h5h4h3h2hAh-KhQhJhTh9h-Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|Ah||--Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckUndo(data1, data2, "Move");
        }

        [TestMethod]
        public void UndoTest4()
        {
            // Undo an ordinary move that causes a discard and turns over a card.
            string data1 = "@2||-7s|8h7h6h5h4h3h2hAh-KhQhJhTh9h-Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|Ah||-7s-Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckUndo(data1, data2, "Move");
        }

        [TestMethod]
        public void UndoTest5()
        {
            // Undo a deal.
            string data1 = "@2|||8h-9h-Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|As2s3s4s5s6s7s8s9sTs@";
            string data2 = "@2|||8hTs-9h9s-Ks8s-Ks7s-Ks6s-Ks5s-Ks4s-Ks3s-Ks2s-KsAs|@";
            CheckUndo(data1, data2, "Deal");
        }

        [TestMethod]
        public void UndoTest6()
        {
            // Undo a deal that causes a discard.
            string data1 = "@2|||8h-9h-Ks-Ks-Ks-Ks-Ks-Ks-Ks-KhQhJhTh9h8h7h6h5h4h3h2h|Ah2s3s4s5s6s7s8s9sTs@";
            string data2 = "@2|Ah||8hTs-9h9s-Ks8s-Ks7s-Ks6s-Ks5s-Ks4s-Ks3s-Ks2s|@";
            CheckUndo(data1, data2, "Deal");
        }

        [TestMethod]
        public void UndoTest7()
        {
            // Undo a deal that causes a discard and turns over a card.
            string data1 = "@2||---------7s|8h-9h-Ks-Ks-Ks-Ks-Ks-Ks-Ks-KhQhJhTh9h8h7h6h5h4h3h2h|Ah2s3s4s5s6s7s8s9sTs@";
            string data2 = "@2|Ah||8hTs-9h9s-Ks8s-Ks7s-Ks6s-Ks5s-Ks4s-Ks3s-Ks2s-7s|@";
            CheckUndo(data1, data2, "Deal");
        }

        [TestMethod]
        public void SwapTest1()
        {
            // A 1/1 swap move, 1 space.
            string data1 = "@2|||9s8h-9h8s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||9s8s-9h8h--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void SwapTest2()
        {
            // A 1/1 whole pile swap move, 1 space.
            string data1 = "@2|||8h-9h8s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s-9h8h--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void SwapTest3()
        {
            // A 1/3 swap move, 2 spaces.
            string data1 = "@2|||9s8h-9h8s7s6s5h---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||9s8s7s6s5h-9h8h---Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void SwapTest4()
        {
            // A 2/2 swap move, 2 spaces.
            string data1 = "@2|||Ts4h3s-5h9s8h---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||Ts9s8h-5h4h3s---Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void SwapTest5()
        {
            // A 2/2 whole pile swap move, 2 spaces.
            string data1 = "@2|||4h3s-5h9s8h---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||9s8h-5h4h3s---Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void SwapTest6()
        {
            // A 1/6 swap move, 3 spaces.
            string data1 = "@2|||Ts4h-5h9s8h7s6h5s4h----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||Ts9s8h7s6h5s4h-5h4h----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void SwapTest7()
        {
            // A 2/5 swap move, 3 spaces.
            string data1 = "@2|||Js4h3s-5hTs9h8s7h6s----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||JsTs9h8s7h6s-5h4h3s----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void SwapTest8()
        {
            // A 3/4 swap move, 3 spaces.
            string data1 = "@2|||Js4h3s2h-5hTs9h8s7h----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||JsTs9h8s7h-5h4h3s2h----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void SwapTest9()
        {
            // A 4/3 swap move, 3 spaces.
            string data1 = "@2|||5hTs9h8s7h-Js4h3s2h----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||5h4h3s2h-JsTs9h8s7h----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void SwapTest10()
        {
            // A 5/2 swap move, 3 spaces.
            string data1 = "@2|||5hTs9h8s7h6s-Js4h3s----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||5h4h3s-JsTs9h8s7h6s----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void SwapTest11()
        {
            // A 6/1 swap move, 3 spaces.
            string data1 = "@2|||5h9s8h7s6h5s4h-Ts4h----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||5h4h-Ts9s8h7s6h5s4h----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void SwapTest12()
        {
            // A 1/1 out-of-order swap move, 0 spaces, 1 holding pile.
            string data1 = "@2|||9s3h-4h8s-4s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||9s8s-4h3h-4s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMoveSucceeds(data1, data2);
        }

        [TestMethod]
        public void SwapTest13()
        {
            // A 1/1 in-order swap move, 0 spaces, 1 holding pile.
            string data1 = "@4|||4s3h-4h3s-4d-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@4|||4s3s-4h3h-4d-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMoveSucceeds(data1, data2);
        }

        [TestMethod]
        public void SwapTest14()
        {
            // A 1/1 in-order swap move, 0 spaces, 2 holding piles.
            string data1 = "@4|||4s3h-4h3s2h-4d-4c3c-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@4|||4s3s2h-4h3h-4d-4c3c-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMoveSucceeds(data1, data2);
        }

        [TestMethod]
        public void SwapTest15()
        {
            // A 2/2 out-of-order swap move, 1 spaces, 1 holding piles.
            string data1 = "@4|||As6h2hAs-As3h5h4s-4d3d--Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@4|||As6h5h4s-As3h2hAs-4d3d--Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMoveSucceeds(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest1()
        {
            // A 1/1 composite single pile move, 1 space.
            string data1 = "@2|||4s8s-5s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-5s4s-8s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest2()
        {
            // A 1/1 inversion move, 1 space.
            string data1 = "@2|||4s5s-8s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-8s-5s4s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest3()
        {
            // A 1/1/1 composite single pile move, 1 space.
            string data1 = "@2|||Ts3s2s6s-4s-Js--Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-4s3s2s-JsTs-6s-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest4()
        {
            // A 1/1/1 inversion move, 1 space.
            string data1 = "@2|||As2s3s--Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-3s2sAs-Ks-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest5()
        {
            // A 1/1/1 mixed composite single pile move, 1 space.
            string data1 = "@2|||5s2s3s-6s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-6s5s-3s2s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest6()
        {
            // A 1/1/1 inversion move, 1 space
            // with one holding pile.
            string data1 = "@2|||2s3s4s3s2h-5h4h--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-5h4h3s2h-4s3s2s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest7()
        {
            // A 1/1/1 inversion move, 1 space
            // with two holding piles.
            string data1 = "@2|||2s3s4s3h2s-4h3h-5s4s--Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-4h3h2s-5s4s3h-4s3s2s-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest8()
        {
            // A 1/2 composite single pile move, 2 spaces.
            string data1 = "@2|||As8s7h-2s---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-2sAs-8s7h--Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest9()
        {
            // A 1/3 composite single pile move, 2 spaces.
            string data1 = "@2|||As8s7h6s-2s---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s7h6s-2sAs---Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest10()
        {
            // A 2/2 composite single pile move, 2 spaces.
            string data1 = "@2|||Ts9h7h6s-Js---Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-JsTs9h-7h6s--Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest11()
        {
            // A 1/4 composite single pile move, three spaces.
            string data1 = "@2|||Ts8s7h6s5h-Js----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-JsTs-8s7h6s5h---Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest12()
        {
            // A 1/6 composite single pile move, three spaces.
            string data1 = "@2|||Ts8s7h6s5h4s3h-Js----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s7h6s5h4s3h-JsTs----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest13()
        {
            // A 2/5 composite single pile move, three spaces.
            string data1 = "@2|||9s8h8s7h6s5h4s-Ts----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||8s7h6s5h4s-Ts9s8h----Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest14()
        {
            // A 4/3 composite single pile move, three spaces.
            string data1 = "@2|||Ts9h8s7h8s7h6s-Js----Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-JsTs9h8s7h-8s7h6s---Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest15()
        {
            // A 1/1/1/1 composite single pile move with reused piles, 1 space.
            string data1 = "@2|||3s4s7s8s-5s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-5s4s3s-8s7s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest16()
        {
            // A 1/1 composite single pile move, 0 spaces.
            string data1 = "@2|||4s8s-5s-9s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-5s4s-9s8s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMoveSucceeds(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest17()
        {
            // A 1/1/1 composite single pile move with reused pile, 1 space.
            string data1 = "@2|||As3s2s6s-4s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-4s3s2sAs-6s-Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest18()
        {
            // A 1/1/1 partial composite single pile move, 1 space.
            string data1 = "@2|||TsAs5s3s4s-6s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||TsAs-6s5s4s3s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest19()
        {
            // A 1/1/1 reload to from composite single pile move, 1 space.
            string data1 = "@2|||Ts9s5s8s-6s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||Ts9s8s-6s5s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest20()
        {
            // A 1/1/1 interior holding composite single pile move, 1 space.
            string data1 = "@2|||8s5s4h9h-6s-6h5h--Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-6s5s4h-6h5h-9h8s-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest21()
        {
            // A 1/1/1 partial holding composite single pile move, 1 space.
            string data1 = "@2|||Ts8s6s5s4h7s-Ts5h--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||Ts8s7s6s5s4h-Ts5h--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest22()
        {
            // This tests a large offload that can be done as a single pile.
            string data1 = "@2||--Ks------Ks|QhJhTh9h8h7h6h5h--6h-9h----3h2hAh-Kh5hKsQsJhTs9s8s7h6s5s4s3s2sAs-As|@";
            string data2 = "@2||--Ks------Ks|-KsQsJhTs9s8s7h6s5s4s3s2sAs-6h5h-9h----3h2hAh-KhQhJhTh9h8h7h6h5h-As|@";
            CheckMoveSucceeds(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest23()
        {
            // Same test a the previous but requires a holding pile.
            string data1 = "@2||--Ks------Ks|QhJhTh9h8h7h6h5h--6h-9h----3h2hAh-Kh5hKsQsJhTs9h8s7h6s5s4s3s2sAh-Ah|@";
            string data2 = "@2||--Ks------Ks|-KsQsJhTs9h-6h5h-9h8s7h6s5s----3h2hAh-KhQhJhTh9h8h7h6h5h4s3s2sAh-Ah|@";
            CheckMoveSucceeds(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest24()
        {
            // A composite single pile move with a discard.
            string data1 = "@2|||8sKhQhJhTh9h8h7h6h5h4h3h-Ts2hAh7s--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|Ah||8s7s-Ts--Ks-Ks-Ks-Ks-Ks-Ks-Ks|@";
            CheckMove(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest25()
        {
            // A 2/1/1 composite single pile move, 2 spaces, using an uncovering move.
            string data1 = "@2|||Ts9h7h8s-Js5s4h3s2h---Ks6h-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-JsTs9h8s7h---Ks6h5s4h3s2h-Ks-Ks-Ks-Ks-Ks|@";
            CheckMoveSucceeds(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest26()
        {
            // A 2/1/1 composite single pile move, 2 spaces, using an uncovering move with a holding pile.
            string data1 = "@2|||Ts9h7h8s-Js5s4h3s2hAs---Ks6h-Ks3h-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||-JsTs9h8s7h---Ks6h5s4h3s2hAs-Ks3h-Ks-Ks-Ks-Ks|@";
            CheckMoveSucceeds(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest27()
        {
            // A 2/1/1 composite single pile move that turns over a card, 2 spaces, using an uncovering move.
            string data1 = "@2||Qh|Ts9h7h8s-Js5s4h3s2h---Ks6h-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||Qh-JsTs9h8s7h---Ks6h5s4h3s2h-Ks-Ks-Ks-Ks-Ks|@";
            CheckMoveSucceeds(data1, data2);
        }

        [TestMethod]
        public void CompositeSinglePileTest28()
        {
            // A 2/1/1 composite single pile move that turns over a card, 2 spaces, using an uncovering move with a holding pile.
            string data1 = "@2||Qh|Ts9h7h8s-Js5s4h3s2hAs---Ks6h-Ks3s-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||Qh-JsTs9h8s7h---Ks6h5s4h3s2hAs-Ks3s-Ks-Ks-Ks-Ks|@";
            CheckMoveSucceeds(data1, data2);
        }

        [TestMethod]
        public void SearchTest1()
        {
            string data1 = "@2|||Ts-9s-8s-7s-Ks-Ks-Ks-Ks-Ks-Ks|@";
            string data2 = "@2|||Ts9s8s7s----Ks-Ks-Ks-Ks-Ks-Ks|@";
            string expected = data2;
            game = new Game(data1);
            game.SearchMoves();
            string actual = game.ToAsciiString();
            Assert.AreEqual(expected, actual);
        }

#if false
        [TestMethod]
        public void DebugTest()
        {
            string data = "@2||--8h6s4h-5h9s3h9hQs----9h2s9h2s-Kh4h|3h2hKh-Ts9s8s7s6h5s\r\n4s3s2sKs-8sQhJhTh7h-KhQhJhTh5s-KsQsJsTs3h2h7h-8s6s-KsQsJsTs9\r\ns8s7s6s5s4s3s2sQs-KhQhJhTh9h8h7h6h5h4h3h2hJs-KsQh3s2hAs-6h5h\r\n4s3s8h7hAs|5h9s7s4hAh7s6sAs4s5sAsJsAh8hThAhAhJhTs6h@";
            CheckMoveSucceeds(data, data);
        }
#endif

        private void CheckResults(string initial, string expected, string actual)
        {
            if (expected != actual)
            {
                PrintGame(new Game(initial));
                PrintCandidates();
                Game.PrintGamesSideBySide(new Game(expected), game);
                Utils.WriteLine("expected: {0}", expected);
                Utils.WriteLine("actual:   {0}", actual);
            }
            Assert.AreEqual(expected, actual);
        }

        private void CheckMoveSucceeds(string initial, string expected)
        {
            // Check that the only available move is made.
            game = new Game(initial);
            game.Diagnostics = true;
            Assert.IsTrue(game.MakeMove());
            string actual = TrimAll(game.ToAsciiString());
            CheckResults(initial, expected, actual);
        }

        private void CheckMoveFails(string initial)
        {
            // Check that the move is not made
            // or that a last resort move was made.
            game = new Game(initial);
            int before = game.Tableau.NumberOfSpaces;
            bool moved = game.MakeMove();
            if (moved)
            {
                int after = game.Tableau.NumberOfSpaces;
                if (!(after < before))
                {
                    PrintGame(new Game(initial));
                    PrintCandidates();
                    PrintGame();
                }
                Assert.IsTrue(after < before);
            }
            else
            {
                string actual = TrimAll(game.ToAsciiString());
                CheckResults(initial, initial, actual);
            }
        }

        private void CheckMove(string initial, string expected)
        {
            // Check that the only available move is made.
            CheckMoveSucceeds(initial, expected);

            // Check that the move is not made with one fewer space.
            CheckMoveFails(FillSpace(initial));
        }

        private void CheckUndo(string initial, string expected, string action)
        {
            // Check that the only available move is made.
            game = new Game(initial);
            game.Diagnostics = true;
            int timeStamp = game.Tableau.TimeStamp;
            if (action == "Move")
            {
                Assert.IsTrue(game.MakeMove());
            }
            else if (action == "Deal")
            {
                game.Tableau.Deal();
            }
            else
            {
                throw new Exception("unknown action: " + action);
            }
            string actual = TrimAll(game.ToAsciiString());
            CheckResults(initial, expected, actual);
            game.Tableau.Revert(timeStamp);
            string undone = TrimAll(game.ToAsciiString());
            CheckResults(initial, initial, undone);
        }

        private string TrimAll(string s)
        {
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (!char.IsWhiteSpace(c))
                {
                    b.Append(c);
                }
            }
            return b.ToString();
        }

        private string FillSpace(string data)
        {
            return data.Replace("--", "-Ks-");
        }

        private void PrintGame()
        {
            PrintGame(game);
        }

        private void PrintGame(Game game)
        {
            Utils.ColorizeToConsole(game.ToString());
            Trace.WriteLine(game.ToString());
        }

        private void PrintCandidates()
        {
            PrintCandidates(game);
        }

        private void PrintCandidates(Game game)
        {
            int count = 0;
            foreach (ComplexMove move in game.ComplexCandidates)
            {
                Utils.WriteLine("move[{0}] = {1}", count, move.ScoreMove);
                foreach (Move subMove in move.SupplementaryMoves)
                {
                    Utils.WriteLine("    supplementary: {0}", subMove);
                }
                foreach (Move holdingMove in move.HoldingList)
                {
                    Utils.WriteLine("    holding: {0}", holdingMove);
                }
                count++;
            }
        }
    }
}
