using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class Game
    {
        private int _type;
        private int _state;

        private Client _client1;
        private Client _client2;

        private String _clientInput1;
        private String _clientInput2;

        private bool _client1Inputted;
        private bool _client2Inputted;

        private int _clientScore1;
        private int _clientScore2;

        public Game(int type, Client inviteSender, Client inviteReciever)
        {
            _type = type;
            _state = 0;
            _clientScore1 = 0;
            _clientScore2 = 0;
            _client1Inputted = false;
            _client2Inputted = false;
            _client1 = inviteSender;
            _client2 = inviteReciever;
            _client1.SetCurrentGame(this);
            _client2.SetCurrentGame(this);
        }

        public int GetState()
        {
            return _state;
        }
        public int GetGameType()
        {
            return _type;
        }

        public Client GetClient1()
        {
            return _client1;
        }

        public Client GetClient2()
        {
            return _client2;
        }

        public void Close()
        {
            _client1.SetCurrentGame(null);
            _client2.SetCurrentGame(null);
        }

        public void Start()
        {
            _state = 1;
        }

        public void SetClient1Input(String input)
        {
            _clientInput1 = input;
            _client1Inputted = true;
        }

        public void SetClient2Input(String input)
        {
            _clientInput2 = input;
            _client2Inputted = true;
        }

        public bool GetClient1Inputted()
        {
            return _client1Inputted;
        }

        public bool GetClient2Inputted()
        {
            return _client2Inputted;
        }

        public void NextRound()
        {
            _client1Inputted = false;
            _client2Inputted = false;
            _clientInput1 = "";
            _clientInput2 = "";
        }

        public void GetRPSResult(ref String client1Message, ref String client2Message)
        {
            switch (_clientInput1)
            {
                case "R":
                    switch (_clientInput2)
                    {
                        case "R":
                            client1Message = "<AAAA00[" + _client2.GetName() + "]> played rock. <774400It's a draw!> ";
                            client2Message = "<AAAA00[" + _client1.GetName() + "]> played rock. <774400It's a draw!> ";
                            _clientScore1++;
                            return;
                        case "P":
                            client1Message = "<AAAA00[" + _client2.GetName() + "]> played paper. <774400You lose!> ";
                            client2Message = "<AAAA00[" + _client1.GetName() + "]> played rock. <774400You win!> ";
                            _clientScore2++;
                            return;
                        case "S":
                            client1Message = "<AAAA00[" + _client2.GetName() + "]> played scissors. <774400You win!> ";
                            client2Message = "<AAAA00[" + _client1.GetName() + "]> played rock. <774400You lose!> ";
                            return;
                        default:
                            return;
                    }
                case "P":
                    switch (_clientInput2)
                    {
                        case "R":
                            client1Message = "<AAAA00[" + _client2.GetName() + "]> played rock. <774400You win!> ";
                            client2Message = "<AAAA00[" + _client1.GetName() + "]> played paper. <774400You lose!> ";
                            _clientScore1++;
                            return;
                        case "P":
                            client1Message = "<AAAA00[" + _client2.GetName() + "]> played paper. <774400It's a draw!> ";
                            client2Message = "<AAAA00[" + _client1.GetName() + "]> played paper. <774400It's a draw!> ";
                            return;
                        case "S":
                            client1Message = "<AAAA00[" + _client2.GetName() + "]> played scissors. <774400You lose!> ";
                            client2Message = "<AAAA00[" + _client1.GetName() + "]> played paper. <774400You win!> ";
                            _clientScore2++;
                            return;
                        default:
                            return;
                    }
                case "S":
                    switch (_clientInput2)
                    {
                        case "R":
                            client1Message = "<AAAA00[" + _client2.GetName() + "]> played rock. <774400You lose!> ";
                            client2Message = "<AAAA00[" + _client1.GetName() + "]> played scissors. <774400You win!> ";
                            _clientScore2++;
                            return;
                        case "P":
                            client1Message = "<AAAA00[" + _client2.GetName() + "]> played paper. <774400You win!> ";
                            client2Message = "<AAAA00[" + _client1.GetName() + "]> played scissors. <774400You lose!> ";
                            _clientScore1++;
                            return;
                        case "S":
                            client1Message = "<AAAA00[" + _client2.GetName() + "]> played scissors. <774400It's a draw!> ";
                            client2Message = "<AAAA00[" + _client1.GetName() + "]> played scissors. <774400It's a draw!> ";
                            return;
                        default:
                            return;
                    }
                default:
                    return;
            }
        }
    }
}
