using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public static class BoardEvaluation
{
    #region Evaluation values
    private const float pawnValue = 1f;
    private const float lanceValue = 3f;
    private const float knightValue = 4f;
    private const float silverValue = 6f;
    private const float goldValue = 6f;
    private const float bishopValue = 8f;
    private const float rookValue = 10f;
    private const float promotedBishopValue = 10f;
    private const float promotedRookValue = 12f;
    private const float kingValue = 10000f;
    private const float capturedPieceDeficit = 0.5f;
    private const float controledTileValue = 0.5f;
    private const float baseKingEscapeTileValue = 2f;
    private const float baseKingProtectedTileValue = 2f;
    #endregion

    private static List<List<BoardState>> boardStates;
    

    private static void SaveState(AIBoardManager board, int currentDebth){
        boardStates[currentDebth].Add(new BoardState(board));
    }
    public static float EvaluatePosition(AIBoardManager board, ShogiPlayer player){
        ShogiPlayer opponentPlayer;
        ShogiPlayer currentPlayer;
        if (player == board.player1){
            currentPlayer = board.player1;
            opponentPlayer = board.player2;
        }
        else {
            currentPlayer = board.player2;
            opponentPlayer = board.player1;
        }

        float Score;

        float currentPlayerMaterial = EvaluateMaterial(currentPlayer);
        float opponentPlayerMaterial = EvaluateMaterial(opponentPlayer);
        float currentPlayerControledTiles = EvaluateControlledTiles(currentPlayer);
        float opponentPlayerControledTiles = EvaluateControlledTiles(opponentPlayer);
        float currentPlayerKingSafety = EvaluateKingSafety(board, currentPlayer);
        float opponentPlayerKingSafety = EvaluateKingSafety(board, opponentPlayer);

        float materialScore = currentPlayerMaterial - opponentPlayerMaterial;
        float controlScore = currentPlayerControledTiles - opponentPlayerControledTiles;
        float kingSafetyScore = currentPlayerKingSafety - opponentPlayerKingSafety;

        Score = materialScore + controlScore + kingSafetyScore;

        return Score;
        // return (player1Score, player2Score);
    }
    private static float EvaluateMaterial(ShogiPlayer player){
        // calculate all the material a player has based on the set values
        // could experiment with giving a slightly lower score to pieces that are in the capture board
        float materialSum = 0f;
        foreach (ShogiPiece piece in player.allPieces){
            if (piece is Pawn){
                if (piece.isPromoted) materialSum += goldValue;
                else materialSum += pawnValue;
                if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
            }
            if (piece is Lance){
                if (piece.isPromoted) materialSum += goldValue;
                else materialSum += lanceValue;
                if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
            }
            if (piece is Knight){
                if (piece.isPromoted) materialSum += goldValue;
                else materialSum += knightValue;
                if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
            }
            if (piece is SilverGeneral){
                if (piece.isPromoted) materialSum += goldValue;
                else materialSum += silverValue;
                if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
            }
            if (piece is GoldGeneral){
                materialSum += goldValue;
                if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
            }
            if (piece is Bishop){
                if (piece.isPromoted) materialSum += promotedBishopValue;
                else materialSum += bishopValue;
                if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
            }
            if (piece is Rook){
                if (piece.isPromoted) materialSum += promotedRookValue;
                else materialSum += rookValue;
                if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
            }
            if (piece is King){
                if (piece.isPromoted) materialSum += goldValue;
                else materialSum += pawnValue;
            }
        }
        return materialSum;
    }
    private static float EvaluateControlledTiles(ShogiPlayer player){
        float score = 0f;
        bool[,] attackedTiles = GetAttacketTiles(player);
        for (int x = 0; x < C.numberRows; x++)
            for (int y = 0; y < C.numberRows; y++){
                if (player.attackedTiles[x, y]) score += 0.5f;
            }
        return score;
    }
    private static float EvaluateKingSafety(AIBoardManager board, ShogiPlayer player){
        // King safety is more valueble in the later stages of the game
        // Would be good to add a consideration for attackers
        float score = 0f;
        bool[,] kingMoves = player.king.PossibleMoves();
        bool[,] attackedTiles = GetAttacketTiles(player, false);

        int a = 1;
        for (int t = -a; t <= a; t++)
            for (int s = -a; s <= a; s++){
                if (!(t == 0 && s == 0))
                    if (player.king.CurrentX + t >= 0 && player.king.CurrentY + s >= 0 && player.king.CurrentX + t < C.numberRows && player.king.CurrentY + s < C.numberRows){

                        if (kingMoves[player.king.CurrentX + t, player.king.CurrentY + s])
                            score += baseKingEscapeTileValue;
                        else if (board.ShogiPieces[player.king.CurrentX + t, player.king.CurrentY + s])
                            if (board.ShogiPieces[player.king.CurrentX + t, player.king.CurrentY + s].player == player.playerNumber)
                                score += baseKingProtectedTileValue;
                        else if (attackedTiles[player.king.CurrentX + t, player.king.CurrentY + s])
                            score += baseKingProtectedTileValue;
                    }
            }

        return score;
    }
    private static bool[,] GetAttacketTiles(ShogiPlayer player, bool includeKing = true){
        bool[,] attackedTiles = new bool[C.numberRows, C.numberRows];
        foreach(ShogiPiece piece in player.piecesInPlay){
            if (piece is King && !includeKing) continue;
            bool[,] moves = piece.PossibleMoves(); 
            for (int x=0; x < C.numberRows; x++)
                for (int y=0; y < C.numberRows; y++){
                    if (moves[x,y]){
                        attackedTiles[x,y] = true;
                    }
                }
        }
        return attackedTiles;
    }

    public static Move MiniMax(AIBoardManager board, ShogiPlayer player){
        // Put board in a certain possition then evaluate that position
        float currentScore = EvaluatePosition(board, player);
        float tempScore = currentScore;
        float maxScore = -100000f;

        Move bestMove = board.RandomMove(board.AIPlayer);
        Move tempMove = new Move{pieceX = 0, pieceY = 0, targetX = 0, targetY = 0, promote = false};

        // This will have to be done every time start testing a new possible move
        boardStates = new List<List<BoardState>>();
        // This will have to be done every time we go deeper into the tree
        boardStates.Add(new List<BoardState>());
        // This will have to be done every time a move in the current debth is done
        SaveState(board, 0);

        List<Move> possibleMoves = player.possibleMoves;
        // foreach ((int pieceX, int pieceY, int targetX, int targetY, bool promote) move in player.possibleMoves){
        //     possibleMoves.Add((move.pieceX, move.pieceY, move.targetX, move.targetY, move.promote));
        // }

        foreach (Move move in possibleMoves){
            board.DoMove(move);
            tempScore = EvaluatePosition(board, player);
            if (tempScore > maxScore){
                    maxScore = tempScore;
                    tempMove = move;
                }
            board.RestoreGameState(boardStates[0][boardStates[0].Count - 1]);
        }
        if (maxScore > currentScore){
            bestMove = tempMove;
        }

        return bestMove;

        // bool tempHasPossibleMoves = player.hasPossibleMoves;
        // bool[,] tempAttackedTile = player.attackedTiles.Clone() as bool[,];
        // int tempNrMoves = player.nrMoves;
        // bool tempIsAttackingKing = player.isAttackingKing;
        //player.CalculatePossibleMoves(true, false);
    }
}
