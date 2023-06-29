using System;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Host;
using Neo.ApplicationFramework.Tools.WebServer.WebFramework.Http;

namespace Neo.ApplicationFramework.Tools.WebServer.WebFramework
{
    public static class IRequestExtensions 
    {
        public static bool IsModified(this IRequest request, DateTime lastWrite)
        {
            DateTime lastModified = DateTime.MinValue;
            string ifModifiedSince = request.GetHeader(HeaderNames.IfModifiedSince);

            if (!string.IsNullOrEmpty(ifModifiedSince))
            {
                lastModified = DateTime.Parse(ifModifiedSince);
            }

            if (lastWrite.ToUniversalTime() <= lastModified.ToUniversalTime())
            {
                return false;
            }

            return true;
        }
    }
}
