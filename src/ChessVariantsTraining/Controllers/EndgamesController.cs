using ChessVariantsTraining.MemoryRepositories;
using ChessVariantsTraining.Models;
using ChessVariantsTraining.Services;
using ChessDotNet;
using ChessDotNet.Variants.Atomic;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Dynamic;
using ChessDotNet.Variants.Antichess;
using ChessVariantsTraining.DbRepositories;

namespace ChessVariantsTraining.Controllers
{
    public class EndgamesController : CVTController
    {
        IEndgameTrainingSessionRepository endgameTrainingSessionRepository;
        IMoveCollectionTransformer moveCollectionTransformer;

        public EndgamesController(IUserRepository _userRepository,
            IPersistentLoginHandler _loginHandler, 
            IEndgameTrainingSessionRepository _endgameTrainingSessionRepository,
            IMoveCollectionTransformer _moveCollectionTransformer) : base(_userRepository, _loginHandler)
        {
            endgameTrainingSessionRepository = _endgameTrainingSessionRepository;
            moveCollectionTransformer = _moveCollectionTransformer;
        }

        [Route("/Endgames")]
        public ActionResult Index()
        {
            return View();
        }

        IActionResult StartNewSession(Piece[][] board, string variant)
        {
            GameCreationData gcd = new GameCreationData();
            gcd.Board = board;
            gcd.EnPassant = null;
            gcd.HalfMoveClock = 0;
            gcd.FullMoveNumber = 1;
            gcd.WhoseTurn = Player.White;
            gcd.CanBlackCastleKingSide = gcd.CanBlackCastleQueenSide = gcd.CanWhiteCastleKingSide = gcd.CanWhiteCastleQueenSide = false;

            ChessGame game = variant == "Atomic" ? (ChessGame)new AtomicChessGame(gcd) : new AntichessGame(gcd);


            string sessionId;
            do
            {
                sessionId = Guid.NewGuid().ToString();
            } while (endgameTrainingSessionRepository.Exists(sessionId));

            EndgameTrainingSession session = new EndgameTrainingSession(sessionId, game);
            if (session.WasAlreadyLost)
            {
                return null;
            }

            endgameTrainingSessionRepository.Add(session);
            string fen = session.InitialFEN;
            return Json(new { success = true, fen = fen, sessionId = sessionId });
        }

        [HttpGet]
        [Route("/Endgames/Atomic/{type:regex(KRR-K-Adjacent-Kings|KQQ-K-Adjacent-Kings|KQ-K-Adjacent-Kings-Blocked-Pawn|KRN-K-Separated-Kings|KRN-K-Adjacent-Kings)}")]
        public IActionResult AtomicEndgame(string type)
        {
            return View("Train");
        }

        [HttpGet]
        [Route("/Endgames/Antichess/{type:regex(R-vs-K|R-vs-N|B-vs-N|Q-vs-N|K-vs-N|Q-vs-K)}")]
        public IActionResult AntichessEndgame(string type)
        {
            return View("Train");
        }

        [HttpPost]
        [Route("/Endgames/Atomic/KRR-K-Adjacent-Kings/Start")]
        public IActionResult KRRvsKWithAdjacentKingsStart()
        {
            Piece[][] board = BoardExtensions.GenerateEmptyBoard()
                                             .AddAdjacentKings()
                                             .AddWhiteRook()
                                             .AddWhiteRook();
            return StartNewSession(board, "Atomic");
        }

        [HttpPost]
        [Route("/Endgames/Atomic/KQQ-K-Adjacent-Kings/Start")]
        public IActionResult KQQvsKWithAdjacentKingsStart()
        {
            Piece[][] board = BoardExtensions.GenerateEmptyBoard()
                                             .AddAdjacentKings()
                                             .AddWhiteQueen()
                                             .AddWhiteQueen();
            return StartNewSession(board, "Atomic");
        }

        [HttpPost]
        [Route("/Endgames/Atomic/KQ-K-Adjacent-Kings-Blocked-Pawn/Start")]
        public IActionResult KQvsKWithAdjacentKingsAndBlockedPawnStart()
        {
            Piece[][] board = BoardExtensions.GenerateEmptyBoard()
                                             .AddAdjacentKings()
                                             .AddBlockedPawns()
                                             .AddWhiteQueen();
            return StartNewSession(board, "Atomic");
        }

        [HttpPost]
        [Route("/Endgames/Atomic/KRN-K-Separated-Kings/Start")]
        public IActionResult KRNvsKWithSeparatedKingsStart()
        {
            IActionResult result;
            do
            {
                Piece[][] board = BoardExtensions.GenerateEmptyBoard()
                                                 .AddSeparatedKings()
                                                 .AddWhiteRook()
                                                 .AddWhiteKnight();
                result = StartNewSession(board, "Atomic");
            } while (result == null);
            return result;
        }

        [HttpPost]
        [Route("/Endgames/Atomic/KRN-K-Adjacent-Kings/Start")]
        public IActionResult KRNvsKWithAdjacentKingsStart()
        {
            Piece[][] board = BoardExtensions.GenerateEmptyBoard()
                                             .AddAdjacentKings()
                                             .AddWhiteRook()
                                             .AddWhiteKnight();
            return StartNewSession(board, "Atomic");
        }

        [HttpPost]
        [Route("/Endgames/Antichess/R-vs-K/Start")]
        public IActionResult AntichessRvsKStart()
        {
            IActionResult result;
            do
            {
                Piece[][] board = BoardExtensions.GenerateEmptyBoard()
                                                 .AddWhiteRook()
                                                 .AddBlackKing();
                result = StartNewSession(board, "Antichess");
            } while (result == null);
            return result;
        }

        [HttpPost]
        [Route("/Endgames/Antichess/R-vs-N/Start")]
        public IActionResult AntichessRvsNStart()
        {
            IActionResult result;
            do
            {
                Piece[][] board = BoardExtensions.GenerateEmptyBoard()
                                                 .AddWhiteRook()
                                                 .AddBlackKnight();
                result = StartNewSession(board, "Antichess");
            } while (result == null);
            return result;
        }

        [HttpPost]
        [Route("/Endgames/Antichess/B-vs-N/Start")]
        public IActionResult AntichessBvsNStart()
        {
            IActionResult result;
            do
            {
                Piece[][] board = BoardExtensions.GenerateEmptyBoard()
                                                 .AddWhiteBishop()
                                                 .AddBlackKnight();
                result = StartNewSession(board, "Antichess");
            } while (result == null);
            return result;
        }

        [HttpPost]
        [Route("/Endgames/Antichess/Q-vs-N/Start")]
        public IActionResult AntichessQvsNStart()
        {
            IActionResult result;
            do
            {
                Piece[][] board = BoardExtensions.GenerateEmptyBoard()
                                                 .AddWhiteQueen()
                                                 .AddBlackKnight();
                result = StartNewSession(board, "Antichess");
            } while (result == null);
            return result;
        }

        [HttpPost]
        [Route("/Endgames/Antichess/K-vs-N/Start")]
        public IActionResult AntichessKvsNStart()
        {
            IActionResult result;
            do
            {
                Piece[][] board = BoardExtensions.GenerateEmptyBoard()
                                                 .AddWhiteKing()
                                                 .AddBlackKnight();
                result = StartNewSession(board, "Antichess");
            } while (result == null);
            return result;
        }

        [HttpPost]
        [Route("/Endgames/Antichess/Q-vs-K/Start")]
        public IActionResult AntichessQvsKStart()
        {
            IActionResult result;
            do
            {
                Piece[][] board = BoardExtensions.GenerateEmptyBoard()
                                                 .AddWhiteQueen()
                                                 .AddBlackKing();
                result = StartNewSession(board, "Antichess");
            } while (result == null);
            return result;
        }

        [Route("/Endgames/GetValidMoves/{trainingSessionId}")]
        public IActionResult GetValidMoves(string trainingSessionId)
        {
            EndgameTrainingSession session = endgameTrainingSessionRepository.Get(trainingSessionId);
            if (session == null)
            {
                return Json(new { success = false, error = "Training session ID not found." });
            }

            return Json(new { success = true, dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(session.Game.GetValidMoves(session.Game.WhoseTurn)) });
        }

        [HttpPost]
        [Route("/Endgames/SubmitMove")]
        public IActionResult SubmitMove(string trainingSessionId, string origin, string destination, string promotion = null)
        {
            if (promotion != null && promotion.Length != 1)
            {
                return Json(new { success = false, error = "Invalid promotion parameter." });
            }

            EndgameTrainingSession session = endgameTrainingSessionRepository.Get(trainingSessionId);
            if (session == null)
            {
                return Json(new { success = false, error = "Training session ID not found." });
            }

            Move move = new Move(origin, destination, session.Game.WhoseTurn, promotion?[0]);
            SubmittedMoveResponse response = session.SubmitMove(move);
            dynamic jsonResp = new ExpandoObject();
            jsonResp.success = response.Success;
            jsonResp.correct = response.Correct;
            jsonResp.check = response.Check;
            if (response.Error != null) jsonResp.error = response.Error;
            if (response.FEN != null) jsonResp.fen = response.FEN;
            if (response.FenAfterPlay != null)
            {
                jsonResp.fenAfterPlay = response.FenAfterPlay;
                jsonResp.checkAfterAutoMove = response.CheckAfterAutoMove;
            }
            if (response.Moves != null) jsonResp.dests = moveCollectionTransformer.GetChessgroundDestsForMoveCollection(response.Moves);
            if (response.LastMove != null) jsonResp.lastMove = response.LastMove;
            jsonResp.drawAfterAutoMove = response.DrawAfterAutoMove;
            jsonResp.winAfterAutoMove = response.WinAfterAutoMove;
            return Json(jsonResp);
        }
    }
}
