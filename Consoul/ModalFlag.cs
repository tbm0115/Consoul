using System;
using System.Linq.Expressions;

namespace ConsoulLibrary
{
    public class ModalFlag<T>
    {
        private bool _ask { get; set; } = true;
        private bool _rememberChoice { get; set; } = false;
        private bool _asked { get; set; } = false;

        public Expression<Func<T, string>> PromptMessage { get; set; }
        public Expression<Func<bool, string>> RememberMessage { get; set; }

        public ModalFlag(Expression<Func<T, string>> promptMessage, Expression<Func<bool, string>> rememberMessage)
        {
            PromptMessage = promptMessage;
            RememberMessage = rememberMessage;
        }

        public bool Test(T source)
        {
            bool response = _rememberChoice;
            if (_ask && Consoul.Ask(PromptMessage.Compile()(source)))
            {
                response = true;
            }
            if (!_asked)
            {
                if (Consoul.Ask(RememberMessage.Compile()(response)))
                {
                    _rememberChoice = response;
                    _ask = false;
                }
                _asked = true;
            }
            return response;
        }
    }

}
