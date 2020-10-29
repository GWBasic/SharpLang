using System;
using System.Threading.Tasks;

namespace SharpLang
{
    /// <summary>
    /// Delegate type for unhandled exceptions
    /// </summary>
    public delegate void ExceptionHandler(Exception exception);
}