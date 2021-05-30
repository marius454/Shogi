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

    public static List<List<BoardState>> boardStates;
    private static int selectedDepth;


    private static void SaveState(AIBoardManager board, int currentDebth){
        boardStates[currentDebth].Add(new BoardState(board));
    }
    public static float EvaluatePosition(SimulatedBoard simBoard, AIBoardManager boardManager){
        float Score;

        float materialScore = EvaluateMaterial(simBoard, boardManager.AIPlayer.playerNumber, boardManager.localPlayer.playerNumber);
        float controlScore = EvaluateControlledTiles(simBoard, boardManager.AIPlayer.playerNumber, boardManager.localPlayer.playerNumber);
        float kingSafetyScore = EvaluateKingSafety(simBoard, boardManager.AIPlayer.playerNumber, boardManager.localPlayer.playerNumber);

        Score = materialScore + controlScore + kingSafetyScore;

        return Score;
    }
    private static float EvaluateMaterial(SimulatedBoard simBoard, PlayerNumber currentPlayer, PlayerNumber opponentPlayer){
        // calculate all the material a player has based on the set values
        // could experiment with giving a slightly lower score to pieces that are in the capture board
        float currentPlayerMaterial = 0f;
        float opponentPlayerMaterial = 0f;

        for (int x = 0; x < C.numberRows; x++)
            for (int y = 0; y < C.numberRows; y++){
                if (!simBoard.mainBoard[x,y].empty){
                    if (simBoard.mainBoard[x,y].player == currentPlayer){
                        currentPlayerMaterial += GetMaterialValue(simBoard.mainBoard[x, y]);
                    }else{
                        opponentPlayerMaterial += GetMaterialValue(simBoard.mainBoard[x, y]);
                    }
                }
            }
        return currentPlayerMaterial - opponentPlayerMaterial;
    }
    private static float GetMaterialValue(SimulatedBoard.Piece piece){
        float materialValue = 0;
        if (piece.pieceType == PieceType.pawn){
            if (piece.isPromoted) materialValue += goldValue;
            else materialValue += pawnValue;
            if (piece.isCaptured) materialValue -= capturedPieceDeficit;
            return materialValue;
        }
        else if (piece.pieceType == PieceType.lance){
            if (piece.isPromoted) materialValue += goldValue;
            else materialValue += lanceValue;
            if (piece.isCaptured) materialValue -= capturedPieceDeficit;
            return materialValue;
        }
        else if (piece.pieceType == PieceType.knight){
            if (piece.isPromoted) materialValue += goldValue;
            else materialValue += knightValue;
            if (piece.isCaptured) materialValue -= capturedPieceDeficit;
            return materialValue;
        }
        else if (piece.pieceType == PieceType.silver){
            if (piece.isPromoted) materialValue += goldValue;
            else materialValue += silverValue;
            if (piece.isCaptured) materialValue -= capturedPieceDeficit;
            return materialValue;
        }
        else if (piece.pieceType == PieceType.gold){
            materialValue += goldValue;
            if (piece.isCaptured) materialValue -= capturedPieceDeficit;
            return materialValue;
        }
        else if (piece.pieceType == PieceType.bishop){
            if (piece.isPromoted) materialValue += promotedBishopValue;
            else materialValue += bishopValue;
            if (piece.isCaptured) materialValue -= capturedPieceDeficit;
            return materialValue;
        }
        else if (piece.pieceType == PieceType.rook){
            if (piece.isPromoted) materialValue += promotedRookValue;
            else materialValue += rookValue;
            if (piece.isCaptured) materialValue -= capturedPieceDeficit;
            return materialValue;
        }
        else{
            return kingValue;
        }
    }
    private static float EvaluateControlledTiles(SimulatedBoard simBoard, PlayerNumber currentPlayer, PlayerNumber opponentPlayer){
        float currentPlayerScore = 0f;
        float opponentPlayerScore = 0f;
        bool[,] currentPlayerAttackedTiles = GetAttacketTiles(simBoard, currentPlayer);
        bool[,] opponentPlayerAttackedTiles = GetAttacketTiles(simBoard, opponentPlayer);

        for (int x = 0; x < C.numberRows; x++)
            for (int y = 0; y < C.numberRows; y++){
                // make own camp more valuable, but might need to add a special case so that it doesn't move pieces out of the camp only to have more attacked tiles there
                if (y <= 2){
                    if (currentPlayer == PlayerNumber.Player1){
                        if (currentPlayerAttackedTiles[x, y]) currentPlayerScore += controledCampTileValue;
                        if (opponentPlayerAttackedTiles[x, y]) opponentPlayerScore += controledTileValue;
                    }
                    if (currentPlayer == PlayerNumber.Player2){
                        if (currentPlayerAttackedTiles[x, y]) currentPlayerScore += controledTileValue;
                        if (opponentPlayerAttackedTiles[x, y]) opponentPlayerScore += controledCampTileValue;
                    }
                }
                if (y >= C.numberRows - 3){
                    if (currentPlayer == PlayerNumber.Player1){
                        if (currentPlayerAttackedTiles[x, y]) currentPlayerScore += controledTileValue;
                        if (opponentPlayerAttackedTiles[x, y]) opponentPlayerScore += controledCampTileValue;
                    }
                    if (currentPlayer == PlayerNumber.Player2){
                        if (currentPlayerAttackedTiles[x, y]) currentPlayerScore += controledCampTileValue;
                        if (opponentPlayerAttackedTiles[x, y]) opponentPlayerScore += controledTileValue;
                    }
                }
                else if (currentPlayerAttackedTiles[x, y]) currentPlayerScore += controledTileValue;
                else if (opponentPlayerAttackedTiles[x, y]) opponentPlayerScore += controledTileValue;
        
            }
        return currentPlayerScore - opponentPlayerScore;
    }
    private static float EvaluateKingSafety(SimulatedBoard simBoard, PlayerNumber currentPlayer, PlayerNumber opponentPlayer){
        // King safety is more valueble in the later stages of the game
        // Would be good to add a consideration for attackers
        float currentPlayerScore = 0f;
        float opponentPlayerScore = 0f;

        for (int x = 0; x < C.numberRows; x++)
            for (int y = 0; y < C.numberRows; y++){
                if (!simBoard.mainBoard[x, y].empty && simBoard.mainBoard[x, y].pieceType == PieceType.king){
                    SimulatedBoard.Piece king = simBoard.mainBoard[x, y];
                    bool[,] kingMoves = simBoard.GetPiecePossibleMoves(king, true);
                    int a = 1;
                    for (int t = -a; t <= a; t++)
                        for (int s = -a; s <= a; s++){
                            if (!(t == 0 && s == 0))
                                if (king.x + t >= 0 && king.y + s >= 0 && king.x + t < C.numberRows && king.y + s < C.numberRows){

                                    if (kingMoves[king.x + t, king.y + s]){
                                        if (king.player == currentPlayer)
                                            currentPlayerScore += baseKingEscapeTileValue;
                                        else opponentPlayerScore += baseKingEscapeTileValue;
                                    }
                                    else if (!simBoard.mainBoard[king.x + t, king.y + s].empty)
                                        if (simBoard.mainBoard[king.x + t, king.y + s].player == currentPlayer && king.player == currentPlayer)
                                            currentPlayerScore += baseKingProtectedTileValue;
                                        if (simBoard.mainBoard[king.x + t, king.y + s].player == opponentPlayer && king.player == opponentPlayer)
                                            opponentPlayerScore += baseKingProtectedTileValue;
                                }
                        }
                }
            }
        return currentPlayerScore - opponentPlayerScore;
        
    }
    private static bool[,] GetAttacketTiles(SimulatedBoard simBoard, PlayerNumber player){
        bool[,] attackedTiles = new bool[C.numberRows, C.numberRows];
        for (int x = 0; x < C.numberRows; x++)
            for (int y = 0; y < C.numberRows; y++){
                if (!simBoard.mainBoard[x, y].empty && simBoard.mainBoard[x, y].player == player){
                    bool[,] moves = simBoard.GetPiecePossibleMoves(simBoard.mainBoard[x, y], true);
                    for (int X=0; X < C.numberRows; X++)
                        for (int Y=0; Y < C.numberRows; Y++){
                            if (moves[X,Y]){
                                attackedTiles[X,Y] = true;
                            }
                        }
                }
            }
        return attackedTiles;
    }

    public static Move GetAIMove(int depth, AIBoardManager board){
        selectedDepth = depth;
        
        boardStates = new List<List<BoardState>>();
        for (int i = 0; i < depth; i++){
            boardStates.Add(new List<BoardState>());
        }
        SimulatedBoard simBoard = new SimulatedBoard(board);
        // List<Move> simulatedMoves = simBoard.GetAllMoves(board.AIPlayer.playerNumber);
        SaveState(board, 0);
        // Move bestMove = MiniMax(debth, maximizing, float.MinValue, float.MaxValue, board);
        Move bestMove = MiniMax(depth, true, float.MinValue, float.MaxValue, simBoard, board);

        return bestMove;
    }
    public static Move MiniMax(int depth, bool maximize, float alpha, float beta, SimulatedBoard simBoard, AIBoardManager boardManager){
        if (boardManager.gameOver){
            boardManager.AiThread.Abort();
        }

        // List<Move> AIMoves = simBoard.GetAllMoves(boardManager.AIPlayer.playerNumber);
        // List<Move> localPlayerMoves = simBoard.GetAllMoves(boardManager.localPlayer.playerNumber);

        float currentScore;
        currentScore = EvaluatePosition(simBoard, boardManager);
        // if (AIMoves.Count <= 0){
        //     // currentScore -= kingValue;
        //     Debug.Log("AIPlayer lose score " + currentScore);
        // }
        // if (localPlayerMoves.Count <= 0){
        //     // currentScore += kingValue;
        //     Debug.Log("AIPlayer win score " + currentScore);
        // }
        
        if (depth == 0 || currentScore > kingValue/10 || currentScore < -kingValue/10) 
            return new Move{pieceX = -1, pieceY = -1, targetX = -1, targetY = -1, promote = false, score = currentScore};

        // Debug.Log("AI debth " + depth);
        float tempScore;
        // BoardState boardState = new BoardState(board);

        if (maximize){
            List<Move> AIMoves = simBoard.GetAllMoves(boardManager.AIPlayer.playerNumber);
            Move bestMove = RandomMove(AIMoves);
            float maxScore = float.MinValue;
            SimulatedBoard tempSimBoard = new SimulatedBoard(simBoard);

            foreach (Move move in AIMoves){
                simBoard.DoMove(move);
                tempScore = MiniMax(depth-1, false, alpha, beta, simBoard, boardManager).score;
                simBoard.CopyBoard(tempSimBoard);

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
            List<Move> localPlayerMoves = simBoard.GetAllMoves(boardManager.localPlayer.playerNumber);
            Move bestMove = RandomMove(localPlayerMoves);
            float minScore = float.MaxValue;
            SimulatedBoard tempSimBoard = new SimulatedBoard(simBoard);

            foreach (Move move in localPlayerMoves){
                simBoard.DoMove(move);
                tempScore = MiniMax(depth-1, true, alpha, beta, simBoard, boardManager).score;
                simBoard.CopyBoard(tempSimBoard);

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




    // public static float EvaluatePosition(AIBoardManager board, ShogiPlayer player){
    //     ShogiPlayer opponentPlayer;
    //     ShogiPlayer currentPlayer;
    //     if (player == board.player1){
    //         currentPlayer = board.player1;
    //         opponentPlayer = board.player2;
    //     }
    //     else {
    //         currentPlayer = board.player2;
    //         opponentPlayer = board.player1;
    //     }

    //     float Score;

    //     float currentPlayerMaterial = EvaluateMaterial(currentPlayer);
    //     float opponentPlayerMaterial = EvaluateMaterial(opponentPlayer);
    //     float currentPlayerControledTiles = EvaluateControlledTiles(currentPlayer);
    //     float opponentPlayerControledTiles = EvaluateControlledTiles(opponentPlayer);
    //     float currentPlayerKingSafety = EvaluateKingSafety(board, currentPlayer);
    //     float opponentPlayerKingSafety = EvaluateKingSafety(board, opponentPlayer);

    //     float materialScore = currentPlayerMaterial - opponentPlayerMaterial;
    //     float controlScore = currentPlayerControledTiles - opponentPlayerControledTiles;
    //     float kingSafetyScore = currentPlayerKingSafety - opponentPlayerKingSafety;

    //     Score = materialScore + controlScore + kingSafetyScore;

    //     return Score;
    // }
    // private static float EvaluateMaterial(ShogiPlayer player){
    //     // calculate all the material a player has based on the set values
    //     // could experiment with giving a slightly lower score to pieces that are in the capture board
    //     float materialSum = 0f;
    //     foreach (ShogiPiece piece in player.allPieces){
    //         if (piece is Pawn){
    //             if (piece.isPromoted) materialSum += goldValue;
    //             else materialSum += pawnValue;
    //             if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
    //         }
    //         if (piece is Lance){
    //             if (piece.isPromoted) materialSum += goldValue;
    //             else materialSum += lanceValue;
    //             if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
    //         }
    //         if (piece is Knight){
    //             if (piece.isPromoted) materialSum += goldValue;
    //             else materialSum += knightValue;
    //             if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
    //         }
    //         if (piece is SilverGeneral){
    //             if (piece.isPromoted) materialSum += goldValue;
    //             else materialSum += silverValue;
    //             if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
    //         }
    //         if (piece is GoldGeneral){
    //             materialSum += goldValue;
    //             if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
    //         }
    //         if (piece is Bishop){
    //             if (piece.isPromoted) materialSum += promotedBishopValue;
    //             else materialSum += bishopValue;
    //             if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
    //         }
    //         if (piece is Rook){
    //             if (piece.isPromoted) materialSum += promotedRookValue;
    //             else materialSum += rookValue;
    //             if (player.capturedPieces.Contains(piece)) materialSum -= capturedPieceDeficit;
    //         }
    //         if (piece is King){
    //             materialSum += kingValue;
    //         }
    //     }
    //     return materialSum;
    // }
    // private static float EvaluateControlledTiles(ShogiPlayer player){
    //     float score = 0f;
    //     bool[,] attackedTiles = GetAttacketTiles(player);
    //     for (int x = 0; x < C.numberRows; x++)
    //         for (int y = 0; y < C.numberRows; y++){
    //             if (attackedTiles[x, y]) score += controledTilesValue;
    //             // make own camp more valuable, but might need to add a special case so that it doesn't move pieces out of the camp only to have more attacked tiles there
    //             if (player.playerNumber == PlayerNumber.Player1 && y <= 2){
    //                 score += controledCampTileValue;
    //             }
    //             if (player.playerNumber == PlayerNumber.Player2 && y >= C.numberRows - 3){
    //                 score += controledCampTileValue;
    //             }
    //         }
    //     return score;
    // }
    // private static float EvaluateKingSafety(AIBoardManager board, ShogiPlayer player){
    //     // King safety is more valueble in the later stages of the game
    //     // Would be good to add a consideration for attackers
    //     float score = 0f;
    //     bool[,] kingMoves = player.king.PossibleMoves();
    //     bool[,] attackedTiles = GetAttacketTiles(player, false);

    //     int a = 1;
    //     for (int t = -a; t <= a; t++)
    //         for (int s = -a; s <= a; s++){
    //             if (!(t == 0 && s == 0))
    //                 if (player.king.CurrentX + t >= 0 && player.king.CurrentY + s >= 0 && player.king.CurrentX + t < C.numberRows && player.king.CurrentY + s < C.numberRows){

    //                     if (kingMoves[player.king.CurrentX + t, player.king.CurrentY + s])
    //                         score += baseKingEscapeTileValue;
    //                     else if (board.ShogiPieces[player.king.CurrentX + t, player.king.CurrentY + s])
    //                         if (board.ShogiPieces[player.king.CurrentX + t, player.king.CurrentY + s].player == player.playerNumber)
    //                             score += baseKingProtectedTileValue;
    //                     // else if (attackedTiles[player.king.CurrentX + t, player.king.CurrentY + s])
    //                     //     score += baseKingProtectedTileValue;
    //                 }
    //         }
    //     return score;
    // }
    // private static bool[,] GetAttacketTiles(ShogiPlayer player, bool includeKing = true){
    //     bool[,] attackedTiles = new bool[C.numberRows, C.numberRows];
    //     foreach(ShogiPiece piece in player.piecesInPlay){
    //         if (piece is King && !includeKing) continue;
    //         bool[,] moves = piece.PossibleMoves(); 
    //         for (int x=0; x < C.numberRows; x++)
    //             for (int y=0; y < C.numberRows; y++){
    //                 if (moves[x,y]){
    //                     attackedTiles[x,y] = true;
    //                 }
    //             }
    //     }
    //     return attackedTiles;
    // }

    // public static Move MiniMax(int depth, bool maximize, float alpha, float beta, AIBoardManager board){
    //     if (board.gameOver){
    //         board.AiThread.Abort();
    //     }
    //     board.AIPlayer.CalculatePossibleMoves();
    //     board.localPlayer.CalculatePossibleMoves();

    //     float currentScore;
    //     currentScore = EvaluatePosition(board, board.AIPlayer);
    //     if (!board.AIPlayer.hasPossibleMoves){
    //         currentScore -= kingValue;
    //         Debug.Log("AIPlayer lose score " + currentScore);
    //     }
    //     if (!board.localPlayer.hasPossibleMoves){
    //         currentScore += kingValue;
    //         Debug.Log("AIPlayer win score " + currentScore);
    //     }
        
    //     if (depth == 0 || !board.AIPlayer.hasPossibleMoves || !board.localPlayer.hasPossibleMoves) 
    //         return new Move{pieceX = -1, pieceY = -1, targetX = -1, targetY = -1, promote = false, score = currentScore};

    //     Debug.Log("AI debth " + depth);
    //     float tempScore;

    //     // BoardState boardState = new BoardState(board);

    //     if (maximize){
    //         Move bestMove = board.RandomMove(board.AIPlayer);
    //         List<Move> possibleMoves = board.AIPlayer.possibleMoves;
    //         float maxScore = float.MinValue;

    //         foreach (Move move in possibleMoves){
    //             board.DoMove(move, true);
    //             if (depth > 1) SaveState(board, selectedDepth - depth + 1);
    //             tempScore = MiniMax(depth-1, false, alpha, beta, board).score;
    //             board.RestoreGameState(boardStates[selectedDepth - depth][boardStates[selectedDepth - depth].Count - 1], false);
    //             if (tempScore > maxScore){
    //                 maxScore = tempScore;
    //                 if (depth == selectedDepth)
    //                     bestMove = move;
    //             }
    //             alpha = Math.Max(tempScore, alpha);
    //             if (beta <= alpha){
    //                 break;
    //             }
    //         }
            
    //         bestMove.score = maxScore;
    //         return bestMove;
    //     }
    //     else{
    //         Move bestMove = board.RandomMove(board.localPlayer);
    //         List<Move> possibleMoves = board.localPlayer.possibleMoves;

    //         float minScore = float.MaxValue;
    //         foreach (Move move in possibleMoves){
    //             board.DoMove(move, true);
    //             if (depth > 1) SaveState(board, selectedDepth - depth + 1);
    //             tempScore = MiniMax(depth-1, true, alpha, beta, board).score;
    //             board.RestoreGameState(boardStates[selectedDepth - depth][boardStates[selectedDepth - depth].Count - 1], false);
    //             if (tempScore < minScore){
    //                 minScore = tempScore;
    //                 if (depth == selectedDepth)
    //                     bestMove = move;
    //             }
    //             beta = Math.Min(tempScore, beta);
    //             if (beta <= alpha){
    //                 break;
    //             }
    //         }
    //         bestMove.score = minScore;
    //         return bestMove;
    //     }
    // }
}
