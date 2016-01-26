﻿using Sakuno.KanColle.Amatsukaze.Views.History;
using Sakuno.SystemInterop;
using Sakuno.UserInterface.Controls;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Sakuno.KanColle.Amatsukaze.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            PowerMonitor.RegisterMonitor(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (MessageBox.Show(this, StringResources.Instance.Main.Window_ClosingConfirmation, ProductInfo.FullAppName, MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No) == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == Key.F3)
                new ExpeditionHistoryWindow().Show();
        }

    }
}
