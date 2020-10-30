using System;

namespace SharpLang
{
    /// <summary>
    /// Thrown when code is called on the wrong fiber
    /// </summary>
    public class WrongFiberException : Exception
    {
        public WrongFiberException(IFiber expectedFiber)
            : base($"Wrong fiber, expected to be called on {expectedFiber.Name}")
        {
            this.ExpectedFiber = expectedFiber;
        }

        /// <summary>
        /// The fiber that the code should be called on
        /// </summary>
        public IFiber ExpectedFiber { get; }
    }
}
