﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace TA4
{
    class Program
    {
        static void Main(string[] args)
        {
            string sentence;
            string lineNumbersContent;
            bool unusedTextBox;
            string textBlockVisibility;

            string predictiveAnalysisTableName;
            DataView predictiveAnalysisDataTable;
            string startNonterminal;
            Dictionary<string, List<string>> FIRST;
            Dictionary<string, List<string>> FOLLOW;

            DefaultDialogService dialogService = new DefaultDialogService();

            string[] machineFromTXT = File.ReadAllLines("new grammar rules.txt");
            if (machineFromTXT.Length != 0)
            {
                Dictionary<string, List<string>> grammar;
                startNonterminal = dialogService.BuildGrammarDictionary(machineFromTXT, out grammar);


                FIRST = new Dictionary<string, List<string>>();
                foreach (var grammarRules in grammar)
                    FIRST = dialogService.ConstructFIRST(grammar, grammarRules.Value, grammarRules.Key, FIRST);

                FOLLOW = dialogService.ConstructFOLLOW(FIRST, startNonterminal, grammar);

                DataTable predictiveAnalysisTable = dialogService.GeneratePredictiveAnalysisTable(grammar, FIRST, FOLLOW);

                string Sentence = File.ReadAllText("example.txt");
                string[] textRows = Sentence.Replace("\r\n", "\n").Split('\n');
                int[] numberOfCharactersInEachRow = new int[textRows.Count()];
                for (int i = 0; i < textRows.Count(); i++)
                    numberOfCharactersInEachRow[i] = textRows[i].Count();
                dialogService.TextCorrectnessVerification(predictiveAnalysisTable, FIRST, FOLLOW,
                    startNonterminal, Sentence, numberOfCharactersInEachRow);
            }
        }
    }
}
