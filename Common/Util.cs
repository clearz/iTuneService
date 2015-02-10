using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace Common
{
    public static class Util
    {
        /// <summary>
        /// Gets the directory name of the first <see cref="FileAppender"/> in the
        /// list of appenders configured for log4net. Returns null if not found.
        /// </summary>
        public static string GetLog4NetLogPath()
        {
            var hierarchy = ((Hierarchy)LogManager.GetRepository());
            var rootAppender = hierarchy.Root.Appenders.OfType<FileAppender>()
                                        .FirstOrDefault();

            return rootAppender != null
                       ? Path.GetDirectoryName(rootAppender.File)
                       : null;
        }
    }
}
