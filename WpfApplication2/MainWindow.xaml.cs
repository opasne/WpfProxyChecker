﻿using System.Windows;
using System.Windows.Input;
using System.Collections;
using System.Windows.Data;
using System.Windows.Controls;
using System;
using System.Media;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace prxSearcher
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ProxiesList pl;

        public MainWindow()
        {
            InitializeComponent();
            tbStatus.Text = "Idle";
            pbStatus.Visibility = Visibility.Collapsed;
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            MessageBox.Show(String.Format("ver: {0}", version));
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void mainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (pl == null)
                {
                     if (MessageBox.Show("Do you want exit?", "exit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        Close();
                    }
                }
                else if(pl.mIsRun)
                {
                    //Task.Run(() => { pl.StopProxiesLoading(); });
                    pl.StopProxiesLoading();
                }
                else if (MessageBox.Show("Do you want exit?", "exit", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    Close();
                }              
            }
            else if(e.Key == Key.F1)
            {
                MenuAbout_Click(this,null);
            }
        }        

        private void menuFindPrxs_Click(object sender, RoutedEventArgs e)
        {
            pl = new ProxiesList(20, 500);
            dtUnsrtd.ItemsSource = pl;
            pbStatus.Visibility = Visibility.Visible;
            pl.Changed += new EventHandler(UpdateDataGrid);            
            pl.GetProxiesList("proxy list");
        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(pl != null)
                pl.Dispose();
        }

        private void UpdateDataGrid(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    CollectionViewSource.GetDefaultView(dtUnsrtd.ItemsSource).Refresh();
                    pbStatus.Value = pl.mProgressValue;
                    if (pbStatus.Value == 0 && !pl.mIsRun)
                    {
                        pbStatus.Visibility = Visibility.Collapsed;
                        string pathToWav = String.Format(@"{0}\Media\Windows Notify.wav", Environment.GetEnvironmentVariable("SystemRoot"));
                        using (var soundPlayer = new SoundPlayer(pathToWav))
                        {
                            soundPlayer.Play(); // can also use soundPlayer.PlaySync()
                        }
                    }
                    tbStatus.Text = pl.mStatus;
                });
            }
            catch (Exception) { }
        }
    }
}
