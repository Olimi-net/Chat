using ChatServer.ViewModel;
using System;
using System.Collections.Generic;
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

/// <author>
/// Lada 
/// </author>
/// <created>
/// 2017.03.11
/// </created>

namespace ChatServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServerViewModel vm;
        public MainWindow()
        {
            InitializeComponent();
            vm = new ServerViewModel();
            DataContext = vm;
         
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vm.OnCloseWindow();
        }
    }
}
