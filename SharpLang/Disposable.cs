using System;
namespace SharpLang
{
    public class Disposable : IDisposable
    {
        private Action callback;

        internal Disposable(Action callback)
        {
            this.callback = callback;
        }

        public void Dispose()
        {
            this.callback();
        }
    }
}
