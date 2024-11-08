﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Takerman.Backups.Services.Abstraction;
using Takerman.Extensions;

namespace Takerman.Backups.Services
{
    public class AutoBackupService(ISqlService _sqlService, ILogger<AutoBackupService> _logger, IHostEnvironment _hostEnvironment) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (_hostEnvironment.IsDevelopment() == false)
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var databases = await _sqlService.GetAllDatabasesAsync();

                        foreach (var database in databases.Where(x => x.Name.StartsWith("takerman")))
                        {
                            await _sqlService.BackupAsync(database.Name);
                        }

                        _logger.LogInformation("**Backups Service**: Daily backups finished");

                        await _sqlService.MaintainBackups();

                        _logger.LogInformation("**Backups Service**: Maintenance finished");

                        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "**Backups Service**: " + ex.GetMessage());
            }
        }
    }
}