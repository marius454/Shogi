using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;
using System.Threading;
using System.Threading.Tasks;


public static class BoardEvaluation
{
    #region Evaluation values
    private const float pawnValue = 2f;
    private const float lanceValue = 3f;
    private const float knightValue = 4f;
    private const float silverValue = 5f;
    private const float goldValue = 6f;
    private const float bishopValue = 8f;
    private const float rookValue = 10f;
    private const float promotedBishopValue = 10f;
    private const float promotedRookValue = 12f;
    private const float kingValue = 100000f;
    private const float capturedPieceDeficit = 1f;
    private const float controledTileValue = 1f;
    private const float controledCampTileValue = 1.5f;
    private const float baseKingEscapeTileValue = 3f;
    private const float baseKingProtectedTileValue = 3f;
    #endregion

    public static List<List<BoardState>> boardStates { set; get; }
    private static int selectedDepth;


    private static void SaveState(AIBoardManager board, int currentDepth){
        boardStates[currentDepth].Add(new BoardState(board));
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
                materialSum += kingValue;
            }
        }
        return materialSum;
    }
    private static float EvaluateControlledTiles(ShogiPlayer player){
        float score = 0f;
        bool[,] attackedTiles = GetAttacketTiles(player);
        for (int x = 0; x < C.numberRows; x++)
            for (int y = 0; y < C.numberRows; y++){
                if (attackedTiles[x, y]) score += controledTileValue;
                // make own camp more valuable, but might need to add a special case so that it doesn't move pieces out of the camp only to have more attacked tiles there
                if (player.playerNumber == PlayerNumber.Player1 && y <= 2){
                    score += controledCampTileValue;
                }
                if (player.playerNumber == PlayerNumber.Player2 && y >= C.numberRows - 3){
                    score += controledCampTileValue;
                }
            }
        return score;
    }
    private static float EvaluateKingSafety(AIBoardManager board, ShogiPlayer player){
        // King safety is more valueble in the later stages of the game
        // Would be good to add a consideration for attackers
        float score = 0f;
        bool[,] kingMoves = player.king.PossibleMoves();
        // bool[,] attackedTiles = GetAttacketTiles(player, false);

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
                        // else if (attackedTiles[player.king.CurrentX + t, player.king.CurrentY + s])
                        //     score += baseKingProtectedTileValue;
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

    public static Move RandomMove(List<Move> moves){
        System.Random random = new System.Random();
        if (moves.Count <= 0){
            return new Move{pieceX = -1, pieceY = -1, targetX = -1, targetY = -1, promote = false};
        }
        int randomMove = random.Next(moves.Count - 1);
        int pieceX = moves[randomMove].pieceX;
        int pieceY = moves[randomMove].pieceY;
        int targetX = moves[randomMove].targetX;
        int targetY = moves[randomMove].targetY;
        bool promote = moves[randomMove].promote;

        return new Move{pieceX = pieceX, pieceY = pieceY, targetX = targetX, targetY = targetY, promote = promote};
    }
    
    public static Move GetAIMove(int depth, AIBoardManager board){
        selectedDepth = depth;
        
        boardStates = new List<List<BoardState>>();
        for (int i = 0; i < depth; i++){
            boardStates.Add(new List<BoardState>());
        }
        SaveState(board, 0);
        Move bestMove = MiniMax(depth, true, float.MinValue, float.MaxValue, board);

        return bestMove;
    }
    public static Move MiniMax(int depth, bool maximize, float alpha, float beta, AIBoardManager board){
        if (board.gameOver){
            board.AiThread.Abort();
        }

        float currentScore;
        currentScore = EvaluatePosition(board, board.AIPlayer);
        
        if (depth == 0 || currentScore > kingValue/10 || currentScore < -kingValue/10) 
            return new Move{pieceX = -1, pieceY = -1, targetX = -1, targetY = -1, promote = false, score = currentScore};

        float tempScore;

        if (maximize){
            board.AIPlayer.CalculatePossibleMoves();
            Move bestMove = board.RandomMove(board.AIPlayer);
            List<Move> possibleMoves = board.AIPlayer.possibleMoves;
            float maxScore = float.MinValue;

            foreach (Move move in possibleMoves){
                board.DoMove(move, true);
                if (depth > 1) SaveState(board, selectedDepth - depth + 1);
                tempScore = MiniMax(depth-1, false, alpha, beta, board).score;
                board.RestoreGameState(boardStates[selectedDepth - depth][boardStates[selectedDepth - depth].Count - 1], false);
                if (tempScore > maxScore){
                    maxScore = tempScore;
                    if (depth == selectedDepth)
                        bestMove = move;
                }
                alpha = Math.Max(tempScore, alpha);
                if (beta <= alpha){
                    break;
                }
            }
            
            bestMove.score = maxScore;
            return bestMove;
        }
        else{
            board.localPlayer.CalculatePossibleMoves();
            Move bestMove = board.RandomMove(board.localPlayer);
            List<Move> possibleMoves = board.localPlayer.possibleMoves;
            float minScore = float.MaxValue;

            foreach (Move move in possibleMoves){
                board.DoMove(move, true);
                if (depth > 1) SaveState(board, selectedDepth - depth + 1);
                tempScore = MiniMax(depth-1, true, alpha, beta, board).score;
                board.RestoreGameState(boardStates[selectedDepth - depth][boardStates[selectedDepth - depth].Count - 1], false);
                if (tempScore < minScore){
                    minScore = tempScore;
                    if (depth == selectedDepth)
                        bestMove = move;
                }
                beta = Math.Min(tempScore, beta);
                if (beta <= alpha){
                    break;
                }
            }
            bestMove.score = minScore;
            return bestMove;
        }
    }
}
