using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using C = Constants;


public class SimulatedBoard
{
    public struct Piece{
        public bool empty;
        public int x;
        public int y;
        public PieceType pieceType;
        public PlayerNumber player;
        public bool isPromoted;
        public bool isAttacked;
        public bool isCaptured;
    }
    public Piece[,] mainBoard;
    public Piece[,] player1CaptureBoard;
    public Piece[,] player2CaptureBoard;
    // private List<Piece> player1Captures;
    // private List<Piece> player2Captures;
    public SimulatedBoard originalBoard;

    public SimulatedBoard(BoardManager board){
        mainBoard = new Piece[C.numberRows, C.numberRows];
        for (int x = 0; x < C.numberRows; x++)
            for (int y = 0; y < C.numberRows; y++){
                if (board.ShogiPieces[x, y]){
                    mainBoard[x, y] = new Piece{empty = false, x = x, y = y, pieceType = board.ShogiPieces[x, y].pieceType, 
                        player = board.ShogiPieces[x, y].player, isPromoted = board.ShogiPieces[x, y].isPromoted, 
                        isAttacked = false, isCaptured = false};
                }
                else mainBoard[x, y] = new Piece{empty = true, isCaptured = false};
            }
        
        player1CaptureBoard = new Piece[C.captureNumberColumns, C.captureNumberRows];
        player2CaptureBoard = new Piece[C.captureNumberColumns, C.captureNumberRows];

        for (int x = 0; x < C.captureNumberColumns; x++)
            for (int y = 0; y < C.captureNumberRows; y++){
                if (board.player1.captureBoard.capturedPieces[x, y]){
                    player1CaptureBoard[x, y] = new Piece{empty = false, x = x, y = y, pieceType = board.player1.captureBoard.capturedPieces[x, y].pieceType, 
                        player = board.player1.captureBoard.capturedPieces[x, y].player, isPromoted = board.player1.captureBoard.capturedPieces[x, y].isPromoted, 
                        isAttacked = false, isCaptured = true};
                }
                else player1CaptureBoard[x, y] = new Piece{empty = true, isCaptured = true};

                if (board.player2.captureBoard.capturedPieces[x, y]){
                    player2CaptureBoard[x, y] = new Piece{empty = false, x = x, y = y, pieceType = board.player2.captureBoard.capturedPieces[x, y].pieceType, 
                        player = board.player2.captureBoard.capturedPieces[x, y].player, isPromoted = board.player2.captureBoard.capturedPieces[x, y].isPromoted, 
                        isAttacked = false, isCaptured = true};
                }
                else player2CaptureBoard[x, y] = new Piece{empty = true, isCaptured = true};
            }


        // player1Captures = new List<Piece>();
        // player2Captures = new List<Piece>();

        // foreach (ShogiPiece piece in board.player1.captureBoard.capturedPieces){
        //     if (piece){
        //         if (!player1Captures.Exists(item => item.pieceType == piece.pieceType)){
        //             player1Captures.Add(new Piece{empty = false, x = piece.CurrentX, y = piece.CurrentY, 
        //             pieceType = piece.pieceType, 
        //             player = piece.player, 
        //             isPromoted = piece.isPromoted, 
        //             isAttacked = false, numberOfCaptures = 1});
        //         }
        //         else{
        //             int index = player1Captures.FindIndex(item => item.pieceType == piece.pieceType);
        //             player1Captures[index] = new Piece{empty = false, x = piece.CurrentX, y = piece.CurrentY, 
        //             pieceType = piece.pieceType, 
        //             player = piece.player, 
        //             isPromoted = piece.isPromoted, 
        //             isAttacked = false, 
        //             numberOfCaptures = player1Captures[index].numberOfCaptures + 1};
        //         }
        //     }
        // }
        // foreach (ShogiPiece piece in board.player2.captureBoard.capturedPieces){
        //     if (piece){
        //         if (!player2Captures.Exists(item => item.pieceType == piece.pieceType)){
        //             player2Captures.Add(new Piece{empty = false, x = piece.CurrentX, y = piece.CurrentY, 
        //             pieceType = piece.pieceType, 
        //             player = piece.player, 
        //             isPromoted = piece.isPromoted, 
        //             isAttacked = false});
        //         }
        //         else{
        //             int index = player2Captures.FindIndex(item => item.pieceType == piece.pieceType);
        //             player2Captures[index] = new Piece{empty = false, x = piece.CurrentX, y = piece.CurrentY, 
        //             pieceType = piece.pieceType, 
        //             player = piece.player, 
        //             isPromoted = piece.isPromoted, 
        //             isAttacked = false, 
        //             numberOfCaptures = player2Captures[index].numberOfCaptures + 1};
        //         }
        //     }
        // }
    }
    public SimulatedBoard (SimulatedBoard board){
        CopyBoard(board);
    }
    public void CopyBoard(SimulatedBoard board){
        mainBoard = board.mainBoard.Clone() as Piece[,];
        player1CaptureBoard = board.player1CaptureBoard.Clone() as Piece[,];
        player2CaptureBoard = board.player2CaptureBoard.Clone() as Piece[,];
    }
    public void DoMove(Move move){
        Debug.Log(move.pieceX + " " + move.pieceY + " " + move.targetX + " " + move.targetY + " " + move.promote);
        if (move.pieceX >= 0 && move.pieceY >= 0 && move.pieceX < C.numberRows && move.pieceY < C.numberRows){
            if (!mainBoard[move.targetX, move.targetY].empty){
                bool stopLooking = false;
                for (int x = 0; x < C.captureNumberColumns; x++){
                    for (int y = 0; y < C.captureNumberRows; y++){
                        if (mainBoard[move.targetX, move.targetY].player == PlayerNumber.Player1){
                            if (player1CaptureBoard[x, y].empty){
                                player1CaptureBoard[x, y] = mainBoard[move.targetX, move.targetY];
                                player1CaptureBoard[x, y].isCaptured = true;
                                stopLooking = true;
                                break;
                            }
                        }
                        if (mainBoard[move.targetX, move.targetY].player == PlayerNumber.Player2){
                            if (player2CaptureBoard[x, y].empty){
                                player2CaptureBoard[x, y] = mainBoard[move.targetX, move.targetY];
                                player2CaptureBoard[x, y].isCaptured = true;
                                stopLooking = true;
                                break;
                            }
                        }
                    }
                    if (stopLooking){
                        break;
                    }
                }
            }
            // if (move.pieceX == 4 && move.pieceY == 0 && move.targetX == 3 && move.targetY == 0){
            //     Debug.Log("bad");
            // }
            mainBoard[move.targetX, move.targetY] = mainBoard[move.pieceX, move.pieceY];
            mainBoard[move.pieceX, move.pieceY].empty = true;
        }
        else if (move.pieceX >= (C.numberRows + 1) && move.pieceY >= (C.numberRows - C.captureNumberRows) 
          && move.pieceX <= (C.numberRows + C.captureNumberColumns) 
          && move.pieceY <= C.captureNumberColumns){
            mainBoard[move.targetX, move.targetY] = player1CaptureBoard[move.pieceX, move.pieceY];
            mainBoard[move.targetX, move.targetY].isCaptured = false;
            player1CaptureBoard[move.pieceX, move.pieceY].empty = true;


            // mainBoard[move.targetX, move.targetY] = player1Captures.Find(item => item.x == move.pieceX && item.y == move.pieceY);
            // int index = player1Captures.FindIndex(item => item.x == move.pieceX && item.y == move.pieceY);
            // player1Captures[index] = new Piece{empty = true};
        }
        else if (move.pieceX >= (-1 - C.captureNumberColumns) && move.pieceY >= 0 
          && move.pieceX <= -2
          && move.pieceY <= (C.numberRows - 1)){
            mainBoard[move.targetX, move.targetY] = player2CaptureBoard[move.pieceX, move.pieceY];
            mainBoard[move.targetX, move.targetY].isCaptured = false;
            player2CaptureBoard[move.pieceX, move.pieceY].empty = true;



            // mainBoard[move.targetX, move.targetY] = player2Captures.Find(item => item.x == move.pieceX && item.y == move.pieceY);
            // if (player2Captures.Find(item => item.x == move.pieceX && item.y == move.pieceY).numberOfCaptures > 1){
            //     Piece piece = player2Captures.Find(item => item.x == move.pieceX && item.y == move.pieceY);
            //     int index = player2Captures.FindIndex(item => item.pieceType == piece.pieceType);
            //     player2Captures[index] = new Piece{empty = false, x = piece.x, y = piece.x, 
            //     pieceType = piece.pieceType, 
            //     player = piece.player, 
            //     isPromoted = piece.isPromoted, 
            //     isAttacked = false, 
            //     numberOfCaptures = player2Captures[index].numberOfCaptures - 1};
            // }
            // else {
            //     int index = player2Captures.FindIndex(item => item.x == move.pieceX && item.y == move.pieceY);
            //     player2Captures[index] = new Piece{empty = true};
            // }
        }
    }
    

    public List<Move> GetAllMoves(PlayerNumber player, bool checkForSelfCheck = true, bool checkDrops = true){
        List<Move> possibleMoves = new List<Move>();
        if (player == PlayerNumber.Player1){
            foreach (Piece piece in mainBoard){
                if (!piece.empty){
                    if (piece.player == PlayerNumber.Player1){
                        bool[,] moves = GetPiecePossibleMoves(piece, checkForSelfCheck);
                        for (int x=0; x < C.numberRows; x++)
                            for (int y=0; y < C.numberRows; y++){
                                if (moves[x,y]){
                                    if (CheckIfCouldBePromoted(piece, y))
                                        possibleMoves.Add(new Move{pieceX = piece.x, pieceY = piece.y, 
                                            targetX = x, targetY = y, promote = true});
                                    if (!CheckIfMustPromote(piece, y))
                                        possibleMoves.Add(new Move{pieceX = piece.x, pieceY = piece.y, 
                                            targetX = x, targetY = y, promote = false});
                                    
                                }
                            }
                    }
                }
            }
            if (checkDrops){
                List<PieceType> calculatedTypes = new List<PieceType>();
                foreach (Piece piece in player1CaptureBoard){
                    if (!piece.empty){
                        if (!calculatedTypes.Exists(item => item == piece.pieceType)){
                        calculatedTypes.Add(piece.pieceType);
                            bool[,] moves = GetPiecePossibleDrops(piece, checkForSelfCheck);
                            for (int x=0; x < C.numberRows; x++)
                                for (int y=0; y < C.numberRows; y++){
                                    if (moves[x,y]){
                                        possibleMoves.Add(new Move{pieceX = piece.x, pieceY = piece.y, 
                                            targetX = x, targetY = y, promote = false});
                                    }
                                }
                        }
                    }
                }
            }  
        }
        else{
            foreach (Piece piece in mainBoard){
                if (!piece.empty){
                    if (piece.player == PlayerNumber.Player2){
                        bool[,] moves = GetPiecePossibleMoves(piece, checkForSelfCheck);
                        for (int x=0; x < C.numberRows; x++)
                            for (int y=0; y < C.numberRows; y++){
                                if (moves[x,y]){
                                    possibleMoves.Add(new Move{pieceX = piece.x, pieceY = piece.y, 
                                            targetX = x, targetY = y, promote = false});
                                }
                            }
                    }
                }
            }
            if (checkDrops){
                List<PieceType> calculatedTypes = new List<PieceType>();
                foreach (Piece piece in player2CaptureBoard){
                    if (!piece.empty){
                        if (!calculatedTypes.Exists(item => item == piece.pieceType)){
                        calculatedTypes.Add(piece.pieceType);
                            bool[,] moves = GetPiecePossibleDrops(piece, checkForSelfCheck);
                            for (int x=0; x < C.numberRows; x++)
                                for (int y=0; y < C.numberRows; y++){
                                    if (moves[x,y]){
                                        possibleMoves.Add(new Move{pieceX = piece.x, pieceY = piece.y, 
                                            targetX = x, targetY = y, promote = false});
                                    }
                                }
                        }
                    }
                }
            } 
        }
        return possibleMoves;
    }
    private bool CheckIfCouldBePromoted(Piece piece, int y){
        if (piece.pieceType == PieceType.king || piece.pieceType == PieceType.gold){
            return false;
        }
        if (piece.player == PlayerNumber.Player1){
            if (y >= C.numberRows - 3) {
                return true;
            }
            return false;
        }
        else{
            if (y <= 2) {
                return true;
            }
            return false;
        }
    }
    private bool CheckIfMustPromote(Piece piece, int y){
        if (piece.pieceType == PieceType.king || piece.pieceType == PieceType.gold){
            return false;
        }
        if (piece.player == PlayerNumber.Player1){
            if (piece.pieceType == PieceType.lance || piece.pieceType == PieceType.pawn){
                if (y >= C.numberRows - 1) return true;
            }
            if (piece.pieceType == PieceType.knight){
                if (y >= C.numberRows - 2) return true;
            }
            return false;
        }
        else{
            if (piece.pieceType == PieceType.lance || piece.pieceType == PieceType.pawn){
                if (y <= 0) return true;
            }
            if (piece.pieceType == PieceType.knight){
                if (y <= 1) return true;
            }
            return false;
        }
    }
    private Piece GetKing(PlayerNumber player){
        Piece king = new Piece{empty = true};
        foreach (Piece gamePiece in mainBoard){
            if (!gamePiece.empty){
                if (gamePiece.pieceType == PieceType.king && gamePiece.player == player){
                    king = gamePiece;
                    break;
                }
            }
        }
        return king;
    }

    #region Piece Moves
    public bool[,] GetPiecePossibleMoves(Piece piece, bool checkForSelfCheck){
        // Don't need to check if king is attacked if the minimax debth is more than 1
        if (piece.pieceType == PieceType.bishop)
            return BishopMoves(piece, checkForSelfCheck);
        if (piece.pieceType == PieceType.gold)
            return GoldMoves(piece, checkForSelfCheck);
        if (piece.pieceType == PieceType.king)
            return KingMoves(piece, checkForSelfCheck);
        if (piece.pieceType == PieceType.knight)
            return KnightMoves(piece, checkForSelfCheck);
        if (piece.pieceType == PieceType.lance)
            return LanceMoves(piece, checkForSelfCheck);
        if (piece.pieceType == PieceType.pawn)
            return PawnMoves(piece, checkForSelfCheck);
        if (piece.pieceType == PieceType.rook)
            return RookMoves(piece, checkForSelfCheck);
        if (piece.pieceType == PieceType.silver)
            return SilverMoves(piece, checkForSelfCheck);

        return new bool[C.numberRows, C.numberRows];
    }
    private bool[,] BishopMoves(Piece piece, bool checkForSelfCheck){
        bool [,] moves = new bool[C.numberRows,C.numberRows];

        // Forward left
        DiagonalLine(moves, DirectionDiagonal.forwardLeft, piece);
        // Forward right
        DiagonalLine(moves, DirectionDiagonal.forwardRight, piece);
        // Backward left
        DiagonalLine(moves, DirectionDiagonal.backLeft, piece);
        // Backward right
        DiagonalLine(moves, DirectionDiagonal.backRight, piece);

        if (piece.isPromoted){
            SingleMove(moves, piece.x, piece.y + 1, piece);
            SingleMove(moves, piece.x, piece.y - 1, piece);
            SingleMove(moves, piece.x + 1, piece.y, piece);
            SingleMove(moves, piece.x - 1, piece.y, piece);
        }

        moves = RemoveIllegalMoves(moves, piece, checkForSelfCheck);
        return moves;
    }
    private bool[,] GoldMoves(Piece piece, bool checkForSelfCheck){
        bool [,] moves = new bool[C.numberRows,C.numberRows];
        int a = 1;
        if (piece.player == PlayerNumber.Player1){
            for (int t = -a; t <= a; t++)
                for (int s = -a; s <= a; s++){
                    if (!(t == 0 && s == 0) && !(t == 1 && s == -1) && !(t == -1 && s == -1))
                        SingleMove(moves, piece.x + t, piece.y + s, piece);
                }
        }
        else if (piece.player == PlayerNumber.Player2) {
            for (int t = -a; t <= a; t++)
                for (int s = -a; s <= a; s++){
                    if (!(t == 0 && s == 0) && !(t == 1 && s == 1) && !(t == -1 && s == 1))
                        SingleMove(moves, piece.x + t, piece.y + s, piece);
                }
        }
        else {
            throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        }
        moves = RemoveIllegalMoves(moves, piece, checkForSelfCheck);

        return moves;
    }
    private bool[,] KingMoves(Piece piece, bool checkForSelfCheck){
        bool [,] moves = new bool[C.numberRows,C.numberRows];
        int a = 1;

        for (int t = -a; t <= a; t++)
            for (int s = -a; s <= a; s++){
                if (!(t == 0 && s == 0))
                    SingleMove(moves, piece.x + t, piece.y + s, piece);
            }
        moves = RemoveIllegalMoves(moves, piece, checkForSelfCheck);
        
        return moves;
    }
    private bool[,] KnightMoves(Piece piece, bool checkForSelfCheck){
        bool [,] moves = new bool[C.numberRows,C.numberRows];

        if (!piece.isPromoted){
            if (piece.player == PlayerNumber.Player1){
                SingleMove(moves, piece.x + 1, piece.y + 2, piece);
                SingleMove(moves, piece.x - 1, piece.y + 2, piece);
            }
            else if (piece.player == PlayerNumber.Player2) {
                SingleMove(moves, piece.x + 1, piece.y - 2, piece);
                SingleMove(moves, piece.x - 1, piece.y - 2, piece);
            }
            else throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        }
        else{
            return GoldMoves(piece, checkForSelfCheck);
        }

        moves = RemoveIllegalMoves(moves, piece, checkForSelfCheck);
        return moves;
    }
    private bool[,] LanceMoves(Piece piece, bool checkForSelfCheck){
        bool [,] moves = new bool[C.numberRows,C.numberRows];

        if (!piece.isPromoted){
            if (piece.player == PlayerNumber.Player1)
                OrthagonalLine(moves, DirectionOrthagonal.forward, piece);
            else if (piece.player == PlayerNumber.Player2) 
                OrthagonalLine(moves, DirectionOrthagonal.back, piece);
            else throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        }
        else{
            return GoldMoves(piece, checkForSelfCheck);
        }

        moves = RemoveIllegalMoves(moves, piece, checkForSelfCheck);
        return moves;
    }
    private bool[,] PawnMoves(Piece piece, bool checkForSelfCheck){
        bool [,] moves = new bool[C.numberRows,C.numberRows];

        if (!piece.isPromoted){
            if (piece.player == PlayerNumber.Player1)
                SingleMove(moves, piece.x, piece.y + 1, piece);
            else if (piece.player == PlayerNumber.Player2)
                SingleMove(moves, piece.x, piece.y - 1, piece);
            else throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
        }
        else {
            return GoldMoves(piece, checkForSelfCheck);
        }
        
        moves = RemoveIllegalMoves(moves, piece, checkForSelfCheck);
        return moves;
    }
    private bool[,] RookMoves(Piece piece, bool checkForSelfCheck){
        bool [,] moves = new bool[C.numberRows,C.numberRows];

        // Right
        OrthagonalLine(moves, DirectionOrthagonal.right, piece);
        // Left
        OrthagonalLine(moves, DirectionOrthagonal.left, piece);
        // Forwards
        OrthagonalLine(moves, DirectionOrthagonal.forward, piece);
        // Backwards
        OrthagonalLine(moves, DirectionOrthagonal.back, piece);
        
        if (piece.isPromoted){
            SingleMove(moves, piece.x + 1, piece.y + 1, piece);
            SingleMove(moves, piece.x - 1, piece.y + 1, piece);
            SingleMove(moves, piece.x + 1, piece.y - 1, piece);
            SingleMove(moves, piece.x - 1, piece.y - 1, piece);
        }

        moves = RemoveIllegalMoves(moves, piece, checkForSelfCheck);
        return moves;
    }
    private bool[,] SilverMoves(Piece piece, bool checkForSelfCheck){
        bool [,] moves = new bool[C.numberRows,C.numberRows];
        int a = 1;

        if (!piece.isPromoted){
            // Select all moves in a 3x3 square around the piece, except it's current position and the tile it cannot go
            if (piece.player == PlayerNumber.Player1){
                for (int t = -a; t <= a; t++)
                    for (int s = -a; s <= a; s++){
                        if (!(t == 0 && s == 0) && !(t == 1 && s == 0) && !(t == -1 && s == 0) && !(t == 0 && s == -1))
                            SingleMove(moves, piece.x + t, piece.y + s, piece);
                    }
            }
            else if (piece.player == PlayerNumber.Player2) {
                for (int t = -a; t <= a; t++)
                    for (int s = -a; s <= a; s++){
                        if (!(t == 0 && s == 0) && !(t == 1 && s == 0) && !(t == -1 && s == 0) && !(t == 0 && s == 1))
                            SingleMove(moves, piece.x + t, piece.y + s, piece);
                    }
            }
            else {
                throw new InvalidOperationException("An invalid value has been set for the ShogiPiece 'player' variable");
            }
        }
        else {
            return GoldMoves(piece, checkForSelfCheck);
        }
        
        moves = RemoveIllegalMoves(moves, piece, checkForSelfCheck);
        return moves;
    }
    #endregion
    
    #region Template Moves
    private void SingleMove(bool[,] moves, int x, int y, Piece currentPiece){
        Piece potentialTile;
        
        if (x >= 0 && y >= 0 && x < C.numberRows && y < C.numberRows){
            potentialTile = mainBoard[x, y];
            if (potentialTile.empty == true || potentialTile.player != currentPiece.player){
                moves[x, y] = true;
            }
        }
    }
    private bool OrthagonalLine(bool[,] moves, DirectionOrthagonal direction, Piece piece, bool checkForAttacker = false){
        int x = piece.x;
        int y = piece.y;
        Piece potentialTile;
        while (true){
            if (direction == DirectionOrthagonal.right) x++;
            else if (direction == DirectionOrthagonal.forward) y++;
            else if (direction == DirectionOrthagonal.left) x--;
            else if (direction == DirectionOrthagonal.back) y--;

            if (x < 0 || y < 0 || x >= C.numberRows || y >= C.numberRows)
                break;

            potentialTile = mainBoard[x, y];
            if (potentialTile.empty == true){
                moves[x, y] = true;
            } else{
                if (potentialTile.player != piece.player){
                    moves[x, y] = true;
                    if (checkForAttacker){
                        if (potentialTile.pieceType == PieceType.rook 
                         || (piece.player == PlayerNumber.Player1 && direction == DirectionOrthagonal.forward && potentialTile.pieceType == PieceType.lance && !potentialTile.isPromoted)
                         || (piece.player == PlayerNumber.Player2 && direction == DirectionOrthagonal.back && potentialTile.pieceType == PieceType.lance && !potentialTile.isPromoted)){
                            return true;
                        }
                    }
                }
                break;
            }
        }
        return false;
    }
    private bool DiagonalLine(bool[,] moves, DirectionDiagonal direction, Piece piece, bool checkForAttacker = false){
        int x = piece.x;
        int y = piece.y;
        Piece potentialTile;
        while (true){
            if (direction == DirectionDiagonal.forwardLeft) {x--; y++;}
            else if (direction == DirectionDiagonal.forwardRight) {x++; y++;}
            else if (direction == DirectionDiagonal.backLeft) {x--; y--;}
            else if (direction == DirectionDiagonal.backRight) {x++; y--;}

            if (x < 0 || y < 0 || x >= C.numberRows || y >= C.numberRows)
                break;

            potentialTile = mainBoard[x, y];
            if (potentialTile.empty == true){
                moves[x, y] = true;
            } else{
                if (potentialTile.player != piece.player){
                    moves[x, y] = true;
                    if (checkForAttacker){
                        if (potentialTile.pieceType == PieceType.bishop){
                            return true;
                        } 
                    }
                }
                break;
            }
        }
        return false;
    }
    #endregion
    
    #region Illegal Move removal
    private bool[,] RemoveIllegalMoves(bool[,] moves, Piece piece, bool checkForSelfCheck){
        if (checkForSelfCheck){
            for (int x=0; x < C.numberRows; x++)
                for (int y=0; y < C.numberRows; y++){
                    if (moves[x,y]){
                        // check if after this move the players' King will be under attack
                        if(CheckIfMoveWillCauseSelfCheck(x, y, piece)) moves[x, y] = false;
                    }
                }
        }
        return moves;
    }
    private bool CheckIfMoveWillCauseSelfCheck(int x, int y, Piece piece){
        bool wouldCauseSelfCheck = false;

        Piece[,] tempBoard = mainBoard.Clone() as Piece[,];
        
        mainBoard[x, y] = piece;
        mainBoard[piece.x, piece.y].empty = true;
        mainBoard[x, y].x = x;
        mainBoard[x, y].y = y;

        Piece king = GetKing(piece.player);
        if (king.empty){
            Debug.Break();
            throw new InvalidOperationException("King not found!");
        }
        wouldCauseSelfCheck = CheckIfBeingAttacked(king);

        mainBoard = tempBoard.Clone() as Piece[,];
        return wouldCauseSelfCheck;
    }
    // Don't check for repetition check, instead if a king is attacked 4 time in a row cout that as a loss for the attacker
    private bool CheckIfBeingAttacked(Piece piece)
    {
        bool[,] possibleLocations = new bool[C.numberRows, C.numberRows];
        int a = 1;

        // Mark all possible locations from where an enemy piece might attack and check if the the piece in that location has a move that can attack
        if (piece.player == PlayerNumber.Player1){
            if (DiagonalLine(possibleLocations, DirectionDiagonal.forwardLeft, piece, true))
                {piece.isAttacked = false; return true;}
            if (DiagonalLine(possibleLocations, DirectionDiagonal.forwardRight, piece, true))
                {piece.isAttacked = false; return true;}
            if (DiagonalLine(possibleLocations, DirectionDiagonal.backLeft, piece, true))
                {piece.isAttacked = false; return true;}
            if (DiagonalLine(possibleLocations, DirectionDiagonal.backRight, piece, true))
                {piece.isAttacked = false; return true;}
            
            if (OrthagonalLine(possibleLocations, DirectionOrthagonal.forward, piece, true))
                {piece.isAttacked = false; return true;}
            if (OrthagonalLine(possibleLocations, DirectionOrthagonal.left, piece, true))
                {piece.isAttacked = false; return true;}
            if (OrthagonalLine(possibleLocations, DirectionOrthagonal.right, piece, true))
                {piece.isAttacked = false; return true;}
            if (OrthagonalLine(possibleLocations, DirectionOrthagonal.back, piece, true))
                {piece.isAttacked = false; return true;}

            for (int s = a; s >= -a; s--)
                for (int t = a; t >= -a; t--){
                    if (!(t == 0 && s == 0)){
                        SingleMove(possibleLocations, piece.x + t, piece.y + s, piece);
                        if (piece.x  + t >= 0 && piece.y + s >= 0 && piece.x  + t < C.numberRows && piece.y + s < C.numberRows){
                            if (CheckForAttacker(possibleLocations[piece.x  + t, piece.y + s], piece, mainBoard[piece.x  + t, piece.y + s], t, s)){
                                return true;
                            }
                        }
                    }
                }
        }
        else{
            if (DiagonalLine(possibleLocations, DirectionDiagonal.backLeft, piece, true))
                {piece.isAttacked = false; return true;}
            if (DiagonalLine(possibleLocations, DirectionDiagonal.backRight, piece, true))
                {piece.isAttacked = false; return true;}
            if (DiagonalLine(possibleLocations, DirectionDiagonal.forwardLeft, piece, true))
                {piece.isAttacked = false; return true;}
            if (DiagonalLine(possibleLocations, DirectionDiagonal.forwardRight, piece, true))
                {piece.isAttacked = false; return true;}
            
            if (OrthagonalLine(possibleLocations, DirectionOrthagonal.back, piece, true))
                {piece.isAttacked = false; return true;}
            if (OrthagonalLine(possibleLocations, DirectionOrthagonal.right, piece, true))
                {piece.isAttacked = false; return true;}
            if (OrthagonalLine(possibleLocations, DirectionOrthagonal.left, piece, true))
                {piece.isAttacked = false; return true;}
            if (OrthagonalLine(possibleLocations, DirectionOrthagonal.forward, piece, true))
                {piece.isAttacked = false; return true;}

            for (int s = -a; s <= a; s++)
                for (int t = -a; t <= a; t++){
                    if (!(t == 0 && s == 0)){
                        SingleMove(possibleLocations, piece.x  + t, piece.y + s, piece);
                        if (piece.x  + t >= 0 && piece.y + s >= 0 && piece.x  + t < C.numberRows && piece.y + s < C.numberRows){
                            if (CheckForAttacker(possibleLocations[piece.x  + t, piece.y + s], piece, mainBoard[piece.x  + t, piece.y + s], t, s)){
                                return true;
                            }
                        }
                    }
                }
        }
        int y = 2;
        if (piece.player == PlayerNumber.Player2) y = -2;
        if (piece.x  - 1 >= 0 && piece.y + y >= 0 && piece.x  - 1 < C.numberRows && piece.y + y < C.numberRows){
            SingleMove(possibleLocations, piece.x  - 1, piece.y + y, piece);
            if (CheckForAttacker(possibleLocations[piece.x  - 1, piece.y + y], piece, mainBoard[piece.x  - 1, piece.y + y], -1, y)){
                return true;
            }
        }
        if (piece.x  + 1 >= 0 && piece.y + y >= 0 && piece.x  + 1 < C.numberRows && piece.y + y < C.numberRows){
        SingleMove(possibleLocations, piece.x  + 1, piece.y + y, piece);
            if (CheckForAttacker(possibleLocations[piece.x  + 1, piece.y + y], piece, mainBoard[piece.x  + 1, piece.y + y], 1, y)){
                return true;
            }
        }
        return false;
    }
    private bool CheckForAttacker(bool isPossibleLocation, Piece thisPiece, Piece targetPiece, int t, int s){
        if (!isPossibleLocation) return false;
        if (targetPiece.empty == true) return false;

        if (s != 2 && s != -2){
            if ((targetPiece.pieceType == PieceType.rook && targetPiece.isPromoted) 
             || (targetPiece.pieceType == PieceType.bishop && targetPiece.isPromoted)
             || (targetPiece.pieceType == PieceType.king))
                return true;
        }
        if (thisPiece.player == PlayerNumber.Player2){
            t = -t;
            s = -s;
        }
        if ((t==-1 && s==1) || (t==1 && s==1)){
            if ((targetPiece.pieceType == PieceType.pawn && targetPiece.isPromoted) 
                || (targetPiece.pieceType == PieceType.knight && targetPiece.isPromoted)
                || (targetPiece.pieceType == PieceType.lance && targetPiece.isPromoted)
                || (targetPiece.pieceType == PieceType.silver)
                || (targetPiece.pieceType == PieceType.gold))
                    return true;
        }
        else if ((t==0 && s==1)){
            if ((targetPiece.pieceType == PieceType.pawn) 
                || (targetPiece.pieceType == PieceType.knight && targetPiece.isPromoted)
                || (targetPiece.pieceType == PieceType.lance)
                || (targetPiece.pieceType == PieceType.silver)
                || (targetPiece.pieceType == PieceType.gold))
                    return true;
        }
        else if ((t==-1 && s==0) || (t==0 && s==-1) || (t==1 && s==0)){
            if ((targetPiece.pieceType == PieceType.pawn && targetPiece.isPromoted) 
                || (targetPiece.pieceType == PieceType.knight && targetPiece.isPromoted)
                || (targetPiece.pieceType == PieceType.lance && targetPiece.isPromoted)
                || (targetPiece.pieceType == PieceType.silver && targetPiece.isPromoted)
                || (targetPiece.pieceType == PieceType.gold))
                    return true;
        }
        else if ((t==-1 && s==-1) || (t==1 && s==-1)){
            if (targetPiece.pieceType == PieceType.silver && !targetPiece.isPromoted)
                    return true;
        }
        else if ((t==-1 && s==2) || (t==1 && s==2)){
            if (targetPiece.pieceType == PieceType.knight && !targetPiece.isPromoted)
                    return true;
        }
        return false;
    }
    #endregion

    #region Drops
    private bool[,] GetPiecePossibleDrops(Piece piece, bool checkForSelfCheck = true){
        bool [,] drops = new bool[C.numberRows, C.numberRows];
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (mainBoard[x, y].empty == true){
                    drops[x, y] = true;
                }
            }
        if (piece.pieceType == PieceType.pawn || piece.pieceType == PieceType.lance){
            RemoveDropsLastRows(drops, piece.player);
        }
        else if (piece.pieceType == PieceType.knight){
            RemoveDropsLastRows(drops, piece.player, true);
        }
        RemoveIllegalDrops(drops, piece);
        return drops;
    }
    private void RemoveIllegalDrops(bool[,] drops, Piece piece){
        Piece king = GetKing(piece.player);
        
        // if player is in check only allow drops that would stop the check
        if (!king.empty){
            if(CheckIfBeingAttacked(king)){
                for (int x=0; x < C.numberRows; x++)
                    for (int y=0; y < C.numberRows; y++){
                        if (drops[x,y]){
                            if (CheckIfDropWillCauseSelfCheck(x, y, piece)){
                                drops[x,y] = false;
                            }
                        }
                    }
            }
        }
        if (piece.pieceType == PieceType.pawn){
            RemoveIllegalPawnDrops(drops, piece);
        }
    }
    private bool CheckIfDropWillCauseSelfCheck(int x, int y, Piece piece){
        bool wouldCauseSelfCheck = false;
        mainBoard[x,y] = piece;
        mainBoard[x,y].x = x;
        mainBoard[x,y].y = y;

        Piece king = GetKing(piece.player);
        wouldCauseSelfCheck = CheckIfBeingAttacked(king);
        mainBoard[x,y].empty = true;
        
        return wouldCauseSelfCheck;
    }
    public void RemoveDropsLastRows(bool[,] drops, PlayerNumber player, bool removeNextToLast = false){
        int yLast;
        int yNextToLast;
        if (player == PlayerNumber.Player1){
            yLast = C.numberRows - 1;
            yNextToLast = C.numberRows - 2;
        }
        else {
            yLast = 0;
            yNextToLast = 1;
        }
        for (int x=0; x < C.numberRows; x++){
            drops[x, yLast] = false;
            if (removeNextToLast) drops[x, yNextToLast] = false;
        }
    }

    private void RemoveIllegalPawnDrops(bool[,] drops, Piece piece){
        // cant drop pawn on a row with an unpromoted pawn
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                if (!mainBoard[x, y].empty) {
                    if (mainBoard[x, y].pieceType == PieceType.pawn && !mainBoard[x, y].isPromoted && mainBoard[x, y].player == piece.player){
                        for (int Y=0; Y < C.numberRows; Y++){
                            drops[x, Y] = false;
                        }
                    }
                }
            }
        // Can't drop a pawn in place that would cause checkmate
        for (int x=0; x < C.numberRows; x++)
            for (int y=0; y < C.numberRows; y++){
                int pawnMove;
                if (piece.player == PlayerNumber.Player1 && drops[x, y]) pawnMove = 1;
                else pawnMove = -1;
                if (drops[x, y])
                    if (!mainBoard[x, y+pawnMove].empty && mainBoard[x, y+pawnMove].pieceType == PieceType.king 
                        && mainBoard[x, y+pawnMove].player != piece.player)
                        if (CheckIfDropWillCauseCheckmate(x, y, piece)){
                            drops[x, y] = false;
                        }
            }
    }
    private bool CheckIfDropWillCauseCheckmate(int x, int y, Piece piece){
        bool wouldCauseCheckMate = false;
        mainBoard[x,y] = piece;
        mainBoard[x,y].x = x;
        mainBoard[x,y].y = y;

        List<Move> moves = new List<Move>();
        if (piece.player == PlayerNumber.Player1) moves = GetAllMoves(PlayerNumber.Player2, true, false);
        else moves = GetAllMoves(PlayerNumber.Player1, true, false);

        if (moves.Count == 0){
            wouldCauseCheckMate = true;
        }
        else wouldCauseCheckMate = false;
        mainBoard[x,y].empty = true;
        
        return wouldCauseCheckMate;
    }
    #endregion

}
