﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

using MessageTypes;

namespace Winform_Client
{
    public partial class Form1 : Form
    {
        Socket clientSocket;
        private Thread myThread;
        bool bQuit = false;
        bool bConnected = false;

        List<String> currentClientList = new List<String>();

        static void clientProcess(Object o)
        {            
            Form1 form = (Form1)o;

            while ((form.bConnected == false) && (form.bQuit == false))
            {
                try
                {
                    form.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    form.clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8500));
                    form.bConnected = true;
                    form.AddText("Connected to server");

                    Thread receiveThread;

                    receiveThread = new Thread(clientReceive);
                    receiveThread.Start(o);

                    while ((form.bQuit == false) && (form.bConnected == true))
                    {
                        if (form.IsDisposed == true)
                        {
                            form.bQuit = true;
                            form.clientSocket.Close();
                        }
                    }                    

                    receiveThread.Abort();
                }
                catch (System.Exception)
                {
                    form.AddText("No server!");
                    Thread.Sleep(1000);
                }               
            }
        }

        static void clientReceive(Object o)
        {
            Form1 form = (Form1)o;

            while (form.bConnected == true)
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    int result;

                    result = form.clientSocket.Receive(buffer);

                    if (result > 0)
                    {
                        MemoryStream stream = new MemoryStream(buffer);
                        BinaryReader read = new BinaryReader(stream);

                        Msg m = Msg.DecodeStream(read);

                        if (m != null)
                        {
                            Console.Write("Got a message: ");
                            switch (m.mID)
                            {
                                case PublicChatMsg.ID:
                                    {
                                        PublicChatMsg publicMsg = (PublicChatMsg)m;

                                        form.AddText(publicMsg.msg);
                                    }
                                    break;

                                case PrivateChatMsg.ID:
                                    {
                                        PrivateChatMsg privateMsg = (PrivateChatMsg)m;
                                        form.AddText(privateMsg.msg);
                                    }
                                    break;

                                case ClientListMsg.ID:
                                    {
                                        ClientListMsg clientList = (ClientListMsg)m;
                                        
                                        form.SetClientList(clientList);
                                    }
                                    break;

                                case ClientNameMsg.ID:
                                    {
                                        ClientNameMsg clientName = (ClientNameMsg)m;

                                        form.SetClientName(clientName.name);
                                    }
                                    break;

                                case DungeonResponse.ID:
                                    {
                                        DungeonResponse dSponse = (DungeonResponse)m;
                                        form.AddDungeonText(dSponse.response);
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                    }     
                    
                }
                catch (Exception)
                {
                    form.bConnected = false;
                    Console.WriteLine("Lost server!");
                }

            }
        }
        public Form1()
        {
            InitializeComponent();

            myThread = new Thread(clientProcess);
            myThread.Start(this);

            Application.ApplicationExit += delegate { OnExit(); };
        }


        private delegate void AddTextDelegate(String s);

        private void AddText(String s)
        {
            if (textBox_Output.InvokeRequired)
            {
                Invoke(new AddTextDelegate(AddText), new object[] { s });
            }
            else
            {
                textBox_Output.AppendText(s);
            }
        }

        private void AddDungeonText(String s)
        {
            if (TextboxDungeon.InvokeRequired)
            {
                Invoke(new AddTextDelegate(AddDungeonText), new object[] { s });
            }
            else
            {
                TextboxDungeon.AppendText(s);
            }
        }

        private delegate void SetClientNameDelegate(String s);

        private void SetClientName(String s)
        {
            if (this.InvokeRequired)
            {
                Invoke(new SetClientNameDelegate(SetClientName), new object[] {s});
            }
            else
            {
                Text = s;
            }
        }

        private delegate void SetClientListDelegate(ClientListMsg clientList);

        private void SetClientList(ClientListMsg clientList)
        {
            if (this.InvokeRequired)
            {
                Invoke(new SetClientListDelegate(SetClientList), new object[] { clientList });
            }
            else
            {
                listBox_ClientList.DataSource = null;
                currentClientList.Clear();
                currentClientList.Add("All");
                currentClientList.Add("Dung");

                foreach (String s in clientList.clientList)
                {
                    currentClientList.Add(s);
                }
                listBox_ClientList.DataSource = currentClientList;             
            }
        }


        private void buttonSend_Click(object sender, EventArgs e)
        {
            if ( (textBox_Input.Text.Length > 0) && (clientSocket != null))
            {                
                try
                {
                    if (listBox_ClientList.SelectedIndex == 1)
                    {
                        sendDungeonMessage(textBox_Input.Text);
                    }

                    else if (listBox_ClientList.SelectedIndex == 0)
                    {
                        PublicChatMsg publicMsg = new PublicChatMsg();

                        publicMsg.msg = textBox_Input.Text;
                        MemoryStream outStream = publicMsg.WriteData();
                        clientSocket.Send(outStream.GetBuffer());                
                    }
                    else if (listBox_ClientList.SelectedIndex > 1)
                    {
                        PrivateChatMsg privateMsg = new PrivateChatMsg();

                        privateMsg.msg = textBox_Input.Text;
                        privateMsg.destination = currentClientList[listBox_ClientList.SelectedIndex];
                        MemoryStream outStream = privateMsg.WriteData();
                        clientSocket.Send(outStream.GetBuffer());                
                    }
                    
                }
                catch (System.Exception)
                {
                }

                textBox_Input.Text = "";
            }
        }

        private void OnExit()
        {
            bQuit = true;
            Thread.Sleep(500);
            if (myThread != null)
            {
                myThread.Abort();
            }
        }

        private void sendDungeonMessage(String Message)
        {
            DungeonCommand dungMsg = new DungeonCommand();
            dungMsg.command = Message;
            MemoryStream outStream = dungMsg.WriteData();
            clientSocket.Send(outStream.GetBuffer());
        }
        
        private void listBox_ClientList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox_ClientName_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_Input_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ButtonNorth_Click(object sender, EventArgs e)
        {
            String m = "go north";
            sendDungeonMessage(m);
        }

        private void ButtonEast_Click(object sender, EventArgs e)
        {
            String m = "go east";
            sendDungeonMessage(m);
        }

        private void ButtonSouth_Click(object sender, EventArgs e)
        {
            String m = "go south";
            sendDungeonMessage(m);
        }

        private void ButtonWest_Click(object sender, EventArgs e)
        {
            String m = "go west";
            sendDungeonMessage(m);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            String m;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    m = "go north";
                    sendDungeonMessage(m);
                    break;
                case Keys.Left:
                     m = "go east";
                    sendDungeonMessage(m);
                    break;
                case Keys.Down:
                    m = "go south";
                    sendDungeonMessage(m);
                    break;
                case Keys.Right:
                    m = "go west";
                    sendDungeonMessage(m);
                    break;

            }
        }
    }
}