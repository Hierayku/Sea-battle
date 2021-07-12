using System;
using System.Linq;
using System.Threading;

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

        static void FillFieldsAtTheStart()
        {
            for (int i = 0; i < playerOneField.GetLength(0); i++)
            {
                for (int j = 0; j < playerOneField.GetLength(1); j++)
                {
                    playerOneField[i, j] = '.';
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

        static void MakeRandomShipPositions(int playerIndex)
        {
            if (playerIndex == 1)
            {
                BuildShip(4, 1);
                BuildShip(3, 1);
                BuildShip(3, 1);
                BuildShip(2, 1);
                BuildShip(2, 1);
                BuildShip(2, 1);
                BuildShip(1, 1);
                BuildShip(1, 1);
                BuildShip(1, 1);
                BuildShip(1, 1);
            }
            //if player 2
            else
            {
                BuildShip(4, 1);
                BuildShip(3, 1);
                BuildShip(3, 1);
                BuildShip(2, 1);
                BuildShip(2, 1);
                BuildShip(2, 1);
                BuildShip(1, 1);
                BuildShip(1, 1);
                BuildShip(1, 1);
                BuildShip(1, 1);
            }
        }

        //Whole method uses only small positions (1-10)!
        static void BuildShip(int shipLength, int playerIndex)
        {
            Random random = new Random();

            char[,] player;
            if (playerIndex == 1)
                player = playerOneField;
            else
                player = playerTwoField;

            int posX = random.Next(1, 10);
            int posY = random.Next(1, 10);

            int[] positionForMethod = new int[2];
            string cellId;

            //checking whether it is possible to place the ship on specified position
            int i;
            for (i = posX; i < posX + shipLength; i++)
            {
                try
                {
                    if (player[posY, i] == '.')
                        continue;
                    else
                        break;
                }
                catch
                {
                    break;
                }
            }
            if (i == posX + shipLength)
            {
                posX -= shipLength;

                //building ship
                for (int j = posX; j < posX + shipLength; j++)
                {
                    positionForMethod[0] = j;
                    positionForMethod[1] = posY;

                    cellId = FindOutSmallPositionReverse(positionForMethod);
                    EditOneCell(cellId, player, aliveShipCell, playerIndex, true);
                }

                //arranging full (filled) Water

                //upper water
                positionForMethod[1] = posY - 1;
                if (positionForMethod[1] > 0)
                {
                    for (int k = posX - 1; k < posX + shipLength + 1; k++)
                    {
                        positionForMethod[0] = k;

                        cellId = FindOutSmallPositionReverse(positionForMethod);

                        EditOneCell(cellId, player, fullWaterCell, playerIndex, true);
                    }
                }

                //water on the sides (left side)
                positionForMethod[0] = posX - 1;

                if (positionForMethod[0] > 0)
                {
                    positionForMethod[1] = posY;

                    cellId = FindOutSmallPositionReverse(positionForMethod);
                    EditOneCell(cellId, player, fullWaterCell, playerIndex, true);
                }
                //water on the sides (right side)
                positionForMethod[0] = posX + shipLength;

                if (positionForMethod[0] < 11)
                {
                    cellId = FindOutSmallPositionReverse(positionForMethod);
                    EditOneCell(cellId, player, fullWaterCell, playerIndex, true);
                }

                //lower water
                positionForMethod[1] = posY + 1;

                if (positionForMethod[1] < 11)
                {
                    for (int k = posX - 1; k < posX + shipLength + 1; k++)
                    {
                        positionForMethod[0] = k;

                        cellId = FindOutSmallPositionReverse(positionForMethod);

                        EditOneCell(cellId, player, fullWaterCell, playerIndex, true);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Console.Title = "Sea Battle";
            ConsoleScaling(40, 80);

            //start

            FillFieldsAtTheStart();

            //drawing field

            //player1
            for (int i = 0; i < playerOneField.GetLength(0); i++) 
            {
                for (int j = 0; j < playerOneField.GetLength(1); j++)
                {
                    DrawOneCell(j, i, emptyWaterCell, 1);
                }
                Console.WriteLine();
            }
            //player 2
            for (int i = 0; i < playerTwoField.GetLength(0); i++)
            {
                for (int j = 13; j < playerTwoField.GetLength(1) + 13; j++)
                {
                    DrawOneCell(j, i, emptyWaterCell, 2);
                }
                Console.WriteLine();
            }

            //drawing boundaries

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

            BuildShip(4, 1);
            //MakeRandomShipPositions(1);
            //MakeRandomShipPositions(2);



            //EditOneCell("3-B", playerOneField, damagedAliveShipCell, 1, true);
            //EditOneCell("2-B", playerOneField, aliveShipCell, 1, true);

            Console.ReadKey();
        }
    }
}
//Заметка на завтра:
//  Появляется ошибка string input format is incorrect (..endOfNumber), endOfNumber == 0
//  Занятая (заполненая) вода иногда появляется на полях с буквами и цифрами (буквы видел, цифры нет)
//  