using StyletIoC;
using System;
using System.Collections.Generic;

namespace Eto.Forms.Plus
{
	public abstract class BootstrapBase<TRootView>
	{
		protected IContainer Container;

		private void CreateIoCContainer(Application application)
		{
			var builder = new StyletIoCBuilder();
			builder.Assemblies = new List<System.Reflection.Assembly>
			{
				GetType().Assembly,
				typeof(BootstrapBase<>).Assembly
			};

			var viewFactory = new ViewFactory
			{
				Factory = GetInstance,
				ViewAssemblies = builder.Assemblies
			};
			builder.Bind<ViewFactory>().ToInstance(viewFactory);
			builder.Bind<Application>().ToInstance(application);
			builder.Bind<WindowManager>().To<WindowManager>().InSingletonScope();

			ConfigureIoC(builder);

			builder.Autobind();

			Container = builder.BuildContainer();
		}

		protected virtual void ConfigureIoC(IStyletIoCBuilder builder)
		{
		}

		private object GetInstance(Type type)
			=> Container.Get(type);

		public virtual void Run(Application application)
		{
			CreateIoCContainer(application);

			var windowManager = Container.Get<WindowManager>();
			var rootView = windowManager.CreateAndBind<TRootView>() as Form;
			application.Run(rootView);
		}
	}
}
