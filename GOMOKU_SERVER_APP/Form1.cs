using SocketDataNameSpace;
using SocketManagerNamespace;
using System;
using System.Collections;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GOMOKU_SERVER_APP
{
    public partial class Form1 : Form
    {
        #region Properties

        //tao mang luu tru thong tin nguoi choi
        public static ArrayList playerList = new ArrayList();

        public static int numPlayer = 0;
        public static int MAX_PLAYER = 100;

        public const int BUFFER_SIZE = 1024;
        private static ASCIIEncoding encoding = new ASCIIEncoding();
        public static bool isRunning = false;

        private TcpListener listener;

        #endregion Properties

        public Form1()
        {
            InitializeComponent();

            //ngan bao loi xung dot
            Control.CheckForIllegalCrossThreadCalls = false;

            tbLog.Enabled = false;
            lbPlayer.Enabled = false;
        }

        #region other method

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void tbLog_TextChanged(object sender, EventArgs e)
        {
        }

        #endregion other method

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            Thread thClickButton = new Thread(() =>
            {
                if (!isRunning)
                {
                    isRunning = true;

                    //tao server
                    listener = new TcpListener(IPAddress.Any, 8000);

                    //listen
                    listener.Start();

                    tbLog.AppendText("Server started!!!");

                    //dieu chinh giao dien

                    btnStartServer.Text = "Stop Server";
                    lbPlayer.Enabled = true;

                    try
                    {
                        while (true)
                        {
                            Socket newClient = listener.AcceptSocket();
                            tbLog.AppendText("\r\nnewclient: " + newClient.RemoteEndPoint);

                            //tao nguoi choi moi
                            SocketManager temp = new SocketManager();
                            temp.client = newClient;
                            player newPlayer = new player();
                            newPlayer.Player1Socket = temp;
                            newPlayer.Status = "WAITING";
                            newPlayer.Player2Socket = null;

                            //xu ly nguoi choi
                            if (addPlayer(newPlayer))
                            {
                                //tạo luồng chấp nhận kết nối và thêm vào danh sách người chơi + xử lý
                                Thread chapNhanKetNoi = new Thread(ClientThread);
                                chapNhanKetNoi.IsBackground = true;
                                chapNhanKetNoi.Start(newPlayer);
                            }
                            else
                            {
                                //thong bao va ngat ket noi client
                                newPlayer.Player1Socket.Send(new SocketData((int)SocketCommand.NOTIFY, "Lỗi kết nối! Mời kết nối lại!", new Point()));
                                newClient.Close();
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    isRunning = false;
                    //dieu chinh giao dien
                    btnStartServer.Text = "Start Server";
                    lbPlayer.Enabled = false;
                    try
                    {
                        //stop server, send msg to all client and disconnect
                        StopServer();
                    }
                    catch (Exception)
                    {
                    }
                }
            });
            thClickButton.IsBackground = true;
            thClickButton.Start();
        }

        private void matchPlayer(player newPlayer)
        {
            bool checkMatch = false;
            try
            {
                //tim va luu thong tin doi thu
                foreach (player playerTemp in playerList)
                {
                    //neu gap player dang doi
                    if (newPlayer.Player1Socket.client != playerTemp.Player1Socket.client && playerTemp.Status == "WAITING")
                    {
                        checkMatch = true;
                        //thay doi trang thai của player 1 (player đang đợi)
                        playerTemp.Status = "MATCHED1";
                        playerTemp.Player2Socket = newPlayer.Player1Socket;

                        //tìm và thay đổi thông tin player 2 (player mới kết nối đến)
                        foreach (player playerTemp1 in playerList)
                        {
                            if (newPlayer.Player1Socket.client == playerTemp1.Player1Socket.client)
                            {
                                playerTemp1.Status = "MATCHED2";
                                playerTemp1.Player2Socket = playerTemp.Player1Socket;
                                tbLog.AppendText("\r\n" + playerTemp1.Player2Socket.client.RemoteEndPoint + " matched to  " + playerTemp1.Player1Socket.client.RemoteEndPoint);
                                break;
                            }
                        }

                        break;
                    }
                }
                if (!checkMatch)
                {
                    tbLog.AppendText("\r\n" + newPlayer.Player1Socket.client.RemoteEndPoint + " WAITING...");
                }
                //thong bao cho client
                if (newPlayer.Status == "WAITING")
                {
                    newPlayer.Player1Socket.Send(new SocketData((int)SocketCommand.WAITING, "Đã kết nối đến server!", new Point()));
                }
                else if (newPlayer.Status == "MATCHED2")
                {
                    newPlayer.Player1Socket.Send(new SocketData((int)SocketCommand.PLAYER2, "Đã kết nối với\r\n" + newPlayer.Player2Socket.client.RemoteEndPoint + "\r\nChờ đối thủ đánh trước!", new Point()));
                    newPlayer.Player2Socket.Send(new SocketData((int)SocketCommand.PLAYER1, "Đã kết nối với\r\n" + newPlayer.Player1Socket.client.RemoteEndPoint + "\r\nMời bạn đánh trước!", new Point()));
                }
            }
            catch (Exception)
            {
            }
        }

        private void matchPlayerAfter(player newPlayer)
        {
            bool checkMatch = false;
            try
            {
                //tim va luu thong tin doi thu
                foreach (player playerTemp in playerList)
                {
                    //neu gap player dang doi
                    if (newPlayer.Player1Socket.client != playerTemp.Player1Socket.client && playerTemp.Status == "WAITING")
                    {
                        checkMatch = true;
                        //thay doi trang thai của player 1 (player đang đợi)
                        playerTemp.Status = "MATCHED1";
                        playerTemp.Player2Socket = newPlayer.Player1Socket;

                        //tìm và thay đổi thông tin player 2 (player mới kết nối đến)
                        foreach (player playerTemp1 in playerList)
                        {
                            if (newPlayer.Player1Socket.client == playerTemp1.Player1Socket.client)
                            {
                                playerTemp1.Status = "MATCHED2";
                                playerTemp1.Player2Socket = playerTemp.Player1Socket;
                                tbLog.AppendText("\r\n" + playerTemp1.Player2Socket.client.RemoteEndPoint + " matched to  " + playerTemp1.Player1Socket.client.RemoteEndPoint);
                                break;
                            }
                        }

                        break;
                    }
                }
                if (!checkMatch)
                {
                    tbLog.AppendText("\r\n" + newPlayer.Player1Socket.client.RemoteEndPoint + " WAITING...");
                }

                //thong bao cho client
                if (newPlayer.Status == "MATCHED2")
                {
                    newPlayer.Player1Socket.Send(new SocketData((int)SocketCommand.PLAYER2, "Đã kết nối với\r\n" + newPlayer.Player2Socket.client.RemoteEndPoint + "\r\nChờ đối thủ đánh trước!", new Point()));
                    newPlayer.Player2Socket.Send(new SocketData((int)SocketCommand.PLAYER1, "Đã kết nối với\r\n" + newPlayer.Player1Socket.client.RemoteEndPoint + "\r\nMời bạn đánh trước!", new Point()));
                }
            }
            catch (Exception)
            {
            }
        }

        private bool addPlayer(player newPlayer)
        {
            bool check = false;
            //kiem tra so luong nguoi choi
            if (numPlayer == MAX_PLAYER)
            {
                check = false;
            }
            else
            {
                check = true;
                //add nguoi choi
                numPlayer++;
                playerList.Add(newPlayer);

                //xu ly trang thai
                matchPlayer(newPlayer);
                lbPlayer.Items.Add(newPlayer.Player1Socket.client.RemoteEndPoint.ToString());
            }
            return check;
        }

        private void erasePlayer(player deletePlayer)
        {
            if (deletePlayer.Player2Socket != null)
            {
                //cap nhat trang thai doi thu
                foreach (player curPlayer in playerList)
                {
                    if (curPlayer.Player1Socket.client == deletePlayer.Player2Socket.client)
                    {
                        curPlayer.Status = "WAITING";
                        //gui cho doi thu
                        curPlayer.Player1Socket.Send(new SocketData((int)SocketCommand.QUIT, curPlayer.Player2Socket.client.RemoteEndPoint + "\r\n đã thoát!", new Point()));

                        curPlayer.Player2Socket = null;
                        matchPlayerAfter(curPlayer);
                        break;
                    }
                }
            }

            //ngat ket noi
            deletePlayer.Player1Socket.client.Close();

            //xoa khoi danh sach
            foreach (player curPlayer in playerList)
            {
                if (curPlayer == deletePlayer)
                {
                    playerList.Remove(curPlayer);
                    numPlayer--;
                    break;
                }
            }
        }

        private void ClientThread(object cli)
        {
            player newPlayer = (player)cli;

            //xử lý truyền nhận
            try
            {
                do
                {
                    //nhận dữ liệu từ player 1
                    SocketData temp = (SocketData)newPlayer.Player1Socket.Receive();

                    //xử lý dữ liệu ở đây !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    if (temp.Command == (int)SocketCommand.QUIT)
                    {
                        //cap nhat giao dien
                        tbLog.AppendText("\r\n" + newPlayer.Player1Socket.client.RemoteEndPoint + " QUIT!!!");
                        foreach (String item in lbPlayer.Items)
                        {
                            if (item == newPlayer.Player1Socket.client.RemoteEndPoint.ToString())
                            {
                                lbPlayer.Items.Remove(item);
                                //xoa khoi danh sach
                                erasePlayer(newPlayer);
                                break;
                            }
                        }
                    }
                    else
                    {
                        //chuyển dữ liệu cho player 2
                        newPlayer.Player2Socket.Send(temp);
                    }
                } while (true);
            }
            catch (Exception)
            {
            }
        }

        private void StopServer()
        {
            tbLog.AppendText("\r\nServer stopped!!!\r\n");
            try
            {
                //gui tin nhan
                foreach (player playerTemp1 in playerList)
                {
                    SocketData temp = new SocketData((int)SocketCommand.SERVER_OUT, "Server đã tắt, bạn bị ngắt kết nối", new Point());
                    playerTemp1.Player1Socket.Send(temp);
                    playerTemp1.Player1Socket.client.Close();
                }
                playerList.Clear();

                //xoá toàn bộ danh sách player (giao diện)
                lbPlayer.Items.Clear();

                //stop server
                listener.Stop();
            }
            catch (Exception)
            {
            }
        }

        //additional
        private void Listen(SocketManager socket)
        {
            Thread listenThread = new Thread(() =>
            {
                try
                {
                    SocketData data = (SocketData)socket.Receive();

                    ProcessData(data);
                }
                catch (Exception)
                {
                }
            });
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private void ProcessData(SocketData data)
        {
            switch (data.Command)
            {
                default:
                    break;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isRunning)
            {
                listener.Stop();
            }
        }
    }
}