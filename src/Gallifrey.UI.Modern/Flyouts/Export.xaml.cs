﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Gallifrey.Exceptions.JiraIntegration;
using Gallifrey.Jira.Model;
using Gallifrey.UI.Modern.Models;
using MahApps.Metro.Converters;

namespace Gallifrey.UI.Modern.Flyouts
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export
    {
        private readonly MainViewModel viewModel;
        private ExportModel DataModel { get { return (ExportModel)DataContext; } }

        public Export(MainViewModel viewModel, Guid timerId, TimeSpan? exportTime)
        {
            this.viewModel = viewModel;
            InitializeComponent();
            SetupContext(timerId, exportTime);

        }

        private async void SetupContext(Guid timerId, TimeSpan? exportTime)
        {
            var timerToShow = viewModel.Gallifrey.JiraTimerCollection.GetTimer(timerId);
            Issue jiraIssue = null;

            DataContext = new ExportModel(timerToShow, exportTime, viewModel.Gallifrey.Settings.ExportSettings.DefaultRemainingValue);

            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                var jiraDownloadTask = Task.Factory.StartNew(() => viewModel.Gallifrey.JiraConnection.GetJiraIssue(timerToShow.JiraReference, true), cancellationTokenSource.Token);

                var controller = await viewModel.DialogCoordinator.ShowProgressAsync(viewModel, "Please Wait", "Downloading Jira Work Logs To Ensure Accurate Export", true);
                var controllerCancel = Task.Factory.StartNew(() =>
                {
                    while (!controller.IsCanceled)
                    {

                    }
                });

                var cancelled = false;
                if (await Task.WhenAny(jiraDownloadTask, controllerCancel) == controllerCancel)
                {
                    cancellationTokenSource.Cancel();
                    cancelled = true;
                }

                await controller.CloseAsync();

                if (!cancelled)
                {
                    if (jiraDownloadTask.IsCompleted)
                    {
                        jiraIssue = jiraDownloadTask.Result;
                    }
                    else
                    {
                        throw new Exception("NoResults");
                    }
                }
                else
                {
                    IsOpen = false;
                    return;
                }
            }
            catch (Exception)
            {
                viewModel.DialogCoordinator.ShowMessageAsync(viewModel, "Unable To Locate Jira", string.Format("Unable To Locate Jira {0}!\nCannot Export Time\nPlease Verify/Correct Jira Reference", timerToShow.JiraReference));
                IsOpen = false;
                return;
            }

            viewModel.Gallifrey.JiraTimerCollection.RefreshFromJira(timerId, jiraIssue, viewModel.Gallifrey.JiraConnection.CurrentUser);

            timerToShow = viewModel.Gallifrey.JiraTimerCollection.GetTimer(timerId);

            if (timerToShow.FullyExported)
            {
                viewModel.DialogCoordinator.ShowMessageAsync(viewModel, "Nothing To Export", "There Is No Time To Export");
                IsOpen = false;
                return;
            }

            if (timerToShow.IsRunning)
            {
                viewModel.DialogCoordinator.ShowMessageAsync(viewModel, "Timer Is Running", "You Cannot Export A Timer While It Is Running");
                IsOpen = false;
                return;
            }

            DataModel.UpdateTimer(timerToShow);
        }

        private async void ExportButton(object sender, RoutedEventArgs e)
        {
            if (DataModel.Timer.TimeToExport < DataModel.ToExport)
            {
                viewModel.DialogCoordinator.ShowMessageAsync(viewModel, "Invalid Export", string.Format("You Cannot Export More Than The Timer States Un-Exported\nThis Value Is {0}!", DataModel.ToExport.ToString(@"hh\:mm")));
                return;
            }
            
            try
            {
                var canClose = false;
                var cancellationTokenSource = new CancellationTokenSource();
                var jiraDownloadTask = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        viewModel.Gallifrey.JiraConnection.LogTime(DataModel.JiraRef, DataModel.ExportDate, DataModel.ToExport, DataModel.WorkLogStrategy, DataModel.Comment, DataModel.Remaining);
                        canClose = true;
                    }
                    catch (WorkLogException)
                    {
                        cancellationTokenSource.Cancel();
                        viewModel.DialogCoordinator.ShowMessageAsync(viewModel, "Error Exporting", "Unable To Log Work!");
                    }
                    catch (StateChangedException)
                    {
                        cancellationTokenSource.Cancel();
                        viewModel.DialogCoordinator.ShowMessageAsync(viewModel, "Error Exporting", "Unable To Re-Close A The Jira, Manually Check!!");
                        canClose = true;
                    }

                }, cancellationTokenSource.Token);

                var controller = await viewModel.DialogCoordinator.ShowProgressAsync(viewModel, "Please Wait", "Downloading Jira Work Logs To Ensure Accurate Export", true);
                var controllerCancel = Task.Factory.StartNew(() =>
                {
                    while (!controller.IsCanceled)
                    {

                    }
                });


                if (await Task.WhenAny(jiraDownloadTask, controllerCancel) == controllerCancel)
                {
                    cancellationTokenSource.Cancel();
                }

                await controller.CloseAsync();
                
            }
            catch (Exception)
            {
                viewModel.DialogCoordinator.ShowMessageAsync(viewModel, "Error Exporting", "An Error Occured Exporting Time");
                return;
            }

            viewModel.Gallifrey.JiraTimerCollection.AddJiraExportedTime(DataModel.Timer.UniqueId, DataModel.ToExportHours, DataModel.ToExportMinutes);
            IsOpen = false;
        }
    }
}
