using System;
using System.Linq.Expressions;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides structure for a repeat message that allows the user to dismiss the prompt.
    /// </summary>
    /// <typeparam name="T">State Model</typeparam>
    public class ModalFlag<T>
    {
        private bool _ask { get; set; } = true;
        private bool _rememberChoice { get; set; } = false;
        private bool _asked { get; set; } = false;

        /// <summary>
        /// Expression to prompt the user with a formatted message.
        /// </summary>
        public Expression<Func<T, string>> PromptMessage { get; set; }

        /// <summary>
        /// Expression to prompt the user to allow them to dismiss the recurring prompt.
        /// </summary>
        public Expression<Func<bool, string>> RememberMessage { get; set; }

        /// <summary>
        /// Constructs a new modal flag instance.
        /// </summary>
        /// <param name="promptMessage"><see cref="PromptMessage"/></param>
        /// <param name="rememberMessage"><see cref="RememberMessage"/></param>
        public ModalFlag(Expression<Func<T, string>> promptMessage, Expression<Func<bool, string>> rememberMessage)
        {
            PromptMessage = promptMessage;
            RememberMessage = rememberMessage;
        }

        /// <summary>
        /// Tests the current source model for whether or not to render the prompt.
        /// </summary>
        /// <param name="source"><see cref="T"/></param>
        /// <returns>Flag for wether or not the prompt was rendered.</returns>
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
