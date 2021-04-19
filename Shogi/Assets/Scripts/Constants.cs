using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    // Shogi piece idices
    public const int king = 0;
    public const int rook = 1;
    public const int bishop = 2;
    public const int gold = 3;
    public const int silver = 4;
    public const int knight = 5;
    public const int lance = 6;
    public const int pawn = 7;

    // Numbers coresponding to players (seems self explanatory but helps with code readability)
    public const short player1 = 1;
    public const short player2 = 2;

    // Board specifics
    public const float tileSize = 1.0f;
    public const float tileOffset = 0.5f;
    public const int numberRows = 9;

    // Directions for orthagonal lines in ShogiPiece class and subclasses
    public const int left = 0;
    public const int right = 1;
    public const int forward = 2;
    public const int back = 3;

    // Directions for diagonal lines in ShogiPiece class and subclasses
    public const int forwardLeft = 0;
    public const int forwardRight = 1;
    public const int backLeft = 2;
    public const int backRight = 3;
}
