using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace LingoServer
{
    class Lingo
    {
        public string CurrentWord;
        public string[] CorrectLetters;
        public int TurnCount;
        public int TurnSeat;
        public int MaxTurns = 5;
        public int LetterCount = 5;
        public List<string> WordList = new List<string>();
        public int TeamOneScore = 0;
        public int TeamTwoScore = 0;
        Random random = new Random();
        public bool Running = false;
        public string LastGuess = "";
        public int[] WordCheck;
        

        public List<int> Seats = new List<int>() { 1, 2, 3, 4 };
        public List<Player> Players = new List<Player>();

        public Lingo()
        {
            ImportFiveLetterWords();
            NewRound();
        }
        public void AddPlayer(Player player)
        {
            if (Seats.Contains(player.Seat))
            {
                Seats.Remove(player.Seat);
                Players.Add(player);
            }
        }
        public void RemovePlayer(Player player)
        {
            if (Players.Contains(player)){
                Seats.Add(player.Seat);
                Players.Remove(player);
            }
        }

        public bool SeatAvailable(int seat)
        {
            return Seats.Contains(seat);
        }

        public Player FindPlayerById(int id)
        {
            return Players.Find(player => player.ClientId == id);
        }

        public bool IsGameFull()
        {
            return Players.Count() == 4;
        }

        public bool IsPlayerTurn(Player player)
        {
            return player.Seat == TurnSeat;
        }
        public void NewGame()
        {
            TeamOneScore = 0;
            TeamTwoScore = 0;
            TurnSeat = random.Next(1, 5);
        }

        public void NewRound()
        {
            CurrentWord = GetRandomWord();
            TurnCount = 1;
            CorrectLetters = new string[CurrentWord.Length];
            CorrectLetters[0] = CurrentWord.Substring(0, 1);
            Console.WriteLine(CurrentWord);
        }

        public void EndRound()
        {

        }

        public void Guess(string guess)
        {
            LastGuess = guess.Length < LetterCount ? guess : guess.Substring(0, LetterCount);
            WordCheck = new int[CurrentWord.Length];
            if (LastGuess.Length < LetterCount || !WordList.Contains(LastGuess))
            {
                WordCheck[0] = -1;
            }
            else
            {
                string tmpWord = CurrentWord;
                for (int i = LastGuess.Length - 1; i >= 0; i--)
                {
                    if (LastGuess[i] == CurrentWord[i])
                    {
                        WordCheck[i] = 0;
                        tmpWord = tmpWord.Remove(i, 1);
                        CorrectLetters[i] = CurrentWord[i].ToString();
                    }
                    else
                    {
                        WordCheck[i] = 3;
                    }
                }

                for (int i = 0; i < LastGuess.Length; i++)
                {
                    if (WordCheck[i] > 0)
                    {
                        if (tmpWord.Contains(LastGuess[i]))
                        {
                            tmpWord = tmpWord.Remove(tmpWord.IndexOf(LastGuess[i]), 1);
                            WordCheck[i] = 1;
                        }
                        else
                        {
                            WordCheck[i] = 2;
                        }
                    }
                }
            }
        }

        public void GiveLetter()
        {
             var result = Enumerable.Range(0, CorrectLetters.Count())
             .Where(i => CorrectLetters[i] == null)
             .ToList();

            int randomIndex = random.Next(0, result.Count);
            CorrectLetters[result[randomIndex]] = CurrentWord[result[randomIndex]].ToString();

        }

        public void NextTurn()
        {
            TurnCount++;
            if (IsWordCorrect()) //Word correct
            {
                NewRound();
                if(GetTeamTurn() == 1)
                {
                    TeamOneScore += 25;
                }
                else
                {
                    TeamTwoScore += 25;
                }
            }
            else if(TurnCount > MaxTurns) //Switch team turn
            {
               
                if (GetTeamTurn() == 1)
                {
                    TurnSeat = random.Next(3, 5);
                }
                else
                {
                    TurnSeat = random.Next(1, 3);
                }
                GiveLetter();
            }
            else
            {
                if(TurnSeat == 1)
                {
                    TurnSeat = 2;
                }
                else if(TurnSeat == 2){
                    TurnSeat = 1;
                }
                else if(TurnSeat == 3)
                {
                    TurnSeat = 4;
                }
                else if (TurnSeat == 4)
                {
                    TurnSeat = 3;
                }
            }

        }
        public int GetTeamTurn()
        {
            if (TurnSeat < 3)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        public bool IsWordCorrect()
        {
            return CurrentWord == string.Join("", CorrectLetters);
        }
        
        public string GetRandomWord()
        {
            
            return WordList[random.Next(WordList.Count())];
        }

        public void ImportFiveLetterWords()
        {
            int counter = 0;
            string line;

            System.IO.StreamReader file =
            new System.IO.StreamReader(@"wordlist.txt");
            while ((line = file.ReadLine()) != null)
            {
                if(line.Length == 5 && Regex.IsMatch(line, @"^[a-z]+$"))
                {
                    WordList.Add(line);
                }
                counter++;
            }
        }

        public NextTurn GetJSONNextTurn()
        {

            string correctLetters = "";
            foreach(string letter in CorrectLetters)
            {
                if(letter == null)
                {
                    correctLetters += ".";
                }
                else
                {
                    correctLetters += letter;
                }
            }
            NextTurn nextTurn = new NextTurn
            {
                playerturn = TurnSeat.ToString(),
                turncount = TurnCount.ToString(),
                correctletters = correctLetters,
                guessedword = LastGuess.Length < LetterCount ? LastGuess : LastGuess.Substring(0, LetterCount),
                check = WordCheck,
                teamonescore = TeamOneScore,
                teamtwoscore = TeamTwoScore

            };
            return nextTurn;
        }
    }
}
