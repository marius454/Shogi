using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using C = Constants;

public class BoardState
{
    // public List<(int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, bool promoted, int x, int y)> shogiPieceState;
    // public List<(int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, int x, int y)> captureBoardPlayer1State;
    // public List<(int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, int x, int y)> captureBoardPlayer2State;
    public (int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, bool promoted)[,] shogiPieceState;
    public (int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, int x, int y)[,] captureBoardPlayer1State;
    public (int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, int x, int y)[,] captureBoardPlayer2State;
    public BoardState (AIBoardManager board){
        // shogiPieceState = new List<(int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, bool promoted, int x, int y)>();
        // captureBoardPlayer1State = new List<(int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, int x, int y)>();
        // captureBoardPlayer2State = new List<(int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, int x, int y)>();
        
        // foreach (ShogiPiece piece in board.ShogiPieces){
        //     if (piece)
        //         shogiPieceState.Add((piece.id, piece.player, piece.pieceType, piece.isPromoted, piece.CurrentX, piece.CurrentY));
        // }
        // foreach (ShogiPiece piece in board.player1.capturedPieces){
        //     if (piece)
        //         captureBoardPlayer1State.Add((piece.id, piece.player, piece.pieceType, piece.CurrentX, piece.CurrentY));
        // }
        // foreach (ShogiPiece piece in board.player2.capturedPieces){
        //     if (piece)
        //         captureBoardPlayer2State.Add((piece.id, piece.player, piece.pieceType, piece.CurrentX, piece.CurrentY));
        // }

        shogiPieceState = new (int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, bool promoted)[C.numberRows, C.numberRows];
        captureBoardPlayer1State = new (int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, int x, int y)[C.captureNumberColumns, C.captureNumberRows];
        captureBoardPlayer2State = new (int pieceIndex, PlayerNumber playerNumber, PieceType pieceType, int x, int y)[C.captureNumberColumns, C.captureNumberRows];
        ShogiPiece piece;
        for (int x = 0; x < C.numberRows; x++)
            for (int y = 0; y < C.numberRows; y++){
                piece = board.ShogiPieces[x,y];
                if (piece)
                    shogiPieceState[x,y] = (piece.id, piece.player, piece.pieceType, piece.isPromoted);
                else shogiPieceState[x,y] = (-1, PlayerNumber.Player1, PieceType.pawn, false);
            }
        for (int x = 0; x < C.captureNumberColumns; x++)
            for (int y = 0; y < C.captureNumberRows; y++){
                piece = board.player1.captureBoard.capturedPieces[x,y];
                if (piece)
                    captureBoardPlayer1State[x,y] = (piece.id, piece.player, piece.pieceType, piece.CurrentX, piece.CurrentY);
                else captureBoardPlayer1State[x,y] = (-1, PlayerNumber.Player1, PieceType.pawn, x, y);
            }
        for (int x = 0; x < C.captureNumberColumns; x++)
            for (int y = 0; y < C.captureNumberRows; y++){
                piece = board.player2.captureBoard.capturedPieces[x,y];
                if (piece)
                    captureBoardPlayer2State[x,y] = (piece.id, piece.player, piece.pieceType, piece.CurrentX, piece.CurrentY);
                else captureBoardPlayer2State[x,y] = (-1, PlayerNumber.Player1, PieceType.pawn, x, y);
            }
    }
}
