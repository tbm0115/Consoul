using System;
using System.Threading.Tasks;

namespace ConsoulLibrary.Views
{
    /// <summary>
    /// Contains methods for fluently chaining renderings and managing multiple <see cref="IView"/>s
    /// </summary>
    public class ViewRenderer
    {
        /// <summary>
        /// Renders the specified implementation of <see cref="IView"/>
        /// </summary>
        /// <typeparam name="T">Implementation of <see cref="IView"/></typeparam>
        /// <returns>Reference to the current <see cref="ViewRenderer"/> to fluently chain commands.</returns>
        public ViewRenderer Render<T>() where T : IView
        {
            try
            {
                var view = Activator.CreateInstance(typeof(T));
                (view as IView).Render();
            }
            catch (Exception ex)
            {
                Consoul.Write(ex, "Failed to render Consoul view");
                if (RenderOptions.WaitOnError)
                {
                    Consoul.Wait();
                }
            }
            return this;
        }

        /// <summary>
        /// Renders the specified implementation of <see cref="IView"/>
        /// </summary>
        /// <typeparam name="T">Implementation of <see cref="IView"/></typeparam>
        /// <returns>Reference to the current <see cref="ViewRenderer"/> to fluently chain commands.</returns>
        public async Task<ViewRenderer> RenderAsync<T>() where T : IView
        {
            try
            {
                var view = Activator.CreateInstance(typeof(T));
                await (view as IView).RenderAsync();
            }
            catch (Exception ex)
            {
                Consoul.Write(ex, "Failed to render Consoul view");
                if (RenderOptions.WaitOnError)
                {
                    Consoul.Wait();
                }
            }
            return this;
        }

        /// <summary>
        /// Saves all inputs to the specified filepath.
        /// </summary>
        /// <param name="filepath">Filepath to save the XML containing the list of Consoul inputs.</param>
        public void SaveInput(string filepath)
        {
            var xRoutine = new XmlRoutine();
            xRoutine.SaveInputs(filepath);
        }
    }
}
