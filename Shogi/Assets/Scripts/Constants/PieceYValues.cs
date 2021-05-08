using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PieceYValues
{
    // Height values for pieces so they do not clip through the board
    public const float King = 0.155f;
    public const float Rook = 0.076f;
    public const float Bishop = 0.109f;
    public const float GoldGeneral = 0.149f;
    public const float SilverGeneral = 0.092f;
    public const float Knight = 0.104f;
    public const float Lance = 0.084f;
    public const float Pawn = 0.105f;

    // Height values for promoted pieces
    public const float promotedRook = 0.092f;
    public const float promotedBishop = 0.06f;
    public const float promotedSilverGeneral = 0.073f;
    public const float promotedKnight = 0.054f;
    public const float promotedLance = 0.07f;
    public const float promotedPawn = 0.041f;
}

