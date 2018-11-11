using System.Collections.Generic;

namespace Assets.Scripts
{
    public enum GameState
    {
        Playing,
        Pat,
        Mat
    }
    
    public enum PieceType
    {
        King = 0,
        Queen,
        Bishop,
        Knight,
        Rook,
        Pawn
    }

    public enum PlayerColor
    {
        Black = -1,
        White = 1
    }
    
    public class Chessman {
        public readonly PieceType Type;
        public readonly PlayerColor Color;

        public Chessman(PieceType type, PlayerColor color)
        {
            Type = type;
            Color = color;
        }
    }
    
    public class Square
    {
        public readonly int Row;
        public readonly int Column;

        public Square(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            var p = (Square) obj; 
            return Row == p.Row && Column == p.Column;
        }
    }
    
    public interface IMove
    {
        Square StartSquare { get; } 
        Square LandingSquare { get; } 
    }

    public interface IChessBoardRefereeDelegate
    {
        void OnGameStatusChanged(GameState newState);

        void OnSpecialUpdateRequired(Square squareToRefresh);

    }
    
    public interface IChessGameReferee
    {
//        /// <summary>
//        /// Export the current game in a serializable format
//        /// </summary>
//        /// <returns></returns>
//        byte[] Export();
//        
//        /// <summary>
//        /// Import a backup and set the game
//        /// </summary>
//        /// <param name="savedState"></param>
//        void Import(byte[] savedState);

        /// <summary>
        /// The size of the board
        /// </summary>
        int BoardSize { get; }
        
        /// <summary>
        /// A IChessBoardRefereeDelegate that will received game event 
        /// </summary>
        IChessBoardRefereeDelegate Delegate { get; set; }
        
        /// <summary>
        /// The current game state
        /// </summary>
        GameState CurrentGameState { get; }
        
        /// <summary>
        /// return the chessman a the given position
        /// </summary>
        /// <param name="position"></param>
        Chessman this[Square position] { get; }
            
        /// <summary>
        /// Color of the player to play next
        /// </summary>
        PlayerColor CurrentPlayer { get; }

        /// <summary>
        /// Reset the board, start a new game
        /// </summary>
        void ResetBoard();
        
        /// <summary>
        /// Get the list of eaten chassman
        /// </summary>
        IList<Chessman> EatenChessman { get; }

        

        /// <summary>
        /// Get the move object
        /// </summary>
        /// <param name="start"></param>
        /// <param name="landing"></param>
        /// <returns></returns>
        IMove GetMove(Square start, Square landing);
        
        /// <summary>
        /// Do the specified Move
        /// </summary>
        /// <param name="move"></param>
        void DoMove(IMove move);

        /// <summary>
        /// Generate the list of possible move for the current player
        /// </summary>
        /// <returns></returns>
        HashSet<IMove> PossibleMoveList { get; }

        /// <summary>
        /// Convenient method to determine if a given position has available moves
        /// </summary>
        /// <returns></returns>
        bool CanMove(Square square);

    }
}