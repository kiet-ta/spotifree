using Spotifree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotifree.IServices
{
    public interface ISettingsService
    {
        // Loads the application settings from storage.
        Task<AppSettings> GetAsync();

        // Saves the application settings to storage.
        Task SaveAsync(AppSettings settings);
    }
}
