using System;
using System.Collections.Generic;
using System.Reflection;

namespace StyletIoC
{
    /// <summary>
    /// This IStyletIoCBuilder is the only way to create an IContainer. Binding are registered using the builder, than an IContainer generated.
    /// </summary>
    public interface IStyletIoCBuilder
    {
        /// <summary>
        /// Gets or sets the list of assemblies searched by Autobind and ToAllImplementatinos
        /// </summary>
        List<Assembly> Assemblies { get; set; }

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <param name="serviceType">Service to bind</param>
        /// <returns>Fluent interface to continue configuration</returns>
        IBindTo Bind(Type serviceType);

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <typeparam name="TService">Service to bind</typeparam>
        /// <returns>Fluent interface to continue configuration</returns>
        IBindTo Bind<TService>();

        /// <summary>
        /// Search the specified assembly(s) / the current assembly for concrete types, and self-bind them
        /// </summary>
        /// <param name="assemblies">Assembly(s) to search, or leave empty / null to search the current assembly</param>
        void Autobind(IEnumerable<Assembly> assemblies);

        /// <summary>
        /// Search the specified assembly(s) / the current assembly for concrete types, and self-bind them
        /// </summary>
        /// <param name="assemblies">Assembly(s) to search, or leave empty / null to search the current assembly</param>
        void Autobind(params Assembly[] assemblies);

        /// <summary>
        /// Add a single module to this builder
        /// </summary>
        /// <param name="module">Module to add</param>
        void AddModule(StyletIoCModule module);

        /// <summary>
        /// Add many modules to this builder
        /// </summary>
        /// <param name="modules">Modules to add</param>
        void AddModules(params StyletIoCModule[] modules);

        /// <summary>
        /// Once all bindings have been set, build an IContainer from which instances can be fetches
        /// </summary>
        /// <returns>An IContainer, which should be used from now on</returns>
        IContainer BuildContainer();
    }
}