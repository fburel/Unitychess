using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;

namespace Assets.Scripts
{
    public class ClassicChessReferee : IChessGameReferee
    {

        #region public API

        public int BoardSize
        {
            get { return 8; }
        }

        public IChessBoardRefereeDelegate Delegate { get; set; }

        public GameState CurrentGameState { get; private set; }

        public Chessman this[Square position]
        {
            get
            {
                var idx = (position.Row - 1) * 8 + position.Column - 1;
                return Board[idx];
            }
            private set
            {
                var idx = (position.Row - 1) * 8 + position.Column - 1;
                Board[idx] = value;
            }
        }

        public PlayerColor CurrentPlayer { get; private set; }

        public void ResetBoard()
        {
            CurrentGameState = GameState.Playing;

            Board = new Chessman[64];

            CurrentPlayer = PlayerColor.White;

            for (int i = 0; i < BoardSize; i++)
            {

                SetPiece(new Square(2, i + 1), PieceType.Pawn, PlayerColor.White);
                SetPiece(new Square(7, i + 1), PieceType.Pawn, PlayerColor.Black);
                switch (i)
                {
                    case 0:
                    case 7:
                        SetPiece(new Square(1, i + 1), PieceType.Rook, PlayerColor.White);
                        SetPiece(new Square(8, i + 1), PieceType.Rook, PlayerColor.Black);
                        break;
                    case 1:
                    case 6:
                        SetPiece(new Square(1, i + 1), PieceType.Knight, PlayerColor.White);
                        SetPiece(new Square(8, i + 1), PieceType.Knight, PlayerColor.Black);
                        break;
                    case 2:
                    case 5:
                        SetPiece(new Square(1, i + 1), PieceType.Bishop, PlayerColor.White);
                        SetPiece(new Square(8, i + 1), PieceType.Bishop, PlayerColor.Black);
                        break;
                    case 3:
                        SetPiece(new Square(1, i + 1), PieceType.Queen, PlayerColor.White);
                        SetPiece(new Square(8, i + 1), PieceType.Queen, PlayerColor.Black);
                        break;
                    case 4:
                        SetPiece(new Square(1, i + 1), PieceType.King, PlayerColor.White);
                        SetPiece(new Square(8, i + 1), PieceType.King, PlayerColor.Black);
                        break;
                }
            }
            
            GenerateMoveList();
        }

        public IList<Chessman> EatenChessman { get; private set; }

        public IMove GetMove(Square start, Square landing)
        {
            return PossibleMoveList.FirstOrDefault(x => x.StartSquare.Equals(start) && x.LandingSquare.Equals(landing));
        }

        public void DoMove(IMove move)
        {
            var chessman = this[move.StartSquare];
            this[move.StartSquare] = null;
            this[move.LandingSquare] = chessman;

            CurrentPlayer = (PlayerColor) (-1 * (int) CurrentPlayer);
            GenerateMoveList();
        }

        public HashSet<IMove> PossibleMoveList { get; private set; }

        public bool CanMove(Square square)
        {
            // TODO : if "square" is empty, would CurrentPlayer's king be attacked ?
            return this[square] != null && this[square].Color == CurrentPlayer;
        }
        
        #endregion

        private Chessman[] Board;

        private void SetPiece(Square square, PieceType type, PlayerColor color)
        {
            var idx = (square.Row - 1) * 8 + square.Column - 1;
            Board[idx] = new Chessman(type, color);
        }
        
        
        private bool IsValidPosition(int[] p)
        {
            return Math.Abs(p[0] * 2 - 9) < 8 && Math.Abs(p[1] * 2 - 9) < 8;
        }

        private void GenerateMoveList()
        {
            HashSet<IMove> resp = new HashSet<IMove>();

            for (int i = 0; i < 64; i++)
            {
                HashSet<Square> landingPositions = new HashSet<Square>();
                Chessman chessman = Board[i];
                if (chessman == null || chessman.Color != CurrentPlayer) continue;

                // Start calculate possible landing position, based on move patterns and pieces on board
                int[] currentPosition = {i / 8 + 1, i % 8 + 1};
                
                if (chessman.Type == PieceType.Rook || chessman.Type == PieceType.Queen)
                {
                    int[][] movePatterns = {new[]{0, 1}, new[]{0, -1}, new[]{1, 0}, new[]{-1, 0}};
                    
                    for(int j = 0; j < 4; j++)
                    {
                        var mvPtrn = movePatterns[j];
                        int[] evaluatedPosition = {0, 0};
                        evaluatedPosition[0] = currentPosition[0];
                        evaluatedPosition[1] = currentPosition[1];
                        while (true)
                        {
                            evaluatedPosition[0] = evaluatedPosition[0] + mvPtrn[0];
                            evaluatedPosition[1] = evaluatedPosition[1] + mvPtrn[1];
                            // out of board
                            if (!IsValidPosition(evaluatedPosition)) break;
                            var s = new Square(evaluatedPosition[0], evaluatedPosition[1]);
                            var occupant = this[s];
                            if (occupant == null ||
                                occupant.Color != CurrentPlayer && occupant.Type != PieceType.King)
                            {
                                landingPositions.Add(s);
                            }
                            if(occupant != null) break;
                        }
                        
                    }
                }
                if (chessman.Type == PieceType.Bishop || chessman.Type == PieceType.Queen)
                {
                    int[][] movePatterns = {new[]{1, 1}, new[]{-1, -1}, new[]{1, -1}, new[]{-1, 1}};

                    for(int j = 0; j < 4; j++)
                    {
                        var mvPtrn = movePatterns[j];
                        int[] evaluatedPosition = {0, 0};
                        evaluatedPosition[0] = currentPosition[0];
                        evaluatedPosition[1] = currentPosition[1];
                        while (true)
                        {
                            evaluatedPosition[0] = evaluatedPosition[0] + mvPtrn[0];
                            evaluatedPosition[1] = evaluatedPosition[1] + mvPtrn[1];
                            // out of board
                            if (!IsValidPosition(evaluatedPosition)) break;
                            var s = new Square(evaluatedPosition[0], evaluatedPosition[1]);
                            var occupant = this[s];
                            if (occupant == null ||
                                occupant.Color != CurrentPlayer && occupant.Type != PieceType.King)
                            {
                                landingPositions.Add(s);
                            }
                            if(occupant != null) break;
                        }
                        
                    }
                }
                if (chessman.Type == PieceType.Knight)
                {
                    int[][] movePatterns =
                    {
                        new[]{2, 1}, new[]{2, -1},
                        new[]{-2, 1}, new[]{-2, -1},
                        new[]{1, 2}, new[]{1, -2},
                        new[]{-1, 2}, new[]{-1, -2}
                    };

                    for(int j = 0; j < 8; j++)
                    {
                        var mvPtrn = movePatterns[j];
                        int[] evaluatedPosition = {0, 0};
                        evaluatedPosition[0] = currentPosition[0];
                        evaluatedPosition[1] = currentPosition[1];
                        evaluatedPosition[0] = evaluatedPosition[0] + mvPtrn[0];
                        evaluatedPosition[1] = evaluatedPosition[1] + mvPtrn[1];
                        if (!IsValidPosition(evaluatedPosition)) continue;
                        var s = new Square(evaluatedPosition[0], evaluatedPosition[1]);
                        var occupant = this[s];
                        if (occupant == null ||
                            occupant.Color != CurrentPlayer && occupant.Type != PieceType.King)
                        {
                            landingPositions.Add(s);
                        }
                    }
                }
                if (chessman.Type == PieceType.King)
                {
                    int[][] movePatterns =
                    {
                        new[]{-1, -1}, new[]{-1, 0}, new[]{-1, 1}, 
                        new[]{0, -1}, new[]{0, 1},
                        new[]{1, -1}, new[]{1, 0}, new[]{1, 1}
                    };

                    for(int j = 0; j < 8; j++)
                    {
                        var mvPtrn = movePatterns[j];
                        int[] evaluatedPosition = {0, 0};
                        evaluatedPosition[0] = currentPosition[0];
                        evaluatedPosition[1] = currentPosition[1];
                        evaluatedPosition[0] = evaluatedPosition[0] + mvPtrn[0];
                        evaluatedPosition[1] = evaluatedPosition[1] + mvPtrn[1];
                        if (!IsValidPosition(evaluatedPosition)) continue;
                        var s = new Square(evaluatedPosition[0], evaluatedPosition[1]);
                        var occupant = this[s];
                        if (occupant == null ||
                            occupant.Color != CurrentPlayer && occupant.Type != PieceType.King)
                        {
                            landingPositions.Add(s);
                        }
                    }
                }
                if (chessman.Type == PieceType.Pawn)
                {
                    int[] evaluatedPosition = {0, 0};
                    evaluatedPosition[0] = currentPosition[0];
                    evaluatedPosition[1] = currentPosition[1];

                    if (chessman.Color == PlayerColor.Black)
                    {
                        evaluatedPosition[0] = evaluatedPosition[0] - 1;
                    }
                    else
                    {
                        evaluatedPosition[0] = evaluatedPosition[0] + 1;
                    }
                    if (!IsValidPosition(evaluatedPosition)) continue;
                    var s = new Square(evaluatedPosition[0], evaluatedPosition[1]);
                    var occupant = this[s];
                    if (occupant == null)
                    {
                        landingPositions.Add(s);
                    }
                }

                var mvs = landingPositions.Select(x => new Move(new Square(currentPosition[0], currentPosition[1]), x));
                foreach (var move in mvs)
                {
                    resp.Add(move);
                }
            }

            PossibleMoveList = resp;
        }

        private class Move : IMove
        {
            public Move(Square start, Square end)
            {
                StartSquare = start;
                LandingSquare = end;
            }
            public Square StartSquare { get; private set; }
            public Square LandingSquare { get; private set; }
        }
    }
}