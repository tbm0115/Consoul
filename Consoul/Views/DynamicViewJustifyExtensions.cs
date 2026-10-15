using System;
using RgbColor = ConsoulLibrary.Color.RgbColor;
using SpectreColor = ConsoulLibrary.Color.Color;

namespace ConsoulLibrary
{
    /// <summary>
    /// Provides Spectre-style <see cref="Justify"/> alignment overloads for MVC-style dynamic view sections.
    /// </summary>
    public static class DynamicViewJustifyExtensions
    {
        /// <summary>
        /// Adds a named model section with a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Action<T> renderer, Justify alignment)
        {
            return Require(view).Section(title, renderer, (ConsoleColor?)null, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named model section with a title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Action<T> renderer, ConsoleColor? titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named model section with style text and a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Action<T> renderer, string titleStyle, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named model section with a reusable title style and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Action<T> renderer, Style titleStyle, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named model section with a Spectre-style title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Action<T> renderer, SpectreColor titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named model section with an RGB title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Action<T> renderer, RgbColor titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named model section with a dynamic title and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Justify alignment)
        {
            return Require(view).Section(title, renderer, (Func<T, ConsoleColor?>)null, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named model section with dynamic title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Func<T, ConsoleColor?> titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named model section with dynamic style text and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleStyle">Function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Func<T, string> titleStyle, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named model section with a dynamic reusable title style and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleStyle">Function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Func<T, Style> titleStyle, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named model section with a dynamic Spectre-style title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Func<T, SpectreColor> titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named model section with a dynamic RGB title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Func<T, RgbColor> titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named reusable-component section with a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the view renders.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, Justify alignment)
        {
            return Require(view).Section(title, renderable, (ConsoleColor?)null, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named reusable-component section with a title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the view renders.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, ConsoleColor? titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderable, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named reusable-component section with style text and a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the view renders.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, string titleStyle, Justify alignment)
        {
            return Require(view).Section(title, renderable, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named reusable-component section with a reusable title style and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the view renders.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, Style titleStyle, Justify alignment)
        {
            return Require(view).Section(title, renderable, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named reusable-component section with a Spectre-style title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the view renders.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, SpectreColor titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderable, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named reusable-component section with an RGB title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the view renders.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, RgbColor titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderable, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, Justify alignment)
        {
            return Require(view).Section(title, renderer, (ConsoleColor?)null, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with a title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, ConsoleColor? titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with style text and a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, string titleStyle, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with a reusable title style and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, Style titleStyle, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with a Spectre-style title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, SpectreColor titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with an RGB title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, RgbColor titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with a dynamic title and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Justify alignment)
        {
            return Require(view).Section(title, renderer, (Func<T, ConsoleColor?>)null, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with dynamic title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, ConsoleColor?> titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with dynamic style text and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleStyle">Function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, string> titleStyle, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with a dynamic reusable title style and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleStyle">Function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, Style> titleStyle, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with a dynamic Spectre-style title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, SpectreColor> titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a named renderable-factory section with a dynamic RGB title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> Section<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, RgbColor> titleColor, Justify alignment)
        {
            return Require(view).Section(title, renderer, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Action<T> renderer, Func<T, bool> isVisible, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, (ConsoleColor?)null, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with a title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Action<T> renderer, Func<T, bool> isVisible, ConsoleColor? titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with style text and a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Action<T> renderer, Func<T, bool> isVisible, string titleStyle, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with a reusable title style and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Action<T> renderer, Func<T, bool> isVisible, Style titleStyle, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with a Spectre-style title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Action<T> renderer, Func<T, bool> isVisible, SpectreColor titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with an RGB title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Action<T> renderer, Func<T, bool> isVisible, RgbColor titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with a dynamic title and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Func<T, bool> isVisible, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, (Func<T, ConsoleColor?>)null, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with dynamic title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Func<T, bool> isVisible, Func<T, ConsoleColor?> titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with dynamic style text and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Func<T, bool> isVisible, Func<T, string> titleStyle, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with a dynamic reusable title style and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Func<T, bool> isVisible, Func<T, Style> titleStyle, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with a dynamic Spectre-style title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Func<T, bool> isVisible, Func<T, SpectreColor> titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional named model section with a dynamic RGB title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Action that writes section content from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Action<T> renderer, Func<T, bool> isVisible, Func<T, RgbColor> titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional reusable-component section with a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the section is visible.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, Func<T, bool> isVisible, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderable, isVisible, (ConsoleColor?)null, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional reusable-component section with a title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the section is visible.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, Func<T, bool> isVisible, ConsoleColor? titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderable, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional reusable-component section with style text and a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the section is visible.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, Func<T, bool> isVisible, string titleStyle, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderable, isVisible, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional reusable-component section with a reusable title style and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the section is visible.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, Func<T, bool> isVisible, Style titleStyle, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderable, isVisible, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional reusable-component section with a Spectre-style title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the section is visible.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, Func<T, bool> isVisible, SpectreColor titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderable, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional reusable-component section with an RGB title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderable">Renderable component written each time the section is visible.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, IConsoulRenderable renderable, Func<T, bool> isVisible, RgbColor titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderable, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, (ConsoleColor?)null, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with a title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Optional foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, ConsoleColor? titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with style text and a <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, string titleStyle, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with a reusable title style and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Optional foreground style for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Style titleStyle, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with a Spectre-style title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, SpectreColor titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with an RGB title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Section title rendered as a horizontal rule.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Foreground color for the section rule title.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, string title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, RgbColor titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with a dynamic title and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, (Func<T, ConsoleColor?>)null, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with dynamic title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Func<T, ConsoleColor?> titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with dynamic style text and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Func<T, string> titleStyle, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with a dynamic reusable title style and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleStyle">Function that creates the rule title style from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Func<T, Style> titleStyle, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleStyle, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with a dynamic Spectre-style title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Func<T, SpectreColor> titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        /// <summary>
        /// Adds a conditional renderable-factory section with a dynamic RGB title color and <see cref="Justify"/> title alignment.
        /// </summary>
        /// <typeparam name="T">The model type used by the dynamic view.</typeparam>
        /// <param name="view">The dynamic view to configure.</param>
        /// <param name="title">Function that creates the section title from the current model state.</param>
        /// <param name="renderer">Function that creates a renderable component from the current model state.</param>
        /// <param name="isVisible">Predicate that returns <c>true</c> when the section should render.</param>
        /// <param name="titleColor">Function that creates the rule title color from the current model state.</param>
        /// <param name="alignment">Spectre-style alignment used for the section rule title.</param>
        /// <returns>The current <see cref="DynamicView{T}"/> instance.</returns>
        public static DynamicView<T> SectionWhen<T>(this DynamicView<T> view, Func<T, string> title, Func<T, IConsoulRenderable> renderer, Func<T, bool> isVisible, Func<T, RgbColor> titleColor, Justify alignment)
        {
            return Require(view).SectionWhen(title, renderer, isVisible, titleColor, ToRuleAlignment(alignment));
        }

        private static DynamicView<T> Require<T>(DynamicView<T> view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            return view;
        }

        private static RuleTitleAlignment ToRuleAlignment(Justify alignment)
        {
            return JustifyMap.ToRuleTitleAlignment(alignment);
        }
    }
}
