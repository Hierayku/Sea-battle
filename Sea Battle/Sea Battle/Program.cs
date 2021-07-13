using System;
using System.Linq;
using System.Threading;

//4 days 
// ~16.5 hours

//Нужно сделать "Press any key" в правом нижнем углу

namespace Sea_Battle
{
    class Program
    {
        static char[] letters = { '.', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J' };

        static char[,] emptyWaterCell = 
            {
                { '.', '.', '.', '.', '.', '.', '.' }, //cell symbol - '.'
                { '.', '.', '.', '.', '.', '.', '.' },
                { '.', '.', '.', '.', '.', '.', '.' }
            };
        static char[,] aliveShipCell=
            {
                {  '/', '0', '0', '0', '0', '0', '\\' }, //cell symbol - '0'
                {  '0', '0', '0', '0', '0', '0', '0'  },
                { '\\', '0', '0', '0', '0', '0', '/'  }
            };
        static char[,] damagedAliveShipCell =
            {
                { '/', '8', '-', '8', '8', '0', '\\' }, //cell symbol - '8'
                { '0', '8', '0', '0', '-', '8', '8'  },
                { '\\', '8', '0', '0', '0', '8', '/'  }
            };
        static char[,] deadShipCell =
            {
                { '/', '\\', '8', '/', '-', '/', '\\' }, //cell symbol - '\\'
                { '=',  '-', '-', '-', '/', '-', '|'  },
                { '\\', '-', '\\', '9', '/', '8', '/'  }
            };
        static char[,] fullWaterCell =
            {
                { '.', '/', '.', '\\', '.', '.', '/' }, //cell symbol - '/'
                { '.', '.', '/', '.', '\\', '.', '.' },
                { '.', '.', '/', '.', '\\', '.', '.' }
            };


        static char[,] playerOneField = new char[11, 11];
        static char[,] playerTwoField = new char[11, 11];

        static void FillFieldsAtTheStart(char[,] player)
        {
            for (int i = 0; i < player.GetLength(0); i++)
            {
                for (int j = 0; j < player.GetLength(1); j++)
                {
                    player[i, j] = '.';
                }
            }
        }

        static void ConsoleScaling(int height, int width)
        {
            Console.WindowHeight = Console.LargestWindowHeight;
            Console.WindowWidth = Console.LargestWindowWidth;
        }

        static int[] FindPositionOnVisibleField(int posX, int posY)
        {
            posX *= 8;
            posY *= 4;

            int[] result = { posX, posY };
            return result;
        }

        static int FindOutTheEndOfTheNumber(string cellId)
        {
            int endOfNumber = cellId.IndexOf('-');

            return endOfNumber;
        }

        static int[] FindOutSmallPosition(string cellId)
        {
            int endOfNumber = FindOutTheEndOfTheNumber(cellId);

            int cellNumber = Convert.ToInt32(cellId[..endOfNumber]);
            char cellLetter = cellId[endOfNumber + 1];

            int posX = cellNumber;
            int posY = Array.IndexOf(letters, cellLetter);

            int[] result = { posX, posY };
            return result;
        }
        static string FindOutSmallPositionReverse(int[] position)
        {
            string result = "";

            int posX = position[0];
            int posY = position[1];

            result = posX + "-" + letters[posY];
            return result;
        }

        static void EditOneCell(string cellId, char[,] player, char[,] cellSymbols, int playerIndex, bool isOne)
        {
            char symbol = cellSymbols[0, 1];

            int[] smallPosition = FindOutSmallPosition(cellId);
            int posX;
            int posY;

            if (playerIndex == 1)
            {
                posX = smallPosition[0];
                posY = smallPosition[1];
                player[posY, posX] = symbol;
            }
            else
            {
                posX = smallPosition[0] + 13;
                posY = smallPosition[1];
                player[posY, posX - 13] = symbol;
            }

            if (playerIndex == 1)
                DrawOneCell(posX, posY, cellSymbols, playerIndex);
            if (isOne)
                DrawBoundaries(playerIndex);
        }

        static void DrawOneCell(int posX, int posY, char[,] cellSymbols, int playerIndex)
        {
            char symbol = cellSymbols[0, 1];

            int[] position = FindPositionOnVisibleField(posX, posY);
            posX = position[0];
            posY = position[1];

            //cell boundaries
            for (int j = posX; j < posX + 8; j++)
            {
                Console.SetCursorPosition(j, posY);
                Console.Write('—');
            }
            for (int i = posY; i < posY + 5; i++)
            {
                Console.SetCursorPosition(posX, i);
                Console.Write('|');
                Console.SetCursorPosition(posX + 8, i);
                Console.Write('|');
            }
            for (int j = posX + 1; j <= posX + 7; j++)
            {
                Console.SetCursorPosition(j, posY + 4);
                Console.Write('—');
            }

            //inside
            int x = 0, y = 0;

            for (int i = posY + 1; i < posY + 4; i++)
            {
                for (int j = posX + 1; j < posX + 8; j++)
                {
                    Console.SetCursorPosition(j, i);
                    Console.Write(cellSymbols[y, x]);

                    x++;
                }
                x = 0;
                y++;
            }

        }

        static void DrawBoundaries(int player)
        {
            if (player == 1)
            {
                for (int i = 0; i < playerOneField.GetLength(1); i++)
                {
                    DrawBoundary($"{i}-.", 1);
                }
                for (int i = 0; i < playerOneField.GetLength(0); i++)
                {
                    DrawBoundary($"0-{letters[i]}", 1);
                }
            }
            else
            {
                for (int i = 0; i < playerTwoField.GetLength(1); i++)
                {
                    DrawBoundary($"{i}-.", 2);
                }
                for (int i = 0; i < playerTwoField.GetLength(0); i++)
                {
                    DrawBoundary($"0-{letters[i]}", 2);
                }
            }
        }

        static void DrawBoundary(string cellId, int player)
        {
            int[] smallPosition = FindOutSmallPosition(cellId);
            int posX = smallPosition[0];
            int posY = smallPosition[1];

            int[] position = FindPositionOnVisibleField(posX, posY);
            if (player == 1)
            {
                posX = position[0];
                posY = position[1];
            }
            else
            {
                posX = position[0] + 104;
                posY = position[1];
            }

            //drawing boundary rectangle
            for (int j = posX; j < posX + 8; j++)
            {
                Console.SetCursorPosition(j, posY);
                Console.Write('☺');
            }
            for (int i = posY; i < posY + 4; i++)
            {
                Console.SetCursorPosition(posX, i);
                Console.Write('☺');
                Console.SetCursorPosition(posX + 8, i);
                Console.Write('☺');
            }
            for (int j = posX; j <= posX + 8; j++)
            {
                Console.SetCursorPosition(j, posY + 4);
                Console.Write('☺');
            }


        }
        static void DrawNumberOnBoundaries(string cellId, char numberOrLetterIndex, int playerIndex)
        {
            int[] smallPosition = FindOutSmallPosition(cellId);
            int posX = smallPosition[0];
            int posY = smallPosition[1];

            int[] position = FindPositionOnVisibleField(posX, posY);

            if (playerIndex == 1)
            {
                posX = position[0];
                posY = position[1];
            }
            else
            {
                posX = position[0] + 104;
                posY = position[1];
            }
            //drawing boundary numbers

            Console.SetCursorPosition(posX + 4, posY + 2);
            if (numberOrLetterIndex != ':')
                Console.Write(numberOrLetterIndex);
            else
                Console.Write(10);
        }

        static void DrawAllBoundaries()
        {
            //player 1
            DrawBoundaries(1);
            //player 2
            DrawBoundaries(2);

            //drawing numbers on boundaries

            //player 1
            for (int i = 1; i < playerOneField.GetLength(1); i++)
            {
                DrawNumberOnBoundaries($"{i}-.", (char)(i + '0'), 1);
            }
            for (int i = 1; i < playerOneField.GetLength(0); i++)
            {
                DrawNumberOnBoundaries($"0-{letters[i]}", letters[i], 1);
            }

            //player 2
            for (int i = 1; i < playerOneField.GetLength(1); i++)
            {
                DrawNumberOnBoundaries($"{i}-.", (char)(i + '0'), 2);
            }
            for (int i = 1; i < playerOneField.GetLength(0); i++)
            {
                DrawNumberOnBoundaries($"0-{letters[i]}", letters[i], 2);
            }
        }

        static void MakeRandomShipPositions(int playerIndex, bool isOne)
        {
                BuildShipOnRandomPosition(4, playerIndex, isOne);
                DrawAllBoundaries();
                BuildShipOnRandomPosition(3, playerIndex, isOne);
                DrawAllBoundaries();
                BuildShipOnRandomPosition(3, playerIndex, isOne);
                DrawAllBoundaries();
                BuildShipOnRandomPosition(2, playerIndex, isOne);
                DrawAllBoundaries();
                BuildShipOnRandomPosition(2, playerIndex, isOne);
                DrawAllBoundaries();
                BuildShipOnRandomPosition(2, playerIndex, isOne);
                DrawAllBoundaries();
                BuildShipOnRandomPosition(1, playerIndex, isOne);
                DrawAllBoundaries();
                BuildShipOnRandomPosition(1, playerIndex, isOne);
                DrawAllBoundaries();
                BuildShipOnRandomPosition(1, playerIndex, isOne);
                DrawAllBoundaries();
                BuildShipOnRandomPosition(1, playerIndex, isOne);
                DrawAllBoundaries();
        }

        //Whole method uses only small positions (1-10)!
        static void BuildShipOnRandomPosition(int shipLength, int playerIndex, bool isOne) //warning : i suppose, it's super unoptimized, but my head gonna burn if i try
        {
            Random random = new Random();

            char[,] player;
            if (playerIndex == 1)
                player = playerOneField;
            else
                player = playerTwoField;

            //generating correct position
            int posX = -100;
            int posY = -100;

            bool shipIsPossibleToBeGenerated = false;
            bool isNeeded;
            bool shipIsHorizontal = random.Next(1, 3) == 1; //if 1 then ship is horizontal

            int i = 0;
            string cellId;
            int[] positionForMethod = new int[2];

            while (!shipIsPossibleToBeGenerated)
            {
                isNeeded = true;

                if (shipIsHorizontal)
                {
                    do
                    {
                        posX = random.Next(1, 11);
                        posY = random.Next(1, 11);
                    }
                    while (posX + shipLength > 11); 
                }
                else
                {
                    do
                    {
                        posX = random.Next(1, 11);
                        posY = random.Next(1, 11);
                    }
                    while (posY + shipLength > 11);
                }

                //checking whether it is possible to place the ship on specified position
                if (shipIsHorizontal)
                {
                    for (i = posX; i < (posX + shipLength) && isNeeded; i++)
                    {
                        try
                        {
                            if (player[posY, i] == '.')
                            {
                                shipIsPossibleToBeGenerated = true;
                                continue;
                            }
                            else
                            {
                                isNeeded = false;
                                shipIsPossibleToBeGenerated = false;
                                break;
                            }
                        }
                        catch
                        {
                            shipIsPossibleToBeGenerated = false;
                            break;
                        }
                    }
                }
                else
                {
                    for (i = posY; i < (posY + shipLength) && isNeeded; i++)
                    {
                        try
                        {
                            if (player[i, posX] == '.')
                            {
                                shipIsPossibleToBeGenerated = true;
                                continue;
                            }
                            else
                            {
                                isNeeded = false;
                                shipIsPossibleToBeGenerated = false;
                                break;
                            }
                        }
                        catch
                        {
                            shipIsPossibleToBeGenerated = false;
                            break;
                        }
                    }
                }
            }

            if (i == posX + shipLength || i == posY + shipLength)
            {
                //building ship
                if (shipIsHorizontal)
                {
                    for (int j = posX; j < posX + shipLength; j++)
                    {
                        positionForMethod[0] = j;
                        positionForMethod[1] = posY;

                        cellId = FindOutSmallPositionReverse(positionForMethod);
                        EditOneCell(cellId, player, aliveShipCell, playerIndex, isOne);
                    }
                }
                else
                {
                    for (int j = posY; j < posY + shipLength; j++)
                    {
                        positionForMethod[0] = posX;
                        positionForMethod[1] = j;

                        cellId = FindOutSmallPositionReverse(positionForMethod);
                        EditOneCell(cellId, player, aliveShipCell, playerIndex, isOne);
                    }
                }

                //arranging full (filled) Water

                if (shipIsHorizontal)
                {
                    //upper water
                    positionForMethod[1] = posY - 1;
                    if (positionForMethod[1] > 0)
                    {
                        for (int k = posX - 1; k < posX + shipLength + 1; k++)
                        {
                            if (k > 0 && k < 11)
                            {
                                positionForMethod[0] = k;

                                cellId = FindOutSmallPositionReverse(positionForMethod);

                                EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne);
                            }
                        }
                    }

                    //water on the sides (left side)
                    positionForMethod[0] = posX - 1;

                    if (positionForMethod[0] > 0)
                    {
                        positionForMethod[1] = posY;

                        cellId = FindOutSmallPositionReverse(positionForMethod);
                        EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne);
                    }
                    //water on the sides (right side)
                    positionForMethod[0] = posX + shipLength;

                    if (positionForMethod[0] < 11)
                    {
                        positionForMethod[1] = posY;

                        cellId = FindOutSmallPositionReverse(positionForMethod);
                        EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne);
                    }

                    //lower water
                    positionForMethod[1] = posY + 1;

                    if (positionForMethod[1] < 11)
                    {
                        for (int k = posX - 1; k < posX + shipLength + 1; k++)
                        {
                            if (k > 0 && k < 11)
                            {
                                positionForMethod[0] = k;

                                cellId = FindOutSmallPositionReverse(positionForMethod);

                                EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne);
                            }
                        }
                    }
                }
                else
                {
                    //upper water
                    positionForMethod[1] = posY - 1;
                    if (positionForMethod[1] > 0)
                    {
                        for (int k = posX - 1; k < posX + 2; k++)
                        {
                            if (k > 0 && k < 11)
                            {
                                positionForMethod[0] = k;

                                cellId = FindOutSmallPositionReverse(positionForMethod);

                                EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne);
                            }
                        }
                    }

                    //water on the sides (left side)
                    positionForMethod[0] = posX - 1;

                    for (int k = posY; k < posY + shipLength; k++)
                    {
                        if (positionForMethod[0] > 0)
                        {
                            positionForMethod[1] = k;

                            cellId = FindOutSmallPositionReverse(positionForMethod);
                            EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne);
                        }
                    }
                    //water on the sides (right side)
                    positionForMethod[0] = posX + 1;

                    for (int k = posY; k < posY + shipLength; k++)
                    {
                        if (positionForMethod[0] < 11)
                        {
                            positionForMethod[1] = k;

                            cellId = FindOutSmallPositionReverse(positionForMethod);
                            EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne);
                        }
                    }

                    //lower water
                    positionForMethod[1] = posY + shipLength;

                    if (positionForMethod[1] < 11)
                    {
                        for (int k = posX - 1; k < posX + 2; k++)
                        {
                            if (k > 0 && k < 11)
                            {
                                positionForMethod[0] = k;

                                cellId = FindOutSmallPositionReverse(positionForMethod);

                                EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne);
                            }
                        }
                    }

                }
            }
        }

        static void DisplayMessage(string message, int messageLevel, bool isStart)
        {
            int posX = 1;
            int posY = (isStart ? 1 : 46) + messageLevel * 2;

            Console.SetCursorPosition(posX, posY);
            DisplayTextSmoothly(message, 50);

            Console.SetCursorPosition(posX, posY + 2);
        }

        static void ClearMessage(string message, int messageLevel, bool isStart, bool clearReadKey)
        {
            int posX = 1;
            int posY = (isStart ? 1 : 46) + messageLevel * 2;

            if (clearReadKey)
            {
                Console.SetCursorPosition(posX, posY + 2);
                Console.Write(" ");
            }

            for (int i = posX; i < message.Length + posX; i++)
            {
                Console.SetCursorPosition(i, posY);
                Console.Write(" ");
                Thread.Sleep(50);
            }

            Console.SetCursorPosition(posX, posY + 2);
        }

        static void DisplayTextSmoothly(string text, uint smoothLevel)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '*')
                {
                    if (text[i..(i + 7)] == "*PAUSE*")
                    {
                        text = text.Remove(i, 7);
                        Thread.Sleep(250);
                    }
                }
                Console.Write(text[i]);
                Thread.Sleep(Convert.ToInt32(smoothLevel));
            }
                
            
        }

        static void Main(string[] args)
        {
            Console.Title = "Sea Battle";
            ConsoleScaling(40, 80);

            //start

            ////greetings and instruction
            DisplayMessage("Expand the game to full screen to avoid glitches/bugs. Then press any key to read short instruction and start sea battle.", 0, true);
            Console.ReadKey();

            DisplayMessage("Hi! *PAUSE*I am Dima and this is my game \"Sea Battle\". *PAUSE*I have spent X days and Y hours of continuous work making it. Press any key to read short instruction.", 1, true);
            Console.ReadKey();
            ClearMessage("", 1, true, true);

            DisplayMessage("Each player have 10 ships : 1 four-segmented, 2 three-segmented, 3 two-segmented and 4 one-segmented. *PAUSE*You have to destroy all enemy ships and hope that enemy *PAUSE*won't *PAUSE*do *PAUSE*that *PAUSE*earlier.", 2, true);
            Console.ReadKey();

            DisplayMessage("\t\t\t\t\t\t\t  Damaged", 8, true); //don't tell me about this 4 lines, i know this is horrible
            DisplayMessage("\t\t\t  Full\t\t  Alive\t\t  Alive\t\t  Destroyed", 9, true);
            DisplayMessage("\t  Water\t\t  Water\t\t  Ship\t\t  Ship\t\t  Ship", 10, true);
            DisplayMessage("\t  Cell\t\t  Cell\t\t  Cell\t\t  Cell \t\t  Cell", 11, true);

            Thread.Sleep(250);
            DrawOneCell(1, 6, emptyWaterCell, 1);
            Thread.Sleep(250);
            DrawOneCell(3, 6, fullWaterCell, 1);
            Thread.Sleep(250);
            DrawOneCell(5, 6, aliveShipCell, 1);
            Thread.Sleep(250);
            DrawOneCell(7, 6, damagedAliveShipCell, 1);
            Thread.Sleep(250);
            DrawOneCell(9, 6, deadShipCell, 1);
            Console.ReadKey();


            DisplayMessage("When you hit the water, you can miss the shot (water will become full), hit the ship or destroy it. In this case, you will have extra move.", 3, true);
            Console.ReadKey();
            
            DisplayMessage("You have to make your move by specifying cell without spaces. Example : \"4-B\", \"10-G\"", 4, true);
            Console.ReadKey();
            
            DisplayMessage("Press any key to start sea battle. Good luck!", 5, true);
            Console.ReadKey();
            Console.Clear();

            //drawing field
            FillFieldsAtTheStart(playerOneField);
            FillFieldsAtTheStart(playerTwoField);

            //player1's field
            for (int i = 0; i < playerOneField.GetLength(0); i++) 
            {
                for (int j = 0; j < playerOneField.GetLength(1); j++)
                {
                    DrawOneCell(j, i, emptyWaterCell, 1);
                }
                Console.WriteLine();
            }
            //player2's field
            for (int i = 0; i < playerTwoField.GetLength(0); i++)
            {
                for (int j = 13; j < playerTwoField.GetLength(1) + 13; j++)
                {
                    DrawOneCell(j, i, emptyWaterCell, 2);
                }
                Console.WriteLine();
            }
            DrawAllBoundaries();

            //generating ships
            MakeRandomShipPositions(1, false);
            MakeRandomShipPositions(2, false);



            //ClearMessage("Press any key to start sea battle.", 0 , true, true);

            //EditOneCell("3-B", playerOneField, damagedAliveShipCell, 1, true);
            //EditOneCell("2-B", playerOneField, aliveShipCell, 1, true);

            Console.ReadKey();
        }
    }
}