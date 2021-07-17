using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

//7 days 
// ~36 hours

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
        static char[,] playerOneVisibleField = new char[11, 11];

        static char[,] playerTwoField = new char[11, 11];
        static char[,] playerTwoVisibleField = new char[11, 11];

        static bool someoneWin = false;
        static bool firstPlayerIsWinner;
        static bool hasExtraMove = false;

        static bool enemyHitTheShip = false;
        static int[] hitByEnemyShipPosition = new int[2];
        static char shipAxis = '?';
        static bool secondAttack = false;

        static int firstPlayerShipsCount = 10;
        static int secondPlayerShipsCount = 10;

        static List<int> otherDamagedSegmentsPosX = new List<int>();
        static List<int> otherDamagedSegmentsPosY = new List<int>();

        static List<int> simpleOtherDamagedSegmentsPosX = new List<int>();
        static List<int> simpleOtherDamagedSegmentsPosY = new List<int>();


        static void FillFieldsAtTheStart(char[,] player, char[,] visiblePlayer)
        {
            for (int i = 0; i < player.GetLength(0); i++)
            {
                for (int j = 0; j < player.GetLength(1); j++)
                {
                    player[i, j] = '.';
                    visiblePlayer[i, j] = '.';
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

        static bool CheckCellIdForValidation(string cellId)
        {
            try {
                int endOfNumber = FindOutTheEndOfTheNumber(cellId);

                int cellNumber = Convert.ToInt32(cellId[..endOfNumber]);
                char cellLetter = cellId[endOfNumber + 1];

                if (cellNumber > 0 && cellNumber < 11 && Array.IndexOf(letters, cellLetter) > 0 && Array.IndexOf(letters, cellLetter) < 11)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
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

        static void EditOneCell(string cellId, char[,] player, char[,] cellSymbols, int playerIndex, bool isNeededInBoundaries, bool isAttacked)
        {
            char symbol = cellSymbols[0, 1];
            bool isWater = symbol == '/';

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

            if ((playerIndex == 1 && !isWater) || isAttacked)
                DrawOneCell(posX, posY, cellSymbols, playerIndex);

            if (isNeededInBoundaries)
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
                        EditOneCell(cellId, player, aliveShipCell, playerIndex, isOne, false);
                    }
                }
                else
                {
                    for (int j = posY; j < posY + shipLength; j++)
                    {
                        positionForMethod[0] = posX;
                        positionForMethod[1] = j;

                        cellId = FindOutSmallPositionReverse(positionForMethod);
                        EditOneCell(cellId, player, aliveShipCell, playerIndex, isOne, false);
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

                                EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne, false);
                            }
                        }
                    }

                    //water on the sides (left side)
                    positionForMethod[0] = posX - 1;

                    if (positionForMethod[0] > 0)
                    {
                        positionForMethod[1] = posY;

                        cellId = FindOutSmallPositionReverse(positionForMethod);
                        EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne, false);
                    }
                    //water on the sides (right side)
                    positionForMethod[0] = posX + shipLength;

                    if (positionForMethod[0] < 11)
                    {
                        positionForMethod[1] = posY;

                        cellId = FindOutSmallPositionReverse(positionForMethod);
                        EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne, false);
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

                                EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne, false);
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

                                EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne, false);
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
                            EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne, false);
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
                            EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne, false);
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

                                EditOneCell(cellId, player, fullWaterCell, playerIndex, isOne, false);
                            }
                        }
                    }

                }
            }
        }

        static void DisplayMessage(string message, int messageLevel, uint speed, bool isStart, bool isNeededAnyKeyToContinue)
        {
            int posX = 1;
            int posY = (isStart ? 1 : 46) + messageLevel * 2;

            Console.SetCursorPosition(posX, posY);
            DisplayTextSmoothly(message, speed);

            if (isNeededAnyKeyToContinue)
            {
                Console.SetCursorPosition(Console.WindowWidth - 30, Console.WindowHeight - 3);
                Console.Write("Press any key to continue");

                if (Console.ReadKey().Key == ConsoleKey.Enter)
                    Console.SetCursorPosition(Console.WindowWidth - 30, Console.WindowHeight - 3);

                Console.SetCursorPosition(Console.GetCursorPosition().Left - 29, Console.GetCursorPosition().Top);
                for (int i = 1; i < 30; i++)
                {
                    Console.Write(" ");
                }
            }

            Console.SetCursorPosition(posX, posY + 2);
        }

        static void ClearMessage(string message, int messageLevel, uint speed, bool isStart, bool clearReadKey, bool isFast)
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

                if(!isFast)
                    Thread.Sleep(Convert.ToInt32(speed));
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

        static bool FindAllDamagedSegments(int posX, int posY, int playerIndex)
        {
            char[,] player = playerIndex == 1 ? playerOneField : playerTwoField;

            otherDamagedSegmentsPosX.Add(posX);
            otherDamagedSegmentsPosY.Add(posY);

            //checking for 4 segments in 4 directions

            //left side
            for (int j = 1; j <= 3; j++)
            {
                if (player[posY, posX - j] == '8')
                {
                    otherDamagedSegmentsPosX.Add(posX - j);
                    otherDamagedSegmentsPosY.Add(posY);
                    continue;
                }
                else if (player[posY, posX - j] == '0')
                {
                    otherDamagedSegmentsPosX.Clear();
                    otherDamagedSegmentsPosY.Clear();
                    return false;
                }
                else
                {
                    break;
                }
            }
            //right side
            for (int j = 1; j <= 3; j++)
            {
                if (posX + j < 11)
                {
                    if (player[posY, posX + j] == '8')
                    {
                        otherDamagedSegmentsPosX.Add(posX + j);
                        otherDamagedSegmentsPosY.Add(posY);
                        continue;
                    }
                    else if (player[posY, posX + j] == '0')
                    {
                        otherDamagedSegmentsPosX.Clear();
                        otherDamagedSegmentsPosY.Clear();
                        return false;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //upper side
            for (int j = 1; j <= 3; j++)
            {
                if (player[posY - j, posX] == '8')
                {
                    otherDamagedSegmentsPosX.Add(posX);
                    otherDamagedSegmentsPosY.Add(posY - j);
                    continue;
                }
                else if (player[posY - j, posX] == '0')
                {
                    otherDamagedSegmentsPosX.Clear();
                    otherDamagedSegmentsPosY.Clear();
                    return false;
                }
                else
                {
                    break;
                }
            }
            //lower side
            for (int j = 1; j <= 3; j++)
            {
                if (posY + j < 11)
                {
                    if (player[posY + j, posX] == '8')
                    {
                        otherDamagedSegmentsPosX.Add(posX);
                        otherDamagedSegmentsPosY.Add(posY + j);
                        continue;
                    }
                    else if (player[posY + j, posX] == '0')
                    {
                        otherDamagedSegmentsPosX.Clear();
                        otherDamagedSegmentsPosY.Clear();
                        return false;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            otherDamagedSegmentsPosX.Sort();
            otherDamagedSegmentsPosY.Sort();

            return true;
        }

        static void DestroyShip(int firstPosX, int firstPosY, int lastPosX, int lastPosY, int playerIndex)
        {
            char[,] player = playerIndex == 1 ? playerOneField : playerTwoField;
            char[,] visiblePlayer = playerIndex == 1 ? playerOneVisibleField : playerTwoVisibleField;
            char axis = firstPosX == lastPosX ? 'y' : 'x';

            //ship
            int loopLimit = axis == 'x' ? lastPosX : lastPosY;
            for (int i = axis == 'x' ? firstPosX : firstPosY; i <= loopLimit; i++)
            {
                int[] segmentPosition = new int[2];
                string segmentCellId;

                segmentPosition[0] = axis == 'x' ? i : firstPosX;
                segmentPosition[1] = axis == 'x' ? firstPosY : i;

                segmentCellId = FindOutSmallPositionReverse(segmentPosition);

                EditOneCell(segmentCellId, player, deadShipCell, playerIndex, true, true);
            }



            //upper water
            if (firstPosY - 1 > 0/* && lastPosY + 1 < 11)*/)
            {
                for (int i = firstPosX - 1; i <= lastPosX + 1; i++)
                {
                    if (i > 0 && i < 11)
                    {
                        int[] segmentPosition = new int[2];
                        string segmentCellId;

                        segmentPosition[0] = i;
                        segmentPosition[1] = firstPosY - 1;

                        segmentCellId = FindOutSmallPositionReverse(segmentPosition);

                        EditOneCell(segmentCellId, player, fullWaterCell, playerIndex, true, true);
                        visiblePlayer[segmentPosition[1], segmentPosition[0]] = '/';
                    }
                }
            }

            //left side water
            if (firstPosX - 1 > 0)
            {
                for (int i = firstPosY; i <= lastPosY; i++)
                {
                    int[] segmentPosition = new int[2];
                    string segmentCellId;

                    segmentPosition[0] = firstPosX - 1;
                    segmentPosition[1] = i;

                    segmentCellId = FindOutSmallPositionReverse(segmentPosition);

                    EditOneCell(segmentCellId, player, fullWaterCell, playerIndex, true, true);
                    visiblePlayer[segmentPosition[1], segmentPosition[0]] = '/';
                }
            }

            //right side water
            if (lastPosX + 1 < 11)
            {
                for (int i = firstPosY; i <= lastPosY; i++)
                {
                    int[] segmentPosition = new int[2];
                    string segmentCellId;

                    segmentPosition[0] = lastPosX + 1;
                    segmentPosition[1] = i;

                    segmentCellId = FindOutSmallPositionReverse(segmentPosition);

                    EditOneCell(segmentCellId, player, fullWaterCell, playerIndex, true, true);
                    visiblePlayer[segmentPosition[1], segmentPosition[0]] = '/';
                }
            }

            //lower water
            if (lastPosY + 1 < 11/* && firstPosY + 1 < 11*/)
            {
                for (int i = firstPosX - 1; i <= lastPosX + 1; i++)
                {
                    if (i > 0 && i < 11)
                    {
                        int[] segmentPosition = new int[2];
                        string segmentCellId;

                        segmentPosition[0] = i;
                        segmentPosition[1] = lastPosY + 1;

                        segmentCellId = FindOutSmallPositionReverse(segmentPosition);

                        EditOneCell(segmentCellId, player, fullWaterCell, playerIndex, true, true);
                        visiblePlayer[segmentPosition[1], segmentPosition[0]] = '/';
                    }
                }
            }

            otherDamagedSegmentsPosX.Clear();
            otherDamagedSegmentsPosY.Clear();
        }

        static void Attack(int playerIndex)
        {
            string cellId;
            int[] position = new int[2];

            bool attackIsPossible = true;
            string attackProblemText = "no problems"; //text which is used when error appears

            string attackReport = "nothing is happened"; //text which is used when saying which cell enemy attacked

            bool shipIsDestroyed = false;

            char[,] player = playerIndex == 1 ? playerTwoField : playerOneField;
            char[,] visiblePlayer = playerIndex == 1 ? playerTwoVisibleField : playerOneVisibleField;
            playerIndex = playerIndex == 1 ? 2 : 1; //reversing player (from who attacks to who will be attacked)

            RepeatAttack:

            //attack problem text (if exists)
            if (!attackIsPossible && playerIndex == 2)
            {
                ClearMessage(new string('*', 170), 0, 20, false, false, true);
                ClearMessage(new string('*', 170), 1, 20, false, false, true);

                DisplayMessage(attackProblemText, 0, 10, false, false);

                Thread.Sleep(2000);

                ClearMessage(new string('*', 170), 0, 20, false, false, true);
                ClearMessage(new string('*', 170), 1, 20, false, false, true);
            }

            

            //text
            if (playerIndex == 2)
            {
                ClearMessage(new string('*', 170), 0, 20, false, false, true);
                ClearMessage(new string('*', 170), 1, 20, false, false, true);

                DisplayMessage("Your turn. Enter cell index to attack. Example : \"2-B\",\"10-F\"", 0, 10, false, false);
                cellId = Console.ReadLine();
            }
            else
            {
                ClearMessage(new string('*', 170), 0, 20, false, false, true);
                ClearMessage(new string('*', 170), 1, 20, false, false, true);

                DisplayMessage("Enemy's turn, wait a few seconds.", 0, 10, false, false);

                Thread.Sleep(1000);

                RepeatPositionAssignment:

                Random random = new Random();

                do
                {
                   if (!enemyHitTheShip)
                    {
                        position[0] = random.Next(1, 11);
                        position[1] = random.Next(1, 11);
                    }
                    else
                    {
                        position[0] = hitByEnemyShipPosition[0];
                        position[1] = hitByEnemyShipPosition[1];

                        simpleOtherDamagedSegmentsPosX.Add(position[0]);
                        simpleOtherDamagedSegmentsPosY.Add(position[1]);

                        simpleOtherDamagedSegmentsPosX.Sort();
                        simpleOtherDamagedSegmentsPosY.Sort();

                        int directionX;
                        int directionY;

                        switch (shipAxis)
                        {
                            case '?':
                                int k = random.Next(0, 2);
                                switch (k)
                                {
                                    case 0: //X axis
                                        directionX = 0;
                                        while (directionX == 0)
                                        {
                                            directionX = random.Next(-1, 2);
                                        }

                                        if (position[0] + directionX < 1 || position[0] + directionX > 10)
                                            break;
                                        else
                                            position[0] += directionX;

                                        break;
                                    case 1: //Y axis
                                        directionY = 0;
                                        while (directionY == 0)
                                        {
                                            directionY = random.Next(-1, 2);
                                        }

                                        if (position[1] + directionY < 1 || position[1] + directionY > 10)
                                            break;
                                        else
                                            position[1] += directionY;

                                        break;
                                }
                                break;
                            case 'x':
                                directionX = 0;
                                while (directionX == 0)
                                {
                                    directionX = random.Next(-1, 2);
                                }

                                if (directionX == -1)
                                    position[0] = simpleOtherDamagedSegmentsPosX[0] - 1;
                                else
                                    position[0] = simpleOtherDamagedSegmentsPosX.Last() + 1;

                                break;
                            case 'y':
                                directionY = 0;
                                while (directionY == 0)
                                {
                                    directionY = random.Next(-1, 2);
                                }

                                if (directionY == -1)
                                    position[1] = simpleOtherDamagedSegmentsPosY[0] - 1;
                                else
                                    position[1] = simpleOtherDamagedSegmentsPosY.Last() + 1;

                                break;
                        }

                    }
                }
                while (visiblePlayer[position[1], position[0]] != '.');

                try
                {
                    cellId = FindOutSmallPositionReverse(position);
                }
                catch
                {
                    goto RepeatPositionAssignment;
                }
            }
            
            //checking if cellId is correct
            int cellNumber;
            char cellLetter;
            if (CheckCellIdForValidation(cellId))
            {
                int endOfNumber = FindOutTheEndOfTheNumber(cellId);

                cellNumber = Convert.ToInt32(cellId[..endOfNumber]);
                cellLetter = cellId[endOfNumber + 1];
            }
            else
            {
                attackIsPossible = false;
                attackProblemText = "Invalid cell index. Try again.";
                goto RepeatAttack;
            }

            if (playerIndex == 2)
                position = FindOutSmallPosition(cellId);


            //if it is possible, attacking cell, or warning
            int posX = position[0];
            int posY = position[1];

            if (visiblePlayer[posY, posX] == '.')
            {
                switch (player[posY, posX])
                {
                    case '.':
                        player[posY, posX] = '/';
                        visiblePlayer[posY, posX] = '/';

                        attackReport = "hit the water.";

                        EditOneCell(cellId, player, fullWaterCell, playerIndex, true, true);
                        break;
                    case '/':
                        player[posY, posX] = '/';
                        visiblePlayer[posY, posX] = '/';

                        attackReport = "hit the water.";

                        EditOneCell(cellId, player, fullWaterCell, playerIndex, true, true);
                        break;
                    case '0':
                        player[posY, posX] = '8';
                        visiblePlayer[posY, posX] = '8';

                        if (secondAttack)
                            shipAxis = posY == hitByEnemyShipPosition[1] ? 'x' : 'y';

                        if (playerIndex == 1)
                        {
                            enemyHitTheShip = true;
                            secondAttack = true;

                            hitByEnemyShipPosition[0] = posX;
                            hitByEnemyShipPosition[1] = posY;
                        }

                        string attacker2has = playerIndex == 1 ? "enemy has" : "you have";
                        attackReport = $"hit the ship. Now {attacker2has} an extra move.";
                        hasExtraMove = true;

                        EditOneCell(cellId, player, damagedAliveShipCell, playerIndex, true, true);

                        if (FindAllDamagedSegments(posX, posY, playerIndex))
                        {
                            DestroyShip(otherDamagedSegmentsPosX[0], otherDamagedSegmentsPosY[0], otherDamagedSegmentsPosX.Last(), otherDamagedSegmentsPosY.Last(), playerIndex);

                            shipIsDestroyed = true;

                            //deleting information about destroyed ship for AI
                            if (playerIndex == 1)
                            {
                                hitByEnemyShipPosition[0] = -1;
                                hitByEnemyShipPosition[1] = -1;
                                secondAttack = false;
                                enemyHitTheShip = false;
                                shipAxis = '?';
                                simpleOtherDamagedSegmentsPosX.Clear();
                                simpleOtherDamagedSegmentsPosY.Clear();
                            }

                            if (playerIndex == 2)
                            {
                                secondPlayerShipsCount--;

                                if (secondPlayerShipsCount == 0)
                                    someoneWin = true;
                            }
                            else
                            {
                                firstPlayerShipsCount--;

                                if (firstPlayerShipsCount == 0)
                                    someoneWin = true;
                            }
                        }

                        break;
                }

                visiblePlayer[posY, posX] = player[posY, posX];
            }
            else
            {
                attackIsPossible = false;
                attackProblemText = "The cell is already attacked. Try another cell.";
                goto RepeatAttack;
            }

            string attacker = playerIndex == 1 ? "Enemy" : "You";
            
            ClearMessage(new string('*', 170), 0, 20, false, false, true);
            ClearMessage(new string('*', 170), 1, 20, false, false, true);

            if (someoneWin)
            {
                if (playerIndex == 2) //player won
                {
                    ClearMessage(new string('*', 170), 0, 20, false, false, true);
                    ClearMessage(new string('*', 170), 1, 20, false, false, true);

                    DisplayMessage("You have won! The gods of the sea blessed you. Enter \"play\" if you want to play again and \"exit\" if you are tired and want to finish.", 0, 10, false, false);
                    firstPlayerIsWinner = true;
                }
                else //enemy won
                {
                    ClearMessage(new string('*', 170), 0, 20, false, false, true);
                    ClearMessage(new string('*', 170), 1, 20, false, false, true);

                    DisplayMessage("The enemy has won. Luck was chasing you but you were faster! Try a little slower next time. Enter \"play\" if you want to play again and \"exit\" if you are tired and want to finish.", 0, 10, false, false);
                    firstPlayerIsWinner = false;
                }
                return;
            }
            else if (!shipIsDestroyed)
                DisplayMessage($"{attacker} attacked {cellId}. {attacker} {attackReport}", 0, 10, false, false);
            else if (playerIndex == 2)
                DisplayMessage("Excellent! *PAUSE*The enemy ship has been destroyed. Now you have extra move.", 0, 10, false, false);
            else
                DisplayMessage("Bad news! *PAUSE*The enemy has destroyed your ship. Now he has extra move.", 0, 10, false, false);

            shipIsDestroyed = false;

            if (!someoneWin)
                Thread.Sleep(1500);

            if (hasExtraMove)
            {
                hasExtraMove = false;
                attackIsPossible = true;
                goto RepeatAttack;
            }

        }

        static void Main(string[] args)
        {
            Console.Title = "Sea Battle";
            ConsoleScaling(40, 80);

            //start

            ////greetings and instruction
            DisplayMessage("Expand the game to full screen to avoid bugs/glitches. *PAUSE*Then press any key to read short instruction and start sea battle.", 0, 50, true, true);


            DisplayMessage("Hi! *PAUSE*I am Dima and this is my game \"Sea Battle\". *PAUSE*I have spent 7 days and about 36 hours of continuous work making it. *PAUSE*Press any key to read short instruction.", 1, 50, true, true);

            DisplayMessage("Each player have 10 ships : 1 four-segmented, 2 three-segmented, 3 two-segmented and 4 one-segmented. *PAUSE*You have to destroy all enemy ships and hope that enemy won't do that earlier.", 2, 50, true, false);

            DisplayMessage("\t\t\t\t\t\t\t  Damaged", 8, 20, true, false); //don't tell me about this 4 lines, i know this is horrible
            DisplayMessage("\t\t\t  Full\t\t  Alive\t\t  Alive\t\t  Destroyed", 9, 20, true, false);
            DisplayMessage("\t  Water\t\t  Water\t\t  Ship\t\t  Ship\t\t  Ship", 10, 20, true, false);
            DisplayMessage("\t  Cell\t\t  Cell\t\t  Cell\t\t  Cell \t\t  Cell", 11, 20, true, false);

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
            DisplayMessage("", 2, 50, true, true);

            DisplayMessage("When you shoot the water and there is no ship there, it means you missed and the water will become full. Also you can hit the ship or destroy it. In this case, you will get extra move.", 3, 50, true, true);

            DisplayMessage("The ships are randomly generated. *PAUSE*You have to make your move by specifying cell without spaces. *PAUSE*Example : \"4-B\", \"10-G\"", 4, 50, true, true);

            DisplayMessage("Press any key to start sea battle. *PAUSE*Good luck!", 5, 50, true, true);
            Console.Clear();

            bool isRestarted = false;

            RESTART:
            //drawing field
            FillFieldsAtTheStart(playerOneField, playerOneVisibleField);
            FillFieldsAtTheStart(playerTwoField, playerTwoVisibleField);

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

            RequestError:
            if (isRestarted)
                DisplayMessage("The ships are successfully generated randomly. Enter \"restart\" if you want to randomly generate ships again. Enter \"play\" if you want to start the game. ", 0, 10, false, false);
            else
                DisplayMessage("This is your field with randomly generated ships. Enter \"restart\" if you want to randomly generate ships again. Enter \"play\" if you want to start the game.", 0, 20, false, false);

            switch (Console.ReadLine())
            {
                case "restart":
                    if (isRestarted)
                    {
                        ClearMessage("The ships are successfully generated randomly. Enter \"restart\" if you want to randomly generate ships again. Enter \"play\" if you want to start the game. ", 0, 10, false, false, true);
                        ClearMessage(new string('*', 170), 1, 20, false, false, true);
                    }
                    else
                    {
                        ClearMessage("This is your field with randomly generated ships. Enter \"restart\" if you want to randomly generate ships again. Enter \"play\" if you want to start the game.", 0, 20, false, false, true);
                        ClearMessage(new string('*', 170), 1, 20, false, false, true);
                    }

                    isRestarted = true;
                    goto RESTART;
                case "play":
                    break;
                default:
                    ClearMessage(new string('*', 170), 0, 20, false, false, true);
                    ClearMessage(new string('*', 170), 1, 20, false, false, true);
                    DisplayMessage("Invalid request. Press any key to start again.", 0, 10, false, false);
                    Console.ReadKey();
                    ClearMessage(new string('*', 170), 1, 20, false, false, true);
                    ClearMessage(new string('*', 170), 1, 20, false, false, true);

                    isRestarted = false;
                    goto RequestError;
            }

            ClearMessage(new string('*', 170), 0, 20, false, false, true);
            ClearMessage(new string('*', 170), 1, 20, false, false, true);

            //playing cycle
            while (!someoneWin)
            {
                //1 player move
                Attack(1);

                if (someoneWin)
                    break;

                //2 player move
                Attack(2);
            }


            //winning message is outputted in Attack() function.

            WinningInputError:
            switch (Console.ReadLine())
            {
                case "play":
                    firstPlayerShipsCount = 10;
                    secondPlayerShipsCount = 10;
                    someoneWin = false;
                    otherDamagedSegmentsPosX.Clear();
                    otherDamagedSegmentsPosY.Clear();
                    
                    ClearMessage(new string('*', 170), 0, 20, false, false, true);
                    ClearMessage(new string('*', 170), 1, 20, false, false, true);

                    goto RESTART;
                case "exit":
                    Environment.Exit(0);
                    break;
                default:
                    ClearMessage(new string('*', 170), 0, 20, false, false, true);
                    ClearMessage(new string('*', 170), 1, 20, false, false, true);

                    DisplayMessage("Invalid request. Press any key to start again.", 0, 10, false, false);
                    Console.ReadKey();

                    ClearMessage(new string('*', 170), 0, 20, false, false, true);
                    ClearMessage(new string('*', 170), 1, 20, false, false, true);

                    goto WinningInputError;
            }

            //ClearMessage("Press any key to start sea battle.", 0 , true, true);

            //EditOneCell("3-B", playerOneField, damagedAliveShipCell, 1, true);
            //EditOneCell("2-B", playerOneField, aliveShipCell, 1, true);

            Console.ReadKey();
        }
    }
}