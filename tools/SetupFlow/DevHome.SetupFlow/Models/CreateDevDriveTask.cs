﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

extern alias Projection;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DevHome.Common.Extensions;
using DevHome.Common.Models;
using DevHome.Common.ResultHelper;
using DevHome.Common.Services;
using DevHome.SetupFlow.Common.Contracts;
using DevHome.SetupFlow.Common.Helpers;
using DevHome.SetupFlow.Common.TelemetryEvents;
using DevHome.SetupFlow.Services;
using DevHome.Telemetry;
using Microsoft.Extensions.Hosting;
using Projection::DevHome.SetupFlow.ElevatedComponent;
using Windows.Foundation;

namespace DevHome.SetupFlow.Models;

internal class CreateDevDriveTask : ISetupTask
{
    private readonly TaskMessages _taskMessages;
    private readonly ActionCenterMessages _actionCenterMessages = new ();
    private readonly ISetupFlowStringResource _stringResource;
    private readonly IHost _host;

    public event ISetupTask.ChangeMessageHandler AddMessage;

    public bool RequiresAdmin => true;

    public bool RequiresReboot => false;

    public bool DependsOnDevDriveToBeInstalled => false;

    public IDevDrive DevDrive
    {
        get; set;
    }

    public CreateDevDriveTask(IDevDrive devDrive, IHost host, ISetupFlowStringResource stringResource)
    {
        DevDrive = devDrive;
        _stringResource = stringResource;
        _taskMessages = new TaskMessages
        {
            Executing = _stringResource.GetLocalized(StringResourceKey.DevDriveCreating),
            Finished = _stringResource.GetLocalized(StringResourceKey.DevDriveCreated),
            Error = _stringResource.GetLocalized(StringResourceKey.DevDriveUnableToCreateError),
            NeedsReboot = _stringResource.GetLocalized(StringResourceKey.DevDriveRestart),
        };
        _host = host;
    }

    public ActionCenterMessages GetErrorMessages() => _actionCenterMessages;

    public TaskMessages GetLoadingMessages() => _taskMessages;

    public ActionCenterMessages GetRebootMessage() => new ();

    /// <summary>
    /// Get the arguments for this task
    /// </summary>
    /// <returns>Arguments for this task</returns>
    public CreateDevDriveTaskArguments GetArguments()
    {
        return new CreateDevDriveTaskArguments
        {
            VirtDiskPath = Path.Combine(DevDrive.DriveLocation, $"{DevDrive.DriveLabel}.vhdx"),
            SizeInBytes = DevDrive.DriveSizeInBytes,
            NewDriveLetter = DevDrive.DriveLetter,
            DriveLabel = DevDrive.DriveLabel,
        };
    }

    /// <summary>
    /// Not used, as Dev Drive creation requires elevation
    /// </summary>
    IAsyncOperation<TaskFinishedState> ISetupTask.Execute()
    {
        return Task.Run(() =>
        {
            AddMessage(_stringResource.GetLocalized(StringResourceKey.DevDriveNotAdminError));
            return TaskFinishedState.Failure;
        }).AsAsyncOperation();
    }

    IAsyncOperation<TaskFinishedState> ISetupTask.ExecuteAsAdmin(IElevatedComponentOperation elevatedComponentOperation)
    {
        return Task.Run(async () =>
        {
            Stopwatch timer = Stopwatch.StartNew();
            var result = 0;

            try
            {
                // Critical level approved by subhasan
                TelemetryFactory.Get<ITelemetry>().Log("CreateDevDrive_CreatingDevDrive_Event", LogLevel.Critical, new EmptyEvent());
                var manager = _host.GetService<IDevDriveManager>();
                var validation = manager.GetDevDriveValidationResults(DevDrive);
                manager.RemoveAllDevDrives();

                if (!validation.Contains(DevDriveValidationResult.Successful))
                {
                    var localizedMsg = _stringResource.GetLocalized("DevDrive" + validation.First().ToString());
                    _actionCenterMessages.PrimaryMessage = _stringResource.GetLocalized(StringResourceKey.DevDriveErrorWithReason, localizedMsg);
                    return TaskFinishedState.Failure;
                }

                Result.ThrowIfFailed(await elevatedComponentOperation.CreateDevDriveAsync());
                return TaskFinishedState.Success;
            }
            catch (Exception ex)
            {
                result = ex.HResult;
                Log.Logger?.ReportError(Log.Component.DevDrive, $"Failed to create Dev Drive.", ex);
                _actionCenterMessages.PrimaryMessage = _stringResource.GetLocalized(StringResourceKey.DevDriveErrorWithReason, _stringResource.GetLocalizedErrorMsg(ex.HResult, Log.Component.DevDrive));
                TelemetryFactory.Get<ITelemetry>().LogException("CreatingDevDriveException", ex);
                return TaskFinishedState.Failure;
            }
            finally
            {
                timer.Stop();

                // Critical level approved by subhasan
                TelemetryFactory.Get<ITelemetry>().Log("CreateDevDriveTriggered", LogLevel.Critical, new DevDriveTriggeredEvent(DevDrive, timer.ElapsedTicks, result));
            }
        }).AsAsyncOperation();
    }
}
