﻿using Client.view;
using messenger.model;
using messenger.utility;
using messenger.viewmodel;
using PacketLib;
using PacketLib.model;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using tcpip;

namespace Client
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewmodel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();

            TcpIp.Instance.PacketReceived += OnPacketReceived;

            lv_chatrooms.ItemsSource = viewmodel.ChatRoomList;

            // 초기 필요한 데이터 요청
            TcpIp.Instance.SendPacket(Command.CLIENT.REQUEST_CHATROOM_LIST);
        }

        




        


        private void btn_create_chat_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(tb_chat_name.Text) || string.IsNullOrEmpty(tb_nickname.Text))
            {
                return;
            }

            ChatRoomModel chatRoomModel = new ChatRoomModel();

            // init
            ChatRoomListItemModel chatRoomInfo = new ChatRoomListItemModel();
            chatRoomInfo.Name = tb_chat_name.Text;
            chatRoomInfo.Creater = tb_nickname.Text;
            chatRoomInfo.Cnt = 1;

            List<string> chatters = new List<string>();
            chatters.Add(tb_chat_name.Text);


            chatRoomModel.chatRoomInfo = chatRoomInfo;
            chatRoomModel.chatters = chatters;

            tb_chat_name.Text = "";
            ////////

            TcpIp.Instance.SendPacket(Command.CLIENT.REQUEST_CHATROOM_CREATE, chatRoomModel);

        }


        private void lv_chatrooms_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListView listView = (ListView)sender;

            if(listView.SelectedItem == null)
            {
                return;
            }

            // 채팅방 입장 요청
            ChatRoomListItemModel chatRoom = (ChatRoomListItemModel)listView.SelectedItem;

            TcpIp.Instance.SendPacket(Command.CLIENT.REQUEST_CHATROOM_ENTER, chatRoom.Id);
        }


        private void OnPacketReceived(Packet packet)
        {
            if (packet.Command == (int)Command.SERVER.SEND_CHATROOM_LIST)                       //채팅방 목록
            {
                Dispatcher.Invoke(() => {
                    viewmodel.UpdateChatRoomList((List<ChatRoomListItemModel>)packet.Data);
                });
            }
            else if(packet.Command == (int)Command.SERVER.ACCEPT_CHATROOM_ENTER)                // 채팅방 입장
            {
                ChatRoomModel chatRoomModel = (ChatRoomModel)packet.Data;

                Dispatcher.Invoke(() => {
                    ChattingRoom chattingRoom = new ChattingRoom(chatRoomModel, tb_nickname.Text);
                    chattingRoom.Show();
                });
                
                
            }
            else if (packet.Command == (int)Command.SERVER.ACCEPT_CHATROOM_CREATE)              // 채팅방 생성
            {
                ChatRoomModel chatRoomModel = (ChatRoomModel)packet.Data;

                Dispatcher.Invoke(() => {
                    ChattingRoom chattingRoom = new ChattingRoom(chatRoomModel, tb_nickname.Text);
                    chattingRoom.Show();
                });


            }
        }
    }
}
