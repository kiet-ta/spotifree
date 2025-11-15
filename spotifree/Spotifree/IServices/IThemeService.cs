using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotifree.IServices;

public interface IThemeService
{
    void ApplyThemeOnStart();
    void SetTheme(bool isDark);
}
